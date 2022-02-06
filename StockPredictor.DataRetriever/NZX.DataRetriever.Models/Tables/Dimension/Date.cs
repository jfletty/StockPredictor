using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StockPredictor.DataRetriever.Domain.Tables.Dimension
{
    [Table("Date", Schema = "Dim")]
    public class Date
    {
        [Key]
        public short DateKey { get; set; }
        public DateTime DateTime { get; set; }
        public string LongDateDescription { get; set; }
        public string ShortDateDescription { get; set; }
        public string DateLabel { get; set; }
        public byte Weekday { get; set; }
        public string WeekdayName { get; set; }
        public byte DayNumberInMonth { get; set; }
        public byte MonthNumber { get; set; }
        public string MonthName { get; set; }
    }
}
