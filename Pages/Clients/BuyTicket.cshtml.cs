using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FlightAlright.Data;
using FlightAlright.Models;
using System.Linq;
using System.Collections.Generic;
using Stripe.Checkout;
using System.ComponentModel.DataAnnotations;

namespace FlightAlright.Pages.Clients
{
    public class BuyTicketModel : PageModel
    {
        private readonly FlightAlrightContext _context;

        public BuyTicketModel(FlightAlrightContext context)
        {
            _context = context;
        }

        [BindProperty]
        public int FlightId { get; set; }

        public Flight Flight { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Wybór klasy jest obowi¹zkowy.")]
        public int SelectedClassId { get; set; }

        [BindProperty]
        public int SeatCount { get; set; } = 1;

        [BindProperty]
        public bool ExtraLuggage { get; set; } = false;

        public List<SelectListItem> ClassOptions { get; set; }

        public Account Account { get; set; }
        [BindProperty]
        public string PaymentMethod { get; set; } = "";

        public IActionResult OnGet(int flightId)
        {
            PaymentMethod = "card";
            var accountId = HttpContext.Session.GetInt32("AccountId");
            Account = _context.Account.FirstOrDefault(a => a.Id == accountId);
            Flight = _context.Flight
                .Include(f => f.DepartureAirport)
                .Include(f => f.ArrivalAirport)
                .FirstOrDefault(f => f.Id == flightId && f.Status == true);

            if (Flight == null)
                return RedirectToPage("/Clients/ClientProfile");

            FlightId = flightId;
            LoadClassOptions();

            return Page();
        }

        public IActionResult OnPost()
        {
            var accountId = HttpContext.Session.GetInt32("AccountId");
            if (accountId == null)
                return RedirectToPage("/Login");
            var account = _context.Account.FirstOrDefault(a => a.Id == accountId);

            var price = _context.Price
                .Include(p => p.Class)
                .FirstOrDefault(p => p.FlightId == FlightId && p.Class.Id == SelectedClassId);

            if (price == null)
            {
                ModelState.AddModelError(string.Empty, "Nie znaleziono wybranej klasy.");
                LoadClassOptions();
                return Page();
            }

            var totalSeats = price.Class.SeatsNumber ?? 0;
            var sold = _context.Ticket.Count(t => t.PriceId == price.Id && t.Status == 'K');

            if (SeatCount > (totalSeats - sold))
            {
                ModelState.AddModelError(string.Empty, "Brak wystarczaj¹cej liczby miejsc.");
                LoadClassOptions();
                return Page();
            }

            var LuggagePrice = ExtraLuggage ? _context.Flight.FirstOrDefault(f => f.Id == FlightId).LuggagePrice : 0;
            float totalPrice = 0;
            for (int i = 0; i < SeatCount; i++)
            {
                //Szukam pierwszego dostêpnego biletu i dodajê do niego dane kupuj¹cego
                var ticket = _context.Ticket.FirstOrDefault(f => f.PriceId == price.Id && f.Status=='D');
                ticket.AccountId = accountId.Value;
                ticket.TicketPrice = price.CurrentPrice + LuggagePrice;
                ticket.Status = 'R';
                ticket.ExtraLuggage = ExtraLuggage;
                _context.SaveChanges();

                totalPrice += ticket.TicketPrice.Value;
            }

            _context.SaveChanges();

            if (PaymentMethod == "wallet")
            {
                if (account.Money < totalPrice)
                {
                    TempData["SuccessMessage"] = "Zbyt ma³o œrodków w wirtualnym portfelu.";
                    var reservedTickets = _context.Ticket.Where(t => t.AccountId == accountId && t.Status == 'R').ToList();
                    foreach (var ticket in reservedTickets)
                    {
                        ticket.Status = 'D';
                        ticket.AccountId = null;
                        ticket.TicketPrice = null;
                        ticket.ExtraLuggage = null;
                    }
                    _context.SaveChanges();
                    return RedirectToPage("/Clients/ClientProfile");
                }
                TempData["SuccessMessage"] = $"Kupiono {SeatCount} bilet(ów) w klasie {price.Class?.Name}.";
                account.Money -= totalPrice;
                _context.SaveChanges();
                return RedirectToPage("/Clients/ClientProfile");
            }
            TempData["SuccessMessage"] = $"Kupiono {SeatCount} bilet(ów) w klasie {price.Class?.Name}.";
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
                                UnitAmount = (long)totalPrice*100,
                                Currency = "pln",
                                ProductData = new SessionLineItemPriceDataProductDataOptions
                                {
                                    Name = "Op³ata za bilet",
                                },
                            },
                            Quantity = 1,
                        },
                    },
                Mode = "payment",
                SuccessUrl = "http://localhost:5263/PaymentResults/PaymentSuccess/" + accountId,
                CancelUrl = "http://localhost:5263/PaymentResults/PaymentFailure/" + accountId,
            };

            var service = new SessionService();
            Session session = service.Create(options);
            TempData["PaymentSuccess"] = true;
            return Redirect(session.Url);
        }

        private void LoadClassOptions()
        {
            ClassOptions = _context.Price
                .Include(p => p.Class)
                .Where(p => p.FlightId == FlightId && p.Status == true)
                .Select(p => new SelectListItem
                {
                    Value = p.Class.Id.ToString(),
                    Text = $"{p.Class.Name} (Cena: {p.CurrentPrice} z³)"
                })
                .ToList();
        }
    }
}