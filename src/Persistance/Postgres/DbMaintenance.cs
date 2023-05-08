using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using rainyroute.Models;
using rainyroute.Persistance.Postgres.Models;

namespace rainyroute.Persistance.Postgres;

public class RainyRouteDbMaintenance: BackgroundService
{

    private const int MaintenanceDelay = 60 * 1000;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            RunDbMaintenance();
            await Task.Delay(MaintenanceDelay);
        }
    }

    public void RunDbMaintenance()
    {
        GenerateGermanyBoundingBoxDocuments();
    }

    public void GenerateGermanyBoundingBoxDocuments()
    {
        var boundingBoxGermany = new WeatherBoundingBox()
        {
            MinCoordinate = new GeoCoordinate(47.2701, 5.8663),
            MaxCoordinate = new GeoCoordinate(55.0992, 15.0419)
        };
        var boundingBoxesOfGermany = boundingBoxGermany.DivideIntoSmallerBoxes(100).OfType<WeatherBoundingBox>().ToList();


        using (var session = new RainyrouteContext())
        {
            // as it is static, only generate Bounding boxes if there are none (normally after generation)
            // if (!session.WeatherBoundingBoxes.Any())
            if(true)
            {
                session.AddRange(boundingBoxesOfGermany);
                session.SaveChanges();
            }
        }
    }
}

