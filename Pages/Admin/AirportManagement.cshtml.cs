using FlightAlright.Data;
using FlightAlright.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FlightAlright.Pages.Admin
{
    public class AirportManagementModel : PageModel
    {
        private readonly FlightAlrightContext _context;

        public AirportManagementModel(FlightAlrightContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Airport NewAirport { get; set; } = new();

        public List<Airport> ExistingAirports { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int? EditId { get; set; }

        public void OnGet()
        {
            ExistingAirports = _context.Airport.ToList();

            if (EditId.HasValue)
            {
                var existing = _context.Airport.FirstOrDefault(a => a.Id == EditId.Value);
                if (existing != null)
                    NewAirport = existing;
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                ExistingAirports = _context.Airport.ToList();
                return Page();
            }

            if (NewAirport.Id == 0)
            {
                
                _context.Airport.Add(NewAirport);
            }
            else
            {
                
                var existing = _context.Airport.Find(NewAirport.Id);
                if (existing != null)
                {
                    existing.Code = NewAirport.Code;
                    existing.Name = NewAirport.Name;
                    existing.City = NewAirport.City;
                    existing.Country = NewAirport.Country;
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToPage(); // Odœwie¿ stronê
        }
    }
}
