using Microsoft.AspNetCore.Mvc;
using rainyroute.Models.RequestObject;
using rainyroute.Services;

namespace rainyroute.Controllers;

[ApiController]
[Route("v1/[controller]")]
public class WeatherRouteController : ControllerBase
{
    private readonly ILogger<WeatherRouteController> _logger;
    private readonly IConfiguration _config;

    private readonly HttpClient httpClient;

    public WeatherRouteController(ILogger<WeatherRouteController> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;

        // Initialize new httpClient for all child services to use
        httpClient = new HttpClient(new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(2)
        });
    }

    [HttpPost("GetWeatherRoute")]
    public async Task<IActionResult> GetWeatherRoute([FromBody] RouteRequestObject routeRequestObject)
    {
        var routeService = new RouteServices(_logger, _config, httpClient);

        var generatedResponse = await routeService.GetWeatherRouteResponseObject(routeRequestObject.CoordinatesStart, routeRequestObject.CoordinatesDestination, routeRequestObject.StartTime);
        return new JsonResult(generatedResponse);
    }

}