using DTO.Models.Notification;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository.DAL.Interface.Notification
{
    public interface INotificationMasterDAL
    {
        Task<List<NotificationMasterResponseTO>> GetNotificationPermissionsByRole(int roleId);
        Task<bool> UpdateNotificationPermissions(List<NotificationRoleMappingTO> mappings);
        Task<bool> CheckNotificationPermission(int userId, string notificationCode);
    }
}
