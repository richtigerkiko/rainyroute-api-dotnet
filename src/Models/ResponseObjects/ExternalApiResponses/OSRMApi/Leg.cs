using System.Text.Json.Serialization;

namespace rainyroute.Models.ResponseObjects.ExternalApiResponses.OSRMApi;

public class Leg
    {
        [JsonPropertyName("steps")]
        public List<object> Steps { get; set; }

        [JsonPropertyName("summary")]
        public string Summary { get; set; }

        [JsonPropertyName("weight")]
        public double Weight { get; set; }

        [JsonPropertyName("duration")]
        public double Duration { get; set; }

        [JsonPropertyName("annotation")]
        public Annotation Annotation { get; set; }

        [JsonPropertyName("distance")]
        public double Distance { get; set; }
    }
