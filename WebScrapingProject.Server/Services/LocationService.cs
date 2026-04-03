using System.Globalization;
using System.Net.Http.Json;
using System.Text.RegularExpressions;

namespace WebScrapingProject.Server.Services
{
    public class GeocodeResponse
    {
        public string lat { get; set; } = "";
        public string lon { get; set; } = "";
    }

    public class LocationService
    {
        private readonly HttpClient _httpClient;

        private readonly Dictionary<string, (double Lat, double Lng)> _safeDistrictCenters = new()
        {
            {"izmit", (40.7665, 29.9400)},
            {"gölcük", (40.7160, 29.8220)},
            {"gebze", (40.8020, 29.4300)},
            {"başiskele", (40.7100, 29.9250)},
            {"kartepe", (40.7450, 30.0150)},
            {"derince", (40.7600, 29.8300)},
            {"körfez", (40.7736, 29.7456)},
            {"karamürsel", (40.6880, 29.6150)},
            {"kandıra", (41.0700, 30.1500)},
            {"dilovası", (40.7900, 29.5400)},
            {"çayırova", (40.8200, 29.3800)},
            {"darıca", (40.7650, 29.3900)}
        };

        public LocationService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "KocaeliNewsScraper/1.0");
        }

        public string? ExtractLocationFromText(string text)
        {
            string specificLocation = "";

            var addressMatch = Regex.Match(text, @"(?:[A-ZÇĞİÖŞÜ][a-zçğıöşü]+\s){1,3}(?:Mahallesi|Caddesi|Bulvarı|Sokağı|Mevkii)", RegexOptions.None);
            if (addressMatch.Success)
            {
                specificLocation += addressMatch.Value + ", ";
            }
            else
            {
                var roadMatch = Regex.Match(text, @"\b(D-?100|D-?130|TEM Otoyolu|Kuzey Marmara Otoyolu|Anadolu Otoyolu)\b", RegexOptions.IgnoreCase);
                if (roadMatch.Success)
                {
                    specificLocation += roadMatch.Value + ", ";
                }
            }

            string lowerText = text.ToLower(new CultureInfo("tr-TR"));
            string foundDistrict = "";
            foreach (var district in _safeDistrictCenters.Keys)
            {
                if (Regex.IsMatch(lowerText, $@"\b{district}\b"))
                {
                    foundDistrict = district;
                    break;
                }
            }

            if (!string.IsNullOrWhiteSpace(specificLocation))
            {
                string finalLocation = $"{specificLocation}{foundDistrict} Kocaeli".Replace(" ,", ",").Replace("  ", " ").Trim(',', ' ');
                return finalLocation;
            }

            if (!string.IsNullOrWhiteSpace(foundDistrict))
            {
                return $"{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(foundDistrict)}, Kocaeli";
            }

            return null;
        }

        public async Task<(double Latitude, double Longitude)?> GetCoordinatesAsync(string locationText)
        {
            var loc = locationText.ToLower(new CultureInfo("tr-TR")).Replace(", kocaeli", "").Trim();

            if (_safeDistrictCenters.ContainsKey(loc))
            {
                return _safeDistrictCenters[loc];
            }

            try
            {
                string apiUrl = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(locationText)}&countrycodes=tr&format=json&limit=1";
                var response = await _httpClient.GetFromJsonAsync<List<GeocodeResponse>>(apiUrl);

                if (response != null && response.Count > 0)
                {
                    double lat = double.Parse(response[0].lat, CultureInfo.InvariantCulture);
                    double lng = double.Parse(response[0].lon, CultureInfo.InvariantCulture);

                    Console.WriteLine($"   [API-BAŞARILI] {locationText} Bulunan gerçek konum = {lat}, {lng}");
                    return (lat, lng);
                }
            }
            catch
            {
                
            }

            foreach (var district in _safeDistrictCenters.Keys)
            {
                if (loc.Contains(district))
                {
                    Console.WriteLine($" {locationText} bulunamadı  en spesifik konum olan, {district} merkezine konumlandı.");
                    return _safeDistrictCenters[district];
                }
            }

           
          
            return null;
        }
    }
}