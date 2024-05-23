using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace CryptoPortfolioService_Data.Entities
{
    public class AlarmNotification : TableEntity
    {
        public AlarmNotification()
        {
        }

        public AlarmNotification(DateTime timestamp, string alarmId, int numberOfEmailsSent)
        {
            this.PartitionKey = timestamp.ToString("yyyy-MM-dd");
            this.RowKey = Guid.NewGuid().ToString();
            this.Timestamp = timestamp;
            this.AlarmId = alarmId;
            this.NumberOfEmailsSent = numberOfEmailsSent;
        }

        public string AlarmId { get; set; }

        public int NumberOfEmailsSent { get; set; }
    }
}
