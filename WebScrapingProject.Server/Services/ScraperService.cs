using HtmlAgilityPack;
using WebScrapingProject.Server.Models;
using WebScrapingProject.Server.Services;

namespace WebScrapingProject.Server.Services
{
   
    public class SiteConfig
    {
        public string SourceName { get; set; } = null!;
        public string BaseUrl { get; set; } = null!;
        public List<string> TargetCategoryUrls { get; set; } = new();
    }

    public class ScraperService
    {
        private readonly HttpClient _httpClient;
        private readonly DataProcessingService _dataProcessingService;
        private readonly CategoryService _categoryService;
        private readonly LocationService _locationService;
        private readonly List<SiteConfig> _sitesToScrape;

        public ScraperService(
            DataProcessingService dataProcessingService,
            CategoryService categoryService,
            LocationService locationService)
        {
            _dataProcessingService = dataProcessingService;
            _categoryService = categoryService;
            _locationService = locationService;

            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");

           
            _sitesToScrape = new List<SiteConfig>
{
           
                new SiteConfig {
                SourceName = "Özgür Kocaeli",
                BaseUrl = "https://www.ozgurkocaeli.com.tr",
                TargetCategoryUrls = new List<string> { "https://www.ozgurkocaeli.com.tr" }
                 },
                    new SiteConfig {
             SourceName = "Çağdaş Kocaeli",
             BaseUrl = "https://www.cagdaskocaeli.com.tr",
             TargetCategoryUrls = new List<string> { "https://www.cagdaskocaeli.com.tr" }
    },
          new SiteConfig {
             SourceName = "Ses Kocaeli",
             BaseUrl = "https://www.seskocaeli.com",
             TargetCategoryUrls = new List<string> { "https://www.seskocaeli.com" }
    },
         new SiteConfig {
             SourceName = "Yeni Kocaeli",
             BaseUrl = "https://www.yenikocaeli.com",
             TargetCategoryUrls = new List<string> { "https://www.yenikocaeli.com" }
    },
    
         new SiteConfig {
              SourceName = "Bizim Yaka",
              BaseUrl = "https://www.bizimyaka.com",
              TargetCategoryUrls = new List<string> { "https://www.bizimyaka.com" }
    }
};
        }

        
        public async Task<List<NewsArticle>> ScrapeAllSitesAsync()
        {
            var allArticles = new List<NewsArticle>();
            var threeDaysAgo = DateTime.Now.AddDays(-3);

            Console.WriteLine("=== TÜM SİTELERİ TARAMA İŞLEMİ BAŞLIYOR ===");

            foreach (var site in _sitesToScrape)
            {
                Console.WriteLine($"\n>>> HEDEF SİTE: {site.SourceName} <<<");

                foreach (var url in site.TargetCategoryUrls)
                {
                    try
                    {
                        var html = await _httpClient.GetStringAsync(url);
                        var htmlDoc = new HtmlDocument();
                        htmlDoc.LoadHtml(html);

                        var newsLinks = htmlDoc.DocumentNode.SelectNodes("//a[@href]");
                        if (newsLinks == null) continue;

                        
                        var linksToScrape = newsLinks
                            .Select(n => n.GetAttributeValue("href", ""))
                            .Where(href => (href.Contains("/haber/") || href.Contains("-")) && System.Text.RegularExpressions.Regex.IsMatch(href, @"\d+"))
                            .Distinct()
                            .Take(40)
                            .ToList();

                        foreach (var link in linksToScrape)
                        {
                            
                            var fullUrl = link.StartsWith("http") ? link : site.BaseUrl.TrimEnd('/') + (link.StartsWith("/") ? "" : "/") + link;

                            var articleDoc = await GetHtmlDocumentAsync(fullUrl);
                            if (articleDoc == null) continue;

                            var titleNode = articleDoc.DocumentNode.SelectSingleNode("//h1");
                           
                            var contentNode = articleDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'content') or contains(@class, 'haber-metni') or contains(@class, 'article') or contains(@class, 'detay') or contains(@class, 'text')]");

                            if (titleNode == null || contentNode == null) continue;

                            var rawTitle = _dataProcessingService.CleanArticleContent(titleNode.InnerText);
                            var rawContent = _dataProcessingService.CleanArticleContent(contentNode.InnerText);

                            var category = _categoryService.DetermineCategory(rawContent, rawTitle);

                            if (category != "Diğer")
                            {
                                var locationText = _locationService.ExtractLocationFromText(rawContent);

                                if (locationText != null)
                                {
                                    var newArticle = new NewsArticle
                                    {
                                        Title = rawTitle,
                                        Content = rawContent,
                                        Category = category,
                                        Url = fullUrl,
                                        SourceNames = new List<string> { site.SourceName },
                                        PublishDate = DateTime.Now, 
                                        LocationText = locationText,
                                        Latitude = 40.7654,
                                        Longitude = 29.9408
                                    };

                                    allArticles.Add(newArticle);
                                    Console.WriteLine($"[+] BAŞARILI: {site.SourceName} | Kategori: {category} | Konum: {locationText}");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[-] HATA ({site.SourceName} - {url}): {ex.Message}");
                    }
                }
            }

            Console.WriteLine($"\n=== İŞLEM BİTTİ. Toplam Çekilen Haber: {allArticles.Count} ===");
            return allArticles;
        }

        private async Task<HtmlDocument?> GetHtmlDocumentAsync(string url)
        {
            try
            {
                var html = await _httpClient.GetStringAsync(url);
                var doc = new HtmlDocument();
                doc.LoadHtml(html);
                return doc;
            }
            catch { return null; }
        }
    }
}