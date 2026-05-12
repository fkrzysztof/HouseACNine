using GeocodingModule.Core.Models;

namespace GeocodingModule.Core.Services;

public interface IGeocodingService
{
    Task<GeocodingResult?> GetCoordinatesAsync(string address);
}