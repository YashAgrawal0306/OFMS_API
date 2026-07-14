using DTO.Models.Permission;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OFMS_API.BL.Interface.Permission
{
    public interface IPermissionBL
    {
        Task<List<tblModuleTO>> GetAllModulesAsync();
        Task<List<tblPermissionTO>> GetAllPermissionsAsync();
        
        Task<List<ModulePermissionMatrixDTO>> GetRolePermissionMatrixAsync(int roleId);
        Task<bool> SaveRolePermissionsAsync(SaveRolePermissionsDTO model);
        
        Task<List<tblUserPermissionTO>> GetUserOverridesAsync(int userId);
        Task<bool> SaveUserOverridesAsync(SaveUserOverridesDTO model);
        Task<bool> DeleteUserOverrideAsync(int userId, int permissionId);

        Task<List<PermissionItemDTO>> GetMyEffectivePermissionsAsync(int userId, int roleId);
        Task<List<tblModuleTO>> GetMyAllowedMenuAsync(int userId, int roleId);
        Task<object> GetUserProfileAsync(int userId);
    }
}
