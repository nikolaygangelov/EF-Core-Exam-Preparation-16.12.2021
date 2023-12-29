using System.ComponentModel.DataAnnotations;

namespace Artillery.Data.Models
{
    public class Shell
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Range(2.00, 1680.00)]
        public double ShellWeight { get; set; }

        [Required]
        [MaxLength(30)]
        public string Caliber  { get; set; }

        public ICollection<Gun> Guns { get; set; } = new HashSet<Gun>();
    }
}
