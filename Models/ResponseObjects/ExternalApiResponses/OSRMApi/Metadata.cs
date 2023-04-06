using System.Text.Json.Serialization;

namespace rainyroute.Models.ResponseObjects.ExternalApiResponses.OSRMApi;

public class Metadata
    {
        [JsonPropertyName("datasource_names")]
        public List<string> DatasourceNames { get; set; }
    }
