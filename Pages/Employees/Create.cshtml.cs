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
            var usedAccountIds = _context.Employee
                .Where(e => e.Status == true)
                .Select(e => e.AccountId)
                .ToList();

            var availableAccounts = _context.Account
                .Where(a => !usedAccountIds.Contains(a.Id) && a.RoleId == 1)
                .ToList();

            ViewData["Accounts"] = new SelectList(availableAccounts, "Id", "Login");
            ViewData["Positions"] = new SelectList(_context.Position, "Id", "Name");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var existingEmployee = _context.Employee
                .FirstOrDefault(e => e.AccountId == Employee.AccountId);

            if (existingEmployee != null)
            {
                if (existingEmployee.Status == true)
                {
                    ModelState.AddModelError(string.Empty, "To konto jest już przypisane jako pracownik.");
                    OnGet(); return Page();
                }

                // PRZYWRACANIE ZWOLNIONEGO PRACOWNIKA
                existingEmployee.PositionId = Employee.PositionId;
                existingEmployee.Status = true;
                _context.Employee.Update(existingEmployee);
            }
            else
            {
                Employee.Status = true;
                _context.Employee.Add(Employee);
            }

            var account = await _context.Account.FindAsync(Employee.AccountId);
            if (account != null && account.RoleId == 1)
            {
                account.RoleId = 2;
                _context.Account.Update(account);
            }

            await _context.SaveChangesAsync();
            return RedirectToPage("/Admin/PersonelManagement");
        }

    }
}
