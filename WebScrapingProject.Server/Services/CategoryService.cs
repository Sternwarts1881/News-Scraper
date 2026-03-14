namespace WebScrapingProject.Server.Services
{
    public class CategoryService
    {
       
        private readonly Dictionary<string, Dictionary<string, int>> _categoryWeights = new()
        {
            { "Trafik Kazası", new Dictionary<string, int> {
                { "kaza", 5 }, { "çarpıştı", 4 }, { "devrildi", 4 }, { "şarampole", 4 },
                { "zincirleme", 3 }, { "maddi hasarlı", 3 }, { "otomobil", 1 }, { "tır", 1 }
            }},
            { "Yangın", new Dictionary<string, int> {
                { "yangın", 5 }, { "alev", 4 }, { "kül oldu", 4 }, { "itfaiye", 2 },
                { "kundaklama", 3 }, { "söndürüldü", 3 }, { "duman", 2 }
            }},
            { "Elektrik Kesintisi", new Dictionary<string, int> {
                { "elektrik kesintisi", 5 }, { "sedaş", 4 }, { "karanlıkta kaldı", 4 },
                { "planlı kesinti", 3 }, { "trafo", 3 }, { "ariza", 2 }
            }},
            { "Hırsızlık", new Dictionary<string, int> {
                { "hırsız", 5 }, { "çaldı", 4 }, { "soygun", 4 }, { "gasp", 4 },
                { "yankesici", 3 }, { "kasa", 2 }, { "yakalandı", 1 }
            }},
            { "Kültürel Etkinlikler", new Dictionary<string, int> {
                { "konser", 5 }, { "tiyatro", 5 }, { "sergi", 4 }, { "festival", 4 },
                { "fuar", 3 }, { "etkinlik", 2 }, { "sanat", 2 }, { "sahne", 2 }
            }}
        };

        
        private readonly List<string> _negativeKeywords = new()
        {
            "intihar", "cinayet", "öldürdü", "silahlı saldırı", "siyaset",
            "parti", "milletvekili", "transfer", "şampiyon", "penaltı"
        };

        public string DetermineCategory(string content, string title)
        {
            var textToAnalyze = $"{title} {content}".ToLowerInvariant();

           
            foreach (var negWord in _negativeKeywords)
            {
                if (textToAnalyze.Contains(negWord)) return "Diğer";
            }

            string bestCategory = "Diğer";
            int maxScore = 0;

            foreach (var category in _categoryWeights)
            {
                int currentScore = 0;

                foreach (var keyword in category.Value)
                {
                   
                    int count = (textToAnalyze.Length - textToAnalyze.Replace(keyword.Key, "").Length) / keyword.Key.Length;
                    currentScore += count * keyword.Value;
                }

                
                if (currentScore > maxScore && currentScore >= 4)
                {
                    maxScore = currentScore;
                    bestCategory = category.Key;
                }
            }

            return bestCategory;
        }
    }
}