using HtmlAgilityPack;
using WebScrapingProject.Server.Models;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.SignalR;
using WebScrapingProject.Server.Hubs;

namespace WebScrapingProject.Server.Services
{
    public class ScrapeTask
    {
        public string SourceName { get; set; } = null!;
        public string BaseUrl { get; set; } = null!;
        public string EntryUrl { get; set; } = null!;
        public DateTime? ExactArchiveDate { get; set; }
    }

    public class ScraperService
    {
        private readonly HttpClient _httpClient;
        private readonly DataProcessingService _dataProcessingService;
        private readonly CategoryService _categoryService;
        private readonly LocationService _locationService;
        private readonly HTMLDocumentCleanupService _cleanupService;
        private readonly IHubContext<ScrapingHub> _hubContext; 
        private readonly Random _randomGenerator;
        public bool IsScraping { get; private set; } = false;

        public ScraperService(
            DataProcessingService dataProcessingService,
            CategoryService categoryService,
            LocationService locationService,
            HTMLDocumentCleanupService cleanupService,
            IHubContext<ScrapingHub> hubContext) 
        {
            _dataProcessingService = dataProcessingService;
            _categoryService = categoryService;
            _locationService = locationService;
            _cleanupService = cleanupService;
            _hubContext = hubContext;
            _randomGenerator = new Random();

            var handler = new HttpClientHandler { ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true };
            _httpClient = new HttpClient(handler);
            _httpClient.Timeout = TimeSpan.FromSeconds(25);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.36");
        }

        private List<ScrapeTask> GenerateScrapeTasks(List<DateTime> targetDates)
        {
            var tasks = new List<ScrapeTask>();

            foreach (var date in targetDates)
            {
                string dateStr = date.ToString("yyyy-MM-dd");

                tasks.Add(new ScrapeTask { SourceName = "Özgür Kocaeli", BaseUrl = "https://www.ozgurkocaeli.com.tr", EntryUrl = $"https://www.ozgurkocaeli.com.tr/arsiv/{dateStr}", ExactArchiveDate = date });
                tasks.Add(new ScrapeTask { SourceName = "Çağdaş Kocaeli", BaseUrl = "https://www.cagdaskocaeli.com.tr", EntryUrl = $"https://www.cagdaskocaeli.com.tr/arsiv/{dateStr}", ExactArchiveDate = date });
                tasks.Add(new ScrapeTask { SourceName = "Ses Kocaeli", BaseUrl = "https://www.seskocaeli.com", EntryUrl = $"https://www.seskocaeli.com/arsiv/{dateStr}", ExactArchiveDate = date });
                tasks.Add(new ScrapeTask { SourceName = "Bizim Yaka", BaseUrl = "https://www.bizimyaka.com", EntryUrl = $"https://www.bizimyaka.com/arsiv/{dateStr}", ExactArchiveDate = date });
            }

            tasks.Add(new ScrapeTask { SourceName = "Yeni Kocaeli", BaseUrl = "https://www.yenikocaeli.com", EntryUrl = "https://www.yenikocaeli.com/haber/guncel.html", ExactArchiveDate = null });
            tasks.Add(new ScrapeTask { SourceName = "Yeni Kocaeli", BaseUrl = "https://www.yenikocaeli.com", EntryUrl = "https://www.yenikocaeli.com/haber/polis-adliye.html", ExactArchiveDate = null });

            return tasks;
        }

