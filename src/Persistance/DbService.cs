using rainyroute.Models;
using rainyroute.Models.Data;
using rainyroute.Persistance;
using rainyroute.Persistance.Indexes;
using Raven.Client.Documents;
using Raven.Client.Documents.Queries.Spatial;
using Raven.Client.Documents.Session;

namespace rainyroute.Persistance;
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
            var ids = allBoxes.Select(x => x.Id);
            // var weatherForecastHours = session.Query<WeatherForeCastHour>().Where(x => ids.Contains(x.WeatherBoundingBoxId)).ToList();

            // allBoxes.ForEach(x => x.WeatherForeCastHours = weatherForecastHours.Where(y => y.WeatherBoundingBoxId == x.Id).ToList());
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

    public List<WeatherBoundingBox> GetAllWeatherBoundingBoxes(List<GeoCoordinate> coordinates)
    {
        var bbox = new BoundingBox(coordinates);

        using (var session = _store.OpenSession())
        {
            var allBoxesInBoundingBox = session.Query<WeatherBoundingBox>().Where(x => 
                    x.MinCoordinate.Latitude >= bbox.MinCoordinate.Latitude && 
                    x.MaxCoordinate.Latitude <= bbox.MaxCoordinate.Latitude && 
                    x.MinCoordinate.Longitude >= bbox.MinCoordinate.Longitude && 
                    x.MaxCoordinate.Longitude <= bbox.MaxCoordinate.Longitude)
                .ToList();
            
            return allBoxesInBoundingBox;
        }
    }
}