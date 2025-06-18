using FlightAlright.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;

namespace FlightAlright.Pages.Admin.Reports
{
    public class ReportsModel : PageModel
    {
        private readonly FlightAlrightContext _context;
        public ReportsModel(FlightAlrightContext context) => _context = context;

        /* ---------- Filters (query-string) ---------- */
        [BindProperty(SupportsGet = true)] public string? Tab { get; set; } = "Tickets";

        // Tickets
        [BindProperty(SupportsGet = true)] public string? FlightCodeFilter { get; set; }
        [BindProperty(SupportsGet = true)] public string? ClassFilter { get; set; }
        [BindProperty(SupportsGet = true)] public char? StatusFilter { get; set; }
        [BindProperty(SupportsGet = true)] public string? DateFilter { get; set; }   

        // Flights
        [BindProperty(SupportsGet = true)] public bool OnlyWithSales { get; set; }

        /* ---------- dropdown lists ---------- */
        public IList<string> FlightCodes { get; private set; } = new List<string>();
        public IList<string> ClassNames { get; private set; } = new List<string>();
        public IList<DateTime> FlightDates { get; private set; } = new List<DateTime>();

        /* ---------- view-model ---------- */
        public IList<TicketDto> Tickets { get; private set; } = new List<TicketDto>();
        public IList<FlightDto> Flights { get; private set; } = new List<FlightDto>();

        public int TicketCount { get; private set; }
        public decimal TicketRevenue { get; private set; }
        public int FlightCount { get; private set; }
        public double AvgOccupancy { get; private set; }

        /* ---------- page handlers ---------- */
        public IActionResult OnGet()
        {
            if (!IsAdmin()) return RedirectToPage("/AccessDenied");
            LoadData();
            return Page();
        }

