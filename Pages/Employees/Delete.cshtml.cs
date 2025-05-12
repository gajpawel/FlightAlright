using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using FlightAlright.Data;
using FlightAlright.Models;
using System.Threading.Tasks;

namespace FlightAlright.Pages.Employees
{
    public class DeleteModel : PageModel
    {
        private readonly FlightAlrightContext _context;

        public DeleteModel(FlightAlrightContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Employee Employee { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
                return NotFound();

            Employee = await _context.Employee
                .Include(e => e.Account)
                .Include(e => e.Position)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (Employee == null)
                return NotFound();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null || _context.Employee == null)
                return NotFound();

            var employee = await _context.Employee
                .Include(e => e.Account)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee != null)
            {
                if (employee.Account != null)
                {
                    employee.Account.RoleId = 1;
                    _context.Account.Update(employee.Account);
                }

                employee.Status = false;
                _context.Employee.Update(employee);

                await _context.SaveChangesAsync();
            }


            return RedirectToPage("./Index");
        }
    }
}
