using System.Text.Json.Serialization;

namespace rainyroute.Models.ResponseObjects.ExternalApiResponses.WeatherAPI;

// Model from https://json2csharp.com/ because I'm lazy


public class Forecastday
{
    [JsonPropertyName("date")]
    public string Date { get; set; }

    [JsonPropertyName("date_epoch")]
    public int DateEpoch { get; set; }

    [JsonPropertyName("day")]
    public Day Day { get; set; }

    [JsonPropertyName("astro")]
    public Astro Astro { get; set; }

    [JsonPropertyName("hour")]
    public List<Hour> Hour { get; set; }
}