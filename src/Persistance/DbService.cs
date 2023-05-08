using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using rainyroute.Models.ResponseObjects.ExternalApiResponses.WeatherAPI;
using rainyroute.Persistance.Models;

namespace rainyroute.Persistance;

public class DbService
{
    private RainyrouteContext _dbContext;

    public DbService(RainyrouteContext dbContext)
    {
        _dbContext = dbContext;
    }

    public List<WeatherBoundingBox> GetAllWeatherBoundingBoxes()
    {

        var allBoxes = _dbContext.WeatherBoundingBoxes.ToList();

        return allBoxes;

    }

    public List<WeatherBoundingBox> GetWeatherBoundingBoxesOfCoordinates(List<Point> points)
    {
        var coordList = points.Select(x => x.Coordinate).ToList();
        var envelope = new Envelope(coordList);
        var envelopeGeometry = GeometryFactory.Default.CreatePolygon(
            new Coordinate[] {
                new Coordinate(envelope.MinX, envelope.MinY),
                new Coordinate(envelope.MaxX, envelope.MinY),
                new Coordinate(envelope.MaxX, envelope.MaxY),
                new Coordinate(envelope.MinX, envelope.MaxY),
                new Coordinate(envelope.MinX, envelope.MinY)
            });

        var crossedBoxes = _dbContext.WeatherBoundingBoxes
            .Where(x => envelopeGeometry.Contains(x.MinCoordinate) || envelopeGeometry.Contains(x.MaxCoordinate))
            .Include(x => x.WeatherForeCastHours)
            .ToList();

        return crossedBoxes;

    }

    public void SetWeatherBoundingBoxeWeather(WeatherBoundingBox box, Forecastday day)
    {

        try
        {
            foreach (var hour in day.Hour)
            {
                var WeatherForeCastHour = new WeatherForeCastHour(hour);
                box.WeatherForeCastHours.Add(WeatherForeCastHour);
            }
            _dbContext.Update(box);
            _dbContext.SaveChanges();
        }
        catch (Exception ex)
        {

        }

    }

    public List<WeatherBoundingBox> GetFullWeatherMap(int day, int hour)
    {
        var allboxes = _dbContext.WeatherBoundingBoxes.Include(x => x.WeatherForeCastHours.
                                                            Where(y => y.Time.Day == day).Where(y => y.Time.Hour == hour))
                                                      .ToList();

        return allboxes;
    }
}