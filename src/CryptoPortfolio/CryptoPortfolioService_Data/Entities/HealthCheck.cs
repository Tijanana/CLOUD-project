using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoPortfolioService_Data.Entities
{
    public class HealthCheck : TableEntity
    {
        public HealthCheck()
        {
            PartitionKey = "HealthCheck";
            RowKey = DateTime.UtcNow.ToString("o"); // ISO 8601 format
        }

        public string Status { get; set; }
        public string Service { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
