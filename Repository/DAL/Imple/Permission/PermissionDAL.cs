using Dapper;
using DTO.Models.Permission;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using OFMS_API.Repository.DAL.Interface.Permission;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace OFMS_API.Repository.DAL.Imple.Permission
{
    public class PermissionDAL : IPermissionDAL
    {
        private readonly string _connectionString;

        public PermissionDAL(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<List<tblModuleTO>> GetAllModulesAsync()
        {
            using var conn = new SqlConnection(_connectionString);
            string sql = "SELECT * FROM tblModule WHERE IsActive = 1 ORDER BY DisplayOrder ASC";
            var result = await conn.QueryAsync<tblModuleTO>(sql);
            return result.ToList();
        }

        public async Task<List<tblPermissionTO>> GetAllPermissionsAsync()
        {
            using var conn = new SqlConnection(_connectionString);
            string sql = "SELECT * FROM tblPermission WHERE IsActive = 1";
            var result = await conn.QueryAsync<tblPermissionTO>(sql);
            return result.ToList();
        }

        public async Task<List<tblRolePermissionTO>> GetRolePermissionsAsync(int roleId)
        {
            using var conn = new SqlConnection(_connectionString);
            string sql = "SELECT * FROM tblRolePermission WHERE RoleId = @RoleId";
            var result = await conn.QueryAsync<tblRolePermissionTO>(sql, new { RoleId = roleId });
            return result.ToList();
        }

        public async Task<int> SaveRolePermissionAsync(tblRolePermissionTO rolePermission)
        {
            using var conn = new SqlConnection(_connectionString);
            string sql = @"
                IF EXISTS (SELECT 1 FROM tblRolePermission WHERE RoleId = @RoleId AND IdPermission = @IdPermission)
                BEGIN
                    UPDATE tblRolePermission 
                    SET IsAllowed = @IsAllowed, UpdatedOn = GETDATE()
                    WHERE RoleId = @RoleId AND IdPermission = @IdPermission
                END
                ELSE
                BEGIN
                    INSERT INTO tblRolePermission (RoleId, IdPermission, IsAllowed, CreatedOn)
                    VALUES (@RoleId, @IdPermission, @IsAllowed, GETDATE())
                END";
            
            return await conn.ExecuteAsync(sql, new { 
                RoleId = rolePermission.RoleId, 
                IdPermission = rolePermission.IdPermission, 
                IsAllowed = rolePermission.IsAllowed 
            });
        }

        public async Task<List<tblUserPermissionTO>> GetUserPermissionsAsync(int userId)
        {
            using var conn = new SqlConnection(_connectionString);
            string sql = "SELECT * FROM tblUserPermission WHERE UserId = @UserId";
            var result = await conn.QueryAsync<tblUserPermissionTO>(sql, new { UserId = userId });
            return result.ToList();
        }

        public async Task<int> SaveUserPermissionAsync(tblUserPermissionTO userPermission)
        {
            using var conn = new SqlConnection(_connectionString);
            string sql = @"
                IF EXISTS (SELECT 1 FROM tblUserPermission WHERE UserId = @UserId AND IdPermission = @IdPermission)
                BEGIN
                    UPDATE tblUserPermission 
                    SET IsAllowed = @IsAllowed, UpdatedOn = GETDATE()
                    WHERE UserId = @UserId AND IdPermission = @IdPermission
                END
                ELSE
                BEGIN
                    INSERT INTO tblUserPermission (UserId, IdPermission, IsAllowed, CreatedOn)
                    VALUES (@UserId, @IdPermission, @IsAllowed, GETDATE())
                END";
            
            return await conn.ExecuteAsync(sql, new { 
                UserId = userPermission.UserId, 
                IdPermission = userPermission.IdPermission, 
                IsAllowed = userPermission.IsAllowed 
            });
        }

        public async Task<int> DeleteUserPermissionAsync(int userId, int permissionId)
        {
            using var conn = new SqlConnection(_connectionString);
            string sql = "DELETE FROM tblUserPermission WHERE UserId = @UserId AND IdPermission = @IdPermission";
            return await conn.ExecuteAsync(sql, new { UserId = userId, IdPermission = permissionId });
        }

        public async Task<List<PermissionItemDTO>> GetUserEffectivePermissionsAsync(int userId, int roleId)
        {
            using var conn = new SqlConnection(_connectionString);
            string sql = @"
                SELECT 
                    p.IdPermission,
                    p.PermissionKey,
                    p.PermissionName,
                    ISNULL(up.IsAllowed, ISNULL(rp.IsAllowed, 0)) as IsAllowed,
                    CAST(CASE WHEN up.IdPermission IS NOT NULL THEN 1 ELSE 0 END AS BIT) as IsOverridden
                FROM tblPermission p
                LEFT JOIN tblRolePermission rp ON p.IdPermission = rp.IdPermission AND rp.RoleId = @RoleId
                LEFT JOIN tblUserPermission up ON p.IdPermission = up.IdPermission AND up.UserId = @UserId
                WHERE p.IsActive = 1
            ";
            
            var result = await conn.QueryAsync<PermissionItemDTO>(sql, new { UserId = userId, RoleId = roleId });
            return result.ToList();
        }

        public async Task<object> GetUserProfileAsync(int userId)
        {
            using var conn = new SqlConnection(_connectionString);

            // Fetch user info + role
            string userSql = @"
                SELECT u.userid        AS UserId,
                       u.username      AS UserName,
                       u.useremail     AS UserEmail,
                       u.Phone_Number  AS PhoneNumber,
                       u.Date_Of_Birth AS DateOfBirth,
                       u.Profile_Image AS ProfileImage,
                       u.IsActive,
                       u.created_at    AS CreatedAt,
                       r.RoleId,
                       r.RoleName
                FROM tbluser u
                LEFT JOIN tblUserRoleMapping m ON u.userid = m.UserId
                LEFT JOIN tblroles r ON r.RoleId = m.RoleId
                WHERE u.userid = @UserId";

            var user = await conn.QueryFirstOrDefaultAsync<dynamic>(userSql, new { UserId = userId });

            // Fetch address mapped to this user
            string addressSql = @"
                SELECT a.IdAddress, a.AddressLine1, a.AddressLine2, a.Area, a.Locality,
                       a.Landmark, a.Pincode,
                       c.CityName, s.StateName, cn.CountryName
                FROM tblAddressMapping am
                LEFT JOIN tblAddress a  ON am.IdAddress = a.IdAddress
                LEFT JOIN dimCity c     ON a.IdCity     = c.IdCity
                LEFT JOIN dimState s    ON a.IdState    = s.IdState
                LEFT JOIN dimCountry cn ON a.IdCountry  = cn.IdCountry
                WHERE am.EntityId = @UserId";

            var address = await conn.QueryFirstOrDefaultAsync<dynamic>(addressSql, new { UserId = userId });

            return new { user, address };
        }
    }
}
