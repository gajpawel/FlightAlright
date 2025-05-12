using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FlightAlright.Data;
using FlightAlright.Models;
using System.Collections.Generic;
using System.Linq;

namespace FlightAlright.Pages.Admin
{
    public class PositionManagementModel : PageModel
    {
        private readonly FlightAlrightContext _context;

        public PositionManagementModel(FlightAlrightContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Position Position { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int? EditId { get; set; }

        public List<Position> Positions { get; set; } = new();

        public string? ErrorMessage { get; set; }

        public void OnGet()
        {
            Positions = _context.Position.OrderBy(p => p.Name).ToList();

            if (EditId != null)
            {
                var position = _context.Position.Find(EditId.Value);
                if (position != null)
                    Position = position;
            }
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                Positions = _context.Position.OrderBy(p => p.Name).ToList();
                return Page();
            }

            if (Position.Id != 0)
            {
                var existing = _context.Position.Find(Position.Id);
                if (existing != null)
                {
                    existing.Name = Position.Name;
                    _context.Position.Update(existing);
                    _context.SaveChanges();
                }
            }
            else
            {
                bool exists = _context.Position.Any(p => p.Name == Position.Name);
                if (!exists)
                {
                    _context.Position.Add(Position);
                    _context.SaveChanges();
                }
            }

            return RedirectToPage();
        }

        public IActionResult OnPostDelete(int id)
        {
            var position = _context.Position.Find(id);

            if (position == null)
                return RedirectToPage();

            bool used = _context.Employee.Any(e => e.PositionId == id);
            if (used)
            {
                TempData["ErrorMessage"] = $"Nie mo¿na usun¹æ stanowiska „{position.Name}”, poniewa¿ jest przypisane do pracownika.";
                return RedirectToPage();
            }

            _context.Position.Remove(position);
            _context.SaveChanges();

            return RedirectToPage();
        }
    }
}
