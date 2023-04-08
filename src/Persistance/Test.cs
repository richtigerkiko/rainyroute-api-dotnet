using rainyroute.Models;
using rainyroute.Models.Data;
using rainyroute.Models.Interfaces;
using rainyroute.Services;
using Raven.Client.Documents;

namespace rainyroute.Persistance;

public class Generator : BaseService
{
    IDocumentStore Store;

    public Generator(RavenDbContext store, ILogger logger, IConfiguration config, HttpClient httpClient) : base(logger, config, httpClient)
    {
        Store = store._documentStore;
    }

    public void GenerateGermanyBoundingBoxDocuments()
    {
        var boundingBoxGermany = new WeatherBoundingBox()
        {
            MinCoordinate = new GeoCoordinate(47.2701, 5.8663),
            MaxCoordinate = new GeoCoordinate(55.0992, 15.0419)
        };

        var boundingBoxesOfGermany = boundingBoxGermany.DivideIntoSmallerBoxes(100).OfType<WeatherBoundingBox>().ToList();

        foreach (var boundingbox in boundingBoxesOfGermany)
        {

        }

        using (var session = Store.OpenSession())
        {
            foreach (var boundingBox in boundingBoxesOfGermany)
            {
                session.Store(boundingBox);
            }

            session.SaveChanges();
        }
    }

    public async Task GenerateWeatherForBoundingBoxDocumentsAsync()
    {
        using (var session = Store.OpenSession())
        {
            // Get all BoundingBoxes
            var boundingBoxes = session.Query<WeatherBoundingBox>().ToList();

            var test = session.Load<WeatherBoundingBox>("WeatherRouteBoundingBoxes/164-A");

            var weatherapi = new WeatherApiService(_logger, _config, _httpClient);

            var requestCount = 40; //Bulk request not m0re than 50, lets say 40

            for (int i = 0; i < boundingBoxes.Count; i += requestCount)
            {
                int countToExtract = Math.Min(requestCount, boundingBoxes.Count - i); // Calculate the number of elements to extract
                // requests need to be below 50
                var fortyBoxes = boundingBoxes.GetRange(i, countToExtract).ToList();
                var response = await weatherapi.GetWeatherApiBulkResponse(fortyBoxes);

                foreach (var apiWeather in response.Bulk)
                {
                    var box = fortyBoxes[int.Parse(apiWeather.Query.CustomId)];

                    
                    apiWeather.Query.Forecast.Forecastday.ForEach(x =>
                    {
                        x.Hour.ForEach(h => 
                        {
                            box.WeatherForeCastHours.Add(new WeatherForeCastHour(h));
                            var weatherForecastHour = new WeatherForeCastHour(h);
                            weatherForecastHour.WeatherRouteBoundingBox = box.Id;

                            session.Store(weatherForecastHour);

                        });
                    });

                }
            }
            session.SaveChanges();
        }
    }
}