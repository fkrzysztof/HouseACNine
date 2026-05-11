

using Microsoft.Extensions.DependencyInjection;
using WeatherModule.Core.Services;

namespace WeatherModule.Core.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddWeatherModule(this IServiceCollection services)
        {
            services.AddHttpClient<IWeatherService, OpenMeteoWeatherService>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(5);
            });
            return services;
        }
    }
}
