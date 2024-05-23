using CryptoPortfolioService_Data.Entities;
using CryptoPortfolioService_Data.Repositories;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace NotifierWorker
{
    static class EmailSender
    {
        private static readonly string ConfigFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
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
                Trace.WriteLine($"An error occurred when trying to read the configuration file: {ex.Message}");
                return null;
            }
        }

        public static async Task<bool> SendNotificationEmail(Alarm alarm)
        {
            try
            {
                if (EmailSettings == null)
                {
                    Trace.WriteLine("An error occurred when trying to send email:\n\tFailed to read email settings from configuration file!");
                    return false;
                }

                var _userRepository = new UserRepository();
                User user = _userRepository.GetUser(alarm.UserId);
                if (user == null)
                {
                    Trace.WriteLine("An error occurred when trying to send email:\n\tUser not found!");
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

                // Fill in message
                mail.IsBodyHtml = true;
                mail.Body = "<h1>Alarm Notification!</h1>\n";

                // Send the email
                await smtp.SendMailAsync(mail);
                return true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"An error occurred when trying to send email: {ex.Message}");
                return false;
            }
        }
    }

    public class EmailSettings
    {
        public string EmailUser { get; set; }
        public string AppPassword { get; set; }
    }
}
