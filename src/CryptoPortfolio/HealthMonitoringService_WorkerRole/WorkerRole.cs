using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using CryptoPortfolioService_Data.Entities;
using CryptoPortfolioService_Data.Repositories;
using Microsoft.WindowsAzure.ServiceRuntime;
using NotificationService_WorkerRole;

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
                    SendAlertEmail(endpoint, timestamp);
                }
            }
            catch (Exception)
            {
                status = "NOT_OK";
                SendAlertEmail(endpoint, timestamp);
            }

            LogHealthCheck(endpoint, timestamp, status);
        }

        private void LogHealthCheck(string endpoint, DateTime timestamp, string status)
        {
            var healthCheck = new HealthCheck()
            {
                Status = status,
                Service = endpoint.Contains("PortfolioService") ? "Portfolio Service" : "Notification Service",
                Timestamp = timestamp
            };

            healthCheckRepository.AddHealthCheck(healthCheck);
        }

        private async void SendAlertEmail(string endpoint, DateTime timestamp)
        {
            EmailSender emailSender = new EmailSender(81);
            await emailSender.SendAlertEmail(endpoint, timestamp);
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
