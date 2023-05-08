using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Newtonsoft.Json;
using rainyroute.Persistance;
using rainyroute.Services;

namespace rainyroute.Controllers;

[ApiController]
[Route("v1/[controller]")]
public class WeatherRouteController : ControllerBase
{
    private readonly ILogger<WeatherRouteController> _logger;
    private readonly IConfiguration _config;

    private readonly RainyrouteContext _dbContext;

    private readonly HttpClient _httpClient;

    public WeatherRouteController(ILogger<WeatherRouteController> logger, IConfiguration config, HttpClient httpClient, RainyrouteContext dbContext)
    {
        _logger = logger;
        _config = config;
        _httpClient = httpClient;
        _dbContext = dbContext;
    }

    [HttpPost("GetWeatherRoute")]
    public async Task<IActionResult> GetWeatherRoute([FromBody] RouteRequestObject routeRequestObject, [FromQuery] RouteRequestMode mode)
    {
        var routeService = new RouteServices(_logger, _config, _httpClient, _dbContext);

        return new JsonResult( await routeService.GetWeatherRouteResponse(routeRequestObject, mode));
    }

    [HttpGet("GetFullMap")]
    public async Task<IActionResult> GetFullMap([FromQuery] int day, [FromQuery] int hour)
    {
        var routeService = new RouteServices(_logger, _config, _httpClient, _dbContext);

        return new JsonResult( routeService.GetFullWeatherMap(day, hour));
    }
}