using FlightAlright.Data;
using FlightAlright.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;

namespace FlightAlright.Pages.Admin
{
    public class FlightManagementModel : PageModel
    {
        private readonly FlightAlrightContext _context;

        public FlightManagementModel(FlightAlrightContext context)
        {
            _context = context;
        }

        public List<Flight> Flights { get; set; }

        public void OnGet()
        {
            Flights = _context.Flight
                .Include(f => f.DepartureAirport)
                .Include(f => f.ArrivalAirport)
                .ToList();
        }
    }
}