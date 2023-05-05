using rainyroute.Models;
using rainyroute.Models.Interfaces;

public class BoundingBox : IBoundingBox
{
    public GeoCoordinate MinCoordinate { get; set; }
    public GeoCoordinate MaxCoordinate { get; set; }

    public GeoCoordinate CenterOfBoundingBox => new GeoCoordinate((MinCoordinate.Latitude + MaxCoordinate.Latitude) / 2, (MinCoordinate.Longitude + MaxCoordinate.Longitude) / 2);

    public BoundingBox()
    {
        MinCoordinate = new GeoCoordinate();
        MaxCoordinate = new GeoCoordinate();
    }

    public BoundingBox(List<GeoCoordinate> coordinates)
    {
        var minlat = coordinates.Min(x => x.Latitude);
        var minlon = coordinates.Min(x => x.Longitude);
        var maxlat = coordinates.Max(x => x.Latitude);
        var maxlon = coordinates.Max(x => x.Longitude);

        MinCoordinate = new GeoCoordinate(minlat, minlon);
        MaxCoordinate = new GeoCoordinate(maxlat, maxlon);
    }

    public bool Contains(double latidude, double longitude)
    {
        return latidude >= MinCoordinate.Latitude && latidude <= MaxCoordinate.Latitude && longitude >= MinCoordinate.Longitude && longitude <= MaxCoordinate.Longitude;
    }

    public bool Contains(GeoCoordinate coordinate)
    {
        return Contains(coordinate.Latitude, coordinate.Longitude);
    }

    public bool Contains(Tuple<double, double> coordinates)
    {
        return Contains(coordinates.Item1, coordinates.Item2);
    }

    public bool Contains(BoundingBox boundingBox)
    {
        return Contains(boundingBox.MinCoordinate) && Contains(boundingBox.MinCoordinate);
    }

    public double GetDistanceTo(IBoundingBox other)
    {
        return CenterOfBoundingBox.GetDistanceTo(other.CenterOfBoundingBox);
    }

    public virtual List<IBoundingBox> DivideIntoSmallerBoxes(double boxSizeSqKm)
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

                var smallerBox = new BoundingBox()
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

    public string ToShapeWkt()
    {
        return $"POLYGON(({MinCoordinate.Longitude} {MinCoordinate.Latitude}, {MinCoordinate.Longitude} {MaxCoordinate.Latitude}, {MaxCoordinate.Longitude} {MaxCoordinate.Latitude}, {MaxCoordinate.Longitude} {MinCoordinate.Latitude}, {MinCoordinate.Longitude} {MinCoordinate.Latitude}))";
    }
}