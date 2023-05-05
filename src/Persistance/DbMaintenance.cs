using rainyroute.Models;
using rainyroute.Models.Data;
using rainyroute.Persistance;
using rainyroute.Services;
using Raven.Client.Documents;
using Raven.Client.Documents.Operations;
using Raven.Client.Documents.Queries;

internal class DbMaintenance
{
    IDocumentStore _store;
    RavenDbContext _context;

    IConfiguration _config;
    ILogger _logger;
    HttpClient _httpClient;

    internal DateTime TodayMidnight = DateTime.Now.Date;

    public DbMaintenance(RavenDbContext dbContext, IConfiguration config, ILogger logger)
    {
        _store = dbContext._documentStore;
        _context = dbContext;

        _httpClient = new HttpClient(new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(2)
        });
        _config = config;
        _logger = logger;
    }

    public void RunDbMaintenace()
    {
        GenerateGermanyBoundingBoxDocuments();
        DeleteAllWeatherDocumentsOlderThanToday();
        FillWeather();
    }

    private void DeleteAllWeatherDocumentsOlderThanToday()
    {

        var operation = new DeleteByQueryOperation(new IndexQuery
        {
            Query = $"FROM WeatherForeCastHours WHERE Time < '{TodayMidnight:s}'"
        });

        _store.Operations.Send(operation);
    }

    public async Task FillWeather()
    {
        // check if Last weather hast time of  midnight in 5 days
        if (!IsWeatherUpToDate())
        {
            var dbService = new DbService(_context);
            var weatherapi = new WeatherApiService(_logger, _config, _httpClient);

            var bboxes = dbService.GetAllWeatherBoundingBoxes();
            var requestCount = 40;

            using (var session = _store.OpenSession())
            {
                for (int i = 0; i < bboxes.Count; i += requestCount)
                {
                    int countToExtract = Math.Min(requestCount, bboxes.Count - i); // Calculate the number of elements to extract

                    // requests need to be below 50
                    var fortyBoxes = bboxes.GetRange(i, countToExtract).ToList();
                    var response = await weatherapi.GetWeatherApiBulkResponse(fortyBoxes);

                    foreach (var apiWeather in response.Bulk)
                    {
                        var box = fortyBoxes[int.Parse(apiWeather.Query.CustomId)];
                        apiWeather.Query.Forecast.Forecastday.ForEach(x =>
                        {
                            x.Hour.ForEach(h =>
                            {
                                var weatherForecastHour = new WeatherForeCastHour(h);
                                weatherForecastHour.WeatherBoundingBoxId = box.Id;

                                session.Store(weatherForecastHour);
                            });
                        });

                    }
                }
                session.SaveChanges();
            }
        }


    }

    private bool IsWeatherUpToDate()
    {
        using (var session = _store.OpenSession())
        {
            var bbox = session.Query<WeatherBoundingBox>().ToList().FirstOrDefault();
            bbox.WeatherForeCastHours = session.Query<WeatherForeCastHour>().Where(x => x.WeatherBoundingBoxId == bbox.Id).ToList();


            if (bbox != null)
            {
                var lastDayInDb = bbox.WeatherForeCastHours.LastOrDefault()?.Time.DayOfYear;
                var in5Days = TodayMidnight.AddDays(5).DayOfYear;
                return bbox.WeatherForeCastHours.LastOrDefault()?.Time.DayOfYear == TodayMidnight.AddDays(5).DayOfYear;
            }
            else
            {
                return false;
            }
        }
    }

    public void GenerateGermanyBoundingBoxDocuments()
    {
        var boundingBoxGermany = new WeatherBoundingBox()
        {
            MinCoordinate = new GeoCoordinate(47.2701, 5.8663),
            MaxCoordinate = new GeoCoordinate(55.0992, 15.0419)
        };

        // as it is static, only generate Bounding boxes if there are none (normally after generation)
        using (var session = _store.OpenSession())
        {
            if (!(session.Query<WeatherBoundingBox>().Any()))
            {
                List<WeatherBoundingBox> boundingBoxesOfGermany = boundingBoxGermany.DivideIntoSmallerBoxes(100).OfType<WeatherBoundingBox>().ToList();
                foreach (var boundingBox in boundingBoxesOfGermany)
                {
                    session.Store(boundingBox);
                }

                session.SaveChanges();
            }
        }
    }

}