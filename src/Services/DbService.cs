using rainyroute.Models;
using rainyroute.Models.Data;
using rainyroute.Persistance;
using Raven.Client.Documents;

namespace rainyroute.Services
{
    internal class DbService
    {
        private IDocumentStore _store;

        public DbService(RavenDbContext dbContext)
        {
            _store = dbContext._documentStore;
        }

        public List<WeatherRouteBoundingBox> GetCrossingBoundingBoxes(List<GeoCoordinate> coordinates)
        {
            var boxList = new List<WeatherRouteBoundingBox>();
            using (var session = _store.OpenSession())
            {

                var allBoxes = session.Query<WeatherBoundingBox>().ToList();

                foreach (var coordinate in coordinates)
                {
                    var box = allBoxes.Where(x => x.ContainsPoint(coordinate)).FirstOrDefault();

                    if (box != null && !(boxList.Any(x => x.Id == box.Id)))
                    {
                        boxList.Add(new WeatherRouteBoundingBox(box));
                    }

                    boxList.Last().CoordinatesInBoundingBox.Add(coordinate);
                }
            }
            return boxList;
        }
    }
}