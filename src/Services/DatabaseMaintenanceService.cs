using rainyroute.Persistance;

namespace rainyroute.Services;

// this service runs in the background, adds new Weather entries and removes old ones
internal class DatabaseMaintenanceService : BackgroundService
{
    IConfiguration _config;
    ILogger _logger;
    RavenDbContext _dbcontext;

    private const int DelayMilliseconds = 60 * 1000;

    public DatabaseMaintenanceService(IConfiguration config, ILogger logger, RavenDbContext store)
    {
        _config = config;
        _logger = logger;
        _dbcontext = store;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            RunDbMaintenance();
            await Task.Delay(DelayMilliseconds);
        }
    }

    private void RunDbMaintenance()
    {
        var dbMaintenanceService = new DbMaintenance(_dbcontext, _config, _logger);
        dbMaintenanceService.RunDbMaintenace();
    }
}