﻿using System.ComponentModel.DataAnnotations;

namespace FlightAlright.Models
{
    public class Role
    {
        [Key]
        public int Id { get; set; }
        public string? Name { get; set; }
    }
}
