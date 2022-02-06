using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StockPredictor.DataRetriever.Domain.Tables.Dimension
{
    [Table("AnnouncementType", Schema = "Dim")]
    public class AnnouncementType
    {
        [Key]
        public int AnnouncementTypeKey { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
