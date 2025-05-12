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

        [BindProperty(SupportsGet = true)]
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
            {
                return RedirectToPage("/AccessDenied");
            }

            if (Request.Form["Mode"] == "new")
            {
                if (string.IsNullOrWhiteSpace(Account.Login) || string.IsNullOrWhiteSpace(Password))
                {
                    ErrorMessage = "Login i has³o s¹ wymagane.";
                    LoadSelectList(); LoadAdminList(); return Page();
                }

                if (_context.Account.Any(a => a.Login == Account.Login))
                {
                    ErrorMessage = "Ten login jest ju¿ zajêty.";
                    LoadSelectList(); LoadAdminList(); return Page();
                }

                if (!IsValidPassword(Password))
                {
                    ErrorMessage = "Has³o musi mieæ min. 8 znaków, zawieraæ ma³¹ i wielk¹ literê oraz znak specjalny.";
                    LoadSelectList(); LoadAdminList(); return Page();
                }

                var hasher = new PasswordHasher<string>();
                Account.Password = hasher.HashPassword(null, Password);
                Account.RoleId = 3; // Administrator
                Account.Status = true;

                _context.Account.Add(Account);
                await _context.SaveChangesAsync();

                SuccessMessage = "Nowy administrator zosta³ dodany.";
                Account = new Account();
                Password = string.Empty;
            }
            else if (Request.Form["Mode"] == "existing")
            {
                if (SelectedAccountId == null)
                {
                    ErrorMessage = "Nie wybrano konta.";
                    LoadSelectList(); LoadAdminList(); return Page();
                }

                var account = await _context.Account.FindAsync(SelectedAccountId.Value);
                if (account == null || account.RoleId == 3)
                {
                    ErrorMessage = "Wybrane konto jest nieprawid³owe lub ju¿ jest administratorem.";
                    LoadSelectList(); LoadAdminList(); return Page();
                }

                account.RoleId = 3; 
                _context.Account.Update(account);
                await _context.SaveChangesAsync();

                SuccessMessage = $"Konto '{account.Login}' zosta³o podniesione do roli administratora.";
            }

            LoadSelectList();
            LoadAdminList();
            return Page();
        }

        private void LoadSelectList()
        {
            var availableAccounts = _context.Account
                .Where(a => a.RoleId == 1 || a.RoleId == 2)
                .Select(a => new { a.Id, a.Login })
                .ToList();

            AccountList = new SelectList(availableAccounts, "Id", "Login");
        }

        private void LoadAdminList()
        {
            AdminList = _context.Account
                .Where(a => a.RoleId == 3)
                .OrderBy(a => a.Login)
                .ToList();
        }

        private bool IsValidPassword(string password)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*[\W_]).{8,}$");
        }
    }
}
