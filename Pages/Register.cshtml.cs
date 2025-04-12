using FlightAlright.Data;
using FlightAlright.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;

namespace FlightAlright.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly FlightAlrightContext _context;

        public string ErrorMessage { get; set; } = string.Empty;

        [BindProperty]
        public Account Account { get; set; } = new Account();

        public RegisterModel(FlightAlrightContext context)
        {
            _context = context;
        }
        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Walidacja modelu
            if (!ModelState.IsValid)
            {
                ErrorMessage = "Wprowadzone dane s¹ nieprawid³owe.";
                return Page();
            }

            // Walidacja loginu i has³a
            if (string.IsNullOrWhiteSpace(Account.Login))
            {
                ModelState.AddModelError("Client.Login", "Login jest wymagany podczas rejestracji.");
                return Page();
            }

            if (string.IsNullOrWhiteSpace(Account.Password))
            {
                ModelState.AddModelError("Account.Password", "Has³o jest wymagane podczas rejestracji.");
                return Page();
            }

            // SprawdŸ, czy login ju¿ istnieje
            if (_context.Account.Any(c => c.Login == Account.Login))
            {
                ModelState.AddModelError("Account.Login", "Ten login jest ju¿ zajêty.");
                return Page();
            }
            if (_context.Employee.Any(c => c.Login == Account.Login))
            {
                ModelState.AddModelError("Account.Login", "Ten login jest ju¿ zajêty.");
                return Page();
            }
            if (_context.Account.Any(c => c.Login == Account.Login))
            {
                ModelState.AddModelError("Account.Login", "Ten login jest ju¿ zajêty.");
                return Page();
            }

            // Haszowanie has³a
            var hasher = new PasswordHasher<string>();
            Account.Password = hasher.HashPassword(null, Account.Password);

            try
            {
                // Dodanie klienta do bazy danych
                _context.Account.Add(Account);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = "Wyst¹pi³ b³¹d podczas zapisywania danych: " + ex.Message;
                return Page();
            }

            // Ustaw komunikat sukcesu i przekierowanie na stronê logowania
            TempData["SuccessMessage"] = "Rejestracja zakoñczona sukcesem! Mo¿esz siê zalogowaæ.";
            return RedirectToPage("/Login", new { login = Account.Login });
        }
    }
}
