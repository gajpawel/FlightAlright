using System.Numerics;
using FlightAlright.Data;
using FlightAlright.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FlightAlright.Pages.Admin.Flights
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
        [BindProperty]
        public int? oldPlaneId { get; set; }


        public void OnGet(int flightId)
        {
            currentFlightId = flightId;
            Flight = _context.Flight.FirstOrDefault(f => f.Id == flightId);
            // Pobierz ID pracowników przypisanych do danego lotu
            SelectedEmployeeIds = _context.Crew
                .Where(c => c.FlightId == Flight.Id)
                .Select(c => c.EmployeeId)
                .ToList();
            PlaneId = Flight.PlaneId;
            oldPlaneId = Flight.PlaneId;
            // Za³aduj wszystkie dane pracowników i ich lotów
            var unavailableEmployeeIds = _context.Crew
                .Include(c => c.Flight)
                .Where(c =>
                    // Pomijamy za³ogê bie¿¹cego lotu, jeœli edytujemy istniej¹cy lot
                    c.FlightId != Flight.Id &&
                    // Wystêpuje konflikt czasowy z bie¿¹cym lotem
                    c.Flight.DepartureDate < Flight.ArrivalDate &&
                    c.Flight.ArrivalDate > Flight.DepartureDate
                )
                .Select(c => c.EmployeeId)
                .Distinct()
                .ToList();

            // Teraz pobierz tylko dostêpnych pracowników
            EmployeeItems = _context.Employee
                .Include(e => e.Account)
                .Include(e => e.Position)
                .Where(e => !unavailableEmployeeIds.Contains(e.Id))
                .ToList()
                .Select(e => new SelectListItem
                {
                    Value = e.Id.ToString(),
                    Text = $"{e.Account.Name} {e.Account.Surname} ({e.Position.Name})",
                    Selected = SelectedEmployeeIds.Contains(e.Id)
                }).ToList();

            // ZnajdŸ zajête samoloty w czasie tego lotu (z wykluczeniem obecnego lotu jeœli edytujesz)
            var unavailablePlaneIds = _context.Flight
                .Where(f =>
                    f.Id != Flight.Id &&
                    f.DepartureDate < Flight.ArrivalDate &&
                    f.ArrivalDate > Flight.DepartureDate
                )
                .Select(f => f.PlaneId)
                .Distinct()
                .ToList();

            // Pobierz dostêpne samoloty (czyli nie na liœcie zajêtych)
            var availablePlanes = _context.Plane
                .Include(p => p.Brand)
                .Where(p => !unavailablePlaneIds.Contains(p.Id) && p.Status != 'N')
                .ToList();

            // Przygotuj listê SelectListItem
            var plane = availablePlanes.Select(p => new SelectListItem
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
            var oldPlane = _context.Plane.FirstOrDefault(p => p.Id == PlaneId);
            var newPlane = _context.Plane.FirstOrDefault(p => p.Id == oldPlaneId);
            if (newPlane?.BrandId != oldPlane?.BrandId)
            {
                var pricesToCancel = _context.Price.Include(p => p.Flight)
                    .Where(p => p.Flight.PlaneId == oldPlaneId && p.FlightId == Flight.Id);
                foreach (var p in pricesToCancel)
                {
                    var debug = p.Id;
                    var ticketsToCancel = _context.Ticket
                        .Where(t => t.PriceId == p.Id && t.AccountId!=null).ToList();
                    foreach (var t in ticketsToCancel)
                    {
                        t.Status = 'A'; //uniewa¿nij zakupione bilety
                    }
                    var ticketsToDelete = _context.Ticket
                        .Where(t => t.PriceId == p.Id && t.AccountId == null).ToList();
                    _context.RemoveRange(ticketsToDelete); //usuñ puste bilety
                    p.Status = false;
                    _context.SaveChanges();
                }
                _context.SaveChanges();
            }
            _context.SaveChanges();
            // Usuñ dotychczasowe przypisania za³ogi dla lotu
            var existingCrew = _context.Crew.Where(c => c.FlightId == Flight.Id);
            _context.Crew.RemoveRange(existingCrew);
            _context.SaveChanges();

            // Dodaj nowe przypisania tylko dla zaznaczonych pracowników
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

            return RedirectToPage("/Admin/Flights/PriceManagement", new { flightId = Flight.Id });
        }

    }
}
