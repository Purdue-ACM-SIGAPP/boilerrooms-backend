using System.Text.Json;

namespace SimpleWebAppReact.Services;

/// <summary>
/// Wraps the Google Maps Geocoding and Distance Matrix APIs, keyed by GoogleMaps:ApiKey
/// </summary>
public class GoogleMapsService
{
    private const string BaseUrl = "https://maps.googleapis.com/maps/api/";

    private readonly HttpClient _httpClient;
    private readonly ILogger<GoogleMapsService> _logger;
    private readonly string? _apiKey;

    public GoogleMapsService(HttpClient httpClient, IConfiguration configuration, ILogger<GoogleMapsService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiKey = configuration["GoogleMaps:ApiKey"];
    }

    /// <summary>
    /// coordinates of an address, or null if it cannot be geocoded
    /// </summary>
    public async Task<(double Latitude, double Longitude)?> GeocodeAsync(string address)
    {
        using var json = await GetJsonAsync($"geocode/json?address={Uri.EscapeDataString(address)}");
        if (json is null || !json.RootElement.TryGetProperty("results", out var results) || results.GetArrayLength() == 0)
        {
            return null;
        }

        var location = results[0].GetProperty("geometry").GetProperty("location");
        return (location.GetProperty("lat").GetDouble(), location.GetProperty("lng").GetDouble());
    }

    /// <summary>
    /// human-readable travel distance and duration between two places, or null if unavailable
    /// </summary>
    public async Task<(string Distance, string Duration)?> GetDistanceAsync(string origin, string destination)
    {
        using var json = await GetJsonAsync(
            $"distancematrix/json?origins={Uri.EscapeDataString(origin)}&destinations={Uri.EscapeDataString(destination)}&units=imperial");
        if (json is null || !json.RootElement.TryGetProperty("rows", out var rows) || rows.GetArrayLength() == 0)
        {
            return null;
        }

        var elements = rows[0].GetProperty("elements");
        if (elements.GetArrayLength() == 0
            || !elements[0].TryGetProperty("distance", out var distance)
            || !elements[0].TryGetProperty("duration", out var duration))
        {
            return null;
        }

        return (distance.GetProperty("text").GetString()!, duration.GetProperty("text").GetString()!);
    }

    private async Task<JsonDocument?> GetJsonAsync(string pathAndQuery)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            _logger.LogWarning("GoogleMaps:ApiKey is not configured");
            return null;
        }

        try
        {
            using var response = await _httpClient.GetAsync($"{BaseUrl}{pathAndQuery}&key={_apiKey}");
            response.EnsureSuccessStatusCode();
            return await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        }
        catch (HttpRequestException e)
        {
            _logger.LogError(e, "Google Maps request failed");
            return null;
        }
    }
}
