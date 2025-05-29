using FlightAlright.Data;
using FlightAlright.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FlightAlright.Pages.Admin
{
    public class AddAdminModel : PageModel
    {
        private readonly FlightAlrightContext _context;

        public AddAdminModel(FlightAlrightContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Account Account { get; set; } = new();

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        [BindProperty]
        public int? SelectedAccountId { get; set; }

        [BindProperty]
        public string Mode { get; set; } = "new";

        public SelectList AccountList { get; set; } = default!;
        public List<Account> AdminList { get; set; } = new();
        public string SuccessMessage { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;

        public void OnGet()
        {
            if (HttpContext.Session.GetString("UserType") != "Administrator")
            {
                Response.Redirect("/AccessDenied");
                return;
            }

            LoadSelectList();
            LoadAdminList();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (HttpContext.Session.GetString("UserType") != "Administrator")
                return RedirectToPage("/AccessDenied");

            LoadSelectList();
            LoadAdminList();

            if (Mode == "new")
            {
                if (string.IsNullOrWhiteSpace(Account.Name) ||
                    string.IsNullOrWhiteSpace(Account.Surname) ||
                    string.IsNullOrWhiteSpace(Account.Login) ||
                    string.IsNullOrWhiteSpace(Password))
                {
                    ErrorMessage = "Wszystkie pola s¹ wymagane.";
                    return Page();
                }

                if (_context.Account.Any(a => a.Login == Account.Login))
                {
                    ErrorMessage = "Podany login jest ju¿ zajêty.";
                    return Page();
                }

                if (!IsValidPassword(Password))
                {
                    ErrorMessage = "Has³o musi mieæ min. 8 znaków, zawieraæ ma³¹ i wielk¹ literê oraz znak specjalny.";
                    return Page();
                }

                var hasher = new PasswordHasher<string>();
                var hashedPassword = hasher.HashPassword(Account.Login, Password);

                Account.Password = hashedPassword;
                Account.RoleId = 3;
                Account.Status = true;

                _context.Account.Add(Account);
                await _context.SaveChangesAsync();

                SuccessMessage = $"Nowy administrator '{Account.Login}' zosta³ dodany.";
                Account = new Account(); // wyczyœæ formularz
                Password = string.Empty;
            }
            else if (Mode == "existing")
            {
                if (SelectedAccountId == null)
                {
                    ErrorMessage = "Nie wybrano konta.";
                    return Page();
                }

                var existing = await _context.Account.FindAsync(SelectedAccountId.Value);

                if (existing == null)
                {
                    ErrorMessage = "Nie znaleziono konta.";
                    return Page();
                }

                if (existing.RoleId == 3)
                {
                    ErrorMessage = "Konto ju¿ jest administratorem.";
                    return Page();
                }

                existing.RoleId = 3;
                existing.Status = true;
                _context.Update(existing);
                await _context.SaveChangesAsync();

                SuccessMessage = $"Konto '{existing.Login}' zosta³o podniesione do roli administratora.";
                SelectedAccountId = null;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostRemoveAdminAsync(int adminId, string passwordToConfirm)
        {
            if (HttpContext.Session.GetString("UserType") != "Administrator")
                return RedirectToPage("/AccessDenied");

            var account = await _context.Account.FindAsync(adminId);
            if (account == null || account.RoleId != 3)
            {
                ErrorMessage = "Nieprawid³owe konto.";
                LoadSelectList(); LoadAdminList(); return Page();
            }

            var hasher = new PasswordHasher<string>();
            var result = hasher.VerifyHashedPassword(account.Login, account.Password, passwordToConfirm);

            if (result != PasswordVerificationResult.Success)
            {
                ErrorMessage = "Niepoprawne has³o administratora.";
                LoadSelectList(); LoadAdminList(); return Page();
            }

            account.RoleId = 1;
            _context.Update(account);
            await _context.SaveChangesAsync();

            SuccessMessage = $"Administrator '{account.Login}' to teraz konto klienta";
            LoadSelectList(); LoadAdminList(); return Page();
        }

        private void LoadSelectList()
        {
            var available = _context.Account
                .Where(a => a.RoleId == 1 || a.RoleId == 2)
                .Select(a => new
                {
                    a.Id,
                    Display = $"{a.Name} {a.Surname} ({a.Login})"
                })
                .ToList();

            AccountList = new SelectList(available, "Id", "Display");
        }

        private void LoadAdminList()
        {
            AdminList = _context.Account
                .Where(a => a.RoleId == 3)
                .OrderBy(a => a.Surname)
                .ThenBy(a => a.Name)
                .ToList();
        }

        private bool IsValidPassword(string password)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*[\W_]).{8,}$");
        }
    }
}
