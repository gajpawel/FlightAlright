using FlightAlright.Models;
using FlightAlright.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FlightAlright.Pages.Admin
{
    public class tempClass
    {
        public string? Name { get; set; }
        public int SeatsNumber { get; set; }
    }

    public class ClassEditModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int SeatsNumber { get; set; }
    }

    public class EditModelData
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int MaxDistance { get; set; }
        public List<ClassEditModel> EditedClasses { get; set; } = new();
        public List<tempClass> NewClasses { get; set; } = new();
    }

    public class FleetManagementModel : PageModel
    {
        private readonly FlightAlrightContext _context;

        [BindProperty]
        public int SelectedBrandId { get; set; }

        [BindProperty]
        public int SelectedModelId { get; set; }

        [BindProperty]
        public int count { get; set; } = 1;

        [BindProperty]
        public string NewBrandName { get; set; } = string.Empty;

        [BindProperty]
        public string NewModelName { get; set; } = string.Empty;

        [BindProperty]
        public int maxRange { get; set; } = 100;

        public List<Brand> planeSelection { get; set; } = new();
        public List<Plane> fleet { get; set; } = new();

        public List<SelectListItem> brands { get; set; } = new();
        public List<SelectListItem> models { get; set; } = new();

        [BindProperty]
        public List<tempClass> classes { get; set; } = new();

        [BindProperty]
        public EditModelData EditModel { get; set; } = new();

        public FleetManagementModel(FlightAlrightContext context)
        {
            _context = context;
        }

        public void OnGet()
        {
            LoadBrands();
            models = _context.Brand
                .Select(a => new SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = a.Model
                })
                .ToList();
            LoadFleet();
            LoadPlaneSelection();
        }

        public IActionResult OnPostSelectBrand()
        {
            LoadBrands();
            if (SelectedBrandId > 0)
            {
                LoadModels(SelectedBrandId);
            }
            LoadFleet();
            LoadPlaneSelection();

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
                    Brand = model,
                    Status = 'D'
                };
                _context.Plane.Add(plane);
            }

            _context.SaveChanges();

            LoadFleet();
            LoadPlaneSelection();

            return RedirectToPage();
        }

        public IActionResult OnPostAddModel()
        {
            // Load all data first to ensure tables will be displayed even if validation fails
            LoadFleet();
            LoadPlaneSelection();
            LoadBrands();
            if (SelectedBrandId > 0)
            {
                LoadModels(SelectedBrandId);
            }

            Brand newPlane;

            // plane validation
            if (!string.IsNullOrWhiteSpace(NewBrandName))
            {
                newPlane = new Brand { Name = NewBrandName, Model = NewModelName, MaxDistance = maxRange };
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
                    return Page();
                }
            }

            // class validation
            if (classes == null || !classes.Any())
            {
                ModelState.AddModelError("", "At least one class option is required.");
                return Page();
            }

            foreach (var option in classes)
            {
                if (string.IsNullOrWhiteSpace(option.Name) || option.SeatsNumber <= 0)
                {
                    ModelState.AddModelError("", "All class options must have a name and seat count > 0.");
                    return Page();
                }
            }

            _context.Brand.Add(newPlane);
            _context.SaveChanges();

            // save classes
            foreach (var option in classes)
            {
                var classEntry = new Class
                {
                    Name = option.Name,
                    SeatsNumber = option.SeatsNumber,
                    BrandId = newPlane.Id
                };
                _context.Class.Add(classEntry);
            }
            classes.Clear();

            _context.SaveChanges();

            // Redirect to clean state
            return RedirectToPage();
        }

        public void LoadFleet()
        {
            fleet = _context.Plane
                .Include(f => f.Brand)
                .ToList();
        }

        public void LoadPlaneSelection()
        {
            planeSelection = _context.Brand
                .Distinct()
                .ToList();
        }

        public void LoadBrands()
        {
            brands = _context.Brand
                .GroupBy(a => a.Name)
                .Select(a => new SelectListItem
                {
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
                .Select(a => new SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = a.Model
                })
                .ToList();
        }

        public IActionResult OnPostChangeStatus(int planeId, char status)
        {
            var plane = _context.Plane.FirstOrDefault(p => p.Id == planeId);
            if (plane == null)
            {
                return NotFound();
            }

            plane.Status = status;
            _context.SaveChanges();
            return RedirectToPage();
        }

        public JsonResult OnGetClassesForBrand(int brandId)
        {
            var classes = _context.Class
                .Where(c => c.BrandId == brandId)
                .Select(c => new { id = c.Id, name = c.Name, seatsNumber = c.SeatsNumber })
                .ToList();

            return new JsonResult(classes);
        }

        public IActionResult OnPostEditPlaneModel()
        {

            ModelState.Remove("NewBrandName");
            ModelState.Remove("NewModelName");

            try
            {
                // Debug information - log what we received
                Console.WriteLine($"EditModel.Id: {EditModel.Id}");
                Console.WriteLine($"EditModel.Name: {EditModel.Name}");
                Console.WriteLine($"EditModel.Model: {EditModel.Model}");
                Console.WriteLine($"EditModel.MaxDistance: {EditModel.MaxDistance}");
                
                if (EditModel.EditedClasses != null)
                {
                    Console.WriteLine($"EditedClasses count: {EditModel.EditedClasses.Count}");
                    foreach (var cls in EditModel.EditedClasses)
                    {
                        Console.WriteLine($"Class ID: {cls.Id}, Name: {cls.Name}, Seats: {cls.SeatsNumber}");
                    }
                }
                else
                {
                    Console.WriteLine("EditedClasses is null");
                }

                if (EditModel.NewClasses != null)
                {
                    Console.WriteLine($"NewClasses count: {EditModel.NewClasses.Count}");
                    foreach (var cls in EditModel.NewClasses)
                    {
                        Console.WriteLine($"New Class Name: {cls.Name}, Seats: {cls.SeatsNumber}");
                    }
                }
                else
                {
                    Console.WriteLine("NewClasses is null");
                }

                // Check if model is valid
                if (!ModelState.IsValid)
                {
                    Console.WriteLine("ModelState is invalid");
                    foreach (var state in ModelState)
                    {
                        if (state.Value.Errors.Count > 0)
                        {
                            Console.WriteLine($"Key: {state.Key}, Errors: {string.Join(", ", state.Value.Errors.Select(e => e.ErrorMessage))}");
                        }
                    }
                    TempData["ErrorMessage"] = "Dane formularza s¹ nieprawid³owe: " + string.Join(", ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));
                    LoadBrands();
                    LoadFleet();
                    LoadPlaneSelection();
                    return Page();
                }

                var brand = _context.Brand.FirstOrDefault(b => b.Id == EditModel.Id);
                if (brand == null)
                {
                    TempData["ErrorMessage"] = "Nie znaleziono modelu samolotu";
                    return RedirectToPage();
                }

                // Update brand fields
                brand.Name = EditModel.Name;
                brand.Model = EditModel.Model;
                brand.MaxDistance = EditModel.MaxDistance;

                // Update existing classes
                if (EditModel.EditedClasses != null && EditModel.EditedClasses.Any())
                {
                    foreach (var edited in EditModel.EditedClasses)
                    {
                        if (edited.Id <= 0) continue;

                        var existing = _context.Class.FirstOrDefault(c => c.Id == edited.Id);
                        if (existing != null && existing.BrandId == brand.Id)
                        {
                            existing.Name = edited.Name;
                            existing.SeatsNumber = edited.SeatsNumber;
                        }
                    }
                }

                // Add new classes
                if (EditModel.NewClasses != null && EditModel.NewClasses.Any())
                {
                    foreach (var newCls in EditModel.NewClasses)
                    {
                        if (!string.IsNullOrWhiteSpace(newCls.Name) && newCls.SeatsNumber > 0)
                        {
                            _context.Class.Add(new Class
                            {
                                Name = newCls.Name,
                                SeatsNumber = newCls.SeatsNumber,
                                BrandId = brand.Id
                            });
                        }
                    }
                }

                _context.SaveChanges();

                TempData["SuccessMessage"] = "Pomyœlnie zaktualizowano model samolotu";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Wyst¹pi³ b³¹d: {ex.Message}";
                LoadBrands();
                LoadFleet();
                LoadPlaneSelection();
                return Page();
            }
        }
    }
}