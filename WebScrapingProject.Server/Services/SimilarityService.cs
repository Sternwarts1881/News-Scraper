namespace WebScrapingProject.Server.Services
{
    public class SimilarityService
    {
        
        public double CalculateCosineSimilarity(string text1, string text2)
        {
            if (string.IsNullOrWhiteSpace(text1) || string.IsNullOrWhiteSpace(text2)) return 0;

            var words1 = GetWordFrequency(text1);
            var words2 = GetWordFrequency(text2);
            var allWords = words1.Keys.Union(words2.Keys).Distinct().ToList();

            double dotProduct = 0;
            double magnitude1 = 0;
            double magnitude2 = 0;

            foreach (var word in allWords)
            {
                double val1 = words1.ContainsKey(word) ? words1[word] : 0;
                double val2 = words2.ContainsKey(word) ? words2[word] : 0;

                dotProduct += val1 * val2;
                magnitude1 += val1 * val1;
                magnitude2 += val2 * val2;
            }

            magnitude1 = Math.Sqrt(magnitude1);
            magnitude2 = Math.Sqrt(magnitude2);

            if (magnitude1 == 0 || magnitude2 == 0) return 0;

            return dotProduct / (magnitude1 * magnitude2);
        }

        private Dictionary<string, int> GetWordFrequency(string text)
        {
            
            var words = text.ToLowerInvariant().Split(new[] { ' ', '.', ',', '!', '?', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            var freq = new Dictionary<string, int>();

            foreach (var w in words)
            {
                if (w.Length < 3) continue;
                if (freq.ContainsKey(w)) freq[w]++;
                else freq[w] = 1;
            }
            return freq;
        }
    }
}