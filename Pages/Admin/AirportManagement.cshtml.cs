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
        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Airport.Add(NewAirport);
            await _context.SaveChangesAsync();


            return RedirectToPage(); // Odœwie¿enie po dodaniu
        }
    }
}
