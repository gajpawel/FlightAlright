using FlightAlright.Models;
using FlightAlright.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Humanizer;

namespace FlightAlright.Pages.Admin
{
    public class FleetManagementModel : PageModel
    {
        private readonly FlightAlrightContext _context;

        [BindProperty]
        public int SelectedBrandId { get; set; }

        [BindProperty]
        public int SelectedModelId { get; set; }

        [BindProperty]
        public int count { get; set; }

        [BindProperty]
        public string NewBrandName { get; set; }

        [BindProperty]
        public string NewModelName { get; set; }

        [BindProperty]
        public int maxRange { get; set; }

        public List<Plane> fleet { get; set; } = new();

        public List<SelectListItem> brands { get; set; }
        public List<SelectListItem> models { get; set; }

        public FleetManagementModel(FlightAlrightContext context)
        {
            _context = context;
        }

        public void OnGet()
        {
            LoadBrands();
            models = _context.Brand
                .Select(a => new SelectListItem {
                    Value = a.Id.ToString(),
                    Text = a.Model
                    })
                .ToList();
            LoadFleet();
        }

        public IActionResult OnPostSelectBrand()
        {
            LoadBrands();
            if (SelectedBrandId > 0)
            {
                LoadModels(SelectedBrandId);
            }
            LoadFleet();

            return Page();
        }

        public IActionResult OnPostAddToFleet()
        {
            LoadBrands();
            LoadModels(SelectedBrandId);

            var model = _context.Brand.FirstOrDefault(a => a.Id == SelectedModelId);

            if (model == null)
            {
                ModelState.AddModelError("", "Invalid model selected.");
                return Page();
            }

            for (int i = 0; i < count; i++)
            {
                var plane = new Plane
                {
                    BrandId = model.Id,
                    Brand = model
                };
                _context.Plane.Add(plane);
            }

            _context.SaveChanges();

            LoadFleet();

            return RedirectToPage();
        }

        public IActionResult OnPostAddModel()
        {
            Brand newPlane;

            if (!string.IsNullOrWhiteSpace(NewBrandName))
            {
                newPlane = new Brand { Name = NewBrandName, Model = NewModelName, MaxDistance = maxRange};
            }
            else
            {
                var selectedBrandName = _context.Brand
                    .Where(a => a.Id == SelectedBrandId)
                    .Select(a => a.Name)
                    .FirstOrDefault();
                newPlane = new Brand { Name = selectedBrandName, Model = NewModelName, MaxDistance = maxRange };
                if (string.IsNullOrWhiteSpace(selectedBrandName))
                {
                    ModelState.AddModelError("", "Invalid brand selected.");
                    LoadBrands();
                    LoadModels(SelectedBrandId);
                    return Page();
                }
            }

            _context.Brand.Add(newPlane);

            _context.SaveChanges();

            LoadBrands();

            // Redirect to clean state
            return RedirectToPage();
        }

        public void LoadFleet()
        {
            fleet = _context.Plane
                .Include(f => f.Brand)
                .ToList();
        }

        public void LoadBrands()
        {
            brands = _context.Brand
                .GroupBy(a => a.Name)
                .Select(a => new SelectListItem {
                    Value = a.First().Id.ToString(),
                    Text = a.Key
                })
                .Distinct()
                .ToList();
        }

        public void LoadModels(int brandID)
        {
            var selectedBrandName = _context.Brand
                .Where(a => a.Id == brandID)
                .Select(a => a.Name)
                .FirstOrDefault();

            models = _context.Brand
                .Where(a => a.Name == selectedBrandName)
                .Select(a => new SelectListItem {
                    Value = a.Id.ToString(),
                    Text = a.Model
                })
                .ToList();
        }

    }
}
