using rainyroute.Models;
using rainyroute.Models.Data;

namespace rainyroute.Models.ResponseObjects;
public class NewWeatherRouteResponse
{
    public GeoCoordinate CoordinatesStart { get; set; }
    public GeoCoordinate CoordinatesDestination { get; set; }

    public DateTime StartTime { get; set; }
    public DateTime ProjectedFinishTime { get; set; }

    public string PolyLine { get; set; }

    public List<WeatherRouteBoundingBox> PassedBoundingBoxes { get; set; }

}