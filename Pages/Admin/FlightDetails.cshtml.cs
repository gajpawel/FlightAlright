using FlightAlright.Data;
using FlightAlright.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FlightAlright.Pages.Admin
{
    public class FlightDetailsModel : PageModel
    {
        private readonly FlightAlrightContext _context;

        public FlightDetailsModel(FlightAlrightContext context)
        {
            _context = context;
        }

        public Flight Flight { get; set; }

        public List<CrewMemberViewModel> CrewMembers { get; set; } = new();
        public List<PriceViewModel> Prices { get; set; } = new();

        public IActionResult OnGet(int id)
        {
            Flight = _context.Flight
                .Include(f => f.DepartureAirport)
                .Include(f => f.ArrivalAirport)
                .Include(f => f.Plane)
                    .ThenInclude(p => p.Brand)
                .FirstOrDefault(f => f.Id == id);

            if (Flight == null)
                return NotFound();

            // Za³oga
            CrewMembers = _context.Crew
                .Where(c => c.FlightId == id)
                .Include(c => c.Employee)
                    .ThenInclude(e => e.Account)
                .Include(c => c.Employee.Position)
                .Select(c => new CrewMemberViewModel
                {
                    Name = c.Employee.Account.Name,
                    Surname = c.Employee.Account.Surname,
                    PositionName = c.Employee.Position.Name
                })
                .ToList();

            // Ceny biletów
            Prices = _context.Price
                .Where(p => p.FlightId == id)
                .Include(p => p.Class)
                .Select(p => new PriceViewModel
                {
                    ClassName = p.Class.Name,
                    CurrentPrice = ((decimal?)p.CurrentPrice) ?? 0,
                    MaxSeatsNumber = p.Class.SeatsNumber,
                    SeatsNumber = p.Class.SeatsNumber - (_context.Ticket.Where(t => t.PriceId == id).ToList()).Count
                })
                .ToList();

            return Page();
        }

        public class CrewMemberViewModel
        {
            public string Name { get; set; }
            public string Surname { get; set; }
            public string PositionName { get; set; }
        }

        public class PriceViewModel
        {
            public string ClassName { get; set; }
            public decimal CurrentPrice { get; set; }
            public int? MaxSeatsNumber { get; set; }
            public int? SeatsNumber { get; set; }
        }
    }
}
