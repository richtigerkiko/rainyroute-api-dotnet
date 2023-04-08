namespace rainyroute.Models.Interfaces;

public interface IBoundingBox
{
    public Tuple<double, double> MinCoordinate { get; set; }
    public Tuple<double, double> MaxCoordinate { get; set; }
    public Tuple<double, double> CenterOfBoundingBox { get; }

    void ContainsPoint(double latidude, double longitude);
    // Tuple<double, double> CenterOfBoundingBox();

    List<IBoundingBox> DivideIntoSmallerBoxes(double boxSizeSqKm);

    double DistanceTo(IBoundingBox other);
}