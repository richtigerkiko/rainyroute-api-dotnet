
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace rainyroute.Persistance.Models;

public class WeatherBoundingBox
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    public Point MinCoordinate { get; set; }

    [Required]
    public Point MaxCoordinate { get; set; }

    public List<WeatherForeCastHour> WeatherForeCastHours { get; set; } = new List<WeatherForeCastHour>();

    [JsonIgnore]
    public Envelope BoundingBox => new Envelope(new Coordinate(MinCoordinate.X, MinCoordinate.Y), new Coordinate(MaxCoordinate.X, MaxCoordinate.Y));

    public List<WeatherBoundingBox> DivideEnvelope(double subdivisionSizeKm)
    {
        var BoundingBox = new Envelope(new Coordinate(MinCoordinate.X, MinCoordinate.Y), new Coordinate(MaxCoordinate.X, MaxCoordinate.Y));
        const double EARTH_RADIUS_KM = 6371.0;

        double centroidY = (BoundingBox.MaxY + BoundingBox.MinY) / 2.0;

        double size = subdivisionSizeKm / EARTH_RADIUS_KM;

        int rows = (int)Math.Ceiling(BoundingBox.Height / size);
        int columns = (int)Math.Ceiling(BoundingBox.Width / size);

        List<WeatherBoundingBox> smallerEnvelopes = new List<WeatherBoundingBox>();

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                double xmin = BoundingBox.MinX + (j * size);
                double ymin = BoundingBox.MinY + (i * size);
                double xmax = xmin + size;
                double ymax = ymin + size;

                Envelope envelope = new Envelope(xmin, xmax, ymin, ymax);
                smallerEnvelopes.Add(WeatherBoundingBoxFromEnvelope(envelope));
            }
        }

        return smallerEnvelopes;
    }

    public WeatherBoundingBox WeatherBoundingBoxFromEnvelope(Envelope envelope)
    {
        return new WeatherBoundingBox()
        {
            MinCoordinate = new Point(envelope.MinX, envelope.MinY),
            MaxCoordinate = new Point(envelope.MaxX, envelope.MaxY)
        };
    }

}