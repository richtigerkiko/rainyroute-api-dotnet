
// Iplementation like old net 4 Geocoordinate: https://learn.microsoft.com/en-us/dotnet/api/system.device.location.geocoordinate?view=netframework-4.8
using Microsoft.EntityFrameworkCore;

namespace rainyroute.Models;

public class GeoCoordinate : IEquatable<GeoCoordinate>
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double Altitude { get; set; }
    public double Speed { get; set; }
    public double Course { get; set; }
    public CardinalDirection CardinalDirection { get; set; }

    public GeoCoordinate()
    {
    }

    public GeoCoordinate(double latitude, double longitude)
    {
        this.Latitude = latitude;
        this.Longitude = longitude;
    }

    public GeoCoordinate(double latitude, double longitude, double altitude, double speed, double course)
    {
        this.Latitude = latitude;
        this.Longitude = longitude;
        this.Altitude = altitude;
        this.Speed = speed;
        this.Course = course;
    }

    // Calculates the distance between two geocoordinates. 
    // Because I'm bad at math i used google https://www.sunearthtools.com/tools/distance.php 
    // distance (A, B) = R * arccos (sin(latA) * sin(latB) + cos(latA) * cos(latB) * cos(lonA-lonB))
    public double GetDistanceTo(GeoCoordinate other)
    {
        const double EarthRadiusInKilometers = 6372.795477598;

        double sinLatA = Math.Sin(ToRadians(Latitude));
        double sinLatB = Math.Sin(ToRadians(other.Latitude));
        double cosLatA = Math.Cos(ToRadians(Latitude));
        double cosLatB = Math.Cos(ToRadians(other.Latitude));
        double cosLonAMinusB = Math.Cos(ToRadians(Longitude) - ToRadians(other.Longitude));

        double distanceInMeters = EarthRadiusInKilometers * 1000 * Math.Acos(sinLatA * sinLatB + cosLatA * cosLatB * cosLonAMinusB);

        // Only calculate if Altitude is not zero
        if (other.Altitude != 0 && Altitude != 0)
        {
            double altitudeDifferenceInMeters = other.Altitude - Altitude;

            distanceInMeters = Math.Sqrt(distanceInMeters * distanceInMeters + altitudeDifferenceInMeters * altitudeDifferenceInMeters);
        }

        return distanceInMeters;
    }

    public bool Equals(GeoCoordinate? other) => ((other is not null) && other.Longitude == Longitude && other.Latitude == Latitude);

    private double ToRadians(double deg)
    {
        return deg * Math.PI / 180;
    }

    // thanks open AI 
    public double CalculateCourse(GeoCoordinate other)
    {
        // Convert latitude and longitude values to radians
        double lat1Radians = ToRadians(Latitude);
        double lon1Radians = ToRadians(Longitude);
        double lat2Radians = ToRadians(other.Latitude);
        double lon2Radians = ToRadians(other.Longitude);

        // Calculate the difference between the longitudes
        double deltaLon = lon2Radians - lon1Radians;

        // Calculate the direction using the atan2 function
        double y = Math.Sin(deltaLon) * Math.Cos(lat2Radians);
        double x = Math.Cos(lat1Radians) * Math.Sin(lat2Radians) - Math.Sin(lat1Radians) * Math.Cos(lat2Radians) * Math.Cos(deltaLon);
        double directionRadians = Math.Atan2(y, x);

        // Convert direction from radians to degrees
        double directionDegrees = directionRadians * 180.0 / Math.PI;

        // Make sure the direction is positive
        if (directionDegrees < 0)
        {
            directionDegrees += 360.0;
        }

        return directionDegrees;
    }


    // sets course to destination
    public void SetCourse(GeoCoordinate destination)
    {
        Course = CalculateCourse(destination);
        CardinalDirection = GetCardinalDirection(destination);
    }

    public Tuple<double, double> ToTuple(){
        return new Tuple<double, double>(Latitude, Longitude);
    }

    // ist noch falsch
    public CardinalDirection GetCardinalDirection(GeoCoordinate other)
    {
    
        var course = CalculateCourse(other);


        int halfQuarter = Convert.ToInt32(course);
        halfQuarter %= 8;
        Console.WriteLine(halfQuarter);
        return (CardinalDirection)halfQuarter;

    }

}