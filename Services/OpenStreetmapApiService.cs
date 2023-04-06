using Microsoft.AspNetCore.WebUtilities;
using rainyroute.Models;
using rainyroute.Models.ResponseObjects.ExternalApiResponses.OSRMApi;

namespace rainyroute.Services;

public class OpenStreetmapApiService: BaseService
{
    public OpenStreetmapApiService(ILogger logger, IConfiguration config, HttpClient httpClient) : base(logger, config, httpClient)
    {

    }

    /// <summary>
    /// Sends apicall to OSRM and calculates route http://project-osrm.org/docs/v5.5.1/api/#general-options
    /// </summary>
    /// <param name="coordinateStart"></param>
    /// <param name="coordinateDestination"></param>
    /// <returns></returns>
    public async Task<OSRMApiResult> GetOSRMApiResult(GeoCoordinate coordinateStart, GeoCoordinate coordinateDestination)
    {

        var resultObj = new OSRMApiResult();

        try
        {
            // Route Request http://project-osrm.org/docs/v5.5.1/api/#requests
            var url = $"http://router.project-osrm.org/route/v1/driving/{coordinateStart.Longitude},{coordinateStart.Latitude};{coordinateDestination.Longitude},{coordinateDestination.Latitude}";
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
        }

        return resultObj;
    }

}