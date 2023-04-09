using rainyroute.Models;
using rainyroute.Models.ResponseObjects;
using rainyroute.Models.ResponseObjects.ExternalApiResponses.OSRMApi;
using PolylinerNet;
using rainyroute.Models.RequestObject;
using rainyroute.Persistance;
using rainyroute.Models.Data;

namespace rainyroute.Services;

public class RouteServices : BaseService
{

    RavenDbContext _dbContext;
    DbService _dbService;

    public RouteServices(ILogger logger, IConfiguration config, HttpClient httpClient, RavenDbContext ravenDbContext) : base(logger, config, httpClient)
    {
        _dbContext = ravenDbContext;
        _dbService = new DbService(_dbContext);
    }

    public async Task<NewWeatherRouteResponse> GetWeatherRouteResponseSingleDayWeather(RouteRequestObject request)
    {
        // Calculate Route
        OSRMApiResult calculatedRoute = await CallOSRMApi(request);

        // get coordinates from polyline
        var geoCoordinateList = DecodeGooglePolyline(calculatedRoute.Routes[0].Geometry);

        // Get all Boundingboxes from db
        List<WeatherRouteBoundingBox> bboxes = GetCrossingBoundingBoxes(geoCoordinateList, request.StartTime);

        // Zeiten zu den Bounding Boxes hinzufügen
        AddTimingsToBoundingBoxList(request, calculatedRoute, geoCoordinateList, bboxes);

        return new NewWeatherRouteResponse()
        {
            CoordinatesStart = request.CoordinatesStart,
            CoordinatesDestination = request.CoordinatesDestination,
            PassedBoundingBoxes = bboxes,
            PolyLine = calculatedRoute.Routes[0].Geometry,
            StartTime = request.StartTime,
            ProjectedFinishTime = request.StartTime.AddSeconds(calculatedRoute.Routes[0].Duration)
        };
    }

    public async Task<NewWeatherRouteResponse> GetWeatherRouteResponseWithMostRain(RouteRequestObject request)
    {
        // Calculate Route
        OSRMApiResult calculatedRoute = await CallOSRMApi(request);

        // get coordinates from polyline
        var geoCoordinateList = DecodeGooglePolyline(calculatedRoute.Routes[0].Geometry);

        // Get all Boundingboxes from db
        List<WeatherRouteBoundingBox> bboxes = GetCrossingBoundingBoxes(geoCoordinateList);

        // Zeiten zu den Bounding Boxes hinzufügen
        AddTimingsToBoundingBoxList(request, calculatedRoute, geoCoordinateList, bboxes);

        FindBestStartTimeForWeather(bboxes);

        if(request.MinimizeResponse) MinimizeResponseObject(bboxes);

        return new NewWeatherRouteResponse()
        {
            CoordinatesStart = request.CoordinatesStart,
            CoordinatesDestination = request.CoordinatesDestination,
            PassedBoundingBoxes = bboxes,
            PolyLine = calculatedRoute.Routes[0].Geometry,
            StartTime = bboxes.First().TimeClosestToCenter,
            ProjectedFinishTime = bboxes.First().TimeClosestToCenter.AddSeconds(calculatedRoute.Routes[0].Duration)
        };
    }

    private void MinimizeResponseObject(List<WeatherRouteBoundingBox> bboxes)
    {
        foreach (var box in bboxes)
        {
            // Minimize WeatherObjects
            var index = box.WeatherForecastAtDurationIndex;
            if (index >= 0 && index < box.WeatherForeCastHours.Count)
            {
                box.WeatherForecastAtDuration = box.WeatherForeCastHours[index];
            }
            box.WeatherForeCastHours.Clear();

            // Minimize coordinates
            box.CoordinatesInBoundingBox.Clear();
        }
    }

    private List<WeatherRouteBoundingBox> GetCrossingBoundingBoxes(List<GeoCoordinate> coordinates, DateTime? specificDate = null)
    {
        var boxList = new List<WeatherRouteBoundingBox>();

        var allBoxes = specificDate.HasValue ? _dbService.GetAllWeatherBoundingBoxes(specificDate.Value) : _dbService.GetAllWeatherBoundingBoxes();
        foreach (var coordinate in coordinates)
        {
            var box = allBoxes.Where(x => x.ContainsPoint(coordinate)).FirstOrDefault();

            if (box != null && !(boxList.Any(x => x.Id == box.Id)))
            {
                boxList.Add(new WeatherRouteBoundingBox(box));
            }

            boxList.Last().CoordinatesInBoundingBox.Add(coordinate);
        }
        return boxList;
    }

    private void FindBestStartTimeForWeather(List<WeatherRouteBoundingBox> allboxes)
    {
        var boxList = allboxes.ToList();

        // check if there is any rain in the next 5 Days
        if (allboxes.Any(x => x.WeatherForeCastHours.Any(y => y.WillItRain)))
        {
            int maxIterations = GetMaxiterations(allboxes);
            // count raintiles and copy it to boxlist if there is a more rainy one
            var count = 0;
            for (int i = 0; i < maxIterations; i++)
            {
                var newCount = allboxes.Count(x => x.WeatherForecastAtDuration.WillItRain);
                if (newCount > count)
                {
                    count = newCount;
                    boxList = allboxes.ToList();
                }
                ChangeStartTime(allboxes, 1);
            }
        }

        allboxes = boxList;
    }

    private int GetMaxiterations(List<WeatherRouteBoundingBox> allboxes)
    {
        var earliestTime = allboxes.First().TimeClosestToCenter;
        var earliestTImeInList = allboxes.First().WeatherForeCastHours.First().Time;

        var hourDifference = (earliestTime - earliestTImeInList).Hours;

        var durationOfRouteInSeconds = allboxes.Last().TotalDurationClosestToCenter;

        var durationHours = durationOfRouteInSeconds / 60 / 60;

        var maxIterations = allboxes.First().WeatherForeCastHours.Count - (int)Math.Ceiling(durationHours) - hourDifference;
        return maxIterations;
    }

    private void AddTimingsToBoundingBoxList(RouteRequestObject request, OSRMApiResult calculatedRoute, List<GeoCoordinate> geoCoordinateList, List<WeatherRouteBoundingBox> bboxes)
    {
        foreach (var box in bboxes)
        {
            var index = geoCoordinateList.IndexOf(box.CoordinateClostestToCenter);
            box.TotalDurationClosestToCenter = calculatedRoute.Routes[0].Legs[0].Annotation.Duration.GetRange(0, index).Sum();
            box.TimeClosestToCenter = request.StartTime.AddSeconds(box.TotalDurationClosestToCenter);
        }
    }

    private async Task<OSRMApiResult> CallOSRMApi(RouteRequestObject request)
    {
        var openStreetmapApiService = new OpenStreetmapApiService(_logger, _config, _httpClient);

        var calculatedRoute = await openStreetmapApiService.GetOSRMApiResult(request.CoordinatesStart, request.CoordinatesDestination);
        return calculatedRoute;
    }

    public List<WeatherBoundingBox> GetFullWeatherMapResponse()
    {
        var dbService = new DbService(_dbContext);

        return dbService.GetAllWeatherBoundingBoxes();
    }

    private List<GeoCoordinate> DecodeGooglePolyline(string polyLine)
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

    public void ChangeStartTime(List<WeatherRouteBoundingBox> bboxes, int hours)
    {
        foreach (var bbox in bboxes)
        {
            bbox.TimeClosestToCenter = bbox.TimeClosestToCenter.AddHours(hours);
        }
    }
}