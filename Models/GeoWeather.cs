using System.Globalization;
using rainyroute.Models.ResponseObjects.ExternalApiResponses.WeatherAPI;

namespace rainyroute.Models;
public class WeatherRoutePoint
{
    public double TotalDistance { get; set; }
    public double DistanceFromLastPoint { get; set; }
    public GeoCoordinate Coordinates { get; set; }
    public double TotalDuration { get; set; }
    public double DurationFromLastPoint { get; set; }
    public Hour? WeatherForecastAtDuration { get; set; }
    public List<Hour> CompleteForecast { get; set; }

    public WeatherRoutePoint(GeoCoordinate coordinates, double distanceFromLastPoint, double durationFromLastPoint, WeatherRoutePoint? LastPoint)
    {
        Coordinates = coordinates;

        DistanceFromLastPoint = distanceFromLastPoint;
        TotalDistance = (LastPoint?.TotalDistance ?? 0) + DistanceFromLastPoint;

        DurationFromLastPoint = durationFromLastPoint;
        TotalDuration = (LastPoint?.TotalDuration ?? 0) + DurationFromLastPoint;

    }

    public double GetDistanceTo(WeatherRoutePoint other)
    {
        return Coordinates.GetDistanceTo(other.Coordinates);
    }

    public double GetDistanceTo(GeoCoordinate other)
    {
        return Coordinates.GetDistanceTo(other);
    }

    public void FillHour (List<Hour> weatherForecast, DateTime routeStartDate){
        var dateTimeAtDuration = routeStartDate.AddSeconds(TotalDuration);

        WeatherForecastAtDuration = weatherForecast.Where(x => (DateTime.ParseExact(x.Time, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture).Hour) == dateTimeAtDuration.Hour).FirstOrDefault();
        CompleteForecast = weatherForecast;
    }
}