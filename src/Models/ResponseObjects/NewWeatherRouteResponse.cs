using rainyroute.Models.Data;

public class NewWeatherRouteResponse
{
    public Tuple<double, double> CoordinatesStart { get; set; }
    public Tuple<double, double> CoordinatesDestination { get; set; }

    public DateTime StartTime { get; set; }
    public DateTime ProjectedFinishTime { get; set; }

    public string PolyLine { get; set; }

    public List<WeatherRouteBoundingBox> PassedBoundingBoxes { get; set; }

}