using FlightAlright.Data;
using FlightAlright.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FlightAlright.Pages.Employees
{
    public class FlightScheduleModel : PageModel
    {
        private readonly FlightAlrightContext _context;

        public FlightScheduleModel(FlightAlrightContext context)
        {
            _context = context;
        }
        public List<Tuple<Flight, List<Crew>>> SchedulePositions = new();
        public List<Flight> Flights { get; set; } = new();
        
        public async Task OnGetAsync()
        {
            //var userEmail = User.Identity?.Name;
            var accountId = HttpContext.Session.GetInt32("AccountId");

            var employee = await _context.Employee
                .FirstOrDefaultAsync(e => e.AccountId == accountId);

            if (employee == null)
                return;

            Flights = await _context.Crew
                .Where(c => c.EmployeeId == employee.Id)
                .Include(c => c.Flight)
                    .ThenInclude(f => f.DepartureAirport)
                .Include(c => c.Flight)
                    .ThenInclude(f => f.ArrivalAirport)
                .Include(c => c.Flight)
                    .Where(c => c.Flight.Status == true)
                .Select(c => c.Flight!)
                .ToListAsync();
            foreach (var flight in Flights)
            {
                var crewList = _context.Crew
                    .Include(c => c.Employee)
                        .ThenInclude(e => e.Account)
                    .Include(c => c.Flight)
                    .Where(c => c.FlightId == flight.Id)
                    .ToList();
                Tuple<Flight, List<Crew>> temp = new Tuple<Flight, List<Crew>>(flight, crewList);
                SchedulePositions.Add(temp);
            }
        }

    }
}
