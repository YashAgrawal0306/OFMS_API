using OFMS_API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository.DAL.Interface.Notification
{
    public interface INotificationDAL
    {
        Task<int> AddNotification(NotificationTO notification);
        Task<List<NotificationTO>> GetUnreadNotifications(int userId);
        Task<int> MarkAsRead(int notificationId);
    }
}
