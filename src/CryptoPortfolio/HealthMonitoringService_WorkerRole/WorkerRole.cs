using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using CryptoPortfolioService_Data.Entities;
using CryptoPortfolioService_Data.Repositories;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace HealthMonitoringService_WorkerRole
{
    public class WorkerRole : RoleEntryPoint
    {
        private HealthCheckRepository healthCheckRepository;
        private static readonly HttpClient httpClient = new HttpClient();
        private readonly string[] endpoints = {
            "http://localhost:80/PortfolioService/health-monitoring",
            "http://localhost:8080/NotificationService/health-monitoring"
        };

        public override void Run()
        {
            Trace.TraceInformation("HealthMonitoringService_WorkerRole is running");

            while (true)
            {
                foreach (var endpoint in endpoints)
                {
                    CheckHealth(endpoint);
                }
                Thread.Sleep(new Random().Next(1000, 5000));
            }
        }

        private async void CheckHealth(string endpoint)
        {
            var timestamp = DateTime.UtcNow;
            var status = "OK";

            try
            {
                var response = await httpClient.GetAsync(endpoint);
                if (!response.IsSuccessStatusCode)
                {
                    status = "NOT_OK";
                    SendAlertEmail(endpoint);
                }
            }
            catch (Exception)
            {
                status = "NOT_OK";
                SendAlertEmail(endpoint);
            }

            LogHealthCheck(endpoint, timestamp, status);
        }

        private void LogHealthCheck(string endpoint, DateTime timestamp, string status)
        {
            var healthCheck = new HealthCheck(endpoint, timestamp)
            {
                Status = status
            };

            healthCheckRepository.AddHealthCheck(healthCheck);
        }

        private void SendAlertEmail(string endpoint)
        {
            // Implement email sending logic here (e.g., using SendGrid or SMTP)
        }

        public override bool OnStart()
        {
            healthCheckRepository = new HealthCheckRepository();
            return base.OnStart();
        }

        public override void OnStop()
        {
            Trace.TraceInformation("HealthMonitoringService is stopping");
            base.OnStop();
        }
    }
}
