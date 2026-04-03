using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WebScrapingProject.Server.Models
{
    public class NewsArticle
    {
         
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

       

        [BsonElement("category")]
        public string Category { get; set; } = null!; 

        [BsonElement("title")]
        public string Title { get; set; } = null!; 

        [BsonElement("content")]
        public string Content { get; set; } = null!; 

        [BsonElement("locationText")]
        public string LocationText { get; set; } = null!; 

        
        [BsonElement("latitude")]
        public double Latitude { get; set; }

        [BsonElement("longitude")]
        public double Longitude { get; set; }

        [BsonElement("publishDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime PublishDate { get; set; }

        [BsonElement("sourceNames")]
        public List<string> SourceNames { get; set; } = new();

        [BsonElement("url")]
        public string Url { get; set; } = null!; 

        
        [BsonElement("isProcessed")]
        public bool IsProcessed { get; set; } = true;

        public float[]? ContentEmbedding { get; set; }
    }
}