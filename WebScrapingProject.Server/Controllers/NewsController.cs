using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
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

        
        [HttpGet]
        public async Task<IActionResult> GetNews([FromQuery] string? category, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            
            var allNews = await _mongoDbService.GetAllAsync();

            
            var filteredNews = allNews.AsQueryable();

            if (!string.IsNullOrEmpty(category) && category != "Tümü")
            {
                filteredNews = filteredNews.Where(n => n.Category == category);
            }

            if (startDate.HasValue)
            {
                filteredNews = filteredNews.Where(n => n.PublishDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                filteredNews = filteredNews.Where(n => n.PublishDate <= endDate.Value);
            }

            
            return Ok(filteredNews.ToList());
        }
    }
}