using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlightAlright.Models
{
    public class Crew
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("Employee")]
        public int? EmployeeId { get; set; }
        public Employee? Employee { get; set; }
        [ForeignKey("Flight")]
        public int? FlightId { get; set; }
        public Flight? Flight { get; set; }
    }
}
