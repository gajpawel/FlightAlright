using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlightAlright.Models
{
    public class Plane
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("Brand")]
        public int? BrandId { get; set; }
        public Brand? Brand { get; set; }
    }
}
