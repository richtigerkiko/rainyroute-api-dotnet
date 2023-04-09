using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using rainyroute.Models;
using rainyroute.Models.Data;
using rainyroute.Models.ResponseObjects.ExternalApiResponses.WeatherAPI;

namespace rainyroute.Services
{
    internal class WeatherApiService : BaseService
    {
        public WeatherApiService(ILogger logger, IConfiguration config, HttpClient httpClient) : base(logger, config, httpClient)
        {
        }

        public async Task<WeatherApiBulkResponse> GetWeatherApiBulkResponse(List<WeatherBoundingBox> boundingBoxes)
        {
            var geoCoordinates = boundingBoxes.Select(x => x.CenterOfBoundingBox).ToList();
            
            return await GetWeatherApiBulkResponse(geoCoordinates);
        }

        private async Task<WeatherApiBulkResponse> GetWeatherApiBulkResponse(List<GeoCoordinate> coordinates)
        {

            var resultObj = new WeatherApiBulkResponse();

            var queryString = "";
            var i = 0;
            coordinates.ForEach(x =>
            {
                queryString += $"{{custom_id:'{i}',q:'{x.Latitude}, {x.Longitude}'}},";
                i++;
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
                _logger.LogError(ex, "Error getting Weather Bulk Request");
            }

            return resultObj;
        }
    }
}