using System.Numerics;
using FlightAlright.Data;
using FlightAlright.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FlightAlright.Pages.Admin
{
    public class CrewManagementModel : PageModel
    {
        private readonly FlightAlrightContext _context;

        public CrewManagementModel(FlightAlrightContext context)
        {
            _context = context;
        }

        [BindProperty]
        public List<int?> SelectedEmployeeIds { get; set; } = new();
        public List<SelectListItem> EmployeeItems { get; set; }
        public List<Crew> existingCrew { get; set; }

        public SelectList PlaneSelectList { get; set; }
        [BindProperty]
        public Flight Flight { get; set; }
        [BindProperty]
        public int currentFlightId { get; set; }
        [BindProperty]
        public int? PlaneId { get; set; }


        public void OnGet(int flightId)
        {
            currentFlightId = flightId;
            Flight = _context.Flight.FirstOrDefault(f => f.Id == flightId);
            // Pobierz ID pracownik�w przypisanych do danego lotu
            SelectedEmployeeIds = _context.Crew
                .Where(c => c.FlightId == Flight.Id)
                .Select(c => c.EmployeeId)
                .ToList();
            PlaneId = Flight.PlaneId;
            // Przygotuj list� wszystkich pracownik�w z oznaczeniem "Selected"
            EmployeeItems = _context.Employee
                .Include(e => e.Account)
                .Include(e => e.Position)
                .ToList()
                .Select(e => new SelectListItem
                {
                    Value = e.Id.ToString(),
                    Text = $"{e.Account.Name} {e.Account.Surname} ({e.Position.Name})",
                    Selected = SelectedEmployeeIds.Contains(e.Id)
                }).ToList();

            var planes = _context.Plane
                .Include(p => p.Brand)
                .ToList();

            var plane = planes.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = $"{p.Brand.Name} {p.Brand.Model} ({p.Id})"
            }).ToList();

            PlaneSelectList = new SelectList(plane, "Value", "Text");
        }

        public IActionResult OnPost()
        {
            Flight = _context.Flight.FirstOrDefault(f => f.Id == currentFlightId);
            Flight.PlaneId = PlaneId;
            _context.SaveChanges();
            // Usu� dotychczasowe przypisania za�ogi dla lotu
            var existingCrew = _context.Crew.Where(c => c.FlightId == Flight.Id);
            _context.Crew.RemoveRange(existingCrew);
            _context.SaveChanges();

            // Dodaj nowe przypisania tylko dla zaznaczonych pracownik�w
            if (SelectedEmployeeIds != null && SelectedEmployeeIds.Any())
            {
                foreach (var empId in SelectedEmployeeIds.Distinct())
                {
                    _context.Crew.Add(new Crew
                    {
                        FlightId = Flight.Id,
                        EmployeeId = empId
                    });
                }

                _context.SaveChanges();
            }

            return RedirectToPage("/Admin/PriceManagement", new { flightId = Flight.Id });
        }

    }
}
