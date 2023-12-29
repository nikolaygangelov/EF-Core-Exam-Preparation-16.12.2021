using Artillery.Data.Models.Enums;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Artillery.DataProcessor.ImportDto
{
    public class ImportGunsDTO
    {
        [Key]
        [JsonProperty("ManufacturerId")]
        public int ManufacturerId { get; set; }

        [Required]
        [Range(100, 1350000)]
        [JsonProperty("GunWeight")]
        public int GunWeight { get; set; }

        [Required]
        [Range(2.00, 35.00)]
        [JsonProperty("BarrelLength")]
        public double BarrelLength { get; set; }

        [JsonProperty("NumberBuild")]
        public int? NumberBuild { get; set; }

        [Required]
        [Range(1, 100000)]
        [JsonProperty("Range")]
        public int Range { get; set; }

        [Required]
        [JsonProperty("GunType")]
        [RegularExpression(@"^(Howitzer|Mortar|FieldGun|AntiAircraftGun|MountainGun|AntiTankGun)\b")]
        public string GunType { get; set; } //format "string" in order to apply regex

        [Required]
        [JsonProperty("ShellId")]
        public int ShellId { get; set; }

        [JsonProperty("Countries")]
        public ImportGunsCountriesDTO[] Countries { get; set; }
    }
}
