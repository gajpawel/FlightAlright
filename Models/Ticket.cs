using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlightAlright.Models
{
    public class Ticket
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("Account")]
        public int? AccountId { get; set; }
        public Account? Account { get; set; }
        [ForeignKey("Price")]
        public int? PriceId { get; set; }
        public Price? Price { get; set; }
        public float? TicketPrice { get; set; }
        public bool? ExtraLuggage { get; set; }
        public bool? Status { get; set; }
    }
}
