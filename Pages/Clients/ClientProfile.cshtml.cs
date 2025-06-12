using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FlightAlright.Data;
using FlightAlright.Models;
using Microsoft.EntityFrameworkCore;
using Stripe.Checkout;

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
        [BindProperty]
        public float? TopUpAmount { get; set; }
        public float? WalletBalance { get; set; }


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

            WalletBalance = account.Money;

            return Page();
        }

        public IActionResult OnPost()
        {
            var accountId = HttpContext.Session.GetInt32("AccountId");
            var account = _context.Account.FirstOrDefault(a => a.Id == accountId);
            account.Money += TopUpAmount;
            _context.SaveChanges();

            //Wywo³anie bramki p³atnoœci
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                    {
                        new SessionLineItemOptions
                        {
                            PriceData = new SessionLineItemPriceDataOptions
                            {
                                UnitAmount = (long)TopUpAmount*100,
                                Currency = "pln",
                                ProductData = new SessionLineItemPriceDataProductDataOptions
                                {
                                    Name = "Do³adowanie œrodków do wirtualnego portfela",
                                },
                            },
                            Quantity = 1,
                        },
                    },
                Mode = "payment",
                SuccessUrl = "http://localhost:5263/PaymentResults/WalletSuccess/",
                CancelUrl = "http://localhost:5263/PaymentResults/WalletFailure/",
            };

            var service = new SessionService();
            Session session = service.Create(options);
            TempData["PaymentSuccess"] = true;
            return Redirect(session.Url);
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
