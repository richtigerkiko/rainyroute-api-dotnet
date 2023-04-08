using rainyroute.Models.Interfaces;

namespace rainyroute.Models.Data;

class WeatherRouteBoundingBox : IBoundingBox
{
    public string Id { get; set; }
    public Tuple<double, double> MinCoordinate { get; set; } = new Tuple<double, double>(0, 0);
    public Tuple<double, double> MaxCoordinate { get; set; } = new Tuple<double, double>(0, 0);
 
    public List<WeatherForeCastHour> WeatherForeCastHours { get; set; } = new List<WeatherForeCastHour>();

    public Tuple<double, double> CenterOfBoundingBox()
    {
        throw new NotImplementedException();
    }

    public void ContainsPoint(double latidude, double longitude)
    {
        throw new NotImplementedException();
    }

    public List<IBoundingBox> DivideIntoSmallerBoxes(double boxSizeSqKm)
    {
        throw new NotImplementedException();
    }
}
