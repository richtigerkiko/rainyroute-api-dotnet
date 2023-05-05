using Microsoft.AspNetCore.Mvc;
using rainyroute.Models.RequestObject;
using rainyroute.Persistance;
using rainyroute.Services;

namespace rainyroute.Controllers;

[ApiController]
[Route("v1/[controller]")]
public class WeatherRouteController : ControllerBase
{
    private readonly ILogger<WeatherRouteController> _logger;
    private readonly IConfiguration _config;

    private static RavenDbContext _ravenDbContext;

    private readonly HttpClient httpClient;

    public WeatherRouteController(ILogger<WeatherRouteController> logger, IConfiguration config, RavenDbContext ravenDbContext)
    {
        _logger = logger;
        _config = config;

        _ravenDbContext = ravenDbContext;
        
        // Initialize new httpClient for all child services to use
        httpClient = new HttpClient(new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(2)
        });
    }


    [HttpPost("GetNewWeatherRoute")]
    public async Task<IActionResult> GetWeatherRouteWithDb([FromBody] RouteRequestObject routeRequestObject)
    {
        var routeService = new RouteServices(_logger, _config, httpClient, _ravenDbContext);


        return new JsonResult( await routeService.GetWeatherRouteResponseSingleDayWeather(routeRequestObject));
    }

    [HttpPost("GetRouteWhenMostRain")]
    public async Task<IActionResult> GetRouteWhenMostRain([FromBody] RouteRequestObject routeRequestObject)
    {
        var routeService = new RouteServices(_logger, _config, httpClient, _ravenDbContext);
        return new JsonResult( await routeService.GetWeatherRouteResponseWithMostRain(routeRequestObject));
    }

    [HttpGet("GetFullWeatherMap")]
    public IActionResult GetFullWeatherMap()
    {
        var routeService = new RouteServices(_logger, _config, httpClient, _ravenDbContext);

        return new JsonResult( routeService.GetFullWeatherMapResponse());
    }


}