﻿using System.ComponentModel.DataAnnotations;

namespace Artillery.Data.Models
{
    public class Manufacturer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(40)]
        public string ManufacturerName  { get; set; }

        [Required]
        [MaxLength(100)]
        public string Founded { get; set; }

        public ICollection<Gun> Guns { get; set; } = new HashSet<Gun>();
    }
}
