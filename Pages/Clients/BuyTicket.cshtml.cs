using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FlightAlright.Data;
using FlightAlright.Models;
using System.Linq;
using System.Collections.Generic;

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
        public int SelectedClassId { get; set; }

        [BindProperty]
        public int SeatCount { get; set; } = 1;

        [BindProperty]
        public bool ExtraLuggage { get; set; } = false;

        public List<SelectListItem> ClassOptions { get; set; }

        public IActionResult OnGet(int flightId)
        {
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

            for (int i = 0; i < SeatCount; i++)
            {
                var ticket = new Ticket
                {
                    AccountId = accountId.Value,
                    PriceId = price.Id,
                    TicketPrice = price.CurrentPrice,
                    Status = 'K',
                    ExtraLuggage = ExtraLuggage,
                    Seating = sold + i + 1
                };

                _context.Ticket.Add(ticket);
            }

            _context.SaveChanges();

            TempData["SuccessMessage"] = $"Kupiono {SeatCount} bilet(ów) w klasie {price.Class?.Name}.";
            return RedirectToPage("/Clients/ClientProfile");
        }

        private void LoadClassOptions()
        {
            ClassOptions = _context.Price
                .Include(p => p.Class)
                .Where(p => p.FlightId == FlightId)
                .Select(p => new SelectListItem
                {
                    Value = p.Class.Id.ToString(),
                    Text = $"{p.Class.Name} (Cena: {p.CurrentPrice} z³)"
                })
                .ToList();
        }
    }
}