using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using FlightAlright.Data;
using FlightAlright.Models;
using System.Linq;
using System.Threading.Tasks;

namespace FlightAlright.Pages.Employees
{
    public class CreateModel : PageModel
    {
        private readonly FlightAlrightContext _context;

        public CreateModel(FlightAlrightContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Employee Employee { get; set; } = default!;

        public void OnGet()
        {
            var usedAccountIds = _context.Employee.Select(e => e.AccountId).ToList();
            var availableAccounts = _context.Account
                .Where(a => !usedAccountIds.Contains(a.Id))
                .ToList();

            ViewData["Accounts"] = new SelectList(availableAccounts, "Id", "Login");
            ViewData["Positions"] = new SelectList(_context.Position, "Id", "Name");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            _context.Employee.Add(Employee);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Admin/PersonelManagement");
        }
    }
}
