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
        var openStreetmapApiService = new OpenStreetmapApiService(_logger, _config, _httpClient);

        var calculatedRoute = await openStreetmapApiService.GetOSRMApiResult(request.CoordinatesStart, request.CoordinatesDestination);

        // get coordinates from polyline
        var geoCoordinateList = DecodeGooglePolyline(calculatedRoute.Routes[0].Geometry);

        var dbService = new DbService(_dbContext);

        var bboxes = dbService.GetCrossingBoundingBoxes(geoCoordinateList);

        
        foreach(var box in bboxes){
            var index = geoCoordinateList.IndexOf(box.CoordinateClostestToCenter);
            box.TotalDurationClosestToCenter = calculatedRoute.Routes[0].Legs[0].Annotation.Duration.GetRange(0, index).Sum();
            box.TimeClosestToCenter = request.StartTime.AddSeconds(box.TotalDurationClosestToCenter);

        }

        return new NewWeatherRouteResponse(){
            CoordinatesStart = request.CoordinatesStart,
            CoordinatesDestination = request.CoordinatesDestination,
            PassedBoundingBoxes = bboxes,
            PolyLine = calculatedRoute.Routes[0].Geometry,
            StartTime = request.StartTime,
            ProjectedFinishTime = request.StartTime.AddSeconds(calculatedRoute.Routes[0].Duration)
        };
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