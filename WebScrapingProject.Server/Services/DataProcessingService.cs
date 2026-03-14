using System.Text.RegularExpressions;
using System.Web;
using HtmlAgilityPack;

namespace WebScrapingProject.Server.Services
{
    public class DataProcessingService
    {
       
        public string CleanArticleContent(string rawHtmlText)
        {
            if (string.IsNullOrWhiteSpace(rawHtmlText))
                return string.Empty;

            var processedText = rawHtmlText;

            
            processedText = RemoveHtmlTags(processedText);

           
            processedText = NormalizeText(processedText);

           
            processedText = RemoveSpecialCharacters(processedText);

           
            processedText = RemoveExtraSpaces(processedText);

            return processedText.Trim();
        }

       

        private string RemoveHtmlTags(string input)
        {
            
            return Regex.Replace(input, "<.*?>", string.Empty);
        }

        private string NormalizeText(string input)
        {
           
            var decoded = HttpUtility.HtmlDecode(input);

            
            decoded = decoded.Replace("\r", " ").Replace("\n", " ").Replace("\t", " ");
            return decoded;
        }

        private string RemoveSpecialCharacters(string input)
        {
           
            return Regex.Replace(input, @"[^a-zA-Z0-9çğıöşüÇĞİÖŞÜ.,!?() \-]", " ");
        }

        private string RemoveExtraSpaces(string input)
        {
            
            return Regex.Replace(input, @"\s+", " ");
        }
    }
}