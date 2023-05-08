using System.ComponentModel.DataAnnotations;
using NetTopologySuite.Geometries;

public class RouteRequestObject
{
    [Required]
    public Point CoordinatesStart { get; set; }
    [Required]
    public Point CoordinatesDestination { get; set; }
    public DateTime StartTime { get; set; } = DateTime.Now;

    public bool MinimizeResponse { get; set; } = false;
}