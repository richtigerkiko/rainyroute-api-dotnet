using System.ComponentModel.DataAnnotations;
using rainyroute.Models;

namespace rainyroute.Models.RequestObject;


public class RouteRequestObject
{
    [Required]
    public GeoCoordinate CoordinatesStart { get; set; }

    [Required]
    public GeoCoordinate CoordinatesDestination { get; set; }

    public DateTime StartTime { get; set; } = DateTime.Now;
}