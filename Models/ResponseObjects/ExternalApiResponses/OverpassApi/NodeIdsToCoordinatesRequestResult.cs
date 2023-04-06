using System.Text.Json.Serialization;

namespace rainyroute.Models.ResponseObjects.ExternalApiResponses.OverpassApi;

public class NodeIdsToCoordinatesRequestResult
{
    [JsonPropertyName("version")]
    public double Version { get; set; }

    [JsonPropertyName("generator")]
    public string Generator { get; set; }

    [JsonPropertyName("osm3s")]
    public Osm3s Osm3s { get; set; }

    [JsonPropertyName("elements")]
    public List<NodeIdToCoordinatesResultElement>? Elements { get; set; }
}


