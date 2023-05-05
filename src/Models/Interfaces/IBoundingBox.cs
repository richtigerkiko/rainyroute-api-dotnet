namespace rainyroute.Models.Interfaces;

public interface IBoundingBox
{
    public GeoCoordinate MinCoordinate { get; set; }
    public GeoCoordinate MaxCoordinate { get; set; }
    public GeoCoordinate CenterOfBoundingBox { get; }

    bool Contains(double latidude, double longitude);
    // GeoCoordinate CenterOfBoundingBox();

    List<IBoundingBox> DivideIntoSmallerBoxes(double boxSizeSqKm);

    double GetDistanceTo(IBoundingBox other);

    string ToShapeWkt();
}