using rainyroute.Models;
using rainyroute.Models.ResponseObjects;
using rainyroute.Models.ResponseObjects.ExternalApiResponses.OSRMApi;
using PolylinerNet;

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

        // getting coordinates from Polyline
        var geoCoordinatesFromPolyLine = GetCoordinatesFromPolyline(osrmRouteResponse.Routes[0].Geometry);

        // 
        var weather = MergePolylineCoordinatesWithOSRMAnnotation(osrmRouteResponse.Routes[0].Geometry, annotation);


        // lowering the resolution
        var lowResolutionAnnotation = LowerAnnotationResolution(annotation, 10000);


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
        for (int i = 0; i < originalAnnotation.Distance.Count - 1; i++)
        {
            // Adding distancecounter
            distanceCounter += originalAnnotation.Distance[i];
            durationCounter += originalAnnotation.Duration[i];

            // distancecounter is as big as resolutioncounter, add to returnobject
            if (distanceCounter >= resolutionMeters)
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

    private List<GeoCoordinate> GetCoordinatesFromPolyline(string polyLine)
    {
        var polyliner = new Polyliner();
        var returnList = new List<GeoCoordinate>();

        var result = polyliner.Decode(polyLine);

        foreach (var polylinePoint in result)
        {
            returnList.Add(new GeoCoordinate(
                polylinePoint.Latitude, polylinePoint.Longitude
            ));
        }

        return returnList;
    }

    private List<WeatherRoutePoint> MergePolylineCoordinatesWithOSRMAnnotation(string polyLine, Annotation osrmAnnotation)
    {
        var geoCoordinateList = GetCoordinatesFromPolyline(polyLine);

        var returnList = new List<WeatherRoutePoint>();

        // we have to iterate on all geocoordinates and get the corresponding annotation Distance, Speed and Duration properties
        for (int i = 0; i < geoCoordinateList.Count; i++)
        {
            var distance = 0.0;
            var duration = 0.0;
            geoCoordinateList[i].Speed = 0.0;

            // first item is a special case because distance speed and duration are 0 there, those lists are also one shorter because of that
            if (i != 0)
            {
                geoCoordinateList[i].Speed = osrmAnnotation.Speed[i - 1];
                distance = osrmAnnotation.Distance[i - 1];
                duration = osrmAnnotation.Duration[i - 1];
            }

            // if its not the last item, set orientation to next coordinate
            if (i < geoCoordinateList.Count - 1)
            {
                geoCoordinateList[i].SetCourse(geoCoordinateList[i + 1]);
            }

            returnList.Add(new WeatherRoutePoint(
                geoCoordinateList[i], distance, duration, null
            ));

        }

        return returnList;
    }
}