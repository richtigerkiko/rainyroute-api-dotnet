using System.Text.Json.Serialization;

namespace rainyroute.Models.ResponseObjects.ExternalApiResponses.OSRMApi;

public class OSRMApiResult
{
    [JsonPropertyName("code")]
    public string Code { get; set; }

    [JsonPropertyName("routes")]
    public List<Route> Routes { get; set; }

    [JsonPropertyName("waypoints")]
    public List<Waypoint> Waypoints { get; set; }
}
