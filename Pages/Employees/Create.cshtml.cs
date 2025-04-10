using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using FlightAlright.Data;
using FlightAlright.Models;

namespace FlightAlright.Pages.Employees
{
    public class CreateModel : PageModel
    {
        private readonly FlightAlright.Data.FlightAlrightContext _context;

        public CreateModel(FlightAlright.Data.FlightAlrightContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
        ViewData["AccountId"] = new SelectList(_context.Set<Account>(), "Id", "Id");
        ViewData["PositionId"] = new SelectList(_context.Set<Position>(), "Id", "Id");
            return Page();
        }

        [BindProperty]
        public Employee Employee { get; set; } = default!;

        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Employee.Add(Employee);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
