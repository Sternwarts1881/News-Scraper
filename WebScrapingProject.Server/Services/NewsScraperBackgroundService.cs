using WebScrapingProject.Server.Services;

namespace WebScrapingProject.Server.BackgroundTasks
{
    public class NewsScraperBackgroundService : BackgroundService
    {
        private readonly ScraperService _scraperService;
        private readonly MongoDbService _mongoDbService;
        private readonly SimilarityService _similarityService;
        private readonly ILogger<NewsScraperBackgroundService> _logger;

        public NewsScraperBackgroundService(
            ScraperService scraperService,
            MongoDbService mongoDbService,
            SimilarityService similarityService,
            ILogger<NewsScraperBackgroundService> logger)
        {
            _scraperService = scraperService;
            _mongoDbService = mongoDbService;
            _similarityService = similarityService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Haber Çekme Arka Plan Servisi Başladı...");

           
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Sitelerden veri çekme işlemi tetikleniyor...");

                    
                    var allExtractedArticles = await _scraperService.ScrapeAllSitesAsync();

                    
                    foreach (var article in allExtractedArticles)
                    {
                        await _mongoDbService.ProcessAndSaveArticleAsync(article, _similarityService);
                    }

                    _logger.LogInformation($"Tarama tamamlandı. {allExtractedArticles.Count} adet potansiyel haber işlendi (Benzerler birleştirildi, yeniler eklendi).");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Arka plan servisinde hata oluştu: {ex.Message}");
                }

                
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
}