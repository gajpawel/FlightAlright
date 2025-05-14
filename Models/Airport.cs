using System.ComponentModel.DataAnnotations;

namespace FlightAlright.Models
{
    public class Airport
    {
        [Key]
        public int Id { get; set; }
        public string? Code { get; set; } //Trzyznakowy kod
        public string? Name { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public int? TimeZoneOffset { get; set; } //np. dla Londynu 0, dla Polski 2, a dla Nowego Jorku -4
    }
}
