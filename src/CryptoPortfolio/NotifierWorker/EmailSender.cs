using CryptoPortfolioService_Data.Entities;
using CryptoPortfolioService_Data.Repositories;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace NotifierWorker
{
    static class EmailSender
    {
        public static async Task<bool> SendNotificationEmail(Alarm alarm)
        {
            try
            {
                var _userRepository = new UserRepository();
                User user = _userRepository.GetUser(alarm.UserId);
                if (user == null)
                {
                    Trace.WriteLine("An error occured when trying to send email:\n\tUser not found!");
                    return false;
                }

                var emailUser = "feedback.requests.info@gmail.com";
                var appPassword = "wgmrzjdbnfcbpsdl";

                // Create client
                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(emailUser, appPassword)
                };

                // Create the email message
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress("feedback.requests.info@gmail.com");
                mail.To.Add(user.Email);

                // Fill in message
                mail.IsBodyHtml = true;
                mail.Body = "<h1>Alarm Notification!</h1>\n";

                // Send the email
                await smtp.SendMailAsync(mail);
                return true;
            }
            catch
            {
                Trace.WriteLine("An error occured when trying to send email");
                return false;
            }
        }
    }
}
