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
            UpdateFlightStatus();
            Flights = _context.Flight
                .Include(f => f.DepartureAirport)
                .Include(f => f.ArrivalAirport)
                .Where(f => f.Status == true)
                .ToList();
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
                }
            }

            _context.SaveChanges();

        }
    }
}