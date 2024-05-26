using CryptoPortfolioService_Data.Entities;
using System.ServiceModel;
using System.Threading.Tasks;

namespace CryptoPortfolioService_NotificationService
{
    [ServiceContract]
    interface IEmailSender
    {
        [OperationContract]
        Task<bool> SendNotificationEmail(Alarm alarm);
    }
}
