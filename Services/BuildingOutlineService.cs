using System.Text.Json;

namespace SimpleWebAppReact.Services;

/// <summary>
/// Fetches building outlines from the OpenStreetMap Overpass API
/// </summary>
public class BuildingOutlineService
{
    private const string OverpassUrl = "https://overpass-api.de/api/interpreter";

    private readonly HttpClient _httpClient;

    public BuildingOutlineService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        // Overpass rejects requests without a User-Agent (406 Not Acceptable)
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("BoilerRooms/1.0");
    }

    /// <summary>
    /// closed polygons of every building within radius (degrees) of a point
    /// </summary>
    public async Task<List<List<(double Lat, double Lon)>>> GetBuildingOutline(double latitude, double longitude, double radius)
    {
        var query = FormattableString.Invariant(
            $"[out:json];way[\"building\"]({latitude - radius},{longitude - radius},{latitude + radius},{longitude + radius});out geom;");

        using var response = await _httpClient.PostAsync(OverpassUrl, new StringContent(query));
        response.EnsureSuccessStatusCode();
        using var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());

        var buildings = new List<List<(double Lat, double Lon)>>();
        foreach (var element in json.RootElement.GetProperty("elements").EnumerateArray())
        {
            if (element.GetProperty("type").GetString() != "way" || !element.TryGetProperty("geometry", out var geometry))
            {
                continue;
            }

            var coordinates = geometry.EnumerateArray()
                .Select(point => (point.GetProperty("lat").GetDouble(), point.GetProperty("lon").GetDouble()))
                .ToList();

            // Close the polygon if the last point does not repeat the first
            if (coordinates.Count > 0 && coordinates[0] != coordinates[^1])
            {
                coordinates.Add(coordinates[0]);
            }

            buildings.Add(coordinates);
        }

        return buildings;
    }
}
