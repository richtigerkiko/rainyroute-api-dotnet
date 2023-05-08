using rainyroute.Persistance.Models;

public class FullWeatherMapResponse
{
    public int Day { get; set; }
    public int Hour { get; set; }
    public List<WeatherRouteBoundingBox> FullWeatherMap { get; set; }
}