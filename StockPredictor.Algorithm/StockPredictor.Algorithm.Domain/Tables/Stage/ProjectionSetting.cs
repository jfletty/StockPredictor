using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using StockPredictor.Algorithm.Domain.Enums;

namespace StockPredictor.Algorithm.Domain.Tables.Stage
{
    [Table("ProjectionSetting", Schema = "Stage")]
    public class ProjectionSetting
    {
        [Key]
        public int ProjectionSettingKey { get; set; }
        public int StockKey { get; set; }
        public UpdateType UpdateType { get; set; }
        public JobStatus Status { get; set; }
        public string FailedMessage { get; set; }
        public DateTimeOffset StartDateTime { get; set; }
        public DateTimeOffset? EndDateTime { get; set; }

        
        public void UpdateAfterCompletion(Exception error = null)
        {
            if (error != null)
            {
                Status = JobStatus.Failed;
                FailedMessage = error.Message;
            }
            else
            {
                Status = JobStatus.Complete;
            }
            EndDateTime = DateTime.UtcNow;
        }
    }
}
