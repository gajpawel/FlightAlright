using System.ComponentModel.DataAnnotations;

namespace FlightAlright.Models
{
    public class Brand
    {
        [Key]
        public int Id { get; set; }
        public string? Model { get; set; }
        public string? Name { get; set; }
        public float? MaxDistance { get; set; }
    }
}
