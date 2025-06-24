using FlightAlright.Data;
using FlightAlright.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Stripe.Checkout;

namespace FlightAlright.Pages.Clients
{
    public class CancelTicketModel : PageModel
    {
        private readonly FlightAlrightContext _context;

        public double days { get; set; }
        public int departureOffset { get; set; }
        public int arrivalOffset { get; set; }
        [BindProperty]
        public Ticket ticket { get; set; }
        [BindProperty]
        public float ticketPrice { get; set; }
        [BindProperty]
        public int ticketId { get; set; }

        public CancelTicketModel(FlightAlrightContext context)
        {
            _context = context;
        }
        public void OnGet(int ticketId)
        {
            ticket = _context.Ticket.Include(t => t.Price).ThenInclude(p => p.Flight).ThenInclude(f => f.ArrivalAirport)
                .FirstOrDefault(t => t.Id == ticketId);
            arrivalOffset = ticket.Price.Flight.ArrivalAirport.TimeZoneOffset.Value;
            var ticket2 = _context.Ticket.Include(t => t.Price).ThenInclude(p => p.Flight).ThenInclude(f => f.DepartureAirport)
                .FirstOrDefault(t => t.Id == ticketId);
            departureOffset = ticket2.Price.Flight.DepartureAirport.TimeZoneOffset.Value;
            days = (ticket.Price.Flight.DepartureDate.Value - DateTime.UtcNow).TotalDays;
            if (days < 20)
                ticketPrice = ticket.TicketPrice.Value * 0.8f;
            else ticketPrice = ticket.TicketPrice.Value;
            ticketId = ticket.Id;
        }

        public IActionResult OnPost()
        {
            var ticket = _context.Ticket.FirstOrDefault(t => t.Id == ticketId);
            if (ticket == null)
                return Page();
            ticket.Status = 'A';
            var accountId = HttpContext.Session.GetInt32("AccountId");
            if (accountId == null)
                return RedirectToPage("/Login");
            var account = _context.Account.FirstOrDefault(a => a.Id == accountId);
            account.Money += ticketPrice;
            Ticket emptyticket = new Ticket
            {
                AccountId = null,
                PriceId = ticket.PriceId,
                TicketPrice = null,
                ExtraLuggage = null,
                Status = 'D',
                Seating = ticket.Seating
            };
            _context.Add(emptyticket);
            _context.SaveChanges();
            return RedirectToPage("/Clients/ClientProfile");
        }
    }
}
