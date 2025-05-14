using FlightAlright.Data;
using FlightAlright.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FlightAlright.Pages.Admin
{
    public class AdminProfileModel : PageModel
    {
        private readonly FlightAlrightContext _context;

        public string AdminName { get; set; } = string.Empty;


        public AdminProfileModel(FlightAlrightContext context)
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

            if (account == null || account.Role?.Name != "Administrator")
                return RedirectToPage("/AccessDenied");

            AdminName = account.Name ?? "Administrator";

            return Page();
        }
    }
}
