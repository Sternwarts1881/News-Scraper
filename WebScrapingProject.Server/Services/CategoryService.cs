using System;
using System.Collections.Generic;

namespace WebScrapingProject.Server.Services
{
    public class CategoryRule
    {
        public List<string> HighValueKeywords { get; set; } = new(); 
        public List<string> NormalKeywords { get; set; } = new(); 
        public List<string> NegativeKeywords { get; set; } = new(); 
    }

    public class CategoryService
    {
        private readonly Dictionary<string, CategoryRule> _categoryRules = new();

        public CategoryService()
        {
          
            _categoryRules.Add("Trafik Kazası", new CategoryRule
            {
                HighValueKeywords = new List<string> { "trafik kazası", "zincirleme kaza", "şarampole", "çarpıştı", "takla attı", "kafa kafaya", "araç devrildi"
                ,"araca çarptı","ağır hasar"
                },
                NormalKeywords = new List<string> { "kaza", "yaralı", "maddi hasar", "direksiyon hakimiyetini", "otomobil", "kamyon", "motosiklet", "olay yerine", "sağlık ekipleri", 
                    "ambulans", "kontrolünden çıktı", "savruldu", "refüje", "çarptı","çarptığı ","devrildi","kazada"
                },
                NegativeKeywords = new List<string> { "iş kazası", "kaza kurşunu", "ucuz atlattı", "maç", "futbol" }
            });

           
            _categoryRules.Add("Yangın", new CategoryRule
            {
                HighValueKeywords = new List<string> { "yangın çıktı", "alev alev", "itfaiye ekipleri", "küle döndü", "kundaklama", "cayır cayır", "yangın", "yangında",
                "yangının","yaktı","yandı","yaktılar"
                },
                NormalKeywords = new List<string> {  "alev", "duman", "itfaiye", "söndürüldü",  "soğutma çalışması", "mahsur kaldı", "tahliye", "ateş" },
                NegativeKeywords = new List<string> { "ateş açtı", "silah", "çatışma", "şampiyonluk ateşi" }
            });


            _categoryRules.Add("Elektrik Kesintisi", new CategoryRule
            {
                HighValueKeywords = new List<string> {
                    "elektrik kesintisi", "sedaş duyurdu", "karanlığa gömüldü", "trafo patladı",
                    "enerji verilemeyecek", "şebeke arızası", "elektrikler kesilecek", "planlı kesinti",
                    "trafo merkezi", "kablo koptu","elektriksiz kalacak","sedaş","elektrik kesintileri",
                    "elektrik kesintileriyle","Elektrikler Ne Zaman Gelecek"
                },
                NormalKeywords = new List<string> {
                    "elektrik", "kesinti", "sedaş", "trafo", "arıza", "şebeke", "kablo", "aydınlatma",
                    "voltaj", "akım", "jeneratör", "karanlık", "bakım onarım", "şalter", "sigorta",
                    "teknisyen", "arıza kaydı", "direk devrildi", "yüksek gerilim", "pano",
                    "şebeke iyileştirme", "enerji nakil", "elektrikçi", "tesisat", "hat çekimi",
                    "planlı bakım", "arıza giderme", "altyapı çalışması", "sayaç", "hat yenileme",
                    "elektrik direği", "sigorta attı", "kısa devre", "sokak lambası", "arıza servisi","kesintisi"
                    
                },
                NegativeKeywords = new List<string> {
                    "togg", "elektrikli araç", "elektrikli bisiklet", "elektrikli scooter",
                    "fatura", "zam", "elektrik faturası", "elektrikli motor", "doğalgaz", "akaryakıt","su kesintisi", "su"
                }
            });

            
            _categoryRules.Add("Hırsızlık", new CategoryRule
            {
                HighValueKeywords = new List<string> {
                    "hırsızlık", "soygun", "gasp", "kapkaç",
                    "nitelikli dolandırıcılık", "siber suçlar", "hırsız","çaldı","çalan","hırsızlar"
                    ,"çalarak","çalınmasıyla","hırsızlıktan", "yankesici", "soyarak"
                },
                NormalKeywords = new List<string> {
                    "polis", "jandarma", "emniyet", "zanlı", "şüpheli", 
                    "cezaevine gönderildi",
                    "adliye", "gözaltına alındı", "tutuklandı", "suçüstü",
                    "hükümlü", "firari", "aranan şahıs","ihbar", "sahte",
                     "yakalama kararı", "firari",
                    "fiziki takip", "teknik takip", "polis ekipleri", "asayiş şube", "kaçak", "el konuldu",
                    "dolandırıcı",  "kovalamaca",
                },
                NegativeKeywords = new List<string> {
                    "kalbini çaldı", "rol çaldı", "zaman çaldı", "maç", "hakem", "gol", "transfer",
                    "şampiyon", "sahadan", "turnuva", "altın madalya", "kupa", "süper lig", "narkotik", "cinayet", "tetikçi", 
                    "terör", "yolsuzluk"
                    ,"şantaj", "esrar", "eroin", "metamfetamin","silahlı saldırı", "rüşvet"
                }
            });

            
            _categoryRules.Add("Kültürel Etkinlikler", new CategoryRule
            {
                HighValueKeywords = new List<string> {
                    "konser", "festival", "tiyatro oyunu", "resim sergisi", "imza günü", "açılış töreni", "sergi", "fuar", "etkinlik", "sahne", "tören", "kutlama",
                    "sahne aldı", "müzik dinletisi", "kültür merkezi", "sanat galerisi", "gala gecesi","kültür","şarkı", "türkü", "gösteri", "dans", "halk oyunları", "folklor",
                    "sanatseverler", "koro", "orkestra", "kurdele kesildi", "sanat", "ressam", "oyuncu", "sanatçı", "müzik","şenlik", "yürüyüş"
                },
                NormalKeywords = new List<string> {
                    "konferans", "yazar", "şair", "belediye başkanı", "ziyaret", "program", "anma",
                    "iftar", "panel", "sempozyum", "söyleşi",
                    "sinema", "film", "seminer", "kürsüye çıktı", "ramazan",
                    "perde", "dekor", "müze", "ören yeri", "tarihi eser", "protokol", "vali",
                    "kaymakam", "rektör", "üniversite", "mezuniyet", "ödül", "plaket", "yarışma",
                    "kermes", "fener alayı", "kütüphane", "eğitim",
                    "öğrenci", "kurs", "kitap"
                },
                NegativeKeywords = new List<string> {
                    "cenaze töreni", "şehit", "operasyon", "kaza", "vefat", "acı kayıp", "cinayet",
                    "trafik", "yangın", "hastane", "tedavi", "yoğun bakım", "maç", "hakem", "gol", "transfer",
                    "şampiyon", "sahadan", "turnuva", "altın madalya", "kupa", "süper lig", "şampiyonluk ateşi", "futbol"
                    , "futbolcu","politika","politik"
                }
            });
        }

        public string DetermineCategory(string content, string title)
        {
            var textToAnalyze = $"{title} {content}".ToLowerInvariant();

            string bestCategory = "Diğer";
            int maxScore = 0;
            int minimumThreshold = 30; 

            foreach (var category in _categoryRules)
            {
                int currentScore = 0;

               
                foreach (var word in category.Value.HighValueKeywords)
                {
                    if (textToAnalyze.Contains(word)) currentScore += 13;
                }

               
                foreach (var word in category.Value.NormalKeywords)
                {
                    if (textToAnalyze.Contains(word)) currentScore += 3;
                }

               
                foreach (var word in category.Value.NegativeKeywords)
                {
                    if (textToAnalyze.Contains(word)) currentScore -= 50;
                }

                if (currentScore >= minimumThreshold && currentScore > maxScore)
                {
                    maxScore = currentScore;
                    bestCategory = category.Key;
                }
            }

            return bestCategory;
        }
    }
}