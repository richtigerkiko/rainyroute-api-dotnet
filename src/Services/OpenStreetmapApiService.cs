using Microsoft.AspNetCore.WebUtilities;
using NetTopologySuite.Geometries;
using rainyroute.Controllers;
using rainyroute.Models;
using rainyroute.Models.ResponseObjects.ExternalApiResponses.OSRMApi;
using rainyroute.Persistance;

namespace rainyroute.Services;

public class OpenStreetmapApiService
{
    private readonly ILogger<WeatherRouteController> _logger;
    private readonly IConfiguration _config;

    private readonly RainyrouteContext _dbContext;

    private readonly HttpClient _httpClient;

    public OpenStreetmapApiService(ILogger<WeatherRouteController> logger, IConfiguration config, HttpClient httpClient, RainyrouteContext dbContext)
    {
        _logger = logger;
        _config = config;
        _httpClient = httpClient;
        _dbContext = dbContext;
    }

    /// <summary>
    /// Sends apicall to OSRM and calculates route http://project-osrm.org/docs/v5.5.1/api/#general-options
    /// </summary>
    /// <param name="coordinateStart"></param>
    /// <param name="coordinateDestination"></param>
    /// <returns></returns>
    public async Task<OSRMApiResult> GetOSRMApiResult(Point coordinateStart, Point coordinateDestination)
    {

        var resultObj = new OSRMApiResult();

        try
        {
            // Route Request http://project-osrm.org/docs/v5.5.1/api/#requests
            var url = $"http://router.project-osrm.org/route/v1/driving/{coordinateStart.Y},{coordinateStart.X};{coordinateDestination.Y},{coordinateDestination.X}";
            var queryParams = new Dictionary<string, string?>(){
                {"annotations", "true"},
                {"steps", "false"},
                {"overview", "full"}
            };

            var urlWithQuery = QueryHelpers.AddQueryString(url, queryParams);

            _logger.LogDebug("Genrated urlQuery to get OSRMApiresult", urlWithQuery);

            using (var response = await _httpClient.GetAsync(urlWithQuery))
            {

                resultObj = await response.Content.ReadFromJsonAsync<OSRMApiResult>() ?? new OSRMApiResult();

                _logger.LogDebug("Json parse was successfull", resultObj);

            }
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Error getting OSRMAPI Result");
            throw new HttpRequestException("couldn't get OSRMAPI Result");
        }

        return resultObj;
    }

}