using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using FlightAlright.Models;
using FlightAlright.Data;

namespace FlightAlright.Pages.Admin
{
    public class PriceManagementModel : PageModel
    {
        private readonly FlightAlrightContext _context;

        public PriceManagementModel(FlightAlrightContext context)
        {
            _context = context;
        }

        [BindProperty]
        public FlightAlright.Models.Flight Flight { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Wybierz klasê miejsc.")]
        public int SelectedClassId { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "WprowadŸ cenê.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Cena musi byæ wiêksza ni¿ 0.")]
        public float Price { get; set; }

        public SelectList ClassSelectList { get; set; }
        public List<Price> ExistingPrices { get; set; }

        public async Task<IActionResult> OnGetAsync(int flightId)
        {
            Flight = await _context.Flight
                .Include(f => f.Plane)
                .ThenInclude(p => p.Brand)
                .Include(f => f.DepartureAirport)
                .Include(f => f.ArrivalAirport)
                .FirstOrDefaultAsync(f => f.Id == flightId);

            if (Flight == null)
                return NotFound();

            await LoadExistingPricesAsync();
            await LoadAvailableClassesAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Flight = await _context.Flight
                .Include(f => f.Plane)
                .ThenInclude(p => p.Brand)
                .FirstOrDefaultAsync(f => f.Id == Flight.Id);

            if (!ModelState.IsValid || Flight == null)
            {
                await LoadAvailableClassesAsync();
                await LoadExistingPricesAsync();
                return Page();
            }

            var newPrice = new Price
            {
                FlightId = Flight.Id,
                ClassId = SelectedClassId,
                CurrentPrice = Price,
                Status = true
            };

            await LoadExistingPricesAsync();
            foreach (var price in ExistingPrices)
            {
                if (price.ClassId == newPrice.ClassId)
                {
                    price.CurrentPrice = Price;
                    await _context.SaveChangesAsync();
                    return RedirectToPage(new { flightId = Flight.Id });
                }
            }
            _context.Price.Add(newPrice);
            await _context.SaveChangesAsync();
            var seatsNumber = await _context.Class
                .Where(c => c.Id == newPrice.ClassId)
                .Select(c => c.SeatsNumber)
                .FirstOrDefaultAsync();
            for (int i = 1; i <= seatsNumber; i++)
            {
                var newEmptyTicket = new Ticket
                {
                    PriceId = newPrice.Id,
                    Status = 'D',
                    Seating = i
                };
                _context.Ticket.Add(newEmptyTicket); //dodanie pustego biletu
            }
            _context.SaveChanges();
            return RedirectToPage(new { flightId = Flight.Id });
        }

        private async Task LoadAvailableClassesAsync()
        {
            var planeBrandId = Flight.Plane.BrandId;

            var classes = await _context.Class
                .Where(c => c.BrandId == planeBrandId)
                .ToListAsync();

            ClassSelectList = new SelectList(classes, "Id", "Name");
        }

        private async Task LoadExistingPricesAsync()
        {
            ExistingPrices = await _context.Price
                .Include(p => p.Class)
                .Where(p => p.FlightId == Flight.Id && p.Status == true)
                .ToListAsync();
        }
    }
}