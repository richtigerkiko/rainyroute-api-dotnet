namespace rainyroute.Models.ResponseObjects;

public class WeatherRouteResponse
{
    public GeoCoordinate CoordinatesStart { get; set; }

    public GeoCoordinate CoordinatesDestination { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime FinishTime { get; set; }
    public List<WeatherRoutePoint> WeatherRoutePoints { get; set; }
    public string PolyLine { get; set; }
}