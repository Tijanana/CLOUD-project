using CryptoPortfolioService_Data.Entities;
using System.ServiceModel;
using System.Threading.Tasks;

namespace NotificationService_WorkerRole
{
    [ServiceContract]
    interface IEmailSender
    {
        [OperationContract]
        Task<bool> SendNotificationEmail(Alarm alarm);
    }
}
