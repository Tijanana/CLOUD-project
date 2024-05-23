using CryptoPortfolioService_Data.Entities;
using CryptoPortfolioService_Data.Repositories;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace NotifierWorker
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);
        private AlarmRepository _alarmRepository = new AlarmRepository();
        private CryptoCurrencyRepository _cryptoCurrencyRepository = new CryptoCurrencyRepository();

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
                foreach (var alarm in alarms)
                {
                    var isActive = CheckIfAlarmIsActive(alarm);

                    if (isActive)
                    {
                        await EmailSender.SendNotificationEmail(alarm);
                    }
                }

                await Task.Delay(10000);
            }
        }

        private bool CheckIfAlarmIsActive(Alarm alarm)
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
