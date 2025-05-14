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

        [BindProperty(SupportsGet = true)]
        public int currentFlightId { get; set; }

        [BindProperty]
        public List<int?> SelectedEmployeeIds { get; set; } = new();
        public List<SelectListItem> EmployeeItems { get; set; }
        public List<Crew> existingCrew { get; set; }


        public void OnGet(int flightId)
        {
            currentFlightId = flightId;
            // Pobierz ID pracowników przypisanych do danego lotu
            SelectedEmployeeIds = _context.Crew
                .Where(c => c.FlightId == currentFlightId)
                .Select(c => c.EmployeeId)
                .ToList();

            // Przygotuj listê wszystkich pracowników z oznaczeniem "Selected"
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
        }

        public IActionResult OnPost()
        {
            if (SelectedEmployeeIds != null && SelectedEmployeeIds.Any())
            {
                foreach (var empId in SelectedEmployeeIds)
                {
                    _context.Crew.Add(new Crew
                    {
                        FlightId = currentFlightId,
                        EmployeeId = empId
                    });
                }

                _context.SaveChanges();
            }
            return RedirectToPage("/Admin/PriceManagement", new { flightId = currentFlightId });
        }
    }
}
