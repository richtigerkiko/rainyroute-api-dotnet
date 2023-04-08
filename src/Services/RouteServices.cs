using rainyroute.Models;
using rainyroute.Models.ResponseObjects;
using rainyroute.Models.ResponseObjects.ExternalApiResponses.OSRMApi;
using PolylinerNet;
using rainyroute.Models.RequestObject;
using rainyroute.Persistance;

namespace rainyroute.Services;

public class RouteServices : BaseService
{

    RavenDbContext _dbContext;

    public RouteServices(ILogger logger, IConfiguration config, HttpClient httpClient, RavenDbContext ravenDbContext) : base(logger, config, httpClient)
    {
        _dbContext = ravenDbContext;
    }

    public async Task<NewWeatherRouteResponse> GetNewWeatherRouteResponse(RouteRequestObject request)
    {
        // Calculate Route
        var openStreetmapApiService = new OpenStreetmapApiService(_logger, _config, _httpClient);

        var calculatedRoute = await openStreetmapApiService.GetOSRMApiResult(request.CoordinatesStart, request.CoordinatesDestination);

        // get coordinates from polyline
        var geoCoordinateList = DecodeGooglePolyline(calculatedRoute.Routes[0].Geometry);

        var dbService = new DbService(_dbContext);

        var bboxes = dbService.GetCrossingBoundingBoxes(geoCoordinateList);


        return new NewWeatherRouteResponse(){
            CoordinatesStart = request.CoordinatesStart,
            CoordinatesDestination = request.CoordinatesDestination,
            PassedBoundingBoxes = bboxes,
            PolyLine = calculatedRoute.Routes[0].Geometry,
            StartTime = request.StartTime,
            ProjectedFinishTime = request.StartTime.AddSeconds(calculatedRoute.Routes[0].Duration)
        };
    }






    /// <summary>
    /// Returns the response object for the Controller calls other services
    /// </summary>
    /// <param name="start">Start Coodrinate Object</param>
    /// <param name="destination">Destination Coordinate Object</param>
    /// <param name="startTime">DateTime object when the route should start</param>
    /// <returns></returns>    
    public async Task<WeatherRouteResponse> GetWeatherRouteResponseObject(GeoCoordinate start, GeoCoordinate destination, DateTime startTime)
    {
        // init apiServices
        var openStreetmapApiService = new OpenStreetmapApiService(_logger, _config, _httpClient);
        var weatherApiService = new WeatherApiService(_logger, _config, _httpClient);

        var returnObject = new WeatherRouteResponse();

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

        // Mergin Geomatrie and Annotation Data
        var weatherPoints = MergePolylineCoordinatesWithOSRMAnnotation(osrmRouteResponse.Routes[0].Geometry, annotation);

        // lowering the resolution
        var lowResolutionWeatherPoints = LowerAnnotationResolution(weatherPoints, 10000);

        // Get weather Data from API
        lowResolutionWeatherPoints = await weatherApiService.AddWeatherToGeoWeatherList(lowResolutionWeatherPoints);

        return new WeatherRouteResponse()
        {
            CoordinatesDestination = destination,
            CoordinatesStart = start,
            StartTime = startTime,
            FinishTime = startTime.AddSeconds(osrmRouteResponse.Routes[0].Legs[0].Duration),
            WeatherRoutePoints = lowResolutionWeatherPoints,
            PolyLine = osrmRouteResponse.Routes[0].Geometry
        };
    }





    /// <summary>
    /// Lowers the resolution of the osrm response
    /// </summary>
    /// <param name="originalAnnotation"></param>
    /// <param name="resolutionMeters"></param>
    /// <returns></returns>
    private List<WeatherRoutePoint> LowerAnnotationResolution(List<WeatherRoutePoint> weatherRoutePoints, double resolutionMeters)
    {
        var resultObj = new List<WeatherRoutePoint>();

        var distanceCounter = 0.0;
        var durationCounter = 0.0;

        // iterate through all nodes
        for (int i = 0; i < weatherRoutePoints.Count; i++)
        {
            // Adding distancecounter
            distanceCounter += weatherRoutePoints[i].DistanceFromLastPoint;
            durationCounter += weatherRoutePoints[i].DurationFromLastPoint;

            // distancecounter is as big as resolutioncounter, add to returnobject, also add the last one
            if (distanceCounter >= resolutionMeters || i == weatherRoutePoints.Count - 1)
            {
                // set distances/durations to accumuleted
                weatherRoutePoints[i].DistanceFromLastPoint = distanceCounter;
                weatherRoutePoints[i].DurationFromLastPoint = durationCounter;

                // Add to return List
                resultObj.Add(weatherRoutePoints[i]);

                // reset counter
                distanceCounter = 0.0;
                durationCounter = 0.0;
            }
        }

        return resultObj;
    }

    private List<Tuple<double, double>> DecodeGooglePolyline(string polyline)
    {
        var polyliner = new Polyliner();

        var coordinateList = new List<Tuple<double, double>>();

        var decodeResult = polyliner.Decode(polyline);
        
        foreach (var point in decodeResult) coordinateList.Add(new Tuple<double, double>(point.Latitude, point.Longitude));

        return coordinateList;

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

            // first item is a special case because distance speed and duration are 0 there, those lists are also one shorter because its "between to points"
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
                geoCoordinateList[i], distance, duration, returnList.LastOrDefault()
            ));

        }

        return returnList;
    }
}