using Microsoft.Extensions.Options;
using rainyroute.Models.Configurations;
using Raven.Client.Documents;
using Raven.Client.ServerWide;
using Raven.Client.ServerWide.Commands;
using Raven.Client.ServerWide.Operations;

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

    public void EnsureDatabaseExists()
    {
        var result = _documentStore.Maintenance.Server.Send(new GetDatabaseRecordOperation(_config.DbName));

        if (result == null)
        {
            // if database doesnt exist, create it
            _documentStore.Maintenance.Server.Send(new CreateDatabaseOperation(new DatabaseRecord(_config.DbName)));
        }
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}