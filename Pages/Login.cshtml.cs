using FlightAlright.Data;
using FlightAlright.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FlightAlright.Pages
{
    public class LoginModel : PageModel
    {
        private readonly FlightAlrightContext _context;

        [BindProperty]
        public string Login { get; set; } = string.Empty;

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        public string ErrorMessage { get; set; } = string.Empty;

        public LoginModel(FlightAlrightContext context)
        {
            _context = context;
        }

        public void OnGet()
        {
            if (TempData["SuccessMessage"] != null)
                ViewData["SuccessMessage"] = TempData["SuccessMessage"].ToString();

            Login = Request.Query["login"];
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = "Wprowadzone dane s¹ nieprawid³owe.";
                return Page();
            }

            var user = _context.Account
                .Include(a => a.Role)
                .FirstOrDefault(a => a.Login == Login);

            if (user == null)
            {
                ErrorMessage = "Nie znaleziono u¿ytkownika z takim loginem.";
                return Page();
            }

            var hasher = new PasswordHasher<Account>();
            var result = hasher.VerifyHashedPassword(user, user.Password, Password);

            if (result != PasswordVerificationResult.Success)
            {
                ErrorMessage = "Nieprawid³owy login lub has³o.";
                return Page();
            }

            // Zapisanie danych sesji
            HttpContext.Session.SetInt32("AccountId", user.Id);
            HttpContext.Session.SetString("UserType", user.Role?.Name ?? "Guest");

            // Przekierowanie na odpowiedni¹ stronê na podstawie roli
            return user.RoleId switch
            {
                1 => RedirectToPage("/Clients/ClientProfile"),      // Klient
                2 => RedirectToPage("/Employees/FlightSchedule"),    // Pracownik
                3 => RedirectToPage("/Admin/AdminProfile"),// Admin
                _ => RedirectToPage("/Index")
            };
        }
    }
}
