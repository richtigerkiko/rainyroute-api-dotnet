using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using rainyroute.Persistance.Postgres.Models;

namespace rainyroute.Persistance.Postgres;

public class RainyrouteContext : DbContext
{
    public DbSet<WeatherBoundingBox> WeatherBoundingBoxes { get; set; }

    public DbSet<WeatherForeCastHour> WeatherForecastHours { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql("Host=127.0.0.1;Database=rainyroutedb;Username=root;Password=root");

}

