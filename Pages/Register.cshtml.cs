using FlightAlright.Data;
using FlightAlright.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.RegularExpressions;

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
            if (!ModelState.IsValid)
            {
                ErrorMessage = "Wprowadzone dane s� nieprawid�owe.";
                return Page();
            }

            if (string.IsNullOrWhiteSpace(Account.Login))
            {
                ModelState.AddModelError("Account.Login", "Login jest wymagany.");
                return Page();
            }

            if (string.IsNullOrWhiteSpace(Account.Password))
            {
                ModelState.AddModelError("Account.Password", "Has�o jest wymagane.");
                return Page();
            }

            if (!IsPasswordValid(Account.Password))
            {
                ModelState.AddModelError("Account.Password", "Has�o musi mie� min. 8 znak�w, ma�� i wielk� liter�, cyfr� oraz znak specjalny.");
                return Page();
            }

            // Sprawdzenie, czy login ju� istnieje w tabeli Account
            if (_context.Account.Any(a => a.Login == Account.Login))
            {
                ModelState.AddModelError("Account.Login", "Ten login jest ju� zaj�ty.");
                return Page();
            }

            // Haszowanie has�a
            var hasher = new PasswordHasher<string>();
            Account.Password = hasher.HashPassword(null, Account.Password);

            // Domy�lnie nowy u�ytkownik ma rol� klienta (np. ID = 1)
            Account.RoleId = 1;
            Account.Status = true; // aktywne konto

            try
            {
                _context.Account.Add(Account);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = "Wyst�pi� b��d podczas zapisu do bazy danych: " + ex.Message;
                return Page();
            }

            TempData["SuccessMessage"] = "Rejestracja zako�czona sukcesem! Mo�esz si� zalogowa�.";
            return RedirectToPage("/Login", new { login = Account.Login });
        }

        private bool IsPasswordValid(string password)
        {
            var regex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).{8,}$");
            return regex.IsMatch(password);
        }

    }
}

