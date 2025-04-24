using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;

namespace FlightAlright.Pages.viewComponents
{
    public class AdminMenuViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var currentPage = ViewContext.RouteData.Values["Page"]?.ToString() ?? "";

            var menuItems = new List<(string Title, string PageName)>
        {
            ("Konto", "./AdminProfile"),
            ("Personel", "./PersonelManagement"),
            ("Lotniska", "./AirportManagement"),
            ("Flota", "./FleetManagement"),
            //("Loty", "./FlightManagement") // psuje sie przy wyborze
        };

            return View((currentPage, menuItems));
        }
    }

}
