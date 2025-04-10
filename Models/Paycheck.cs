using System.ComponentModel.DataAnnotations;

namespace FlightAlright.Models
{
    public class Paycheck
    {
        [Key]
        public int Id { get; set; }
        public int? EmployeeId { get; set; }
        public Employee? Employee { get; set; }
        public float? Amount { get; set; }
        public DateTime? Date {  get; set; }
        public string? BankAccount { get; set; }
    }
}
