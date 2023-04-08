using System.Text.Json.Serialization;

namespace rainyroute.Models.ResponseObjects.ExternalApiResponses.WeatherAPI;
public class Astro
{
    [JsonPropertyName("sunrise")]
    public string Sunrise { get; set; }

    [JsonPropertyName("sunset")]
    public string Sunset { get; set; }

    [JsonPropertyName("moonrise")]
    public string Moonrise { get; set; }

    [JsonPropertyName("moonset")]
    public string Moonset { get; set; }

    [JsonPropertyName("moon_phase")]
    public string MoonPhase { get; set; }

    [JsonPropertyName("moon_illumination")]
    public string MoonIllumination { get; set; }

    [JsonPropertyName("is_moon_up")]
    public int IsMoonUp { get; set; }

    [JsonPropertyName("is_sun_up")]
    public int IsSunUp { get; set; }
}