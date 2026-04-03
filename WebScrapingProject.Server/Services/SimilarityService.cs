using System.Text;
using System.Text.Json;

namespace WebScrapingProject.Server.Services
{
    public class SimilarityService
    {
        private readonly HttpClient _httpClient;

     
        public SimilarityService()
        {
            _httpClient = new HttpClient();
        }

       
        public async Task<float[]> GetEmbeddingAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return Array.Empty<float>();

            try
            {
                
                string url = "http://localhost:11434/api/embed";

                var requestBody = new
                {
                    model = "nomic-embed-text", 
                    input = text
                };

                var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    using var document = JsonDocument.Parse(jsonResponse);

                    
                    var embeddingArray = document.RootElement
                        .GetProperty("embeddings")[0] 
                        .EnumerateArray()
                        .Select(x => x.GetSingle())
                        .ToArray();

                    return embeddingArray;
                }
                else
                {
                  
                }
            }
            catch (Exception ex)
            {
                
                Console.WriteLine($"   [!] Lokal Sunucuya Ulaşılamadı: {ex.Message}");
            }

            return Array.Empty<float>();
        }

        
        public double CalculateCosineSimilarity(float[] vector1, float[] vector2)
        {
            if (vector1 == null || vector2 == null || vector1.Length == 0 || vector1.Length != vector2.Length)
                return 0;

            double dotProduct = 0;
            double magnitude1 = 0;
            double magnitude2 = 0;

            for (int i = 0; i < vector1.Length; i++)
            {
                dotProduct += vector1[i] * vector2[i];
                magnitude1 += Math.Pow(vector1[i], 2);
                magnitude2 += Math.Pow(vector2[i], 2);
            }

            if (magnitude1 == 0 || magnitude2 == 0) return 0;

            return dotProduct / (Math.Sqrt(magnitude1) * Math.Sqrt(magnitude2));
        }
    }
}