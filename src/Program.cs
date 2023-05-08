using System.Globalization;
using System.Reflection;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using rainyroute.Persistance;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(options => {
    options.JsonSerializerOptions.Converters.Add(new NetTopologySuite.IO.Converters.GeoJsonConverterFactory());
    // options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// add Configuration
builder.Configuration.AddJsonFile("appsettings.json", optional: false).AddEnvironmentVariables().AddUserSecrets(Assembly.GetExecutingAssembly(), true);
var configuration = builder.Configuration;
builder.Services.AddSingleton(configuration);

// Make Cors better Later
// var corsAllowedUrls = builder.Configuration["CORS:AllowedUrls"]?.Split(",") ?? new string[] { };
builder.Services.AddCors(options 
  => options.AddPolicy(name: "default", builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

// Initialize HTTPRequests
builder.Services.AddHttpClient();

// Initialize Database
// AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
builder.Services.AddDbContext<RainyrouteContext>(o => o.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQL"), postgreso => postgreso.UseNetTopologySuite()));
builder.Services.AddHostedService<DbMaintenanceService>().AddHttpClient();

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
        SupportedUICultures = new List<CultureInfo> { new CultureInfo(defaultCulture) },
    }
);

var cultureInfo = new CultureInfo(defaultCulture);
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
Thread.CurrentThread.CurrentCulture = cultureInfo;
Thread.CurrentThread.CurrentUICulture = cultureInfo;

// app.UseHttpsRedirection();

app.UseAuthorization();

app.UseCors("default");

app.MapControllers();

app.Run();
