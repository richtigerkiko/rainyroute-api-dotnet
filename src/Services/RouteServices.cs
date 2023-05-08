using NetTopologySuite.Geometries;
using PolylinerNet;
using rainyroute.Controllers;
using rainyroute.Models.ResponseObjects.ExternalApiResponses.OSRMApi;
using rainyroute.Persistance;
using rainyroute.Persistance.Models;

namespace rainyroute.Services;

public class RouteServices
{
    private readonly ILogger<WeatherRouteController> _logger;
    private readonly IConfiguration _config;
    private readonly RainyrouteContext _dbContext;
    private readonly HttpClient _httpClient;

    public RouteServices(ILogger<WeatherRouteController> logger, IConfiguration config, HttpClient httpClient, RainyrouteContext dbContext)
    {
        _logger = logger;
        _config = config;
        _httpClient = httpClient;
        _dbContext = dbContext;
    }

    /// <summary>
    /// This function generates a response Object for the controller. 
    /// It asks the OSRM api for a route calculation then gets all weather objects from the database
    /// </summary>
    /// <param name="routeRequestObject"></param>
    /// <returns></returns>
    public async Task<WeatherRouteResponse> GetWeatherRouteResponse(RouteRequestObject routeRequestObject, RouteRequestMode mode)
    {

        // Calculate Route
        var calculatedRoute = await CallOSRMApi(routeRequestObject);

        // get coordinates from polyline
        var geoCoordinateList = DecodeGooglePolyline(calculatedRoute.Routes[0].Geometry);

        // Get crossing Boundingboxes from db
        List<WeatherRouteBoundingBox> bboxes = GetCrossingBoundingBoxes(geoCoordinateList);

        // Add Timings
        bboxes = AddTimingsToBoundingBoxList(routeRequestObject, calculatedRoute, geoCoordinateList, bboxes);

        // Change starttime depending on mode
        switch (mode)
        {
            case RouteRequestMode.MostRain:
                bboxes = FindBestStartTime(bboxes, x => x.WeatherForecastAtDuration.WillItRain);
                break;
            case RouteRequestMode.MostSun:
                bboxes = FindBestStartTime(bboxes, x => x.WeatherForecastAtDuration.IsSunny);
                break;
            case RouteRequestMode.MostTailwind:
                bboxes = FindBestStartTime(bboxes, x => x.HasTailwind);
                break;
            case RouteRequestMode.Normal:
            default:
                break;
        }

        var returnObject = new WeatherRouteResponse()
        {
            CoordinatesStart = routeRequestObject.CoordinatesStart,
            CoordinatesDestination = routeRequestObject.CoordinatesDestination,
            PassedBoundingBoxes = bboxes,
            PolyLine = calculatedRoute.Routes[0].Geometry,
            StartTime = bboxes.FirstOrDefault().TimeClosestToCenter,
            ProjectedFinishTime = bboxes.LastOrDefault().TimeClosestToCenter
        };

        return returnObject;
    }

    public FullWeatherMapResponse GetFullWeatherMap(int day, int hour)
    {
        var dbService = new DbService(_dbContext);
        var fullWeatherMap = dbService.GetFullWeatherMap(day, hour);

        var bboxList = new List<WeatherRouteBoundingBox>();
        fullWeatherMap.ForEach(x => bboxList.Add(new WeatherRouteBoundingBox(x)));

        return new FullWeatherMapResponse()
        {
            Day = day,
            Hour = hour,
            FullWeatherMap = bboxList
        };
    }


    /// <summary>
    /// This function finds the best starting time for the given route (bboxes) with the given Weather condition
    /// </summary>
    /// <param name="bboxes"></param>
    /// <param name="condition"></param>
    /// <returns></returns>
    private List<WeatherRouteBoundingBox> FindBestStartTime(List<WeatherRouteBoundingBox> bboxes, Func<WeatherRouteBoundingBox, bool> condition)
    {
        // Cast bboxes to a new list, so, that if nothing is found, the original is getting returned
        var boxList = bboxes.Select(x => new WeatherRouteBoundingBox(x)).ToList();

        // get max possbible iterations given the travel time
        var maxIterations = GetMaxiterations(bboxes);
        var count = 0;

        // Iterate all routes and give the one back with most given conditions
        for (int i = 0; i < maxIterations; i++)
        {
            // count it
            var newCount = bboxes.Count(x => condition(x));

            if (newCount > count)
            {
                count = newCount;

                // I want to be 100% if the bounding box has a current weatherforecast at the given time so this is an extra check
                if (!bboxes.Any(x => x.WeatherForecastAtDurationIndex == -1))
                {
                    // cast it on the return object
                    boxList = bboxes.Select(x => new WeatherRouteBoundingBox(x)).ToList();
                }
            }

            // change the starttime of the bounding boxes by 1, that also changes the current weather to the specific time
            bboxes = ChangeStartTime(bboxes, 1);
        }

        return boxList;
    }


