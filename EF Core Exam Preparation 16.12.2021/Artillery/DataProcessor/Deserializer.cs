namespace Artillery.DataProcessor
{
    using Artillery.Data;
    using Artillery.Data.Models;
    using Artillery.Data.Models.Enums;
    using Artillery.DataProcessor.ImportDto;
    using Newtonsoft.Json;
    using System.ComponentModel.DataAnnotations;
    using System.Text;
    using System.Xml.Serialization;

    public class Deserializer
    {
        private const string ErrorMessage =
            "Invalid data.";
        private const string SuccessfulImportCountry =
            "Successfully import {0} with {1} army personnel.";
        private const string SuccessfulImportManufacturer =
            "Successfully import manufacturer {0} founded in {1}.";
        private const string SuccessfulImportShell =
            "Successfully import shell caliber #{0} weight {1} kg.";
        private const string SuccessfulImportGun =
            "Successfully import gun {0} with a total weight of {1} kg. and barrel length of {2} m.";

        public static string ImportCountries(ArtilleryContext context, string xmlString)
        {
            var serializer = new XmlSerializer(typeof(ImportCountriesDTO[]), new XmlRootAttribute("Countries"));
            using StringReader inputReader = new StringReader(xmlString);
            var countriesArrayDTOs = (ImportCountriesDTO[])serializer.Deserialize(inputReader);

            StringBuilder sb = new StringBuilder();
            List<Country> countriesXML = new List<Country>();

            foreach (ImportCountriesDTO countryDTO in countriesArrayDTOs)
            {
                Country countryToAdd = new Country
                {
                    CountryName = countryDTO.CountryName,
                    ArmySize = countryDTO.ArmySize
                };

                if (!IsValid(countryDTO))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                countriesXML.Add(countryToAdd);
                sb.AppendLine(string.Format(SuccessfulImportCountry, countryToAdd.CountryName,
                    countryToAdd.ArmySize));
            }

            context.Countries.AddRange(countriesXML);

            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportManufacturers(ArtilleryContext context, string xmlString)
        {
            var serializer = new XmlSerializer(typeof(ImportManifacturersDTO[]), new XmlRootAttribute("Manufacturers"));
            using StringReader inputReader = new StringReader(xmlString);
            var manufacturersArrayDTOs = (ImportManifacturersDTO[])serializer.Deserialize(inputReader);

            StringBuilder sb = new StringBuilder();
            List<Manufacturer> manufacturersXML = new List<Manufacturer>();

            foreach (ImportManifacturersDTO manufacturerDTO in manufacturersArrayDTOs)
            {

                if (!IsValid(manufacturerDTO))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                if (manufacturersXML.Any(m => m.ManufacturerName == manufacturerDTO.ManufacturerName))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Manufacturer manifacturerToAdd = new Manufacturer
                {
                    ManufacturerName = manufacturerDTO.ManufacturerName,
                    Founded = manufacturerDTO.Founded
                };


                string[] foundedText = manufacturerDTO.Founded.Split(", ");

                string town = foundedText[foundedText.Length - 2];
                string country = foundedText[foundedText.Length - 1];
                string townCountry = $"{town}, {country}";

                manufacturersXML.Add(manifacturerToAdd);
                sb.AppendLine(string.Format(SuccessfulImportManufacturer, manifacturerToAdd.ManufacturerName, townCountry));
            }

            context.Manufacturers.AddRange(manufacturersXML);

            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportShells(ArtilleryContext context, string xmlString)
        {
            var serializer = new XmlSerializer(typeof(ImportShellsDTO[]), new XmlRootAttribute("Shells"));
            using StringReader inputReader = new StringReader(xmlString);
            var shellsArrayDTOs = (ImportShellsDTO[])serializer.Deserialize(inputReader);

            StringBuilder sb = new StringBuilder();
            List<Shell> shellsXML = new List<Shell>();

            foreach (ImportShellsDTO shellDTO in shellsArrayDTOs)
            {

                if (!IsValid(shellDTO))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Shell shellToAdd = new Shell
                {
                    ShellWeight = shellDTO.ShellWeight,
                    Caliber = shellDTO.Caliber
                };

                shellsXML.Add(shellToAdd);
                sb.AppendLine(string.Format(SuccessfulImportShell, shellToAdd.Caliber,
                    shellToAdd.ShellWeight));
            }

            context.Shells.AddRange(shellsXML);

            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportGuns(ArtilleryContext context, string jsonString)
        {
            var gunsArray = JsonConvert.DeserializeObject<ImportGunsDTO[]>(jsonString);

            StringBuilder sb = new StringBuilder();
            List<Gun> gunsList = new List<Gun>();

            foreach (ImportGunsDTO gunDTO in gunsArray)
            {

                if (!IsValid(gunDTO))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Gun gunToAdd = new Gun()
                {
                    ManufacturerId = gunDTO.ManufacturerId,
                    GunWeight = gunDTO.GunWeight,
                    BarrelLength = gunDTO.BarrelLength,
                    NumberBuild = gunDTO.NumberBuild,
                    Range = gunDTO.Range,
                    GunType = (GunType)Enum.Parse(typeof(GunType), gunDTO.GunType),
                    ShellId = gunDTO.ShellId
                };

                foreach (var country in gunDTO.Countries)
                {
                    gunToAdd.CountriesGuns.Add(new CountryGun()
                    {
                        CountryId = country.Id
                    });

                }

                gunsList.Add(gunToAdd);
                sb.AppendLine(string.Format(SuccessfulImportGun, gunToAdd.GunType, gunToAdd.GunWeight,
                    gunToAdd.BarrelLength));
            }

            context.Guns.AddRange(gunsList);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }
        private static bool IsValid(object obj)
        {
            var validator = new ValidationContext(obj);
            var validationRes = new List<ValidationResult>();

            var result = Validator.TryValidateObject(obj, validator, validationRes, true);
            return result;
        }
    }
}