using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;

namespace FlightAlright.Models
{
    public class Flight
    {
        [Key]
        public int Id { get; set; }
        public DateTime? DepartureDate { get; set; }
        [ForeignKey("Airport")]
        public int? DepartureAirportId { get; set; }
        public Airport? DepartureAirport { get; set; }
        public DateTime? ArrivalDate { get; set; }
        [ForeignKey("Airport")]
        public int? ArrivalAirportId { get; set; }
        public Airport? ArrivalAirport { get; set; }
        [ForeignKey("Plane")]
        public int? PlaneId { get; set; }
        public Plane? Plane { get; set; }
        public float? LuggagePrice { get; set; }

        public bool? Status { get; set; } //1 - aktywny, 0 - nieaktywny
    }
}
