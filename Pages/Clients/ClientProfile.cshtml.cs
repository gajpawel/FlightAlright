using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FlightAlright.Data;
using FlightAlright.Models;
using Microsoft.EntityFrameworkCore;

namespace FlightAlright.Pages.Clients
{
    public class ClientProfileModel : PageModel
    {
        private readonly FlightAlrightContext _context;

        public string ClientName { get; set; } = string.Empty;
        public Account account { get; set; }

        public List<Ticket> ActiveTickets { get; set; } = new();
        public List<Ticket> CancelledTickets { get; set; } = new();
        public List<Ticket> PastTickets { get; set; } = new();

        public List<Flight> Flights { get; set; } = new();

        public ClientProfileModel(FlightAlrightContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            var accountId = HttpContext.Session.GetInt32("AccountId");

            if (accountId == null)
                return RedirectToPage("/Login");

            account = _context.Account
                .Include(a => a.Role)
                .FirstOrDefault(a => a.Id == accountId);

            if (account == null)
                return RedirectToPage("/AccessDenied");

            ClientName = $"{account.Name}";

            UpdateFlightStatus();

            var tickets = _context.Ticket
                .Where(t => t.AccountId == accountId)
                .Include(t => t.Price)
                    .ThenInclude(p => p.Flight)
                        .ThenInclude(f => f.DepartureAirport)
                .Include(t => t.Price)
                    .ThenInclude(p => p.Flight)
                        .ThenInclude(f => f.ArrivalAirport)
                .Include(t => t.Price)
                    .ThenInclude(p => p.Class)
                .ToList();

            ActiveTickets = tickets.Where(t => t.Status == 'K').ToList();
            CancelledTickets = tickets.Where(t => t.Status == 'A').ToList();
            PastTickets = tickets.Where(t => t.Status == 'N').ToList();

           
            Flights = _context.Flight
                .Include(f => f.DepartureAirport)
                .Include(f => f.ArrivalAirport)
                .Where(f => f.Status == true)
                .ToList();

            return Page();
        }



        public void UpdateFlightStatus()
        {
            var now = DateTime.UtcNow;

            var flights = _context.Flight
                .Include(f => f.ArrivalAirport)
                .Where(f => f.Status == true)
                .ToList();

            foreach (var flight in flights)
            {
                var adjustedArrivalTime = flight.ArrivalDate;

                if (adjustedArrivalTime < now)
                {
                    flight.Status = false;
                    var oldFightPrices = _context.Price.Where(p => p.FlightId == flight.Id).ToList();
                    foreach (var price in oldFightPrices)
                    {
                        var oldTickets = _context.Ticket.Where(t => t.PriceId == price.Id).ToList();
                        foreach (var ticket in oldTickets)
                        {
                            if (ticket.Status == 'A')
                                _context.Remove(ticket);
                            else if (ticket.Status == 'K')
                                ticket.Status = 'N';
                        }
                    }
                }
            }

            _context.SaveChanges();
        }
    }
}
