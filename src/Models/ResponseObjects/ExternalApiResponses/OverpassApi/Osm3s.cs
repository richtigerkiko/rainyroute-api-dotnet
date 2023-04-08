using System.Text.Json.Serialization;

namespace rainyroute.Models.ResponseObjects.ExternalApiResponses.OverpassApi;

public class Osm3s
{
    [JsonPropertyName("timestamp_osm_base")]
    public DateTime TimestampOsmBase { get; set; }

    [JsonPropertyName("copyright")]
    public string Copyright { get; set; }
}


