using System.Text.Json.Serialization;

namespace rainyroute.Models.ResponseObjects.ExternalApiResponses.OSRMApi;

public class Route
{
    [JsonPropertyName("geometry")]
    public string Geometry { get; set; }

    [JsonPropertyName("legs")]
    public List<Leg> Legs { get; set; }

    [JsonPropertyName("weight_name")]
    public string WeightName { get; set; }

    [JsonPropertyName("weight")]
    public double Weight { get; set; }

    [JsonPropertyName("duration")]
    public double Duration { get; set; }

    [JsonPropertyName("distance")]
    public double Distance { get; set; }
}
