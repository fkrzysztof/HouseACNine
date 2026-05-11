using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeatherModule.Core.Models;

namespace WeatherModule.Core.Services
{
    public interface IWeatherService
    {
        Task<WeatherDto?> GetCurrentAsync(double lat, double lon);
    }
}
