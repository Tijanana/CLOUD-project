using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CryptoPortfolioService_Data.QueueStorage
{
    public class NotificationQueueManager
    {
        private CloudStorageAccount _storageAccount;
        private CloudQueueClient _queueClient;
        private CloudQueue _queue;

        public NotificationQueueManager()
        {
            _storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("DataConnectionString"));
            _queueClient = _storageAccount.CreateCloudQueueClient();
            _queue = _queueClient.GetQueueReference("alarmsdone");
            _queue.CreateIfNotExists();
        }

        public async Task<bool> AddAlarmIdsToQueue(List<string> alarmIds)
        {
            try
            {
                foreach (var alarmId in alarmIds)
                {
                    CloudQueueMessage message = new CloudQueueMessage(alarmId);
                    await _queue.AddMessageAsync(message);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<string>> GetAlarmIdsFromQueue()
        {
            try
            {
                List<string> alarmIds = new List<string>();

                CloudQueueMessage retrievedMessage = await _queue.GetMessageAsync();
                while (retrievedMessage != null)
                {
                    alarmIds.Add(retrievedMessage.AsString);

                    // Delete the message from the queue
                    await _queue.DeleteMessageAsync(retrievedMessage);

                    // Get the next message
                    retrievedMessage = await _queue.GetMessageAsync();
                }

                return alarmIds;
            }
            catch
            {
                return null;
            }
        }
    }
}
