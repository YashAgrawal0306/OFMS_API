using DTO.Models.Notification;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.BL.Interface.Notification
{
    public interface INotificationMasterBL
    {
        Task<List<NotificationMasterResponseTO>> GetNotificationPermissionsByRole(int roleId);
        Task<bool> UpdateNotificationPermissions(List<NotificationRoleMappingTO> mappings);
    }
}
