namespace WebScrapingProject.Server.Models
{
    public class MongoDbSettings
    {     
        //burası cok onemlı değil db baglantısı icin bisi 
        public string ConnectionString { get; set; } = null!;
        public string DatabaseName { get; set; } = null!;
        public string NewsCollectionName { get; set; } = null!;
    }
}