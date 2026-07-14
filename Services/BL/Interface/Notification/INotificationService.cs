using OFMS_API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.BL.Interface.Notification
{
    public interface INotificationService
    {
        Task SendNotificationAsync(int userId, string message, string notificationCode = null);
        Task<List<NotificationTO>> GetUnreadNotificationsAsync(int userId);
        Task<bool> MarkAsReadAsync(int notificationId);
    }
}
