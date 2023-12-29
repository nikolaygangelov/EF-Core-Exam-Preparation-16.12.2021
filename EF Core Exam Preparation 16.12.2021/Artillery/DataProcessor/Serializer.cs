﻿
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

            return JsonConvert.SerializeObject(shellsAndGuns, Formatting.Indented);
        }

        public static string ExportGuns(ArtilleryContext context, string manufacturer)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ExportGunsDTO[]), new XmlRootAttribute("Guns"));

            StringBuilder sb = new StringBuilder();

            using var writer = new StringWriter(sb);

            var xns = new XmlSerializerNamespaces();
            xns.Add(string.Empty, string.Empty);

            var gunsAndCountries = context.Guns
                .Where(g => g.Manufacturer.ManufacturerName == manufacturer)
                .Select(g => new ExportGunsDTO
                {
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

            serializer.Serialize(writer, gunsAndCountries, xns);
            writer.Close();

            return sb.ToString();
        }
    }
}
