using Microsoft.AspNetCore.Mvc;
using WebScrapingProject.Server.Models;
using WebScrapingProject.Server.Services;

namespace WebScrapingProject.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NewsController : ControllerBase
    {
        private readonly MongoDbService _mongoDbService;

        public NewsController(MongoDbService mongoDbService)
        {
            _mongoDbService = mongoDbService;
        }

        [HttpPost("test-connection")]
        public async Task<IActionResult> TestConnection()
        {
            var testArticle = new NewsArticle
            {
                Category = "Test",
                Title = "MongoDB Bağlantı Testi",
                Content = "Db Calısıyor mu diye test obje",
                LocationText = "Gölcük, Kocaeli",
                Latitude = 40.7184,
                Longitude = 29.8215,
                PublishDate = DateTime.Now,
                SourceNames = { "test kaynagı" },
                Url = "http://localhost",
                IsProcessed = true
            };

            await _mongoDbService.CreateTestArticleAsync(testArticle);

            return Ok("basarili");
        }
    }
}