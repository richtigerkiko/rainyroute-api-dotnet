using rainyroute.Models.Data;
using Raven.Client.Documents.Indexes;
namespace rainyroute.Persistance.Indexes;

// Define an index for WeatherBoundingBox
public class WeatherBoundingBoxIndex : AbstractIndexCreationTask<WeatherBoundingBox>
{
    public WeatherBoundingBoxIndex()
    {
        Map = weatherBoundingBoxes => from w in weatherBoundingBoxes
                                      select new
                                      {
                                          CenterOfBoundingBox  = CreateSpatialField(w.CenterOfBoundingBox.Latitude, w.CenterOfBoundingBox.Longitude)
                                      };

        Index(x => x.CenterOfBoundingBox, FieldIndexing.No);

        Spatial(x => x.MinCoordinate, options => options.Geography.Default());
        Spatial(x => x.MaxCoordinate, options => options.Geography.Default());
        Spatial(x => x.CenterOfBoundingBox, options => options.Geography.Default());
    }
}