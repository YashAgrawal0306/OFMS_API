using DTO.Models.Notification;
using Repository.DAL.Interface.Notification;
using Services.BL.Interface.Notification;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.BL.Imple.Notification
{
    public class NotificationMasterBL : INotificationMasterBL
    {
        private readonly INotificationMasterDAL _dal;

        public NotificationMasterBL(INotificationMasterDAL dal)
        {
            _dal = dal;
        }

        public async Task<List<NotificationMasterResponseTO>> GetNotificationPermissionsByRole(int roleId)
        {
            return await _dal.GetNotificationPermissionsByRole(roleId);
        }

        public async Task<bool> UpdateNotificationPermissions(List<NotificationRoleMappingTO> mappings)
        {
            return await _dal.UpdateNotificationPermissions(mappings);
        }
    }
}
