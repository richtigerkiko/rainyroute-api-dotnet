using NetTopologySuite.Geometries;
using rainyroute.Persistance.Models;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Utilities;

public class WeatherRouteBoundingBox : WeatherBoundingBox
{
    public WeatherRouteBoundingBox(WeatherBoundingBox box, List<Point> coordinatesInBoundingBox)
    {
        Id = box.Id;
        MinCoordinate = box.MinCoordinate;
        MaxCoordinate = box.MaxCoordinate;
        WeatherForeCastHours = box.WeatherForeCastHours;
        CoordinateClostestToCenter = FindClosestGeoCoordinateToCenter(coordinatesInBoundingBox);
        SetTravelingDirection(coordinatesInBoundingBox.First(), coordinatesInBoundingBox.Last());
    }

    public WeatherRouteBoundingBox(WeatherBoundingBox box)
    {
        Id = box.Id;
        MinCoordinate = box.MinCoordinate;
        MaxCoordinate = box.MaxCoordinate;
        WeatherForeCastHours = box.WeatherForeCastHours;
        CoordinateClostestToCenter = new Point(BoundingBox.Centre);
    }

    public WeatherRouteBoundingBox(WeatherRouteBoundingBox other)
    {
        Id = other.Id;
        MinCoordinate = other.MinCoordinate;
        MaxCoordinate = other.MaxCoordinate;
        WeatherForeCastHours = other.WeatherForeCastHours;
        TimeClosestToCenter = other.TimeClosestToCenter;
        TotalDurationClosestToCenter = other.TotalDurationClosestToCenter;
        CoordinateClostestToCenter = other.CoordinateClostestToCenter;
    }

    public DateTime TimeClosestToCenter { get; set; }
    public double TotalDurationClosestToCenter { get; set; }

    public Point CoordinateClostestToCenter { get; set; }

    public int TravelingDegrees { get; set; }


    // Computed
    public string CardinalDirecton => GetCardinalDirection();
    public int WeatherForecastAtDurationIndex => Findindex();
    public bool HasTailwind => CalculateIfTailwind();

    private bool CalculateIfTailwind()
    {
        if (WeatherForecastAtDurationIndex != -1)
        {
            var directionDifferenceDegrees = (WeatherForecastAtDuration.WindDegree - TravelingDegrees + 360) % 360;
            return (directionDifferenceDegrees >= 135 && directionDifferenceDegrees <= 225);
        }
        else{
            return false;
        }
    }

    private int Findindex()
    {
        if (WeatherForeCastHours.Count == 1)
        {
            return 0;
        }
        else
        {
            return WeatherForeCastHours.FindIndex(x => x.Time.DayOfYear == TimeClosestToCenter.DayOfYear && x.Time.Hour == TimeClosestToCenter.Hour);
        }

    }

    private WeatherForeCastHour _weatherForecastAtDuration = new WeatherForeCastHour(); // to stop stackoverflow
    public WeatherForeCastHour WeatherForecastAtDuration
    {
        get
        {
            return WeatherForecastAtDurationIndex != -1 ? WeatherForeCastHours[WeatherForecastAtDurationIndex] : _weatherForecastAtDuration;
        }
        set
        {
            _weatherForecastAtDuration = value;
        }
    }


    private Point FindClosestGeoCoordinateToCenter(List<Point> coordinatesInBoundingBox)
    {
        var closestCoord = new Point(0, 0);
        double minDistance = double.MaxValue;
        foreach (var bbcoordinate in coordinatesInBoundingBox)
        {
            var distance = BoundingBox.Centre.Distance(bbcoordinate.Coordinate);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestCoord = bbcoordinate;
            }
        }
        return closestCoord;
    }

    private void SetTravelingDirection(Point start, Point finish)
    {
        var radians = AngleUtility.Angle(start.Coordinate, finish.Coordinate);
        var degrees = (int)Math.Round(radians * (180 / Math.PI)) % 360;
        if (degrees <= 0)
        {
            degrees = degrees + 360;
        }
        this.TravelingDegrees = degrees;
    }

    private string GetCardinalDirection()
    {
        if (TravelingDegrees >= 337.5 || TravelingDegrees < 22.5)
        {
            return "N";
        }
        else if (TravelingDegrees >= 22.5 && TravelingDegrees < 67.5)
        {
            return "NE";
        }
        else if (TravelingDegrees >= 67.5 && TravelingDegrees < 112.5)
        {
            return "E";
        }
        else if (TravelingDegrees >= 112.5 && TravelingDegrees < 157.5)
        {
            return "SE";
        }
        else if (TravelingDegrees >= 157.5 && TravelingDegrees < 202.5)
        {
            return "S";
        }
        else if (TravelingDegrees >= 202.5 && TravelingDegrees < 247.5)
        {
            return "SW";
        }
        else if (TravelingDegrees >= 247.5 && TravelingDegrees < 292.5)
        {
            return "W";
        }
        else
        {
            return "NW";
        }
    }
}