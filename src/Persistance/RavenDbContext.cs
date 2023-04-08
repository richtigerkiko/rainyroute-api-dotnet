using Microsoft.Extensions.Options;
using rainyroute.Models.Configurations;
using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using Raven.Client.Documents.Operations.Indexes;
using Raven.Client.ServerWide;
using Raven.Client.ServerWide.Commands;
using Raven.Client.ServerWide.Operations;

namespace rainyroute.Persistance;

public class RavenDbContext : IDisposable
{
    public IDocumentStore _documentStore;
    private RavenDbConfiguration _config;

    public RavenDbContext(IOptionsMonitor<RavenDbConfiguration> options)
    {
        _config = options.CurrentValue;
        _documentStore = new DocumentStore
        {
            Urls = _config.Urls,
            Database = _config.DbName
        };

        _documentStore.Initialize();
        EnsureDatabaseExists();
    }

    private void EnsureDatabaseExists()
    {
        var result = _documentStore.Maintenance.Server.Send(new GetDatabaseRecordOperation(_config.DbName));

        if (result == null)
        {
            GenerateNewDatabase();
        }
    }

    private void GenerateNewDatabase()
    {
        _documentStore.Maintenance.Server.Send(new CreateDatabaseOperation(new DatabaseRecord(_config.DbName)));

        // Create indexed collections
        _documentStore.Maintenance.Send(new PutIndexesOperation(new[] { new IndexDefinition
        {
            Name = "WeatherForeCastHours/WeatherBoundingBox",
            Maps = { "from wf in docs.WeatherForeCastHours select new { WeatherBoundingBox = wf.WeatherBoundingBoxId }" }
        } }));

        _documentStore.Maintenance.Send(new PutIndexesOperation(new[] { new IndexDefinition
        {
            Name = "WeatherBoundingBox/WeatherForeCastHours",
            Maps = { "from wrb in docs.WeatherBoundingBox select new { WeatherForeCastHours = wrb.WeatherForeCastHours }" }
        } }));

    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}