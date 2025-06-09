using FlightAlright.Data;
using FlightAlright.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Stripe;

namespace FlightAlright.Pages.PaymentResults
{
    public class PaymentFailureModel : PageModel
    {

        private readonly FlightAlrightContext _context;

        public PaymentFailureModel(FlightAlrightContext context)
        {
            _context = context;
        }
        public void OnGet(int accountId)
        {
            var reservedTickets = _context.Ticket.Where(t => t.AccountId == accountId && t.Status == 'R').ToList();
            foreach (var ticket in reservedTickets)
            {
                ticket.Status = 'D';
                ticket.AccountId = null;
                ticket.TicketPrice = null;
                ticket.ExtraLuggage = null;
            }
            _context.SaveChanges();
        }
    }
}
