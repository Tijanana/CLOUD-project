using CryptoPortfolioService_Data.Entities;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Linq;

namespace CryptoPortfolioService_Data.Repositories
{
    public class HealthCheckRepository
    {
        private CloudStorageAccount _storageAccount;
        private CloudTable _table;

        public HealthCheckRepository()
        {
            _storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("DataConnectionString"));
            CloudTableClient tableClient = new CloudTableClient(new Uri(_storageAccount.TableEndpoint.AbsoluteUri), _storageAccount.Credentials);
            _table = tableClient.GetTableReference("HealthCheckTable");
            _table.CreateIfNotExists();
        }

        public IQueryable<HealthCheck> RetrieveAllHealthChecks()
        {
            var results = from g in _table.CreateQuery<HealthCheck>()
                          select g;
            return results;
        }

        public void AddHealthCheck(HealthCheck newHealthCheck)
        {
            TableOperation insertOperation = TableOperation.Insert(newHealthCheck);
            _table.Execute(insertOperation);
        }

        public bool Exists(string endpoint, DateTime timestamp)
        {
            string rowKey = timestamp.ToString("o");
            return RetrieveAllHealthChecks().Where(s => s.PartitionKey == endpoint && s.RowKey == rowKey).FirstOrDefault() != null;
        }

        public void RemoveHealthCheck(string endpoint, DateTime timestamp)
        {
            HealthCheck healthCheck = RetrieveAllHealthChecks().Where(s => s.PartitionKey == endpoint && s.RowKey == timestamp.ToString("o")).FirstOrDefault();
            if (healthCheck != null)
            {
                TableOperation deleteOperation = TableOperation.Delete(healthCheck);
                _table.Execute(deleteOperation);
            }
        }

        public HealthCheck GetHealthCheck(string endpoint, DateTime timestamp)
        {
            return RetrieveAllHealthChecks().Where(p => p.PartitionKey == endpoint && p.RowKey == timestamp.ToString("o")).FirstOrDefault();
        }

        public void UpdateHealthCheck(HealthCheck healthCheck)
        {
            TableOperation updateOperation = TableOperation.Replace(healthCheck);
            _table.Execute(updateOperation);
        }
    }
}
