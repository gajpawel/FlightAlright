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
    public string? DepartureAirportName { get; set; }

    [BindProperty]
    public string? ArrivalAirportName { get; set; }

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
        airports = _context.Airport.Distinct().ToList();
    }

    public void LoadAllFlights()
    {
        allFlights = _context.Flight
            .Include(f => f.DepartureAirport)
            .Include(f => f.ArrivalAirport)
            .Where(f => f.Status == true)
            .ToList();
    }

    public void SearchFlights()
    {
        var query = _context.Flight
            .Include(f => f.DepartureAirport)
            .Include(f => f.ArrivalAirport)
            .Where(f => f.Status == true)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(DepartureAirportName))
        {
            query = query.Where(f =>
                f.DepartureAirport.Name.Contains(DepartureAirportName) ||
                f.DepartureAirport.City.Contains(DepartureAirportName) ||
                f.DepartureAirport.Code.Contains(DepartureAirportName));
        }

        if (!string.IsNullOrWhiteSpace(ArrivalAirportName))
        {
            query = query.Where(f =>
                f.ArrivalAirport.Name.Contains(ArrivalAirportName) ||
                f.ArrivalAirport.City.Contains(ArrivalAirportName) ||
                f.ArrivalAirport.Code.Contains(ArrivalAirportName));
        }

        if (DepartureDate.HasValue)
        {
            var selectedDate = DepartureDate.Value.Date;
            query = query.Where(f => f.DepartureDate.HasValue && f.DepartureDate.Value.Date == selectedDate);
        }

        searchResults = query.ToList();

        int tempCount = searchResults.Count;
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
                var oldPrices = _context.Price.Where(p => p.FlightId == flight.Id).ToList();
                foreach (var price in oldPrices)
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

