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
                ticketPrice = ticket.TicketPrice.Value * 80;
            else ticketPrice = ticket.TicketPrice.Value * 100;
            ticketId = ticket.Id;
        }

        public IActionResult OnPost()
        {
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                    {
                        new SessionLineItemOptions
                        {
                            PriceData = new SessionLineItemPriceDataOptions
                            {
                                UnitAmount = (long)ticketPrice,
                                Currency = "pln",
                                ProductData = new SessionLineItemPriceDataProductDataOptions
                                {
                                    Name = "Zwrot biletu",
                                },
                            },
                            Quantity = 1,
                        },
                    },
                Mode = "payment",
                SuccessUrl = "http://localhost:5263/Clients/PaymentSuccess/" + ticketId.ToString(),
                CancelUrl = "http://localhost:5263/Clients/PaymentCancel",
            };

            var service = new SessionService();
            Session session = service.Create(options);
            TempData["PaymentSuccess"] = true;
            return Redirect(session.Url);
        }
    }
}
