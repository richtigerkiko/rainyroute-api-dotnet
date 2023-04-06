using System.Text.Json.Serialization;

namespace rainyroute.Models.ResponseObjects.ExternalApiResponses.OSRMApi;

public class Annotation
    {
        [JsonPropertyName("metadata")]
        public Metadata Metadata { get; set; }

        [JsonPropertyName("datasources")]
        public List<int> Datasources { get; set; }

        [JsonPropertyName("weight")]
        public List<double> Weight { get; set; }

        [JsonPropertyName("nodes")]
        public List<object> Nodes { get; set; }

        [JsonPropertyName("distance")]
        public List<double> Distance { get; set; }

        [JsonPropertyName("duration")]
        public List<double> Duration { get; set; }

        [JsonPropertyName("speed")]
        public List<double> Speed { get; set; }
    }
