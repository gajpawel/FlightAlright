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

        [BindProperty]
        public List<int> SelectedEmployeeIds { get; set; }

        public SelectList AirportSelectList { get; set; }
        public SelectList PlaneSelectList { get; set; }
        public SelectList EmployeeSelectList { get; set; }


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

            // Przygotuj listê do wyœwietlenia w formacie: "Imiê Nazwisko (Stanowisko)"
            var employeeItems = employees.Select(e => new SelectListItem
            {
                Value = e.Id.ToString(),
                Text = $"{e.Account.Name} {e.Account.Surname} ({e.Position.Name})"
            }).ToList();

            EmployeeSelectList = new SelectList(employeeItems, "Value", "Text");
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
            Flight.DepartureDate.Value.AddHours(Flight.DepartureAirport.TimeZoneOffset.Value);
            Flight.ArrivalDate.Value.AddHours(Flight.ArrivalAirport.TimeZoneOffset.Value);
            _context.Flight.Add(Flight);
            _context.SaveChanges();

            if (SelectedEmployeeIds != null && SelectedEmployeeIds.Any())
            {
                foreach (var empId in SelectedEmployeeIds)
                {
                    _context.Crew.Add(new Crew
                    {
                        FlightId = Flight.Id,
                        EmployeeId = empId
                    });
                }

                _context.SaveChanges();
            }

            return RedirectToPage("/Admin/AdminProfile"); // lub inna strona po zapisaniu
        }
    }
}