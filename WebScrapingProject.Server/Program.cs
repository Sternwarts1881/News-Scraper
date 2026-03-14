using Microsoft.AspNetCore.Builder;
using MongoDB.Driver;
using WebScrapingProject.Server.Models;
using WebScrapingProject.Server.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings"));


builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = builder.Configuration.GetSection("MongoDbSettings").Get<MongoDbSettings>();
    return new MongoClient(settings.ConnectionString);
});

builder.Services.AddSingleton<MongoDbService>();
builder.Services.AddSingleton<ScraperService>();
builder.Services.AddSingleton<DataProcessingService>();
builder.Services.AddSingleton<CategoryService>();
builder.Services.AddSingleton<LocationService>();
builder.Services.AddHostedService<WebScrapingProject.Server.BackgroundTasks.NewsScraperBackgroundService>();
builder.Services.AddSingleton<SimilarityService>();

var app = builder.Build();

app.UseDefaultFiles();
app.MapStaticAssets();


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
   
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();
