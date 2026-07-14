using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using OFMS_API.Models;
using Repository.DAL.Interface.Notification;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repository.DAL.Imple.Notification
{
    public class NotificationDAL : INotificationDAL
    {
        private readonly string _connectionString;

        public NotificationDAL(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
        }

        public async Task<int> AddNotification(NotificationTO notification)
        {
            using var conn = new SqlConnection(_connectionString);
            string sql = @"
                INSERT INTO tblNotifications (UserId, Message, IsRead, CreatedAt)
                VALUES (@UserId, @Message, 0, GETDATE());
                SELECT CAST(SCOPE_IDENTITY() as INT);";

            var id = await conn.QuerySingleOrDefaultAsync<int>(sql, new
            {
                UserId = notification.UserId,
                Message = notification.Message
            });
            return id;
        }

        public async Task<List<NotificationTO>> GetUnreadNotifications(int userId)
        {
            using var conn = new SqlConnection(_connectionString);
            string sql = @"
                SELECT NotificationId, UserId, Message, IsRead, CreatedAt
                FROM tblNotifications
                WHERE UserId = @UserId AND IsRead = 0
                ORDER BY CreatedAt DESC;";

            var result = await conn.QueryAsync<NotificationTO>(sql, new { UserId = userId });
            return result.ToList();
        }

        public async Task<int> MarkAsRead(int notificationId)
        {
            using var conn = new SqlConnection(_connectionString);
            string sql = @"
                UPDATE tblNotifications
                SET IsRead = 1
                WHERE NotificationId = @NotificationId;";

            var result = await conn.ExecuteAsync(sql, new { NotificationId = notificationId });
            return result;
        }
    }
}
