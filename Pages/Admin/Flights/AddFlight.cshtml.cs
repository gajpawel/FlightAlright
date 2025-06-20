using FlightAlright.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using FlightAlright.Data;
using Microsoft.EntityFrameworkCore;

namespace FlightAlright.Pages.Admin.Flights
{
    public class AddFlightModel : PageModel
    {
        private readonly FlightAlrightContext _context;

        public AddFlightModel(FlightAlrightContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Flight Flight { get; set; } = new Flight();

        public SelectList AirportSelectList { get; set; }
        [BindProperty]
        public int newFlightId { get; set; }

        [BindProperty]
        public DateTime? departureDate { get; set; }
        [BindProperty]
        public DateTime? arrivalDate { get; set; }

        public void OnGet(int flightId)
        {
            newFlightId = flightId;
            Flight = _context.Flight.FirstOrDefault(f => f.Id == newFlightId);

            AirportSelectList = new SelectList(_context.Airport.ToList(), "Id", "Name");

            var employees = _context.Employee
            .Include(e => e.Account)
            .Include(e => e.Position)
            .ToList();
            if (flightId != 0)
            {
                departureDate = Flight.DepartureDate?.AddHours(Flight.DepartureAirport.TimeZoneOffset.Value);
                arrivalDate = Flight.ArrivalDate?.AddHours(Flight.ArrivalAirport.TimeZoneOffset.Value);
            }
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                OnGet(newFlightId); // reload lists
                return Page();
            }

            Flight.DepartureAirport = _context.Airport.FirstOrDefault(a => a.Id == Flight.DepartureAirportId);
            Flight.ArrivalAirport = _context.Airport.FirstOrDefault(a => a.Id == Flight.ArrivalAirportId);

            Flight.DepartureDate = departureDate?.AddHours(-Flight.DepartureAirport.TimeZoneOffset.Value);
            Flight.ArrivalDate = arrivalDate?.AddHours(-Flight.ArrivalAirport.TimeZoneOffset.Value);
            Flight.Status = true;

            if (Flight.DepartureDate >= Flight.ArrivalDate ||
                Flight.ArrivalDate < DateTime.UtcNow ||
                Flight.DepartureDate < DateTime.UtcNow ||
                Flight.ArrivalAirportId == Flight.DepartureAirportId)
            {
                ModelState.AddModelError(string.Empty, "SprawdŸ poprawnoœæ wpisywanych danych");
                OnGet(newFlightId);
                return Page();
            }

            if (newFlightId == 0)
            {
                // Nowy lot
                _context.Flight.Add(Flight);
                _context.SaveChanges(); // zapisujemy, ¿eby uzyskaæ wygenerowane Flight.Id
                newFlightId = Flight.Id;
            }
            else
            {
                // Aktualizacja istniej¹cego lotu
                var existingFlight = _context.Flight.FirstOrDefault(f => f.Id == newFlightId);
                if (existingFlight == null)
                {
                    return NotFound();
                }

                // aktualizuj dane
                existingFlight.DepartureAirportId = Flight.DepartureAirportId;
                existingFlight.ArrivalAirportId = Flight.ArrivalAirportId;
                existingFlight.DepartureDate = Flight.DepartureDate;
                existingFlight.ArrivalDate = Flight.ArrivalDate;
                existingFlight.Status = Flight.Status;

                _context.Update(existingFlight);
                _context.SaveChanges();
            }

            return RedirectToPage("/Admin/Flights/CrewManagement", new { flightId = newFlightId });
        }
    }
}