    private List<WeatherRouteBoundingBox> ChangeStartTime(List<WeatherRouteBoundingBox> allboxes, int hours)
    {
        foreach (var bbox in allboxes)
        {
            bbox.TimeClosestToCenter = bbox.TimeClosestToCenter.AddHours(hours);
        }
        return allboxes;
    }

    private async Task<OSRMApiResult> CallOSRMApi(RouteRequestObject request)
    {
        var openStreetmapApiService = new OpenStreetmapApiService(_logger, _config, _httpClient, _dbContext);

        var calculatedRoute = await openStreetmapApiService.GetOSRMApiResult(request.CoordinatesStart, request.CoordinatesDestination);
        return calculatedRoute;
    }

    private List<Point> DecodeGooglePolyline(string polyLine)
    {
        var polyliner = new Polyliner();
        var returnList = new List<Point>();

        var result = polyliner.Decode(polyLine);

        foreach (var polylinePoint in result)
        {
            returnList.Add(new Point(
                polylinePoint.Latitude, polylinePoint.Longitude
            ));
        }

        return returnList;
    }

    /// <summary>
    /// This function returns all crossing bounding boxes from the database.
    /// </summary>
    /// <param name="coordinates"></param>
    /// <returns></returns>
    private List<WeatherRouteBoundingBox> GetCrossingBoundingBoxes(List<Point> coordinates)
    {
        var dbService = new DbService(_dbContext);

        var crossedBoundingBoxes = dbService.GetWeatherBoundingBoxesOfCoordinates(coordinates);

        var weatherRouteBoundingBoxes = new List<WeatherRouteBoundingBox>();

        // sadly GetWeatherBoundingBoxesOfCoordinates returns too many bounding boxes so I check again if there are bounding boxes without coordinates of the route
        foreach (var crossedBox in crossedBoundingBoxes)
        {
            var coordinatesInBox = coordinates.Where(x => crossedBox.BoundingBox.Contains(x.Coordinate)).ToList();
            if (coordinatesInBox.Count != 0)
            {
                // I generate a complete WeatherRouteBoundingBox Object here because the simple Weatherboundingbox is never needed again.
                weatherRouteBoundingBoxes.Add(new WeatherRouteBoundingBox(crossedBox, coordinatesInBox));
            }
        }
        return weatherRouteBoundingBoxes;
    }

    /// <summary>
    /// This function adds the timings to the Boundingboxlist given the route Object
    /// </summary>
    /// <param name="routeRequestObject"></param>
    /// <param name="calculatedRoute"></param>
    /// <param name="geoCoordinateList"></param>
    /// <param name="bboxes"></param>
    /// <returns></returns>
    private List<WeatherRouteBoundingBox> AddTimingsToBoundingBoxList(RouteRequestObject routeRequestObject, OSRMApiResult calculatedRoute, List<Point> geoCoordinateList, List<WeatherRouteBoundingBox> bboxes)
    {
        foreach (var box in bboxes)
        {
            var index = geoCoordinateList.IndexOf(box.CoordinateClostestToCenter);
            box.TotalDurationClosestToCenter = calculatedRoute.Routes[0].Legs[0].Annotation.Duration.GetRange(0, index).Sum();
            box.TimeClosestToCenter = routeRequestObject.StartTime.AddSeconds(box.TotalDurationClosestToCenter);
        }

        return bboxes.OrderBy(x => x.TotalDurationClosestToCenter).ToList();
    }

    /// <summary>
    /// this function calculates the max iterations we can iterate over all boxes, until the time remaining for the route is not enough to travel the complete route from start to finish.
    /// </summary>
    /// <param name="allboxes"></param>
    /// <returns></returns>
    private int GetMaxiterations(List<WeatherRouteBoundingBox> allboxes)
    {
        var earliestTime = allboxes.First().TimeClosestToCenter;
        var earliestTImeInList = allboxes.First().WeatherForeCastHours.First().Time;

        var hourDifference = (earliestTime - earliestTImeInList).Hours;

        var durationOfRouteInSeconds = allboxes.Max(x => x.TotalDurationClosestToCenter);

        var durationHours = durationOfRouteInSeconds / 60 / 60;

        var maxIterations = allboxes.First().WeatherForeCastHours.Count - (int)Math.Ceiling(durationHours) - hourDifference;
        return maxIterations;
    }

}