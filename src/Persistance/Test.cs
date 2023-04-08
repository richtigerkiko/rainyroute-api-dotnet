using rainyroute.Models.Data;
using rainyroute.Models.Interfaces;
using Raven.Client.Documents;

namespace rainyroute.Persistance;

public class Generator
{
    IDocumentStore Store;

    public Generator(RavenDbContext store)
    {
        Store = store._documentStore;
    }

    public void GenerateGermanyBoundingBoxDocuments()
    {
        var boundingBoxGermany = new WeatherRouteBoundingBox()
        {
            MinCoordinate = new Tuple<double, double>(47.2701, 5.8663),
            MaxCoordinate = new Tuple<double, double>(55.0992, 15.0419)
        };

        var boundingBoxesOfGermany = boundingBoxGermany.DivideIntoSmallerBoxes(300).OfType<WeatherRouteBoundingBox>().ToList();

        using (var session = Store.OpenSession())
        {
            foreach (var boundingBox in boundingBoxesOfGermany)
            {
                session.Store(boundingBox);
            }

            session.SaveChanges();
        }
    }

    private List<IBoundingBox> GenerateGermanySubBoxes()
    {
        var boundingBoxGermany = new WeatherRouteBoundingBox()
        {
            MinCoordinate = new Tuple<double, double>(47.2701, 5.8663),
            MaxCoordinate = new Tuple<double, double>(55.0992, 15.0419)
        };

        return boundingBoxGermany.DivideIntoSmallerBoxes(300);
    }
}