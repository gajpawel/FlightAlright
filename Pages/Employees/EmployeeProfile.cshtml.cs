using FlightAlright.Data;
using FlightAlright.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FlightAlright.Pages.Employees
{
    public class EmployeeProfileModel : PageModel
    {
        private readonly FlightAlrightContext _context;

        public string EmployeeName { get; set; } = string.Empty;

        public EmployeeProfileModel(FlightAlrightContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            var accountId = HttpContext.Session.GetInt32("AccountId");

            if (accountId == null)
                return RedirectToPage("/Login");

            var account = _context.Account
                .Include(a => a.Role)
                .FirstOrDefault(a => a.Id == accountId);

            if (account == null || account.Role?.Name != "Employee")
                return RedirectToPage("/AccessDenied");

            EmployeeName = $"{account.Name}";

            return Page();
        }
    }
}
