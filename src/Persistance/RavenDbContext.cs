using Microsoft.Extensions.Options;
using rainyroute.Models.Configurations;
using Raven.Client.Documents;
using Raven.Client.ServerWide;
using Raven.Client.ServerWide.Commands;
using Raven.Client.ServerWide.Operations;

public class RavenDbContext : IDisposable
{
    private static Lazy<IDocumentStore> LazyStore;
    private static RavenDbConfiguration _config;

    private static IDocumentStore Store;

    // Implementation like https://ravendb.net/features/clients/csharp
    public RavenDbContext(IOptionsMonitor<RavenDbConfiguration> options)
    {
        _config = options.CurrentValue;
        LazyStore = new Lazy<IDocumentStore>(() =>
        {

            var store = new DocumentStore()
            {
                Urls = _config.Urls,
                Database = _config.DbName
            };

            return store.Initialize();
        });

        EnsureDatabaseExists();
    }

    public void EnsureDatabaseExists()
    {
        var result = Store.Maintenance.Server.Send(new GetDatabaseRecordOperation(_config.DbName));

        if (result == null)
        {
            // if database doesnt exist, create it
            Store.Maintenance.Server.Send(new CreateDatabaseOperation(new DatabaseRecord(_config.DbName)));
        }
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}