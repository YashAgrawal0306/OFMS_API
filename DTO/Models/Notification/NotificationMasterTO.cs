namespace DTO.Models.Notification
{
    public class NotificationMasterTO
    {
        public int IdNotificationMaster { get; set; }
        public string NotificationName { get; set; } = string.Empty;
        public string NotificationCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class NotificationRoleMappingTO
    {
        public int IdMapping { get; set; }
        public int IdNotificationMaster { get; set; }
        public int IdRole { get; set; }
        public bool IsActive { get; set; }
    }

    public class NotificationMasterResponseTO : NotificationMasterTO
    {
        public int IdMapping { get; set; }
        public int IdRole { get; set; }
        public bool HasPermission { get; set; }
    }
}
