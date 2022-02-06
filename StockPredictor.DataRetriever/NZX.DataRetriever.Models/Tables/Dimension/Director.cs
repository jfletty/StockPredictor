using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StockPredictor.DataRetriever.Domain.Tables.Dimension
{
    [Table("Director", Schema = "Dim")]
    public class Director
    {
        [Key]
        public int DirectorKey { get; set; }
        public string DirectorName { get; set; }
    }
}
