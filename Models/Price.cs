using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlightAlright.Models
{
    public class Price
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("Class")]
        public int? ClassId { get; set; }
        public Class? Class { get; set; }
        [ForeignKey("Flight")]
        public int? FlightId { get; set; }
        public Flight? Flight { get; set; }
        public float? CurrentPrice {  get; set; }
        public bool? Status { get; set; }
    }
}
