namespace rainyroute.Models.ResponseObjects;

public class WeatherRouteResponse
{
    public WeatherRouteResponse(GeoCoordinate coordinateStart, GeoCoordinate coordinateDestination)
    {
        this.CoordinatesStart = coordinateStart;
        this.CoordinatesDestination = coordinateDestination;
        WeatherRoutePoints = new List<WeatherRoutePoint>();
        PolyLine = "";
        this.StartTime = DateTime.Now;
        this.FinishTime = DateTime.Now;

    }

    public WeatherRouteResponse()
    {
        this.CoordinatesStart = new GeoCoordinate();
        this.CoordinatesDestination = new GeoCoordinate();
        WeatherRoutePoints = new List<WeatherRoutePoint>();
        PolyLine = "";
        this.StartTime = DateTime.Now;
        this.FinishTime = DateTime.Now;

    }

    public WeatherRouteResponse(GeoCoordinate coordinatesStart, GeoCoordinate coordinatesDestination, DateTime finishTime, DateTime startTime, string polyLine)
    {
        this.CoordinatesStart = coordinatesStart;
        this.CoordinatesDestination = coordinatesDestination;
        this.FinishTime = finishTime;
        this.PolyLine = polyLine;
        this.StartTime = startTime;
        this.FinishTime = finishTime;
        WeatherRoutePoints = new List<WeatherRoutePoint>();

    }
    public GeoCoordinate CoordinatesStart { get; set; }

    public GeoCoordinate CoordinatesDestination { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime FinishTime { get; set; }
    public List<WeatherRoutePoint> WeatherRoutePoints { get; set; }
    public string PolyLine { get; set; }
}