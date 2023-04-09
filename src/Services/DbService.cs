using rainyroute.Models;
using rainyroute.Models.Data;
using rainyroute.Persistance;
using rainyroute.Persistance.Indexes;
using Raven.Client.Documents;
using Raven.Client.Documents.Queries.Spatial;
using Raven.Client.Documents.Session;

namespace rainyroute.Services;
internal class DbService
{
    private IDocumentStore _store;

    public DbService(RavenDbContext dbContext)
    {
        _store = dbContext._documentStore;
    }

    public List<WeatherBoundingBox> GetAllWeatherBoundingBoxes()
    {
        using (var session = _store.OpenSession())
        {
            var allBoxes = session.Query<WeatherBoundingBox>().ToList();
            return allBoxes;
        }
    }

    public List<WeatherBoundingBox> GetAllWeatherBoundingBoxes(DateTime specificDate)
    {
        var allBoxes = GetAllWeatherBoundingBoxes().Select(x => new WeatherBoundingBox
        {
            Id = x.Id,
            MinCoordinate = x.MinCoordinate,
            MaxCoordinate = x.MaxCoordinate,
            WeatherForeCastHours = x.WeatherForeCastHours.Where(weatherForeCastHour => weatherForeCastHour.Time.Date.DayOfYear == specificDate.Date.DayOfYear)
        .ToList()
        }).ToList();

        return allBoxes;
    }
}