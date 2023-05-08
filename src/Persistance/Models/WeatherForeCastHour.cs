using System.Text.Json.Serialization;
using rainyroute.Models.ResponseObjects.ExternalApiResponses.WeatherAPI;

namespace rainyroute.Persistance.Models;

public class WeatherForeCastHour
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public DateTime Time { get; set; }
    public int ChanceOfRain { get; set; }
    public int WindDegree { get; set; }
    public int Cloud { get; set; }
    public bool isDay { get; set; }


    // in kph
    public double WindSpeed { get; set; }
    public string WeatherAPIComIconURL { get; set; } = "";
    public string WeatherBoundingBoxId { get; set; }
    
    [JsonIgnore]
    public WeatherBoundingBox WeatherBoundingBox { get; set; }


    public bool WillItRain => ChanceOfRain >= 50;
    public bool IsSunny => (Cloud <= 25 && isDay);

    public WeatherForeCastHour()
    {

    }

    public WeatherForeCastHour(Hour weatherApiHour)
    {
        Time = DateTimeOffset.FromUnixTimeSeconds(weatherApiHour.TimeEpoch).UtcDateTime;
        ChanceOfRain = weatherApiHour.ChanceOfRain;
        WindDegree = weatherApiHour.WindDegree;
        WindSpeed = weatherApiHour.WindKph;
        WeatherAPIComIconURL = weatherApiHour.Condition.Icon;
        Cloud = weatherApiHour.Cloud;
        isDay = weatherApiHour.IsDay == 1;
    }
}