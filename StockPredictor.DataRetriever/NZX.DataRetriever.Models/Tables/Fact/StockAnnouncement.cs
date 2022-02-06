using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StockPredictor.DataRetriever.Domain.Tables.Fact
{
    [Table("StockAnnouncement", Schema = "Fact")]
    public class StockAnnouncement
    {
        [Key]
        public int StockAnnouncementKey { get; set; }
        public int StockKey { get; set; }
        public int AnnouncementTypeKey { get; set; }
        public int SentimentKey { get; set; }
        public int DateKey { get; set; }
        public short TimeOfDayKey { get; set; }
        public string Title { get; set; }
        public string Uri { get; set; }
        public bool RelatedToDirector { get; set; }
        public bool RelatedToCompany { get; set; }
        public bool RelatedToCountry { get; set; }
        public bool RelatedToWorld { get; set; }
        
        
        public bool Equals(StockAnnouncement obj)
        {
            if (obj.TimeOfDayKey != TimeOfDayKey) return false;
            if (obj.StockKey != StockKey) return false;
            if (obj.DateKey != DateKey) return false;
            if (obj.Title != Title) return false;
            if (obj.SentimentKey != SentimentKey) return false;
            if (obj.AnnouncementTypeKey != AnnouncementTypeKey) return false;
            if (obj.RelatedToDirector != RelatedToDirector) return false;
            if (obj.RelatedToCompany != RelatedToCompany) return false;
            if (obj.RelatedToCountry != RelatedToCountry) return false;
            if (obj.RelatedToWorld != RelatedToWorld) return false;

            return true;
        }

        public void Update(StockAnnouncement announcement)
        {
            StockKey = announcement.StockKey;
            AnnouncementTypeKey = announcement.AnnouncementTypeKey;
            SentimentKey = announcement.SentimentKey;
            DateKey = announcement.DateKey;
            TimeOfDayKey = announcement.TimeOfDayKey;
            Title = announcement.Title;
        }
    }
}
