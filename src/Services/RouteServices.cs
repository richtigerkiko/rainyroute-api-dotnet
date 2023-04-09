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

    public RouteServices(ILogger logger, IConfiguration config, HttpClient httpClient, RavenDbContext ravenDbContext) : base(logger, config, httpClient)
    {
        _dbContext = ravenDbContext;
    }

    public async Task<NewWeatherRouteResponse> GetNewWeatherRouteResponse(RouteRequestObject request)
    {
        // Calculate Route
        OSRMApiResult calculatedRoute = await CallOSRMApi(request);

        // get coordinates from polyline
        var geoCoordinateList = DecodeGooglePolyline(calculatedRoute.Routes[0].Geometry);

        // Get all Boundingboxes from db
        List<WeatherRouteBoundingBox> bboxes = GetBoundingBoxesFromDb(geoCoordinateList);

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

    private void AddTimingsToBoundingBoxList(RouteRequestObject request, OSRMApiResult calculatedRoute, List<GeoCoordinate> geoCoordinateList, List<WeatherRouteBoundingBox> bboxes)
    {
        foreach (var box in bboxes)
        {
            var index = geoCoordinateList.IndexOf(box.CoordinateClostestToCenter);
            box.TotalDurationClosestToCenter = calculatedRoute.Routes[0].Legs[0].Annotation.Duration.GetRange(0, index).Sum();
            box.TimeClosestToCenter = request.StartTime.AddSeconds(box.TotalDurationClosestToCenter);
        }
    }

    private List<WeatherRouteBoundingBox> GetBoundingBoxesFromDb(List<GeoCoordinate> geoCoordinateList)
    {
        var dbService = new DbService(_dbContext);

        var bboxes = dbService.GetCrossingBoundingBoxes(geoCoordinateList);
        return bboxes;
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

        return dbService.GetFullWeatherMap();
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