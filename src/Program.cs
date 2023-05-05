using System.Globalization;
using Microsoft.AspNetCore.Localization;
using rainyroute.Models.Configurations;
using rainyroute.Persistance;
using rainyroute.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Cors from appsettings.json
var corsAllowedUrls = builder.Configuration["CORS:AllowedUrls"]?.Split(",") ?? new string[] { };
builder.Services.AddCors(options =>
    {
        // this defines a CORS policy called "default"
        options.AddPolicy("default", policy =>
        {
            policy.WithOrigins(corsAllowedUrls)
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
    });

builder.Services.Configure<RavenDbConfiguration>(builder.Configuration.GetSection("RavenDb"));

builder.Services.AddTransient<ILogger>(s => s.GetRequiredService<ILogger<Program>>());

builder.Services.AddSingleton<RavenDbContext>();

builder.Services.AddHostedService<DatabaseMaintenanceService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var defaultCulture = "en-US";
app.UseRequestLocalization(
    new RequestLocalizationOptions
    {
        DefaultRequestCulture = new RequestCulture(defaultCulture),
        SupportedCultures = new List<CultureInfo> { new CultureInfo(defaultCulture) },
        SupportedUICultures = new List<CultureInfo> { new CultureInfo(defaultCulture) }
    }
);


app.UseHttpsRedirection();

app.UseAuthorization();

app.UseCors("default");

app.MapControllers();

app.Run();
