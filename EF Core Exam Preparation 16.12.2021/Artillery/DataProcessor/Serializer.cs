
namespace Artillery.DataProcessor
{
    using Artillery.Data;
    using Artillery.Data.Models.Enums;
    using Artillery.DataProcessor.ExportDto;
    using Newtonsoft.Json;
    using System.Text;
    using System.Xml.Serialization;

    public class Serializer
    {
        public static string ExportShells(ArtilleryContext context, double shellWeight)
        {
            //turning needed info about shells into a collection using anonymous object
            //using less data
            var shellsAndGuns = context.Shells
                .Where(s => s.ShellWeight > shellWeight)
                .Select(s => new
                {
                    ShellWeight = s.ShellWeight,
                    Caliber = s.Caliber,
                    Guns = s.Guns
                    .Where(g => g.GunType == GunType.AntiAircraftGun)
                    .Select(g => new
                    {
                        GunType = g.GunType.ToString(),
                        GunWeight = g.GunWeight,
                        BarrelLength = g.BarrelLength,
                        Range = g.Range > 3000 ? "Long-range" : "Regular range"
                    })
                    .OrderByDescending(g => g.GunWeight)
                    .ToArray()
                })
                .OrderBy(s => s.ShellWeight)
                .ToArray();

            //Serialize method needs object to convert/map
	        //adding Formatting for better reading 
            return JsonConvert.SerializeObject(shellsAndGuns, Formatting.Indented);
        }

        public static string ExportGuns(ArtilleryContext context, string manufacturer)
        {
            //using Data Transfer Object Class to map it with guns
            XmlSerializer serializer = new XmlSerializer(typeof(ExportGunsDTO[]), new XmlRootAttribute("Guns"));

            //using StringBuilder to gather all info in one string
            StringBuilder sb = new StringBuilder();

            //"using" automatically closes opened connections
            using var writer = new StringWriter(sb);

            var xns = new XmlSerializerNamespaces();

            //one way to display empty namespace in resulted file
            xns.Add(string.Empty, string.Empty);

            var gunsAndCountries = context.Guns
                .Where(g => g.Manufacturer.ManufacturerName == manufacturer)
                .Select(g => new ExportGunsDTO
                {
                    //using identical properties in order to map successfully
                    Manufacturer = g.Manufacturer.ManufacturerName,
                    GunType = g.GunType.ToString(),
                    GunWeight = g.GunWeight,
                    BarrelLength = g.BarrelLength,
                    Range = g.Range,
                    Countries = g.CountriesGuns
                    .Where(cg => cg.Country.ArmySize > 4500000)
                    .Select(cg => new ExportGunsCountriesDTO
                    {
                        Country = cg.Country.CountryName,
                        ArmySize = cg.Country.ArmySize
                    })
                    .OrderBy(c => c.ArmySize)
                    .ToArray()
                })
                .OrderBy(g => g.BarrelLength)
                .ToArray();

            //Serialize method needs file, TextReader object and namespace to convert/map
            serializer.Serialize(writer, gunsAndCountries, xns);

            //explicitly closing connection in terms of reaching edge cases
            writer.Close();

            //using TrimEnd() to get rid of white spaces
            return sb.ToString().TrimEnd();
        }
    }
}
