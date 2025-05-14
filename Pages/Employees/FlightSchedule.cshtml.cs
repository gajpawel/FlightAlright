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

        public List<Flight> Flights { get; set; } = new();

        public async Task OnGetAsync()
        {
            var userEmail = User.Identity?.Name;

            var employee = await _context.Employee
                .Include(e => e.Account)
                .FirstOrDefaultAsync(e => e.Account != null && e.Account.Login == userEmail);

            if (employee == null)
                return;

            Flights = await _context.Crew
                .Where(c => c.EmployeeId == employee.Id)
                .Include(c => c.Flight)
                    .ThenInclude(f => f.Crew)
                        .ThenInclude(cm => cm.Employee)
                            .ThenInclude(e => e.Account)
                .Include(c => c.Flight)
                    .ThenInclude(f => f.DepartureAirport)
                .Include(c => c.Flight)
                    .ThenInclude(f => f.ArrivalAirport)
                .Select(c => c.Flight!)
                .ToListAsync();
        }

    }
}
