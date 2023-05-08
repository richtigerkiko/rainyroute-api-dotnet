using NetTopologySuite.Geometries;

public class WeatherRouteResponse
{
    public Point CoordinatesStart { get; set; }
    public Point CoordinatesDestination { get; set; }

    public DateTime StartTime { get; set; }
    public DateTime ProjectedFinishTime { get; set; }

    public string PolyLine { get; set; }

    public List<WeatherRouteBoundingBox> PassedBoundingBoxes { get; set; }
}