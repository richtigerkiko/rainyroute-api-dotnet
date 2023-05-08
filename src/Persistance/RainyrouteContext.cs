using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using rainyroute.Persistance.Models;

namespace rainyroute.Persistance;

public class RainyrouteContext : DbContext
{

    public RainyrouteContext(DbContextOptions<RainyrouteContext> options) : base(options)
    {
    }

    public DbSet<WeatherBoundingBox> WeatherBoundingBoxes { get; set; }

    public DbSet<WeatherForeCastHour> WeatherForecastHours { get; set; }

    // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseNpgsql(Configuration.GetConnectionString("PostgreSQL"), o =>
    // {
    //     o.UseNetTopologySuite();
    // });

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<WeatherBoundingBox>()
                    .HasIndex(x => new { x.MinCoordinate, x.MaxCoordinate })
                    .HasMethod("gist")
                    .HasName("ix_weatherboundingboxes_coordinates_gist");
    }
    public void EnsureDatabaseCreated()
    {
        Database.EnsureCreated();
    }


}

