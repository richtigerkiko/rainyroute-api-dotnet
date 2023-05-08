
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using rainyroute.Persistance.Models;
using rainyroute.Services;

namespace rainyroute.Persistance;

public class DbMaintenanceService : BackgroundService
{

    private const int MaintenanceDelay = 60 * 1000;
    private readonly IServiceScopeFactory _scopeFactory;

    private HttpClient _httpClient;

    private IConfiguration _configuration;

    public DbMaintenanceService(IServiceScopeFactory scopeFactory, HttpClient httpClient, IConfiguration configuration)
    {
        _scopeFactory = scopeFactory;
        _httpClient = httpClient;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            RunDbMaintenance();
            await Task.Delay(MaintenanceDelay);
        }
    }

    public void RunDbMaintenance()
    {
        GenerateGermanyBoundingBoxDocuments();
        FillWeather();
    }

    public void GenerateGermanyBoundingBoxDocuments()
    {
        var boundingBoxGermany = new WeatherBoundingBox()
        {
            MinCoordinate = new Point(47.2701, 5.8663),
            MaxCoordinate = new Point(55.0992, 15.0419)
        };

        using (var scope = _scopeFactory.CreateScope())
        {
            var _dbContext = scope.ServiceProvider.GetRequiredService<RainyrouteContext>();

            _dbContext.EnsureDatabaseCreated();
            if (!_dbContext.WeatherBoundingBoxes.Any())
            {
                var boundingBoxesOfGermany = boundingBoxGermany.DivideEnvelope(3000);
                _dbContext.AddRange(boundingBoxesOfGermany);
                _dbContext.SaveChanges();
            }
        }
    }

    private async Task FillWeather()
    {
        using (var scope = _scopeFactory.CreateScope())
        {
            var _dbContext = scope.ServiceProvider.GetRequiredService<RainyrouteContext>();
            var dbService = new DbService(_dbContext);
            if (!_dbContext.WeatherForecastHours.Any())
            {
                var weatherApiService = new WeatherApiService(_configuration, _httpClient);
                var allBoxes = dbService.GetAllWeatherBoundingBoxes();


                // subdivide List to chunks of 40 to limit weatherapi calls
                var chunksize = 40;
                var chunkedList = Enumerable.Range(0, (allBoxes.Count + chunksize - 1) / chunksize)
                    .Select(i => allBoxes.Skip(i * chunksize).Take(chunksize).ToList())
                    .ToList();

                foreach (var chunk in chunkedList)
                {
                    var weatherResponse = await weatherApiService.GetWeatherApiBulkResponse(chunk);

                    foreach (var apiWeather in weatherResponse.Bulk)
                    {
                        var box = chunk.Where(x => x.Id == apiWeather.Query.CustomId).FirstOrDefault();
                        if(box != null){
                            foreach(var day in apiWeather.Query.Forecast.Forecastday) dbService.SetWeatherBoundingBoxeWeather(box, day);
                        }
                    }
                }
            }

        }
    }
}
