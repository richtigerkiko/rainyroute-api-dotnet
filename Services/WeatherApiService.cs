namespace rainyroute.Services
{
    internal class WeatherApiService : BaseService
    {
        public WeatherApiService(ILogger logger, IConfiguration config, HttpClient httpClient) : base(logger, config, httpClient)
        {
        }
    }
}