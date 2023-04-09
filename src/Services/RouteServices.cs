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
        List<WeatherRouteBoundingBox> bboxes = GetCrossingBoundingBoxesAndWeatherOfSpecificDate(geoCoordinateList, request.StartTime);

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

    private List<WeatherRouteBoundingBox> GetCrossingBoundingBoxesAndWeatherOfSpecificDate(List<GeoCoordinate> coordinates, DateTime specificDate)
    {
        var boxList = new List<WeatherRouteBoundingBox>();

        var allBoxes = _dbService.GetAllWeatherBoundingBoxes(specificDate);
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
}