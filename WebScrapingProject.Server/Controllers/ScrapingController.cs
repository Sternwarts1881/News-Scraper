using Microsoft.AspNetCore.Mvc;
using WebScrapingProject.Server.Services;

namespace WebScrapingProject.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScrapingController : ControllerBase
    {
        private readonly ScraperService _scraperService;

        public ScrapingController(ScraperService scraperService)
        {
            _scraperService = scraperService;
        }

        
        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            return Ok(new { isScraping = _scraperService.IsScraping });
        }

        [HttpPost("start")]
        public IActionResult StartScraping()
        {
           
            if (_scraperService.IsScraping)
            {
                return BadRequest(new { message = "Tarama arka planda zaten devam ediyor!" });
            }

            Task.Run(async () =>
            {
                var articles = await _scraperService.ScrapeAllSitesAsync();
               
            });

            return Ok(new { message = "Tarama başlatıldı, SignalR üzerinden dinleniyor..." });
        }
    }
}