using rainyroute.Models;
using rainyroute.Models.Data;

public class WeatherRouteBoundingBox : WeatherBoundingBox
{
    public WeatherRouteBoundingBox()
    {
    }

    public WeatherRouteBoundingBox(WeatherBoundingBox box)
    {
        Id = box.Id;
        MinCoordinate = box.MinCoordinate;
        MaxCoordinate = box.MaxCoordinate;
        WeatherForeCastHours = box.WeatherForeCastHours;
    }

    public DateTime TimeClosestToCenter { get; set; }
    public double TotalDurationClosestToCenter { get; set; }
    public List<GeoCoordinate> CoordinatesInBoundingBox { get; set; } = new List<GeoCoordinate>();
    public GeoCoordinate CoordinateClostestToCenter => FindClosestGeoCoordinateToCenter();
    public int WeatherForecastAtDurationIndex =>  WeatherForeCastHours.FindIndex(x => x.Time.DayOfYear == TimeClosestToCenter.DayOfYear && x.Time.Hour == TimeClosestToCenter.Hour);

    private WeatherForeCastHour _weatherForecastAtDuration = new WeatherForeCastHour(); // to stop stackoverflow
    public WeatherForeCastHour WeatherForecastAtDuration
    {
        get
        {
            return WeatherForecastAtDurationIndex != -1 ? WeatherForeCastHours[WeatherForecastAtDurationIndex] : _weatherForecastAtDuration;
        }
        set{
            _weatherForecastAtDuration = value;
        }
    }

    private GeoCoordinate FindClosestGeoCoordinateToCenter()
    {
        var closestCoord = new GeoCoordinate();
        double minDistance = double.MaxValue;
        foreach (var bbcoordinate in CoordinatesInBoundingBox)
        {
            var distance = CenterOfBoundingBox.GetDistanceTo(bbcoordinate);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestCoord = bbcoordinate;
            }
        }
        return closestCoord;
    }
}