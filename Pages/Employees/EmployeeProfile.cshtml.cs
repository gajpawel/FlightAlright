using FlightAlright.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FlightAlright.Pages.Employees
{
    public class EmployeeProfileModel : PageModel
    {
        private readonly FlightAlrightContext _context;

        public string EmployeeName { get; set; } = string.Empty;

        public EmployeeProfileModel(FlightAlrightContext context)
        {
            _context = context;
        }

        public void OnGet()
        {
            // Sprawdzenie, czy u¿ytkownik jest zalogowany
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                RedirectToPage("/Login");
                return;
            }

            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            var account = _context.Account.FirstOrDefault(a => a.Id == userId);
            if (account != null)
            {
                EmployeeName = account.Name;
            }
            else
            {
                EmployeeName = "Nieznany U¿ytkownik";
            }
        }
    }
}
