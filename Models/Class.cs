using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlightAlright.Models
{
    public class Class
    {
        [Key]
        public int Id { get; set; }
        public string? Name { get; set; }
        public int? SeatsNumber {  get; set; }
        [ForeignKey("Brand")]
        public int? BrandId { get; set; }
        public Brand? Brand { get; set; }
    }
}
