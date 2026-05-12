using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeocodingModule.Core.Services
{
    public class AddressPipelineService
    {
        private readonly AddressNormalizerService _normalizer;
        private readonly IGeocodingService _geocoding;

        public AddressPipelineService(
            AddressNormalizerService normalizer,
            IGeocodingService geocoding)
        {
            _normalizer = normalizer;
            _geocoding = geocoding;
        }

        public async Task<(double? lat, double? lon)> ResolveAsync(
            string street,
            string postalCode,
            string city,
            string country)
        {
            var fullAddress = _normalizer.Normalize(street, postalCode, city, country);

            var result = await _geocoding.GetCoordinatesAsync(fullAddress);

            if (result == null)
                return (null, null);

            return (result.Latitude, result.Longitude);
        }
    }
}
