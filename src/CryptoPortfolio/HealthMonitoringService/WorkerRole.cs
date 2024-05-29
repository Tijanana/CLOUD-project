using CryptoPortfolioService_Data.Entities;
using Microsoft.Azure;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HealthMonitoringService
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);
        public List<ServiceConnection> serviceConnections { get; set; } = new List<ServiceConnection>();

        public override void Run()
        {
            Trace.TraceInformation("HealthMonitoringService is running");

            try
            {
                this.RunAsync(this.cancellationTokenSource.Token).Wait();
            }
            finally
            {
                this.runCompleteEvent.Set();
            }
        }

        public override bool OnStart()
        {
            // Use TLS 1.2 for Service Bus connections
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at https://go.microsoft.com/fwlink/?LinkId=166357.

            bool result = base.OnStart();

            Trace.TraceInformation("HealthMonitoringService has been started");

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("HealthMonitoringService is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("HealthMonitoringService has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            serviceConnections = await GetHealthCheckEndpointsAsync();

            while (!cancellationToken.IsCancellationRequested)
            {
                foreach (var serviceConnection in serviceConnections)
                {
                    bool isHealthy = await CheckHealthAsync(serviceConnection.Endpoint);
                    if (isHealthy)
                    {
                        Trace.TraceInformation($"{serviceConnection.Service}[{serviceConnection.Instance}] is healthy.");
                    }
                    else
                    {
                        Trace.TraceInformation($"{serviceConnection.Service}[{serviceConnection.Instance}] is healthy.");
                    }
                }

                Thread.Sleep(new Random().Next(1000, 5000));
            }
        }

        private async Task<List<ServiceConnection>> GetHealthCheckEndpointsAsync()
        {
            var endpoints = new List<ServiceConnection>();

            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("DataConnectionString"));
            var tableClient = storageAccount.CreateCloudTableClient();
            var table = tableClient.GetTableReference("HealthCheckEndpoints");

            var query = new TableQuery<EndpointEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "NotificationService"));
            var results = await table.ExecuteQuerySegmentedAsync(query, null);

            // Sort results by Timestamp in descending order and take the top 3
            var lastThreeEntries = results.Results
                                          .OrderByDescending(e => e.Timestamp)
                                          .Take(3)
                                          .ToList();

            int index = 0;
            foreach (var entity in lastThreeEntries)
            {
                endpoints.Add(new ServiceConnection()
                {
                    Endpoint = entity.Url,
                    Service = "NotificationService",
                    Instance = index.ToString()
                });
                index++;
            }

            return endpoints;
        }

        private async Task<bool> CheckHealthAsync(string url)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.GetAsync(url);
                    return response.IsSuccessStatusCode;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Error checking health of {url}: {ex.Message}");
                return false;
            }
        }
    }

    public class ServiceConnection
    {
        public string Endpoint { get; set; }
        public string Service { get; set; }
        public string Instance { get; set; }
    }
}
