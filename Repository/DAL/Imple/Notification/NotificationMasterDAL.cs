using Dapper;
using DTO.Models.Notification;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Repository.DAL.Interface.Notification;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repository.DAL.Imple.Notification
{
    public class NotificationMasterDAL : INotificationMasterDAL
    {
        private readonly string _connectionString;

        public NotificationMasterDAL(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
        }

        public async Task<List<NotificationMasterResponseTO>> GetNotificationPermissionsByRole(int roleId)
        {
            using var conn = new SqlConnection(_connectionString);
            string sql = @"
                SELECT 
                    m.IdNotificationMaster,
                    m.NotificationName,
                    m.NotificationCode,
                    m.Description,
                    ISNULL(rm.IdMapping, 0) AS IdMapping,
                    @RoleId AS IdRole,
                    CAST(ISNULL(rm.IsActive, 0) AS BIT) AS HasPermission
                FROM tblNotificationMaster m
                LEFT JOIN tblNotificationRoleMapping rm 
                    ON m.IdNotificationMaster = rm.IdNotificationMaster AND rm.IdRole = @RoleId
                WHERE m.IsActive = 1
            ";

            var result = await conn.QueryAsync<NotificationMasterResponseTO>(sql, new { RoleId = roleId });
            return result.ToList();
        }

        public async Task<bool> UpdateNotificationPermissions(List<NotificationRoleMappingTO> mappings)
        {
            if (mappings == null || !mappings.Any()) return true;

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            using var tran = conn.BeginTransaction();

            try
            {
                foreach (var map in mappings)
                {
                    if (map.IdMapping > 0)
                    {
                        string updateSql = "UPDATE tblNotificationRoleMapping SET IsActive = @IsActive WHERE IdMapping = @IdMapping";
                        await conn.ExecuteAsync(updateSql, new { IsActive = map.IsActive, IdMapping = map.IdMapping }, tran);
                    }
                    else
                    {
                        string insertSql = @"
                            INSERT INTO tblNotificationRoleMapping (IdNotificationMaster, IdRole, IsActive)
                            VALUES (@IdNotificationMaster, @IdRole, @IsActive)
                        ";
                        await conn.ExecuteAsync(insertSql, new { IdNotificationMaster = map.IdNotificationMaster, IdRole = map.IdRole, IsActive = map.IsActive }, tran);
                    }
                }
                tran.Commit();
                return true;
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }

        public async Task<bool> CheckNotificationPermission(int userId, string notificationCode)
        {
            using var conn = new SqlConnection(_connectionString);
            string sql = @"
                SELECT CAST(ISNULL(rm.IsActive, 0) AS BIT)
                FROM tblUser u
                INNER JOIN tblUserRoleMapping ur ON u.UserId = ur.UserId
                INNER JOIN tblNotificationRoleMapping rm ON ur.RoleId = rm.IdRole
                INNER JOIN tblNotificationMaster nm ON rm.IdNotificationMaster = nm.IdNotificationMaster
                WHERE u.UserId = @UserId AND nm.NotificationCode = @NotificationCode AND nm.IsActive = 1
            ";

            var result = await conn.QueryFirstOrDefaultAsync<bool?>(sql, new { UserId = userId, NotificationCode = notificationCode });
            return result ?? false;
        }
    }
}
