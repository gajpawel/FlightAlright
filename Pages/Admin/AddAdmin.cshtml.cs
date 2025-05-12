using FlightAlright.Data;
using FlightAlright.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

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
        public Account Account { get; set; } = new Account();

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        public string SuccessMessage { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;

        public void OnGet() 
        {
            if (HttpContext.Session.GetString("UserType") != "Administrator")
            {
                Response.Redirect("Pages/Shared/AccessDenied");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (HttpContext.Session.GetString("UserType") != "Administrator")
            {
                return RedirectToPage("Pages/Shared/AccessDenied");
            }

            if (string.IsNullOrWhiteSpace(Account.Login) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Login i has³o s¹ wymagane.";
                return Page();
            }

            if (_context.Account.Any(a => a.Login == Account.Login))
            {
                ErrorMessage = "Ten login jest ju¿ zajêty.";
                return Page();
            }

            if (!IsValidPassword(Password))
            {
                ErrorMessage = "Has³o musi mieæ min. 8 znaków, zawieraæ ma³¹ i wielk¹ literê oraz znak specjalny.";
                return Page();
            }

            var hasher = new PasswordHasher<string>();
            Account.Password = hasher.HashPassword(null, Password);
            Account.RoleId = 3; // Admin
            Account.Status = true;

            _context.Account.Add(Account);
            await _context.SaveChangesAsync();

            SuccessMessage = "Administrator zosta³ dodany.";
            Account = new Account();
            Password = string.Empty;

            return Page();
        }

        private bool IsValidPassword(string password)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*[\W_]).{8,}$");
        }
    }
}
