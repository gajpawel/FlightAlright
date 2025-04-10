using System.ComponentModel.DataAnnotations;

namespace FlightAlright.Models
{
    public class Airport
    {
        [Key]
        public int Id { get; set; }
        public char? Code { get; set; }
        public string? Name { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
    }
}
