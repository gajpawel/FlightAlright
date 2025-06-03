using FlightAlright.Data;
using FlightAlright.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace FlightAlright.Pages.Admin
{
    public class AddAdminModel : PageModel
    {
        private readonly FlightAlrightContext _context;
        public AddAdminModel(FlightAlrightContext context) => _context = context;

        [BindProperty, Required, StringLength(100)]
        public string Login { get; set; } = string.Empty;


        [BindProperty, Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [BindProperty, Required, StringLength(100)]
        public string Surname { get; set; } = string.Empty;

        [BindProperty, Required, StringLength(100)]
        public string Password { get; set; } = string.Empty;

        [BindProperty]
        public int? SelectedAccountId { get; set; }

        [BindProperty]
        public string Mode { get; set; } = "new";

        public SelectList AccountList { get; private set; } = default!;
        public List<Account> AdminList { get; private set; } = new();
        public string SuccessMessage { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync()
        {
            if (!IsAdmin()) return RedirectToPage("/AccessDenied");
            await LoadListsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!IsAdmin()) return RedirectToPage("/AccessDenied");
            await LoadListsAsync();

            if (Mode == "new")
            {
                if (!ModelState.IsValid)
                {
                    ErrorMessage = "Popraw b³êdy formularza.";
                    return Page();
                }

                if (await _context.Account.AnyAsync(a => a.Login == Login))
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
                var hashedPass = hasher.HashPassword(Login, Password);

                var acc = new Account
                {
                    Login = Login,
                    Name = Name,
                    Surname = Surname,
                    Password = hashedPass,
                    RoleId = 3,
                    Status = true
                };

                _context.Account.Add(acc);
                await _context.SaveChangesAsync();

                SuccessMessage = $"Dodano nowego administratora: {Login}.";
                ClearForm();
            }
            else // existing
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
                    ErrorMessage = "Wybrane konto ju¿ ma uprawnienia administratora.";
                    return Page();
                }

                existing.RoleId = 3;
                existing.Status = true;
                _context.Update(existing);
                await _context.SaveChangesAsync();

                SuccessMessage = $"Konto {existing.Login} podniesiono do roli administratora.";
                SelectedAccountId = null;
            }

            await LoadListsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostRemoveAdminAsync(int adminId, string passwordToConfirm)
        {
            if (!IsAdmin()) return RedirectToPage("/AccessDenied");

            var admin = await _context.Account.FindAsync(adminId);
            if (admin == null || admin.RoleId != 3)
            {
                ErrorMessage = "Nieprawid³owe konto.";
                await LoadListsAsync();
                return Page();
            }

            var hasher = new PasswordHasher<string>();
            var result = hasher.VerifyHashedPassword(admin.Login, admin.Password, passwordToConfirm);

            if (result != PasswordVerificationResult.Success)
            {
                ErrorMessage = "Niepoprawne has³o administratora.";
                await LoadListsAsync();
                return Page();
            }

            admin.RoleId = 1;  // klient
            _context.Update(admin);
            await _context.SaveChangesAsync();

            SuccessMessage = $"Odebrano uprawnienia administratora kontu {admin.Login}.";
            await LoadListsAsync();
            return Page();
        }

        private bool IsAdmin() => HttpContext.Session.GetString("UserType") == "Administrator";

        private async Task LoadListsAsync()
        {
            AccountList = new SelectList(
                await _context.Account
                    .Where(a => a.RoleId == 1 || a.RoleId == 2)
                    .Select(a => new
                    {
                        a.Id,
                        Display = $"{a.Name} {a.Surname} ({a.Login})"
                    })
                    .ToListAsync(),
                "Id", "Display");

            AdminList = await _context.Account
                .Where(a => a.RoleId == 3)
                .OrderBy(a => a.Surname).ThenBy(a => a.Name)
                .ToListAsync();
        }

        private static bool IsValidPassword(string password) =>
            System.Text.RegularExpressions.Regex.IsMatch(
                password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*[\W_]).{8,}$");

        private void ClearForm()
        {
            Login = Name = Surname = Password = string.Empty;
            Mode = "new";
        }
    }
}
