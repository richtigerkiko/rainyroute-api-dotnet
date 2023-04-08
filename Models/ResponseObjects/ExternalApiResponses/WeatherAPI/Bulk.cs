using System.Text.Json.Serialization;

namespace rainyroute.Models.ResponseObjects.ExternalApiResponses.WeatherAPI;

public class Bulk
    {
        [JsonPropertyName("query")]
        public Query Query { get; set; }
    }
