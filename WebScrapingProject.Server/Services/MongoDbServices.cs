using Microsoft.Extensions.Options;
using MongoDB.Driver;
using WebScrapingProject.Server.Models;

namespace WebScrapingProject.Server.Services
{
    public class MongoDbService
    {
        private readonly IMongoCollection<NewsArticle> _newsCollection;

        public MongoDbService(IOptions<MongoDbSettings> mongoDbSettings, IMongoClient mongoClient)
        {
            var database = mongoClient.GetDatabase(mongoDbSettings.Value.DatabaseName);
            _newsCollection = database.GetCollection<NewsArticle>(mongoDbSettings.Value.NewsCollectionName);
        }

        public async Task CreateTestArticleAsync(NewsArticle newArticle)
        {
            await _newsCollection.InsertOneAsync(newArticle);
        }

   
        public async Task ProcessAndSaveArticleAsync(NewsArticle newArticle, SimilarityService similarityService)
        {
           
            string combinedText = $"{newArticle.Title}. {newArticle.Content}";
            newArticle.ContentEmbedding = await similarityService.GetEmbeddingAsync(combinedText);

          
            if (newArticle.ContentEmbedding == null || newArticle.ContentEmbedding.Length == 0) return;

           
            var limitDate = DateTime.Now.Date.AddDays(-3);
            var filter = Builders<NewsArticle>.Filter.Gte(a => a.PublishDate, limitDate);

            var existingArticles = await _newsCollection.Find(filter).ToListAsync();

            bool isDuplicate = false;

            foreach (var existing in existingArticles)
            {

                if (existing.ContentEmbedding == null || existing.ContentEmbedding.Length == 0) continue;

              
                double semanticSimilarity = similarityService.CalculateCosineSimilarity(newArticle.ContentEmbedding, existing.ContentEmbedding);

               
                if (semanticSimilarity >= 0.85)
                {
                    isDuplicate = true;

                    
                    var newSource = newArticle.SourceNames.FirstOrDefault();
                    if (newSource != null && !existing.SourceNames.Contains(newSource))
                    {
                        existing.SourceNames.Add(newSource);

                        var updateFilter = Builders<NewsArticle>.Filter.Eq(a => a.Id, existing.Id);
                        var update = Builders<NewsArticle>.Update.Set(a => a.SourceNames, existing.SourceNames);
                        await _newsCollection.UpdateOneAsync(updateFilter, update);

                        Console.WriteLine($"   aynı haberler birleştiriliyor  Cosine değeri: %{Math.Round(semanticSimilarity * 100)}. '{newSource}' kaynağı eklendi.");
                    }
                    else
                    {
                        Console.WriteLine($"  benzer haber atlanıyor  zaten '{newSource}' kaynağını içeriyor.");
                    }

                    break;
                }
            }

          
            if (!isDuplicate)
            {
                await _newsCollection.InsertOneAsync(newArticle);
            }
        }

        
        public async Task<List<NewsArticle>> GetAllAsync()
        {
           
            var limitDate = DateTime.Now.Date.AddDays(-3);
            var filter = Builders<NewsArticle>.Filter.Gte(x => x.PublishDate, limitDate);

           
            return await _newsCollection.Find(filter)
                                        .SortByDescending(x => x.PublishDate)
                                        .ToListAsync();
        }
    }
}