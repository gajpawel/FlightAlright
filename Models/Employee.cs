using System.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations.Schema;

namespace FlightAlright.Models
{
    public class Employee
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("Account")]
        public int? AccountId { get; set; }
        public Account? Account { get; set; }
        [ForeignKey("Position")]
        public int? PositionId { get; set; }
        public Position? Position { get; set; }
        public bool? Status { get; set; } //1 - aktywny, 0 - zwolniony
    }
}
