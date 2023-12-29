using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Artillery.DataProcessor.ExportDto
{
    [XmlType("Country")]
    public class ExportGunsCountriesDTO
    {
        [XmlAttribute("Country")]
        public string Country { get; set; }
        [XmlAttribute("ArmySize")]
        public int ArmySize { get; set; }
    }
}