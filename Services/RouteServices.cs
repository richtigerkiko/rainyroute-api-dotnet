using rainyroute.Models;
using rainyroute.Models.ResponseObjects;
using rainyroute.Models.ResponseObjects.ExternalApiResponses.OSRMApi;

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

        // We expect only one Route and only one leg, so we can ignore everything else and simplyify the variable
        var annotation = osrmRouteResponse.Routes[0].Legs[0].Annotation;


        throw new NotImplementedException();
    }


    /// <summary>
    /// Lowers the resolution of the osrm response
    /// </summary>
    /// <param name="originalAnnotation"></param>
    /// <param name="resolutionMeters"></param>
    /// <returns></returns>
    private Annotation LowerAnnotationResolution(Annotation originalAnnotation, double resolutionMeters)
    {
        var resultObj = new Annotation();
        resultObj.Metadata = originalAnnotation.Metadata;

        // clear distance, duration and node List and leave first node
        resultObj.Distance = new List<double>();
        resultObj.Duration = new List<double>();
        resultObj.Nodes = new List<object>() { originalAnnotation.Nodes[0] };

        var distanceCounter = 0.0;
        var durationCounter = 0.0;

        // add 0 for first
        resultObj.Distance.Add(0);
        resultObj.Duration.Add(0);

        // iterate through all nodes
        for (int i = 0; i < originalAnnotation.Nodes.Count - 1; i++)
        {
            // Adding distancecounter
            distanceCounter += originalAnnotation.Distance[i];
            durationCounter += originalAnnotation.Duration[i];

            // distancecounter is as big as resolutioncounter, add to returnobject
            if (distanceCounter >= resolutionMeters && i >= originalAnnotation.Nodes.Count - 2)
            {
                resultObj.Distance.Add(distanceCounter);
                resultObj.Duration.Add(durationCounter);
                resultObj.Nodes.Add(originalAnnotation.Nodes[i]);

                distanceCounter = 0.0;
                durationCounter = 0.0;
            }
        }

        return resultObj;
    }
}