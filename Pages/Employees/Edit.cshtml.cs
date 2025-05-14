using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using FlightAlright.Data;
using FlightAlright.Models;

namespace FlightAlright.Pages.Employees
{
    public class EditModel : PageModel
    {
        private readonly FlightAlrightContext _context;

        public EditModel(FlightAlrightContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Employee Employee { get; set; } = default!;

        [BindProperty]
        public string Login { get; set; } = string.Empty;

        [BindProperty]
        public string? Password { get; set; }  // nullable = poprawne

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
                return NotFound();

            Employee = await _context.Employee
                .Include(e => e.Account)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (Employee == null)
                return NotFound();

            Login = Employee.Account?.Login ?? "";

            ViewData["PositionId"] = new SelectList(_context.Position, "Id", "Name", Employee.PositionId);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var employee = await _context.Employee
                .Include(e => e.Account)
                .FirstOrDefaultAsync(e => e.Id == Employee.Id);

            if (employee == null)
                return NotFound();

            employee.PositionId = Employee.PositionId;

            if (employee.Account != null)
            {
                employee.Account.Login = Login;

                if (!string.IsNullOrWhiteSpace(Password))
                {
                    if (!IsValidPassword(Password))
                    {
                        ModelState.AddModelError("Password", "Hasło musi mieć min. 8 znaków, zawierać małą i wielką literę oraz znak specjalny.");
                        ViewData["PositionId"] = new SelectList(_context.Position, "Id", "Name", Employee.PositionId);
                        return Page();
                    }

                    var hasher = new PasswordHasher<string>();
                    employee.Account.Password = hasher.HashPassword(null, Password);
                }

                _context.Account.Update(employee.Account);
            }

            await _context.SaveChangesAsync();
            return RedirectToPage("./Index");
        }

        private bool IsValidPassword(string password)
        {
            return Regex.IsMatch(password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*[\W_]).{8,}$");
        }
    }
}
