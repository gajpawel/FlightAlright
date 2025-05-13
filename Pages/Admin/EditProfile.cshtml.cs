using FlightAlright.Data;
using FlightAlright.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.RegularExpressions;

namespace FlightAlright.Pages.Admin
{
    public class EditProfileModel : PageModel
    {
        private readonly FlightAlrightContext _context;

        [BindProperty]
        public Account Account { get; set; } = new();

        [BindProperty]
        public string? NewPassword { get; set; }

        public string ErrorMessage { get; set; } = string.Empty;
        public string SuccessMessage { get; set; } = string.Empty;

        public EditProfileModel(FlightAlrightContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            var accountId = HttpContext.Session.GetInt32("AccountId");
            if (accountId == null)
                return RedirectToPage("/Login");

            var account = _context.Account.FirstOrDefault(a => a.Id == accountId);
            if (account == null || account.RoleId != 3) 
                return RedirectToPage("/AccessDenied");

            Account = account;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var accountId = HttpContext.Session.GetInt32("AccountId");
            if (accountId == null)
                return RedirectToPage("/Login");

            var existing = _context.Account.FirstOrDefault(a => a.Id == accountId);
            if (existing == null || existing.RoleId != 3)
                return RedirectToPage("/AccessDenied");

            // Walidacja has³a (jeœli zmieniane)
            if (!string.IsNullOrEmpty(NewPassword))
            {
                if (!Regex.IsMatch(NewPassword, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$"))
                {
                    ModelState.AddModelError("NewPassword", "Has³o musi mieæ min. 8 znaków, ma³¹ i wielk¹ literê, cyfrê oraz znak specjalny.");
                    return Page();
                }

                var hasher = new PasswordHasher<string>();
                existing.Password = hasher.HashPassword(null, NewPassword);
            }

            // Aktualizacja danych
            existing.Name = Account.Name;
            existing.Surname = Account.Surname;
            existing.Login = Account.Login;

            try
            {
                await _context.SaveChangesAsync();
                SuccessMessage = "Dane zosta³y zaktualizowane.";
            }
            catch (Exception ex)
            {
                ErrorMessage = "Wyst¹pi³ b³¹d: " + ex.Message;
            }

            return Page();
        }
    }
}