        public async Task<List<NewsArticle>> ScrapeAllSitesAsync()
        {
            if (IsScraping)
            {
                Console.WriteLine(" Tarama zaten devam ediyor. Yeni görev başlatılmadı.");
                return new List<NewsArticle>();
            }

            IsScraping = true;
            var allArticles = new List<NewsArticle>();

            try
            {
                DateTime limitDate = DateTime.Now.Date.AddDays(-3);
                var datesToScrape = new List<DateTime> { DateTime.Now.Date, DateTime.Now.AddDays(-1).Date, DateTime.Now.AddDays(-2).Date };
                var scrapeTasks = GenerateScrapeTasks(datesToScrape);

                int totalTasks = scrapeTasks.Count;
                int currentTask = 0; 

                Console.WriteLine($"\n Arsiv taraması baslangici ");

                foreach (var task in scrapeTasks)
                {
                    string dateLog = task.ExactArchiveDate.HasValue ? task.ExactArchiveDate.Value.ToString("yyyy-MM-dd") : "Gundem";

                   
                    int progressPercent = (int)Math.Round((double)currentTask / totalTasks * 100);
                    await _hubContext.Clients.All.SendAsync("ReceiveProgress", progressPercent, $"{task.SourceName} ({dateLog}) taranıyor...");
                    Console.WriteLine($"\n>>> SİTE: {task.SourceName} | HEDEF: {dateLog} | İlerleme: %{progressPercent} <<<");

                    try
                    {
                        var html = await _httpClient.GetStringAsync(task.EntryUrl);
                        var htmlDoc = new HtmlDocument();
                        htmlDoc.LoadHtml(html);

                        var newsLinks = htmlDoc.DocumentNode.SelectNodes("//a[@href]");
                        if (newsLinks == null) continue;

                        var linksToScrape = newsLinks
                            .Select(n => n.GetAttributeValue("href", ""))
                            .Where(href =>
                                !string.IsNullOrWhiteSpace(href) &&
                                href.Length > 20 &&
                                (href.ToLower().Contains("/haber/") || href.Split('-').Length >= 4) &&
                                !href.ToLower().Contains("/kategori/") &&
                                !href.ToLower().Contains("/etiket/") &&
                                !href.ToLower().Contains("/yazar/") &&
                                !href.ToLower().Contains("page=") &&
                                !href.ToLower().Contains("/page/") &&
                                !href.ToLower().Contains("iletisim") &&
                                !href.ToLower().Contains("kunye") &&
                                !href.ToLower().Contains("haberler.html") &&
                                !href.ToLower().Contains("dunya") &&
                                !href.ToLower().Contains("dünya") &&
                                !href.ToLower().Contains("%20") &&
                                !href.ToLower().Contains("ekonomi") &&
                                !href.ToLower().Contains("magazin") &&
                                !href.ToLower().Contains("cedit") &&
                                !href.ToLower().Contains("kultur-sanat.html")&&
                                !href.ToLower().Contains("polis-adliye.html")

                                )
                            .Distinct()
                            .ToList();

                        foreach (var link in linksToScrape)
                        {
                            var fullUrl = link.StartsWith("http") ? link : task.BaseUrl.TrimEnd('/') + (link.StartsWith("/") ? "" : "/") + link;

                            try
                            {
                                var articleDoc = await GetHtmlDocumentAsync(fullUrl);
                                if (articleDoc == null) continue;


                            
                           
                            articleDoc = _cleanupService.ElementCleaning(articleDoc);
                           
                            if (task.SourceName == "Ses Kocaeli") articleDoc = _cleanupService.sesKocaeliCleanup(articleDoc);
                            


                                var rawTitle = articleDoc.DocumentNode.SelectSingleNode("//meta[@property='og:title']")?.GetAttributeValue("content", "")
                                            ?? articleDoc.DocumentNode.SelectSingleNode("//h1")?.InnerText;

                                if (rawTitle != null && (rawTitle.ToLower().Contains("arşivi") || rawTitle.ToLower() == "gündem")) continue;

                                string rawContent = "";
                                var paragraphs = articleDoc.DocumentNode.SelectNodes("//p");
                                if (paragraphs != null)
                                {
                                    var validParagraphs = paragraphs.Select(p => p.InnerText.Trim()).Where(text => text.Length > 30);
                                    rawContent = string.Join(" ", validParagraphs);
                                }

                                if (string.IsNullOrWhiteSpace(rawContent) || rawContent.Length < 100)
                                {
                                    rawContent = articleDoc.DocumentNode.SelectSingleNode("//meta[@name='description' or @property='og:description']")?.GetAttributeValue("content", "") ?? "";
                                }

                                if (string.IsNullOrWhiteSpace(rawTitle) || string.IsNullOrWhiteSpace(rawContent)) continue;

                                rawTitle = _dataProcessingService.CleanArticleContent(rawTitle);
                                rawContent = _dataProcessingService.CleanArticleContent(rawContent);

                                var category = _categoryService.DetermineCategory(rawContent, rawTitle);

                                if (category == "Diğer") continue;

                                DateTime finalPublishDate = task.ExactArchiveDate ?? DateTime.Now.Date;

                                if (!task.ExactArchiveDate.HasValue)
                                {
                                    var htmlText = articleDoc.DocumentNode.InnerText;
                                    var dateMatch = Regex.Match(htmlText, @"\b([0-2][0-9]|3[0-1]|[1-9])[\./-](0[1-9]|1[0-2]|[1-9])[\./-](\d{4})\b");
                                    if (dateMatch.Success && DateTime.TryParseExact(dateMatch.Value.Replace("-", ".").Replace("/", "."), new[] { "dd.MM.yyyy", "d.M.yyyy", "d.MM.yyyy", "dd.M.yyyy" }, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime regexDate))
                                    {
                                        finalPublishDate = regexDate;
                                    }
                                }

                                if (finalPublishDate.Date < limitDate) continue;

                                var locationText = _locationService.ExtractLocationFromText(rawContent);

                                if (locationText == null)
                                {
                                    string[] defaultLocations = { "İzmit, Kocaeli", "Gebze, Kocaeli", "Gölcük, Kocaeli", "Kartepe, Kocaeli", "Derince, Kocaeli", "Başiskele, Kocaeli" };
                                    locationText = defaultLocations[_randomGenerator.Next(defaultLocations.Length)];
                                }

                                var coordinates = await _locationService.GetCoordinatesAsync(locationText);
                                if (coordinates == null) continue;

                                double latOffset = (_randomGenerator.NextDouble() * 0.003) - 0.0015;
                                double lngOffset = (_randomGenerator.NextDouble() * 0.003) - 0.0015;

                                var newArticle = new NewsArticle
                                {
                                    Title = rawTitle,
                                    Content = rawContent,
                                    Category = category,
                                    Url = fullUrl,
                                    SourceNames = new List<string> { task.SourceName },
                                    PublishDate = finalPublishDate,
                                    LocationText = locationText,
                                    Latitude = coordinates.Value.Latitude + latOffset,
                                    Longitude = coordinates.Value.Longitude + lngOffset
                                };

                                allArticles.Add(newArticle);
                                Console.WriteLine("++ Haber Çekildi: " + rawTitle);
                            }
                            catch {  }
                        }
                    }
                    catch(Exception ex) { Console.WriteLine("Site Hatası " + ex.Message); }

                    
                    currentTask++;
                }

                
                Console.WriteLine($"\n İŞLEM BİTTİ. Toplam Çekilen Haber: {allArticles.Count}");
                await _hubContext.Clients.All.SendAsync("ReceiveProgress", 100, $"Tarama Tamamlandı! Toplam {allArticles.Count} haber çekildi.");
            }
            finally
            {
                IsScraping = false;
            }

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