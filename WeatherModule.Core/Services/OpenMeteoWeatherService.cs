using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WeatherModule.Core.Models;

namespace WeatherModule.Core.Services
{
    public class OpenMeteoWeatherService : IWeatherService
    {
        private readonly HttpClient _http;

        public OpenMeteoWeatherService(HttpClient http)
        {
            _http = http;
        }

        public async Task<WeatherDto?> GetCurrentAsync(double lat, double lon)
        {
            var url =
                $"https://api.open-meteo.com/v1/forecast?latitude={lat}&longitude={lon}&current_weather=true";

            var json = await _http.GetFromJsonAsync<JsonElement>(url);

            var current = json.GetProperty("current_weather");

            return new WeatherDto
            {
                Temperature = current.GetProperty("temperature").GetDouble(),
                WindSpeed = current.GetProperty("windspeed").GetDouble(),
                WeatherCode = current.GetProperty("weathercode").GetInt32()
            };
        }
    }
}
