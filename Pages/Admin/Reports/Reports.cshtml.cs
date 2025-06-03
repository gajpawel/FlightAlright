using FlightAlright.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Linq;

namespace FlightAlright.Pages.Admin.Reports
{
    public class ReportsModel : PageModel
    {
        private readonly FlightAlrightContext _context;

        [BindProperty(SupportsGet = true)]
        public string? Tab { get; set; } = "Tickets";

        public IList<TicketDto> Tickets { get; private set; } = new List<TicketDto>();
        public IList<FlightDto> Flights { get; private set; } = new List<FlightDto>();
        public IList<PaycheckDto> Paychecks { get; private set; } = new List<PaycheckDto>();

        public ReportsModel(FlightAlrightContext context) => _context = context;

        public IActionResult OnGet()
        {
            var accId = HttpContext.Session.GetInt32("AccountId");
            bool admin = _context.Account
                                 .Include(a => a.Role)
                                 .Any(a => a.Id == accId && a.Role!.Name == "Administrator");
            if (!admin) return RedirectToPage("/AccessDenied");

            Tab = (Tab ?? "Tickets") switch
            {
                "Flights" => "Flights",
                "Paycheck" => "Paycheck",
                _ => "Tickets"
            };

            if (Tab == "Tickets")
            {
                Tickets = _context.Ticket
                    .Include(t => t.Price)
                        .ThenInclude(p => p.Flight)
                            .ThenInclude(f => f.DepartureAirport)
                    .Include(t => t.Price)
                        .ThenInclude(p => p.Class)
                    .Select(t => new TicketDto
                    {
                        FlightCode = $"{t.Price!.Flight!.DepartureAirport!.Code}-{t.Price.Flight.Id}",
                        FlightDate = t.Price.Flight.DepartureDate ?? DateTime.MinValue,
                        ClassName = t.Price.Class!.Name,
                        Price = Convert.ToDecimal(t.TicketPrice, CultureInfo.InvariantCulture),
                        Status = t.Status == null ? "—" : t.Status.ToString()
                    })
                    .ToList();
            }
            else if (Tab == "Flights")
            {
                Flights = _context.Flight
                    .Include(f => f.DepartureAirport)
                    .Select(f => new
                    {
                        f.Id,
                        f.DepartureAirport!.Code,
                        f.DepartureDate
                    })
                    .AsEnumerable()
                    .Select(f => new FlightDto
                    {
                        FlightCode = $"{f.Code}-{f.Id}",
                        Date = f.DepartureDate ?? DateTime.MinValue,
                        TotalSeats = _context.Price
                                             .Include(p => p.Class)
                                             .Where(p => p.FlightId == f.Id)
                                             .Sum(p => p.Class.SeatsNumber ?? 0),
                        SoldSeats = _context.Ticket
                                             .Include(t => t.Price)
                                             .Count(t => t.Price.FlightId == f.Id)
                    })
                    .Select(fd => fd with
                    {
                        Occupancy = fd.TotalSeats == 0
                                   ? 0
                                   : (double)fd.SoldSeats / fd.TotalSeats
                    })
                    .ToList();
            }
            else
            {
                Paychecks = _context.Paycheck
                    .Include(p => p.Employee)
                        .ThenInclude(e => e.Position)
                    .Include(p => p.Employee)
                        .ThenInclude(e => e.Account)
                    .Select(p => new PaycheckDto
                    {
                        EmployeeName = $"{p.Employee.Account.Name} {p.Employee.Account.Surname}",
                        Position = p.Employee.Position!.Name,
                        Date = p.Date ?? DateTime.MinValue,
                        Amount = Convert.ToDecimal(p.Amount, CultureInfo.InvariantCulture)
                    })
                    .ToList();
            }

            return Page();
        }

        public record TicketDto
        {
            public string FlightCode { get; init; } = "";
            public DateTime FlightDate { get; init; }
            public string ClassName { get; init; } = "";
            public decimal Price { get; init; }
            public string Status { get; init; } = "";
        }

        public record FlightDto
        {
            public string FlightCode { get; init; } = "";
            public DateTime Date { get; init; }
            public int TotalSeats { get; init; }
            public int SoldSeats { get; init; }
            public double Occupancy { get; init; }
        }

        public record PaycheckDto
        {
            public string EmployeeName { get; init; } = "";
            public string Position { get; init; } = "";
            public DateTime Date { get; init; }
            public decimal Amount { get; init; }
        }
    }
}
