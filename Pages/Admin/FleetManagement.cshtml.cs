using FlightAlright.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FlightAlright.Pages.Admin
{
    public class FleetManagementModel : PageModel
    {
        private readonly FlightAlrightContext _context;

        public FleetManagementModel(FlightAlrightContext context)
        {
            _context = context;
        }

        public void OnGet()
        {
        }

    }
}
