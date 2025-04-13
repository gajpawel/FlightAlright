using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FlightAlright.Pages
{
    public class AccessDeniedModel : PageModel
    {
        public void OnGet()
        {
        }

        public int GetRandom()
        {
            Random random = new Random();
            return random.Next(1, 6);
        }
    }
}
