using System.Text.Json.Serialization;

namespace rainyroute.Models.ResponseObjects.ExternalApiResponses.WeatherAPI;

public class WeatherApiBulkResponse
{
    [JsonPropertyName("bulk")]
    public List<Bulk> Bulk { get; set; }
}