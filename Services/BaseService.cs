namespace rainyroute.Services;

public class BaseService
{
    internal readonly IConfiguration _config;
    internal readonly ILogger _logger;

    internal readonly HttpClient _httpClient;

    public BaseService(ILogger logger, IConfiguration config, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
        _config = config;
    }
}
