using Microsoft.Extensions.DependencyInjection;
using GeocodingModule.Core.Services;

namespace GeocodingModule.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGeocodingModule(this IServiceCollection services)
    {
        services.AddHttpClient<IGeocodingService, NominatimGeocodingService>();
        return services;
    }
}