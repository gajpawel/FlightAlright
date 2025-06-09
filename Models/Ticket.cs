using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlightAlright.Models
{
    public class Ticket
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("Account")]
        public int? AccountId { get; set; } //Jeśli null, to bilet jest wolny - można go kupić
        public Account? Account { get; set; }
        [ForeignKey("Price")]
        public int? PriceId { get; set; }
        public Price? Price { get; set; }
        public float? TicketPrice { get; set; } //Zapamiętaj cenę aktualną w momencie zakupu biletu, na wypadek gdyby uległa zmianie
        public bool? ExtraLuggage { get; set; }
        public char? Status { get; set; } //D - dostępny do zakupu, K - kupiony, A - anulowany, N - nieaktywny (przeszły), R - zarezerwowane tymczasowo (dopóki nie potwierdzono płatności)
        public int? Seating { get; set; }
    }
}
