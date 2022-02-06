using System;

namespace StockPredictor.DataRetriever.Domain.Tables.Api
{
    public class ApiAnnouncement
    {
        public int AnnouncementNumber { get; set; }
        public int AnnouncementPageNumber { get; set; }
        public int AnnouncementLineNumber { get; set; }
        public string AnnouncementSource { get; set; }
        public string SecurityCode { get; set; }
        public string ISIN { get; set; }
        public bool LastLineOfPageFlag { get; set; }
        public bool LastLineOfAnnouncement { get; set; }
        public string AnnouncementLine { get; set; }
        public DateTime GeneratedDateTime { get; set; }
    }
}
//CA - Company Announcement