using CryptoPortfolioService_Data.Entities;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace CryptoPortfolioService_Data.Repositories
{
    public class AlarmRepository
    {
        private CloudStorageAccount _storageAccount;
        private CloudTable _table;

        public AlarmRepository()
        {
            _storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("DataConnectionString"));
            CloudTableClient tableClient = new CloudTableClient(new Uri(_storageAccount.TableEndpoint.AbsoluteUri), _storageAccount.Credentials);
            _table = tableClient.GetTableReference("AlarmTable");
            _table.CreateIfNotExists();
        }

        public void AddAlarm(Alarm newAlarm)
        {
            TableOperation insertOperation = TableOperation.Insert(newAlarm);
            _table.Execute(insertOperation);
        }
    }
}
