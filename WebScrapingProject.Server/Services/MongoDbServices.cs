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
            
            var recentDate = DateTime.Now.AddDays(-4);
            var existingArticles = await _newsCollection.Find(a => a.PublishDate >= recentDate).ToListAsync();

            bool isDuplicate = false;

            foreach (var existing in existingArticles)
            {
               
                double similarityScore = similarityService.CalculateCosineSimilarity(newArticle.Content, existing.Content);

             
                if (similarityScore >= 0.90)
                {
                    isDuplicate = true;

                  
                    var newSource = newArticle.SourceNames.FirstOrDefault();
                    if (newSource != null && !existing.SourceNames.Contains(newSource))
                    {
                        existing.SourceNames.Add(newSource);

                        var filter = Builders<NewsArticle>.Filter.Eq(a => a.Id, existing.Id);
                        var update = Builders<NewsArticle>.Update.Set(a => a.SourceNames, existing.SourceNames);
                        await _newsCollection.UpdateOneAsync(filter, update);

                        Console.WriteLine($"benzer! %{Math.Round(similarityScore * 100)} benzerlik. '{newSource}' kaynağı mevcut habere eklendi.");
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
            return await _newsCollection.Find(_ => true).ToListAsync();
        }
    }
}