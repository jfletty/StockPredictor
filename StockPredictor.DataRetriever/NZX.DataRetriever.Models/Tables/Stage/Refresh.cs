using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using StockPredictor.DataRetriever.Domain.Enums;

namespace StockPredictor.DataRetriever.Domain.Tables.Stage
{
    [Table("Refresh", Schema = "Stage")]
    public class Refresh
    {
        [Key, Column(Order = 0)]
        public string ExternalKey { get; set; }
        public JobStatus Status { get; set; }
        
        [Key, Column(Order = 1)]
        public JobType RefreshType { get; set; }
        public string FailedMessage { get; set; }
        public DateTimeOffset StartDateTime { get; set; }
        public DateTimeOffset? EndDateTime { get; set; }
        public int FailedAttempt { get; private set; }
        
        public void UpdateAfterRefresh(Exception error = null)
        {
            if (error != null)
            {
                Status = JobStatus.Failed;
                FailedMessage = error.Message;
                FailedAttempt++;
            }
            else
            {
                Status = JobStatus.Complete;
                FailedAttempt = 0;
            }
            
            EndDateTime = DateTime.UtcNow;

            if (FailedAttempt >= 5)
            {
                StartDateTime = StartDateTime.AddDays(30);
                EndDateTime = EndDateTime?.AddDays(30);
            }
        }

        public void ResetBeforeRefresh()
        {
            Status = JobStatus.Pending;
            FailedMessage = null;
            StartDateTime = DateTimeOffset.UtcNow;
            EndDateTime = null;
        }
    }
}
