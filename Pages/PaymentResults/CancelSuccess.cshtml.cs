using FlightAlright.Data;
using FlightAlright.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FlightAlright.Pages.Client
{
    public class PaymentSuccessModel : PageModel
    {
        private readonly FlightAlrightContext _context;

        public PaymentSuccessModel(FlightAlrightContext context)
        {
            _context = context;
        }

        public IActionResult OnGet(int ticketId)
        {
            if (TempData["PaymentSuccess"] == null)
            {
                return RedirectToPage("/AccessDenied");
            }
            var ticket = _context.Ticket.FirstOrDefault(t => t.Id == ticketId);
            ticket.Status = 'A';
            _context.SaveChanges();
            Ticket emptyticket = new Ticket
            {
                AccountId = null,
                PriceId = ticket.PriceId,
                TicketPrice = null,
                ExtraLuggage = null,
                Status = 'D',
                Seating = ticket.Seating
            };
            _context.Add(emptyticket);
            _context.SaveChanges();
            return Page();
        }
    }
}