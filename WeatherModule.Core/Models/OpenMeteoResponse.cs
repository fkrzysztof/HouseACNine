using System.Text.Json.Serialization;

namespace WeatherModule.Core.Models;

public class OpenMeteoResponse
{
    [JsonPropertyName("current")]
    public CurrentWeather Current { get; set; }
}

public class CurrentWeather
{
    [JsonPropertyName("temperature_2m")]
    public double Temperature { get; set; }

    [JsonPropertyName("wind_speed_10m")]
    public double WindSpeed { get; set; }

    [JsonPropertyName("weather_code")]
    public int WeatherCode { get; set; }
}