using CryptoPortfolioService_Data.Entities;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public List<Alarm> GetTopAlarms(int count = 20)
        {
            var query = new TableQuery<Alarm>()
                .Where(TableQuery.GenerateFilterConditionForBool("IsTriggered", QueryComparisons.Equal, false))
                .Take(20);
            return _table.ExecuteQuery(query).ToList();
        }

        public async Task UpdateAlarm(Alarm alarm)
        {
            var operation = TableOperation.Replace(alarm);
            await _table.ExecuteAsync(operation);
        }
    }
}
