using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using FlightAlright.Data;
using FlightAlright.Models;

namespace FlightAlright.Pages.Employees
{
    public class IndexModel : PageModel
    {
        private readonly FlightAlright.Data.FlightAlrightContext _context;

        public IndexModel(FlightAlright.Data.FlightAlrightContext context)
        {
            _context = context;
        }

        public IList<Employee> Employee { get;set; } = default!;

        public async Task OnGetAsync()
        {
            Employee = await _context.Employee
                .Include(e => e.Account)
                .Include(e => e.Position).ToListAsync();
        }
    }
}
