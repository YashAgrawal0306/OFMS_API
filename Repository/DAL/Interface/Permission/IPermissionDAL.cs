using DTO.Models.Permission;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OFMS_API.Repository.DAL.Interface.Permission
{
    public interface IPermissionDAL
    {
        Task<List<tblModuleTO>> GetAllModulesAsync();
        Task<List<tblPermissionTO>> GetAllPermissionsAsync();
        
        Task<List<tblRolePermissionTO>> GetRolePermissionsAsync(int roleId);
        Task<int> SaveRolePermissionAsync(tblRolePermissionTO rolePermission);
        
        Task<List<tblUserPermissionTO>> GetUserPermissionsAsync(int userId);
        Task<int> SaveUserPermissionAsync(tblUserPermissionTO userPermission);
        Task<int> DeleteUserPermissionAsync(int userId, int permissionId);

        // Advanced: Get effective permissions for a user (Merging Role + User overrides)
        Task<List<PermissionItemDTO>> GetUserEffectivePermissionsAsync(int userId, int roleId);

        Task<object> GetUserProfileAsync(int userId);
    }
}
