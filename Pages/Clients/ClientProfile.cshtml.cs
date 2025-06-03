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
        public Account account {  get; set; }

        public List<Ticket> ActiveTickets { get; set; } = new();
        public List<Ticket> CancelledTickets { get; set; } = new();
        public List<Ticket> PastTickets { get; set; } = new();


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
                return RedirectToPage("/AccessDenied"); // lub do innej strony

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

            var now = DateTime.Now;

            ActiveTickets = tickets
                .Where(t => t.Status == 'K')
                .ToList();

            CancelledTickets = tickets
                .Where(t => t.Status == 'A')
                .ToList();

            PastTickets = tickets
                .Where(t => t.Status == 'N')
                .ToList();

            // Testowe dane dla konta testowego
            /*
            var account = await _context.Account.FindAsync(accountId);
            if (account != null && account.Login == "testuser" && !_context.Ticket.Any(t => t.AccountId == accountId))
            {
                var flight1 = new Flight
                {
                    DepartureAirportId = 1,
                    ArrivalAirportId = 2,
                    DepartureDate = DateTime.Now.AddDays(2),
                    ArrivalDate = DateTime.Now.AddDays(2).AddHours(2),
                    Status = true
                };
                var flight2 = new Flight
                {
                    DepartureAirportId = 1,
                    ArrivalAirportId = 2,
                    DepartureDate = DateTime.Now.AddDays(-5),
                    ArrivalDate = DateTime.Now.AddDays(-5).AddHours(2),
                    Status = true
                };
                var flight3 = new Flight
                {
                    DepartureAirportId = 1,
                    ArrivalAirportId = 2,
                    DepartureDate = DateTime.Now.AddDays(5),
                    ArrivalDate = DateTime.Now.AddDays(5).AddHours(2),
                    Status = true
                };

                _context.Flight.AddRange(flight1, flight2, flight3);
                await _context.SaveChangesAsync();

                var price1 = new Price { FlightId = flight1.Id, ClassId = 1, CurrentPrice = 400 };
                var price2 = new Price { FlightId = flight2.Id, ClassId = 1, CurrentPrice = 300 };
                var price3 = new Price { FlightId = flight3.Id, ClassId = 1, CurrentPrice = 500 };

                _context.Price.AddRange(price1, price2, price3);
                await _context.SaveChangesAsync();

                var ticket1 = new Ticket
                {
                    AccountId = accountId,
                    PriceId = price1.Id,
                    TicketPrice = 400,
                    Status = 'K',
                    ExtraLuggage = false,
                    Seating = 12
                };
                var ticket2 = new Ticket
                {
                    AccountId = accountId,
                    PriceId = price2.Id,
                    TicketPrice = 300,
                    Status = 'N',
                    ExtraLuggage = true,
                    Seating = 5
                };
                var ticket3 = new Ticket
                {
                    AccountId = accountId,
                    PriceId = price3.Id,
                    TicketPrice = 500,
                    Status = 'A',
                    ExtraLuggage = false,
                    Seating = 20
                };

                _context.Ticket.AddRange(ticket1, ticket2, ticket3);
                await _context.SaveChangesAsync();
            }*/

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

