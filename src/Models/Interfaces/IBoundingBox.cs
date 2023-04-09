namespace rainyroute.Models.Interfaces;

public interface IBoundingBox
{
    public GeoCoordinate MinCoordinate { get; set; }
    public GeoCoordinate MaxCoordinate { get; set; }
    public GeoCoordinate CenterOfBoundingBox { get; }

    bool ContainsPoint(double latidude, double longitude);
    // GeoCoordinate CenterOfBoundingBox();

    List<IBoundingBox> DivideIntoSmallerBoxes(double boxSizeSqKm);

    IBoundingBox GetBoundingBox(List<GeoCoordinate> coordinates);

    double GetDistanceTo(IBoundingBox other);

    string ToShapeWkt();
}