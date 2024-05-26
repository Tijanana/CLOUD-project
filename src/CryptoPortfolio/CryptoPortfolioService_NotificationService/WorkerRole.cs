using CryptoPortfolioService_Data.Entities;
using CryptoPortfolioService_Data.QueueStorage;
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

namespace CryptoPortfolioService_NotificationService
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);
        private AlarmRepository _alarmRepository = new AlarmRepository();
        private CryptoCurrencyRepository _cryptoCurrencyRepository = new CryptoCurrencyRepository();
        private NotificationQueueManager _notificationQueueManager = new NotificationQueueManager();
        private EmailSender _emailSender = new EmailSender();

        public override void Run()
        {
            Trace.TraceInformation("CryptoPortfolioService_NotificationService is running");

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

            Trace.TraceInformation("CryptoPortfolioService_NotificationService has been started");

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("CryptoPortfolioService_NotificationService is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("CryptoPortfolioService_NotificationService has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            // EnterTestDataNotificationService();

            while (!cancellationToken.IsCancellationRequested)
            {
                var alarms = await _alarmRepository.GetTopAlarmsAsync();
                var sentAlarmIds = new List<string>();

                foreach (var alarm in alarms)
                {
                    var isActive = IsAlarmActive(alarm);

                    if (isActive)
                    {
                        bool emailSent = await _emailSender.SendNotificationEmail(alarm);
                        if (emailSent)
                        {
                            Trace.TraceInformation($"[NOTIFICATION SERVICE]: Email sent for alarm with RowKey {alarm.RowKey} successfully.");
                            sentAlarmIds.Add(alarm.RowKey); // Assuming RowKey is the unique identifier for the alarm
                        }
                    }
                    else
                    {
                        // Requeue the alarm if it wasn't activated
                        await _alarmRepository.RequeueAlarmAsync(alarm);
                    }
                }

                if (sentAlarmIds.Count > 0)
                {
                    await _notificationQueueManager.AddAlarmIdsToQueue(sentAlarmIds);
                }

                await Task.Delay(10000);
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

        private void EnterTestDataNotificationService()
        {
            var _userRepository = new UserRepository();
            var _alarmRepository = new AlarmRepository();
            var _cryptoCurrencyRepository = new CryptoCurrencyRepository();

            // Step 1: Add a new user
            User newUser = new User
            {
                Name = "Nemanja",
                Surname = "Jovicic",
                Address = "Some Address",
                City = "Some City",
                Country = "Some Country",
                Phone = "1234567890",
                Email = "nemanja.ja.jovicic7@gmail.com",
                Password = "securepassword",
                PhotoUrl = "http://example.com/photo.jpg"
            };
            _userRepository.AddUsear(newUser);
            Trace.WriteLine("[NOTIFICATION SERVICE]: New user added.");

            // Step 2: Add three new cryptocurrencies
            CryptoCurrency[] newCryptos = new CryptoCurrency[3];
            newCryptos[0] = new CryptoCurrency
            {
                UserId = newUser.RowKey,
                CurrencyName = "Bitcoin",
                Quantity = 10,
                Profit = 5000.0 // Example profit
            };
            newCryptos[1] = new CryptoCurrency
            {
                UserId = newUser.RowKey,
                CurrencyName = "Ethereum",
                Quantity = 20,
                Profit = 3000.0 // Example profit
            };
            newCryptos[2] = new CryptoCurrency
            {
                UserId = newUser.RowKey,
                CurrencyName = "Litecoin",
                Quantity = 15,
                Profit = 1500.0 // Example profit
            };

            foreach (var crypto in newCryptos)
            {
                _cryptoCurrencyRepository.AddCryptoCurrency(crypto);
            }
            Trace.WriteLine("[NOTIFICATION SERVICE]: New cryptocurrencies added.");

            // Step 3: Add five new alarms
            Alarm[] newAlarms = new Alarm[5];
            // Alarms with threshold lower than current profit
            newAlarms[0] = new Alarm
            {
                UserId = newUser.RowKey,
                CurrencyName = "Bitcoin",
                Profit = 4000.0 // Lower than Bitcoin's current profit
            };
            newAlarms[1] = new Alarm
            {
                UserId = newUser.RowKey,
                CurrencyName = "Ethereum",
                Profit = 2000.0 // Lower than Ethereum's current profit
            };
            newAlarms[2] = new Alarm
            {
                UserId = newUser.RowKey,
                CurrencyName = "Litecoin",
                Profit = 1000.0 // Lower than Litecoin's current profit
            };
            // Alarms with threshold higher than current profit
            newAlarms[3] = new Alarm
            {
                UserId = newUser.RowKey,
                CurrencyName = "Bitcoin",
                Profit = 6000.0 // Higher than Bitcoin's current profit
            };
            newAlarms[4] = new Alarm
            {
                UserId = newUser.RowKey,
                CurrencyName = "Ethereum",
                Profit = 4000.0 // Higher than Ethereum's current profit
            };

            foreach (var alarm in newAlarms)
            {
                _alarmRepository.AddAlarmAsync(alarm).Wait();
            }
            Trace.WriteLine("[NOTIFICATION SERVICE]: New alarms added.");
        }
    }
}
