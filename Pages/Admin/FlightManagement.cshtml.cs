using FlightAlright.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using FlightAlright.Data;
using Microsoft.EntityFrameworkCore;

namespace FlightAlright.Pages.Admin
{
    public class FlightManagementModel : PageModel
    {
        private readonly FlightAlrightContext _context;

        public FlightManagementModel(FlightAlrightContext context)
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
            PlaneSelectList = new SelectList(_context.Plane.ToList(), "Id");

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

            return RedirectToPage(); // lub inna strona po zapisaniu
        }
    }
}