using FlightAlright.Data;
using FlightAlright.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Stripe;

namespace FlightAlright.Pages.PaymentResults
{
    public class PaymentSuccessModel : PageModel
    {

        private readonly FlightAlrightContext _context;
        
        public PaymentSuccessModel(FlightAlrightContext context)
        {
            _context = context;
        }
        public void OnGet(int accountId)
        {
            var reservedTickets = _context.Ticket.Where(t => t.AccountId == accountId && t.Status=='R').ToList();
            foreach (var ticket in reservedTickets)
            {
                ticket.Status = 'K';
            }
            _context.SaveChanges();
        }
    }
}
