using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Artillery.DataProcessor.ImportDto
{
    public class ImportGunsCountriesDTO
    {
        [Key]
        [JsonProperty("Id")]
        public int Id { get; set; }
    }
}