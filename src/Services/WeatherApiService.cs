using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using rainyroute.Models.ResponseObjects.ExternalApiResponses.WeatherAPI;
using rainyroute.Persistance.Models;

namespace rainyroute.Services;

public class WeatherApiService
{
    private IConfiguration _config;
    private HttpClient _httpClient;

    public WeatherApiService(IConfiguration config, HttpClient httpClient)
    {
        _config = config;
        _httpClient = httpClient;
    }

    public async Task<WeatherApiBulkResponse> GetWeatherApiBulkResponse(List<WeatherBoundingBox> coordinates)
    {
        var resultObj = new WeatherApiBulkResponse();

        var queryString = "";
        coordinates.ForEach(co =>
        {
            queryString += $"{{custom_id:'{co.Id}',q:'{co.BoundingBox.Centre.X}, {co.BoundingBox.Centre.Y}'}},";
        });
        var requestBody = $"{{locations: [{queryString}]}}";


        try
        {
            var url = "http://api.weatherapi.com/v1/forecast.json";
            var queryParams = new Dictionary<string, string?>(){
                {"key", _config["WeatherApiKey"]},
                {"q", "bulk"},
                {"days", "5"}
                };

            var urlWithQuery = QueryHelpers.AddQueryString(url, queryParams);

            using (var response = await _httpClient.PostAsync(urlWithQuery, new StringContent(requestBody, Encoding.UTF8, "application/json")))
            {
                resultObj = await response.Content.ReadFromJsonAsync<WeatherApiBulkResponse>()!;
            }
        }
        catch (System.Exception ex)
        {
            // _logger.LogError(ex, "Error getting Weather Bulk Request");
        }

        return resultObj;

    }
}