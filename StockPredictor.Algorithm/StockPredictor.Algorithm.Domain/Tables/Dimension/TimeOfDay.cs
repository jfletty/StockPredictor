using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StockPredictor.Algorithm.Domain.Tables.Dimension
{
    [Table("TimeOfDay", Schema = "Dim")]
    public class TimeOfDay
    {
        [Key]
        public byte TimeOfDayKey { get; set; }
        public TimeSpan Time { get; set; }
        
        public TimeSpan Hour { get; set; }
        public byte HourOfDay { get; set; }
        public byte MinuteOfHour { get; set; }
        public byte PeriodIndex { get; set; }
    }
}
