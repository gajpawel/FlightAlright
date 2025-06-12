using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace FlightAlright.Models
{
    public class Account
    {
        [Key]
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? Login { get; set; }
        public string? Password { get; set; }
        public bool? Status{ get; set; }
        [ForeignKey("Role")]
        public int? RoleId { get; set; }
        public Role? Role { get; set; }
        public float? Money { get; set; }
    }
}
