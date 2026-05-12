using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeocodingModule.Core.Services
{
    public class AddressNormalizerService
    {
        public string Normalize(string street, string postalCode, string city, string country)
        {
            city = NormalizeCity(city);
            country = NormalizeCountry(country);
            postalCode = NormalizePostalCode(postalCode);

            return $"{street}, {postalCode} {city}, {country}";
        }

        private string NormalizeCity(string city)
        {
            if (string.IsNullOrWhiteSpace(city))
                return "";

            return city
                .Trim()
                .Replace("ą", "a").Replace("ć", "c")
                .Replace("ę", "e").Replace("ł", "l")
                .Replace("ń", "n").Replace("ó", "o")
                .Replace("ś", "s").Replace("ż", "z")
                .Replace("ź", "z");
        }

        private string NormalizeCountry(string country)
        {
            return country?.Trim() switch
            {
                "Polska" => "Poland",
                "Chorwacja" => "Croatia",
                "Niemcy" => "Germany",
                "Włochy" => "Italy",
                _ => country ?? ""
            };
        }

        private string NormalizePostalCode(string postalCode)
        {
            return postalCode?
                .Replace(" ", "")
                .Trim() ?? "";
        }
    }
}
