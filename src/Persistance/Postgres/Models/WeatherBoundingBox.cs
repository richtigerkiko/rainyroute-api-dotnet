using System.ComponentModel.DataAnnotations;
using rainyroute.Models;
using rainyroute.Models.Interfaces;

namespace rainyroute.Persistance.Postgres.Models;

public class WeatherBoundingBox : IBoundingBox
{

    public string Id { get; set; } = Guid.NewGuid().ToString(); 
    
    public List<WeatherForeCastHour> WeatherForeCastHours { get; set; }
    public GeoCoordinate MinCoordinate { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public GeoCoordinate MaxCoordinate { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public GeoCoordinate CenterOfBoundingBox => throw new NotImplementedException();

    public bool Contains(double latidude, double longitude)
    {
        throw new NotImplementedException();
    }

    public override List<IBoundingBox> DivideIntoSmallerBoxes(double boxSizeSqKm){
        var boundingBoxes = base.DivideIntoSmallerBoxes(boxSizeSqKm);
        return boundingBoxes.Select(x => new WeatherBoundingBox{
            MaxCoordinate = x.MaxCoordinate,
            MinCoordinate = x.MinCoordinate
        }).ToList<IBoundingBox>();
    }

    public double GetDistanceTo(IBoundingBox other)
    {
        throw new NotImplementedException();
    }

    public string ToShapeWkt()
    {
        throw new NotImplementedException();
    }
}
