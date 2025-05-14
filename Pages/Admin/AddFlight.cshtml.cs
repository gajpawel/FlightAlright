using FlightAlright.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using FlightAlright.Data;
using Microsoft.EntityFrameworkCore;

namespace FlightAlright.Pages.Admin
{
    public class AddFlightModel : PageModel
    {
        private readonly FlightAlrightContext _context;

        public AddFlightModel(FlightAlrightContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Flight Flight { get; set; }

        public SelectList AirportSelectList { get; set; }
        public SelectList PlaneSelectList { get; set; }

        public void OnGet()
        {
            AirportSelectList = new SelectList(_context.Airport.ToList(), "Id", "Name");

            var planes = _context.Plane
                .Include(p => p.Brand)
                .ToList();

            var plane = planes.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = $"{p.Brand.Name} {p.Brand.Model} ({p.Id})"
            }).ToList();

            PlaneSelectList = new SelectList(plane, "Value", "Text");

            var employees = _context.Employee
            .Include(e => e.Account)
            .Include(e => e.Position)
            .ToList();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                OnGet(); // reload lists
                return Page();
            }
            Flight.DepartureAirport = _context.Airport.FirstOrDefault(A => A.Id == Flight.DepartureAirportId);
            Flight.ArrivalAirport = _context.Airport.FirstOrDefault(A => A.Id == Flight.ArrivalAirportId);
            Flight.DepartureDate = Flight.DepartureDate?.AddHours(-Flight.DepartureAirport.TimeZoneOffset.Value);
            Flight.ArrivalDate = Flight.ArrivalDate?.AddHours(-Flight.ArrivalAirport.TimeZoneOffset.Value);
            Flight.Status = true;

            if (Flight.DepartureDate >= Flight.ArrivalDate || Flight.ArrivalDate < DateTime.UtcNow || Flight.DepartureDate < DateTime.UtcNow || Flight.ArrivalAirportId==Flight.DepartureAirportId)
            {
                ModelState.AddModelError(string.Empty, "SprawdŸ poprawnoœæ wpisywanych danych");
                OnGet();
                return Page();
            }

            _context.Flight.Add(Flight);
            _context.SaveChanges();

            return RedirectToPage("/Admin/CrewManagement", new { flightId = Flight.Id });
        }
    }
}