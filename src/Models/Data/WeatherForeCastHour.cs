using rainyroute.Models.ResponseObjects.ExternalApiResponses.WeatherAPI;

namespace rainyroute.Models.Data;

public class WeatherForeCastHour
{
    public string Id { get; set; }
    public string WeatherBoundingBoxId { get; set; }
    public DateTime Time { get; set; }
    public int ChanceOfRain { get; set; }
    public int WindDegree { get; set; }

    // in kph
    public double WindSpeed { get; set; }
    public string WeatherAPIComIconURL { get; set; } = "";

    public bool WillItRain => ChanceOfRain >= 50;

    public WeatherForeCastHour()
    {

    }

    public WeatherForeCastHour(Hour weatherApiHour)
    {
        Time = DateTimeOffset.FromUnixTimeSeconds(weatherApiHour.TimeEpoch).DateTime;
        ChanceOfRain = weatherApiHour.ChanceOfRain;
        WindDegree = weatherApiHour.WindDegree;
        WindSpeed = weatherApiHour.WindKph;
        WeatherAPIComIconURL = weatherApiHour.Condition.Icon;
    }
}