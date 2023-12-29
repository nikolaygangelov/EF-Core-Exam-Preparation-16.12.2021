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
            //using Data Transfer Object Class to map it with Countries
            var serializer = new XmlSerializer(typeof(ImportCountriesDTO[]), new XmlRootAttribute("Countries"));

            //Deserialize method needs TextReader object to convert/map 
            using StringReader inputReader = new StringReader(xmlString);
            var countriesArrayDTOs = (ImportCountriesDTO[])serializer.Deserialize(inputReader);

            //using StringBuilder to gather all info in one string
            StringBuilder sb = new StringBuilder();

            //creating List where all valid countries can be kept
            List<Country> countriesXML = new List<Country>();

            foreach (ImportCountriesDTO countryDTO in countriesArrayDTOs)
            {
                //validating info for country from data
                if (!IsValid(countryDTO))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                //creating a valid country
                Country countryToAdd = new Country
                {
                    //using identical properties in order to map successfully
                    CountryName = countryDTO.CountryName,
                    ArmySize = countryDTO.ArmySize
                };

                countriesXML.Add(countryToAdd);
                sb.AppendLine(string.Format(SuccessfulImportCountry, countryToAdd.CountryName,
                    countryToAdd.ArmySize));
            }

            context.Countries.AddRange(countriesXML);

            //actual importing info from data
            context.SaveChanges();

            //using TrimEnd() to get rid of white spaces
            return sb.ToString().TrimEnd();
        }

        public static string ImportManufacturers(ArtilleryContext context, string xmlString)
        {
            //using Data Transfer Object Class to map it with manifacturers
            var serializer = new XmlSerializer(typeof(ImportManifacturersDTO[]), new XmlRootAttribute("Manufacturers"));

            //Deserialize method needs TextReader object to convert/map 
            using StringReader inputReader = new StringReader(xmlString);
            var manufacturersArrayDTOs = (ImportManifacturersDTO[])serializer.Deserialize(inputReader);

            //using StringBuilder to gather all info in one string
            StringBuilder sb = new StringBuilder();

            //creating List where all valid manufacturers can be kept
            List<Manufacturer> manufacturersXML = new List<Manufacturer>();

            foreach (ImportManifacturersDTO manufacturerDTO in manufacturersArrayDTOs)
            {
                //validating info for manufacturer from data
                if (!IsValid(manufacturerDTO))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                //checking for duplicates
                if (manufacturersXML.Any(m => m.ManufacturerName == manufacturerDTO.ManufacturerName))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                //creating a valid manufacturer
                Manufacturer manifacturerToAdd = new Manufacturer
                {
                    //using identical properties in order to map successfully
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

            //actual importing info from data
            context.SaveChanges();

            //using TrimEnd() to get rid of white spaces
            return sb.ToString().TrimEnd();
        }

        public static string ImportShells(ArtilleryContext context, string xmlString)
        {
            //using Data Transfer Object Class to map it with shells
            var serializer = new XmlSerializer(typeof(ImportShellsDTO[]), new XmlRootAttribute("Shells"));

            //Deserialize method needs TextReader object to convert/map 
            using StringReader inputReader = new StringReader(xmlString);
            var shellsArrayDTOs = (ImportShellsDTO[])serializer.Deserialize(inputReader);

            //using StringBuilder to gather all info in one string
            StringBuilder sb = new StringBuilder();

            //creating List where all valid shells can be kept
            List<Shell> shellsXML = new List<Shell>();

            foreach (ImportShellsDTO shellDTO in shellsArrayDTOs)
            {
                //validating info for shell from data
                if (!IsValid(shellDTO))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                //creating a valid shell
                Shell shellToAdd = new Shell
                {
                    //using identical properties in order to map successfully
                    ShellWeight = shellDTO.ShellWeight,
                    Caliber = shellDTO.Caliber
                };

                shellsXML.Add(shellToAdd);
                sb.AppendLine(string.Format(SuccessfulImportShell, shellToAdd.Caliber,
                    shellToAdd.ShellWeight));
            }

            context.Shells.AddRange(shellsXML);

            //actual importing info from data
            context.SaveChanges();

            //using TrimEnd() to get rid of white spaces
            return sb.ToString().TrimEnd();
        }

        public static string ImportGuns(ArtilleryContext context, string jsonString)
        {
            //using Data Transfer Object Class to map it with guns
            var gunsArray = JsonConvert.DeserializeObject<ImportGunsDTO[]>(jsonString);

            //using StringBuilder to gather all info in one string
            StringBuilder sb = new StringBuilder();

            //creating List where all valid guns can be kept
            List<Gun> gunsList = new List<Gun>();

            foreach (ImportGunsDTO gunDTO in gunsArray)
            {
                //validating info for gun from data
                if (!IsValid(gunDTO))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                //creating a valid gun
                Gun gunToAdd = new Gun()
                {
                    //using identical properties in order to map successfully
                    ManufacturerId = gunDTO.ManufacturerId,
                    GunWeight = gunDTO.GunWeight,
                    BarrelLength = gunDTO.BarrelLength,
                    NumberBuild = gunDTO.NumberBuild,
                    Range = gunDTO.Range,
                    GunType = (GunType)Enum.Parse(typeof(GunType), gunDTO.GunType), //using "Parse" method to parse string enum "GunType"
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

            //actual importing info from data
            context.SaveChanges();

            //using TrimEnd() to get rid of white spaces
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
