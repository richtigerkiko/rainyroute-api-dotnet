// Iplementation like old net 4 Geocoordinate: https://learn.microsoft.com/en-us/dotnet/api/system.device.location.geocoordinate?view=netframework-4.8
namespace rainyroute.Models;

public class GeoCoordinates
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }

}