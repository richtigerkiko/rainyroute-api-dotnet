using rainyroute.Models.Interfaces;

namespace rainyroute.Models.Data;

public class WeatherRouteBoundingBox : IBoundingBox
{
    public string Id { get; set; }
    public Tuple<double, double> MinCoordinate { get; set; } 
    public Tuple<double, double> MaxCoordinate { get; set; }
    public Tuple<double, double> CenterOfBoundingBox => new Tuple<double, double>((MinCoordinate.Item1 + MaxCoordinate.Item1) / 2,(MinCoordinate.Item2 + MaxCoordinate.Item2) / 2);

    public List<WeatherForeCastHour> WeatherForeCastHours { get; set; } = new List<WeatherForeCastHour>();


    public void ContainsPoint(double latidude, double longitude)
    {
        throw new NotImplementedException();
    }

    public double DistanceTo(IBoundingBox other)
    {
        const double EarthRadiusInKilometers = 6372.795477598;

        var originLatitude = CenterOfBoundingBox.Item1;
        var originLongitude = CenterOfBoundingBox.Item2;
        var otherLatitude = other.CenterOfBoundingBox.Item1;
        var otherLongitude = other.CenterOfBoundingBox.Item2;

        double sinLatA = Math.Sin(ToRadians(originLatitude));
        double sinLatB = Math.Sin(ToRadians(otherLatitude));
        double cosLatA = Math.Cos(ToRadians(originLatitude));
        double cosLatB = Math.Cos(ToRadians(otherLatitude));
        double cosLonAMinusB = Math.Cos(ToRadians(originLongitude) - ToRadians(otherLongitude));

        double distanceInMeters = EarthRadiusInKilometers * 1000 * Math.Acos(sinLatA * sinLatB + cosLatA * cosLatB * cosLonAMinusB);

        return distanceInMeters;
    }

    public List<IBoundingBox> DivideIntoSmallerBoxes(double boxSizeSqKm)
    {
        double latidudeStep = Math.Sqrt(boxSizeSqKm / 1000); // squarekm to square degrees
        double longitudeStep = latidudeStep / Math.Cos(Math.PI * (MinCoordinate.Item1 + MaxCoordinate.Item1) / 360); // Adjust for latitude distortion

        var smallerBoxes = new List<IBoundingBox>();
        // Iterate boundingbox with latitude Steps
        for (double lat = MinCoordinate.Item1; lat < MaxCoordinate.Item1; lat += latidudeStep)
        {
            // foreach latstep iterate longitude
            for (double lon = MinCoordinate.Item2; lon < MaxCoordinate.Item2; lon += longitudeStep)
            {
                double latUpper = Math.Min(lat + latidudeStep, MaxCoordinate.Item1);
                double lonUpper = Math.Min(lon + longitudeStep, MaxCoordinate.Item2);

                var smallerBox = new WeatherRouteBoundingBox()
                {
                    MinCoordinate = new Tuple<double, double>(lat, lon),
                    MaxCoordinate = new Tuple<double, double>(latUpper, lonUpper)

                };

                smallerBoxes.Add(smallerBox);
            }
        }

        return smallerBoxes;
    }

    private double ToRadians(double deg)
    {
        return deg * Math.PI / 180;
    }
}
