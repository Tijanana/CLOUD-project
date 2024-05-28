﻿using CryptoPortfolioService_Data.Entities;
using CryptoPortfolioService_Data.Repositories;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace NotificationService_WorkerRole
{
    class EmailSender : IEmailSender
    {
        private static readonly string ConfigFolder = AppDomain.CurrentDomain.BaseDirectory.Substring(0, AppDomain.CurrentDomain.BaseDirectory.Length - 77);
        private static readonly string ConfigFilePath = Path.Combine($"{ConfigFolder}NotificationService_WorkerRole\\bin\\Debug\\appsettings.json");
        private static readonly EmailSettings EmailSettings = LoadEmailSettings();

        private static EmailSettings LoadEmailSettings()
        {
            try
            {
                string json = File.ReadAllText(ConfigFilePath);
                return JsonConvert.DeserializeObject<EmailSettings>(json);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"[NOTIFICATION SERVICE]: An error occurred when trying to read the configuration file: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> SendNotificationEmail(Alarm alarm)
        {
            try
            {
                if (EmailSettings == null)
                {
                    Trace.WriteLine("[NOTIFICATION SERVICE]: An error occurred when trying to send email:\n\tFailed to read email settings from configuration file!");
                    return false;
                }

                var _userRepository = new UserRepository();
                User user = _userRepository.GetUser(alarm.UserId);
                if (user == null)
                {
                    Trace.WriteLine("[NOTIFICATION SERVICE]: An error occurred when trying to send email:\n\tUser not found!");
                    return false;
                }

                // Create client
                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(EmailSettings.EmailUser, EmailSettings.AppPassword)
                };

                // Create the email message
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(EmailSettings.EmailUser);
                mail.To.Add(user.Email);

                var _cryptoCurrencyRepository = new CryptoCurrencyRepository();
                var cryptoCurrency = _cryptoCurrencyRepository.RetrieveCurrencyForUser(alarm.CurrencyName, alarm.UserId);

                // Fill in message
                mail.IsBodyHtml = true;
                var body = "<h1>Alarm Notification!</h1>";
                body += $"<h3><br>The alarm for {cryptoCurrency.CurrencyName} has been triggere!";
                body += $"<br>The profit threshold set was: <b>{alarm.Profit}</b>";
                body += $"<br>The current profit is: <b>{cryptoCurrency.Profit}</b></h3>";
                mail.Body = body;
                mail.Subject = $"{cryptoCurrency.CurrencyName} alarm activated";

                // Send the email
                await smtp.SendMailAsync(mail);
                return true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"[NOTIFICATION SERVICE]: An error occurred when trying to send email: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendAlertEmail(string endpoint, DateTime timestamp)
        {
            try
            {
                if (EmailSettings == null)
                {
                    Trace.WriteLine("[HEALTH MONITORING SERVICE]: An error occurred when trying to send email:\n\tFailed to read email settings from configuration file!");
                    return false;
                }

                // Create client
                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(EmailSettings.EmailUser, EmailSettings.AppPassword)
                };

                // Create the email message
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(EmailSettings.EmailUser);
                mail.To.Add(EmailSettings.HealthEmailUser);

                // Fill in message
                mail.IsBodyHtml = true;
                var body = "<h1>Healt Alert!</h1>";
                var service = endpoint.Contains("PortfolioService") ? "Portfolio Service" : "Notification Service";
                body += $"<h3><br>Detected failure of the {service} at {timestamp.Date} : {timestamp.TimeOfDay.ToString("hh\\:mm\\:ss")}!";
                mail.Body = body;
                mail.Subject = $"Health Alert";

                // Send the email
                await smtp.SendMailAsync(mail);
                Trace.WriteLine($"[HEALTH MONITORING SERVICE]: Alerted failure of {service}");
                return true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"[HEALTH MONITORING SERVICE]: An error occurred when trying to send email: {ex.Message}");
                return false;
            }
        }
    }

    public class EmailSettings
    {
        public string HealthEmailUser { get; set; }
        public string EmailUser { get; set; }
        public string AppPassword { get; set; }
    }
}
