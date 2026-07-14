using Microsoft.AspNetCore.SignalR;
using OFMS_API.Hubs;
using OFMS_API.Models;
using Repository.DAL.Interface.Notification;
using Services.BL.Interface.Notification;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OFMS_API.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationDAL _notificationDAL;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly INotificationMasterDAL _notificationMasterDAL;

        public NotificationService(INotificationDAL notificationDAL, IHubContext<NotificationHub> hubContext, INotificationMasterDAL notificationMasterDAL)
        {
            _notificationDAL = notificationDAL;
            _hubContext = hubContext;
            _notificationMasterDAL = notificationMasterDAL;
        }

        public async Task SendNotificationAsync(int userId, string message, string notificationCode = null)
        {
            // Check dynamic permissions if a code is provided
            if (!string.IsNullOrEmpty(notificationCode))
            {
                bool hasPermission = await _notificationMasterDAL.CheckNotificationPermission(userId, notificationCode);
                if (!hasPermission) return; // Silent discard
            }

            var notification = new NotificationTO
            {
                UserId = userId,
                Message = message
            };

            // Save to DB
            int notificationId = await _notificationDAL.AddNotification(notification);
            notification.NotificationId = notificationId;

            // Push via SignalR to the specific user
            await _hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveNotification", notification);
        }

        public async Task<List<NotificationTO>> GetUnreadNotificationsAsync(int userId)
        {
            return await _notificationDAL.GetUnreadNotifications(userId);
        }

        public async Task<bool> MarkAsReadAsync(int notificationId)
        {
            int rowsAffected = await _notificationDAL.MarkAsRead(notificationId);
            return rowsAffected > 0;
        }
    }
}
