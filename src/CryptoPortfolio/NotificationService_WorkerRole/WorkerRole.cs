using CryptoPortfolioService_Data.Entities;
using CryptoPortfolioService_Data.Queue;
using CryptoPortfolioService_Data.Repositories;
using Microsoft.WindowsAzure.ServiceRuntime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace NotificationService_WorkerRole
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);
        private AlarmRepository _alarmRepository = new AlarmRepository();
        private CryptoCurrencyRepository _cryptoCurrencyRepository = new CryptoCurrencyRepository();
        private NotificationQueueManager _notificationQueueManager = new NotificationQueueManager();

        public override void Run()
        {
            Trace.TraceInformation("NotifierWorker is running");

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

            Trace.TraceInformation("NotifierWorker has been started");

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("NotifierWorker is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("NotifierWorker has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {            
            while (!cancellationToken.IsCancellationRequested)
            {
                var alarms = _alarmRepository.GetTopAlarms();
                var sentAlarmIds = new List<string>();

                foreach (var alarm in alarms)
                {                    
                    if (IsAlarmActive(alarm))
                    {
                        bool emailSent = await EmailSender.SendNotificationEmail(alarm);
                        if (emailSent)
                        {
                            Trace.TraceInformation($"Email sent for alarm with RowKey {alarm.RowKey} successfully.");
                            alarm.IsTriggered = true; // Mark the alarm as triggered
                            await _alarmRepository.UpdateAlarm(alarm); // Update the alarm in the database
                            sentAlarmIds.Add(alarm.RowKey); // Assuming RowKey is the unique identifier for the alarm
                        }
                    }
                }

                // Call PersistAlarmNotification to persist information about completed alarm notifications
                foreach (var sentAlarmId in sentAlarmIds)
                {
                    bool notificationPersisted = await _notificationQueueManager.PersistAlarmNotification(DateTime.UtcNow, sentAlarmId, 1); // Assuming 1 email sent per alarm
                    if (notificationPersisted)
                    {
                        Trace.TraceInformation($"Notification for alarm with RowKey {sentAlarmId} persisted successfully.");
                    }
                    else
                    {
                        Trace.TraceError($"Failed to persist notification for alarm with RowKey {sentAlarmId}.");
                    }
                }

                if (sentAlarmIds.Count > 0)
                {
                    await _notificationQueueManager.AddAlarmIdsToQueue(sentAlarmIds);
                }

                await Task.Delay(100000);
            }
        }

        private bool IsAlarmActive(Alarm alarm)
        {
            var cryptoCurrency = _cryptoCurrencyRepository.RetrieveCurrencyForUser(alarm.CurrencyName, alarm.UserId);
            if (cryptoCurrency == null)
            {
                return false;
            }

            if (cryptoCurrency.Profit >= alarm.Profit)
            {
                return true;
            }
            return false;
        }
    }
}
