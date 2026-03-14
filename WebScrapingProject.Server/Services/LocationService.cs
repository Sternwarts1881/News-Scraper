using System.Text.Json;
using System.Text.RegularExpressions;

namespace WebScrapingProject.Server.Services
{
    public class LocationService
    {
        private readonly HttpClient _httpClient;

        
        private readonly string _googleApiKey = "BURAYA_API_KEY_GELECEK";

        
        private readonly Dictionary<string, List<string>> _kocaeliLocations = new()
        {
            { "İzmit", new List<string> { "yahya kaptan", "kozluk", "gündoğdu", "bekirdere", "çarşı", "yürüyüş yolu", "sekapark" } },
            { "Gölcük", new List<string> { "değirmendere", "ihsaniye", "kavaklı", "halıdere", "ulaşlı", "hisareyn" } },
            { "Gebze", new List<string> { "mutlukent", "osman yılmaz", "hacıhalil", "arapçeşme" } },
            { "Karamürsel", new List<string> { "kayacık", "4 temmuz", "ereğli" } },
            { "Kartepe", new List<string> { "uzunçiftlik", "köseköy", "derbent", "maşukiye" } },
            { "Derince", new List<string> { "çınarlı", "yenikent", "sırrıpaşa" } },
            { "Körfez", new List<string> { "yarımca", "tütünçiftlik", "hereke", "kirazlıyalı" } },
            { "Başiskele", new List<string> { "yuvacık", "yeniköy", "bahçecik", "karşıyaka" } },
            { "Kandıra", new List<string> { "kefken", "kerpe", "cebeci", "akçakoca" } },
            { "Dilovası", new List<string> { "diliskelesi", "tavşancıl" } },
            { "Çayırova", new List<string> { "özgürlük", "emek" } },
            { "Darıca", new List<string> { "bayramoğlu", "osmangazi", "sırasöğütler" } }
        };

        public LocationService()
        {
            _httpClient = new HttpClient();
        }

     
        public string? ExtractLocationFromText(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;

            var lowerText = text.ToLowerInvariant();

            
            foreach (var district in _kocaeliLocations)
            {
                foreach (var neighborhood in district.Value)
                {
                    
                    if (Regex.IsMatch(lowerText, $@"\b{neighborhood}\b"))
                    {
                        
                        return $"{ToTitleCase(neighborhood)}, {district.Key}, Kocaeli";
                    }
                }
            }

           
            foreach (var district in _kocaeliLocations)
            {
                if (Regex.IsMatch(lowerText, $@"\b{district.Key.ToLowerInvariant()}\b"))
                {
                    
                    return $"{district.Key}, Kocaeli";
                }
            }

            
            return null;
        }

       
        public async Task<(double Latitude, double Longitude)?> GetCoordinatesAsync(string locationText)
        {
            if (string.IsNullOrEmpty(locationText)) return null;

            try
            {
               
                var encodedAddress = Uri.EscapeDataString(locationText);
                var url = $"https://maps.googleapis.com/maps/api/geocode/json?address={encodedAddress}&key={_googleApiKey}";

                var response = await _httpClient.GetStringAsync(url);
                using var jsonDoc = JsonDocument.Parse(response);

                var root = jsonDoc.RootElement;
                if (root.GetProperty("status").GetString() == "OK")
                {
                    var location = root.GetProperty("results")[0]
                                       .GetProperty("geometry")
                                       .GetProperty("location");

                    double lat = location.GetProperty("lat").GetDouble();
                    double lng = location.GetProperty("lng").GetDouble();

                    return (lat, lng);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Geocoding hatası ({locationText}): {ex.Message}");
            }

            return null; 
        }

        
        private string ToTitleCase(string title)
        {
            return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(title.ToLower());
        }
    }
}