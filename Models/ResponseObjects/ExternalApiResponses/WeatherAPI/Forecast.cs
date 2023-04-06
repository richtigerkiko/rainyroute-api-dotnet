using System.Text.Json.Serialization;

namespace rainyroute.Models.ResponseObjects.ExternalApiResponses.WeatherAPI;

// Model from https://json2csharp.com/ because I'm lazy

public class Forecast
{
    [JsonPropertyName("forecastday")]
    public List<Forecastday> Forecastday { get; set; }
}
