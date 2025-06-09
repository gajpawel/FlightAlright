using FlightAlright.Data;
using FlightAlright.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FlightAlright.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;

    private readonly FlightAlrightContext _context;

    public IndexModel(ILogger<IndexModel> logger, FlightAlrightContext context)
    {
        _logger = logger;
        _context = context;
    }

    public List<Airport> airports { get; set; } = new();
    public List<Flight> allFlights { get; set; } = new();
    public List<Flight> searchResults { get; set; } = new();

    [BindProperty]
    public string grammar { get; set; } = "";

    [BindProperty]
    public int? DepartureAirportId { get; set; }

    [BindProperty]
    public int? ArrivalAirportId { get; set; }

    [BindProperty]
    public DateTime? DepartureDate { get; set; }

    public bool ShowResults { get; set; } = false;

    public void OnGet()
    {
        UpdateFlightStatus();
        LoadAirports();
        LoadAllFlights();
    }

    public void OnPost()
    {
        LoadAirports();
        SearchFlights();
        ShowResults = true;
    }

    public void LoadAirports()
    {
        airports = _context.Airport
            .Distinct()
            .ToList();
    }

    public void LoadAllFlights()
    {
        allFlights = _context.Flight
            .Include(f => f.DepartureAirport)
            .Include(f => f.ArrivalAirport)
            .Where(f => f.Status == true) // Only active flights
            .Distinct()
            .ToList();
    }

    public void SearchFlights()
    {
        var query = _context.Flight
            .Include(f => f.DepartureAirport)
            .Include(f => f.ArrivalAirport)
            .Where(f => f.Status == true) // Only active flights
            .AsQueryable();

        // If both airports are selected, search for exact route
        if (DepartureAirportId.HasValue && ArrivalAirportId.HasValue)
        {
            query = query.Where(f => f.DepartureAirportId == DepartureAirportId &&
                                   f.ArrivalAirportId == ArrivalAirportId);
        }
        // If only departure airport is selected, show all flights from that airport
        else if (DepartureAirportId.HasValue)
        {
            query = query.Where(f => f.DepartureAirportId == DepartureAirportId);
        }
        // If only arrival airport is selected, show all flights to that airport
        else if (ArrivalAirportId.HasValue)
        {
            query = query.Where(f => f.ArrivalAirportId == ArrivalAirportId);
        }
        // If neither is selected, show all flights

        // Filter by departure date if provided
        if (DepartureDate.HasValue)
        {
            var selectedDate = DepartureDate.Value.Date;
            query = query.Where(f => f.DepartureDate.HasValue &&
                                   f.DepartureDate.Value.Date == selectedDate);
        }

        searchResults = query.ToList();

        int tempCount = searchResults.Count();
        int[] grammarhelp = { 2, 3, 4 };

        if (tempCount == 1)
        {
            grammar = "lot";
        }
        else if (grammarhelp.Contains(tempCount % 10))
        {
            grammar = "loty";
        }
        else
        {
            grammar = "lotów";
        }
    }

    public IActionResult OnPostLogout()
    {
        HttpContext.Session.Clear();
        return RedirectToPage("/Index"); 
    }

    //Zmieñ status na false dla lotów, które ju¿ siê odby³y
    public void UpdateFlightStatus()
    {
        var now = DateTime.UtcNow;

        var flights = _context.Flight
            .Include(f => f.ArrivalAirport)
            .Where(f => f.Status == true)
            .ToList();

        foreach (var flight in flights)
        {
            var adjustedArrivalTime = flight.ArrivalDate;

            if (adjustedArrivalTime < now)
            {
                flight.Status = false;
                var oldFightPrices = _context.Price.Where(p => p.FlightId == flight.Id).ToList();
                foreach (var price in oldFightPrices)
                {
                    var oldTickets = _context.Ticket.Where(t => t.PriceId == price.Id).ToList();
                    foreach (var ticket in oldTickets)
                    {
                        if (ticket.Status == 'D')
                            _context.Remove(ticket);
                        else if (ticket.Status == 'K')
                            ticket.Status = 'N';
                    }
                }
            }
        }

        _context.SaveChanges();

    }
}
