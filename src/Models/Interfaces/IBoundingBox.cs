namespace rainyroute.Models.Interfaces;

interface IBoundingBox
{
    public Tuple<double, double> MinCoordinate { get; set; }
    public Tuple<double, double> MaxCoordinate { get; set; }

    void ContainsPoint(double latidude, double longitude);
    Tuple<double, double> CenterOfBoundingBox();

    List<IBoundingBox> DivideIntoSmallerBoxes(double boxSizeSqKm);
}