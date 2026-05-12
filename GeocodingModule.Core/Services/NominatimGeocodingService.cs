using System.Globalization;
using System.Net.Http.Json;
using GeocodingModule.Core.Models;

namespace GeocodingModule.Core.Services;

public class NominatimGeocodingService : IGeocodingService
{
    private readonly HttpClient _http;

    public NominatimGeocodingService(HttpClient http)
    {
        _http = http;
        _http.DefaultRequestHeaders.UserAgent.ParseAdd("GeocodingModule/1.0");
    }

    public async Task<GeocodingResult?> GetCoordinatesAsync(string address)
    {
        try
        {
            var url =
                $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(address)}&format=json&limit=1";

            var result =
                await _http.GetFromJsonAsync<List<NominatimResponse>>(url);

            var item = result?.FirstOrDefault();

            if (item == null)
                return null;

            return new GeocodingResult
            {
                //Latitude = double.Parse(item.Lat, CultureInfo.InvariantCulture),
                //Longitude = double.Parse(item.Lon, CultureInfo.InvariantCulture)
            };
        }
        catch
        {
            return null;
        }
    }
}