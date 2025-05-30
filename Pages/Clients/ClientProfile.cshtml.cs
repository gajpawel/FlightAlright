using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FlightAlright.Data;
using FlightAlright.Models;
using Microsoft.EntityFrameworkCore;

namespace FlightAlright.Pages.Clients
{
    public class ClientProfileModel : PageModel
    {
        private readonly FlightAlrightContext _context;

        public string ClientName { get; set; } = string.Empty;
        public Account account {  get; set; }

        public ClientProfileModel(FlightAlrightContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            var accountId = HttpContext.Session.GetInt32("AccountId");

            if (accountId == null)
                return RedirectToPage("/Login");

            account = _context.Account
                .Include(a => a.Role)
                .FirstOrDefault(a => a.Id == accountId);

            if (account == null)
                return RedirectToPage("/AccessDenied"); // lub do innej strony

            ClientName = $"{account.Name}";

            return Page();
        }
    }
}

