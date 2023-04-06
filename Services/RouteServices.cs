using rainyroute.Models;
using rainyroute.Models.ResponseObjects;

namespace rainyroute.Services;

public class RouteServices : BaseService
{
    public RouteServices(ILogger logger, IConfiguration config, HttpClient httpClient) : base(logger, config, httpClient)
    {
    }


    /// <summary>
    /// Returns the response object for the Controller calls other services
    /// </summary>
    /// <param name="start">Start Coodrinate Object</param>
    /// <param name="destination">Destination Coordinate Object</param>
    /// <param name="startTime">DateTime object when the route should start</param>
    /// <returns></returns>    
    public async Task<WeatherRouteResponse> GetWeatherRouteResponseObject(GeoCoordinate start, GeoCoordinate destination, DateTime? startTime)
    {
        // init apiServices
        var openStreetmapApiService = new OpenStreetmapApiService(_logger, _config, _httpClient);
        var weatherApiService = new WeatherApiService(_logger, _config, _httpClient);


        // getting OSRM route 
        _logger.LogDebug("Getting OSRM Route");
        var osrmRouteResponse = await openStreetmapApiService.GetOSRMApiResult(start, destination);

        if (osrmRouteResponse.Routes.Count == 0)
        {
            _logger.LogInformation("No Route Found", (start, destination));
            return new WeatherRouteResponse(start, destination);
        }



        throw new NotImplementedException();
    }
}