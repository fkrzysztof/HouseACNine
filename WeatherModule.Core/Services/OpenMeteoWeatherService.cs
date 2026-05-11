using System.Globalization;
using System.Net.Http.Json;
using WeatherModule.Core.Models;

namespace WeatherModule.Core.Services;

public class OpenMeteoWeatherService : IWeatherService
{
    private readonly HttpClient _http;

    public OpenMeteoWeatherService(HttpClient http)
    {
        _http = http;
    }

    public async Task<WeatherDto?> GetCurrentAsync(double lat, double lon)
    {
        try
        {
            var url =
                $"https://api.open-meteo.com/v1/forecast?latitude={lat.ToString(CultureInfo.InvariantCulture)}&longitude={lon.ToString(CultureInfo.InvariantCulture)}&current=temperature_2m,weather_code,wind_speed_10m";

            var response =
                await _http.GetFromJsonAsync<OpenMeteoResponse>(url);

            if (response?.Current == null)
                return null;

            return new WeatherDto
            {
                Temperature = response.Current.Temperature,
                WindSpeed = response.Current.WindSpeed,
                WeatherCode = response.Current.WeatherCode
            };
        }
        catch
        {
            return null;
        }
    }
}