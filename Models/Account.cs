using System.ComponentModel.DataAnnotations;

namespace FlightAlright.Models
{
    public class Account
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Surname { get; set; }
    }
}