        public IActionResult OnGetExport(string tab)
        {
            if (!IsAdmin()) return RedirectToPage("/AccessDenied");
            Tab = tab == "Flights" ? "Flights" : "Tickets";
            LoadData();

            var csv = new StringBuilder();

            if (Tab == "Tickets")
            {
                csv.AppendLine("FlightCode;FlightDate;Class;Price;Status");
                foreach (var t in Tickets)
                    csv.AppendLine($"{t.FlightCode};{t.FlightDate:yyyy-MM-dd HH:mm};{t.ClassName};" +
                                   $"{t.Price.ToString(CultureInfo.InvariantCulture)};{t.Status}");
                return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "tickets_report.csv");
            }
            else /* Flights */
            {
                csv.AppendLine("FlightCode;Date;TotalSeats;SoldSeats;Occupancy");
                foreach (var f in Flights)
                    csv.AppendLine($"{f.FlightCode};{f.Date:yyyy-MM-dd};{f.TotalSeats};{f.SoldSeats};" +
                                   f.Occupancy.ToString("P1", CultureInfo.InvariantCulture));
                return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "flights_report.csv");
            }
        }

        /* ---------- helpers ---------- */
        private bool IsAdmin() =>
            HttpContext.Session.GetString("UserType") == "Administrator";

        private void LoadData()
        {
            if (Tab == "Flights") LoadFlights();
            else LoadTickets();
        }

        /* ===== FLIGHTS ===== */
        private void LoadFlights()
        {
            var allFlights = _context.Flight
                .Include(f => f.DepartureAirport)
                .Select(f => new
                {
                    f.Id,
                    Code = f.DepartureAirport.Code,
                    f.DepartureDate
                })
                .AsEnumerable()
                .Select(f =>
                {
                    var total = _context.Price
                                        .Include(p => p.Class)
                                        .Where(p => p.FlightId == f.Id)
                                        .Sum(p => p.Class.SeatsNumber ?? 0);

                    var sold = _context.Ticket.Count(t =>
                                 t.Price.FlightId == f.Id &&
                                 (t.Status == 'K' || t.Status == 'N'));

                    return new FlightDto
                    {
                        FlightCode = $"{f.Code}-{f.Id}",
                        Date = f.DepartureDate ?? DateTime.MinValue,
                        TotalSeats = total,
                        SoldSeats = sold,
                        Occupancy = total == 0 ? 0 : (double)sold / total
                    };
                })
                .ToList();

            IEnumerable<FlightDto> flights = allFlights;
            if (OnlyWithSales)
                flights = flights.Where(f => f.SoldSeats > 0);

            Flights = flights.ToList();

            FlightCount = allFlights.Count;
            AvgOccupancy = FlightCount == 0 ? 0
                                            : allFlights.Average(f => f.Occupancy);
        }

        /* ===== TICKETS ===== */
        private void LoadTickets()
        {
            var all = _context.Ticket
                      .Include(t => t.Price)
                          .ThenInclude(p => p.Flight)
                              .ThenInclude(f => f.DepartureAirport)
                      .Include(t => t.Price).ThenInclude(p => p.Class)
                      .ToList();

            FlightCodes = all.Select(t => $"{t.Price!.Flight!.DepartureAirport!.Code}-{t.Price.Flight.Id}")
                             .Distinct().OrderBy(c => c).ToList();

            ClassNames = all.Select(t => t.Price!.Class!.Name)
                             .Distinct().OrderBy(c => c).ToList();

            FlightDates = all.Where(t => t.Price!.Flight!.DepartureDate.HasValue)
                             .Select(t => t.Price.Flight.DepartureDate!.Value.Date)
                             .Distinct().OrderBy(d => d).ToList();

            IEnumerable<Models.Ticket> filtered = all;

            if (!string.IsNullOrWhiteSpace(FlightCodeFilter))
                filtered = filtered.Where(t =>
                    $"{t.Price!.Flight!.DepartureAirport!.Code}-{t.Price.Flight.Id}" == FlightCodeFilter);

            if (!string.IsNullOrWhiteSpace(ClassFilter))
                filtered = filtered.Where(t => t.Price!.Class!.Name == ClassFilter);

            if (StatusFilter.HasValue)
                filtered = filtered.Where(t => t.Status == StatusFilter.Value);

            if (DateTime.TryParseExact(DateFilter, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                                       DateTimeStyles.None, out var df))
                filtered = filtered.Where(t => t.Price!.Flight!.DepartureDate?.Date == df.Date);

            Tickets = filtered
                .Select(t => new TicketDto
                {
                    FlightCode = $"{t.Price!.Flight!.DepartureAirport!.Code}-{t.Price.Flight.Id}",
                    FlightDate = t.Price.Flight.DepartureDate ?? DateTime.MinValue,
                    ClassName = t.Price.Class!.Name,
                    Price = Convert.ToDecimal(
                                    t.TicketPrice ?? t.Price.CurrentPrice ?? 0f,
                                    CultureInfo.InvariantCulture),
                    Status = t.Status switch
                    {
                        'D' => "Dostêpny",
                        'K' => "Kupiony",
                        'A' => "Anulowany",
                        'N' => "Nieaktywny",
                        'R' => "Rezerwacja",
                        _ => "—"
                    },
                    StatusCode = t.Status ?? ' '
                })
                .ToList();

            var soldAll = all.Where(t => t.Status is 'K' or 'N');

            TicketCount = soldAll.Count();

            TicketRevenue = soldAll.Sum(t =>
                Convert.ToDecimal(
                    t.TicketPrice ?? t.Price.CurrentPrice ?? 0f,
                    CultureInfo.InvariantCulture));
        }

        /* ===== DTOs ===== */
        public record TicketDto
        {
            public string FlightCode { get; init; } = "";
            public DateTime FlightDate { get; init; }
            public string ClassName { get; init; } = "";
            public decimal Price { get; init; }
            public string Status { get; init; } = "";
            public char StatusCode { get; init; }
        }

        public record FlightDto
        {
            public string FlightCode { get; init; } = "";
            public DateTime Date { get; init; }
            public int TotalSeats { get; init; }
            public int SoldSeats { get; init; }
            public double Occupancy { get; init; }
        }
    }
}
