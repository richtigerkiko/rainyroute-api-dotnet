using rainyroute.Models.Interfaces;

namespace rainyroute.Models.Data;

public class WeatherBoundingBox : BoundingBox, IBoundingBox
{

    public string Id { get; set; }
    public override List<IBoundingBox> DivideIntoSmallerBoxes(double boxSizeSqKm){
        var boundingBoxes = base.DivideIntoSmallerBoxes(boxSizeSqKm);
        return boundingBoxes.Select(x => new WeatherBoundingBox{
            MaxCoordinate = x.MaxCoordinate,
            MinCoordinate = x.MinCoordinate
        }).ToList<IBoundingBox>();
    }

    public List<WeatherForeCastHour> WeatherForeCastHours { get; set; } = new List<WeatherForeCastHour>();

}
