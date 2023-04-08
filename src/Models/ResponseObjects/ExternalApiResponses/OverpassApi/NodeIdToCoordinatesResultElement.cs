using System.Text.Json.Serialization;

namespace rainyroute.Models.ResponseObjects.ExternalApiResponses.OverpassApi;

public class NodeIdToCoordinatesResultElement
{
    // [JsonPropertyName("type")]
    // public string Type { get; set; }

    [JsonPropertyName("id")]
    public decimal Id { get; set; }

    [JsonPropertyName("lat")]
    public double Lat { get; set; }

    [JsonPropertyName("lon")]
    public double Lon { get; set; }
}


