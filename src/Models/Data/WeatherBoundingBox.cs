using rainyroute.Models.Interfaces;

namespace rainyroute.Models.Data;

public class WeatherBoundingBox : IBoundingBox
{

    public string Id { get; set; }
    public GeoCoordinate MinCoordinate { get; set; } 
    public GeoCoordinate MaxCoordinate { get; set; }
    public GeoCoordinate CenterOfBoundingBox => new GeoCoordinate((MinCoordinate.Latitude + MaxCoordinate.Latitude) / 2,(MinCoordinate.Longitude + MaxCoordinate.Longitude) / 2);

    public List<WeatherForeCastHour> WeatherForeCastHours { get; set; } = new List<WeatherForeCastHour>();


    public bool ContainsPoint(double latidude, double longitude)
    {
        return latidude >= MinCoordinate.Latitude && latidude <= MaxCoordinate.Latitude && longitude >= MinCoordinate.Longitude && longitude <= MaxCoordinate.Longitude;
    }

    public bool ContainsPoint(GeoCoordinate coordinate)
    {
        return ContainsPoint(coordinate.Latitude, coordinate.Longitude);
    }

    public bool ContainsPoint(Tuple<double, double> coordinates){
        return ContainsPoint(coordinates.Item1, coordinates.Item2);
    }

    public double GetDistanceTo(IBoundingBox other)
    {
        return CenterOfBoundingBox.GetDistanceTo(other.CenterOfBoundingBox);
    }

    public List<IBoundingBox> DivideIntoSmallerBoxes(double boxSizeSqKm)
    {
        double latidudeStep = Math.Sqrt(boxSizeSqKm / 1000); // squarekm to square degrees
        double longitudeStep = latidudeStep / Math.Cos(Math.PI * (MinCoordinate.Latitude + MaxCoordinate.Latitude) / 360); // Adjust for latitude distortion

        var smallerBoxes = new List<IBoundingBox>();
        // Iterate boundingbox with latitude Steps
        for (double lat = MinCoordinate.Latitude; lat < MaxCoordinate.Latitude; lat += latidudeStep)
        {
            // foreach latstep iterate longitude
            for (double lon = MinCoordinate.Longitude; lon < MaxCoordinate.Longitude; lon += longitudeStep)
            {
                double latUpper = Math.Min(lat + latidudeStep, MaxCoordinate.Latitude);
                double lonUpper = Math.Min(lon + longitudeStep, MaxCoordinate.Longitude);

                var smallerBox = new WeatherBoundingBox()
                {
                    MinCoordinate = new GeoCoordinate(lat, lon),
                    MaxCoordinate = new GeoCoordinate(latUpper, lonUpper)

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
