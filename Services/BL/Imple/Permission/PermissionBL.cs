using DTO.Models.Permission;
using OFMS_API.BL.Interface.Permission;
using OFMS_API.Repository.DAL.Interface.Permission;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OFMS_API.BL.Imple.Permission
{
    public class PermissionBL : IPermissionBL
    {
        private readonly IPermissionDAL _permissionDAL;

        public PermissionBL(IPermissionDAL permissionDAL)
        {
            _permissionDAL = permissionDAL;
        }

        public async Task<List<tblModuleTO>> GetAllModulesAsync()
        {
            return await _permissionDAL.GetAllModulesAsync();
        }

        public async Task<List<tblPermissionTO>> GetAllPermissionsAsync()
        {
            return await _permissionDAL.GetAllPermissionsAsync();
        }

        public async Task<List<ModulePermissionMatrixDTO>> GetRolePermissionMatrixAsync(int roleId)
        {
            var modules = await _permissionDAL.GetAllModulesAsync();
            var allPermissions = await _permissionDAL.GetAllPermissionsAsync();
            var rolePermissions = await _permissionDAL.GetRolePermissionsAsync(roleId);

            var matrix = new List<ModulePermissionMatrixDTO>();

            foreach (var module in modules)
            {
                var moduleDto = new ModulePermissionMatrixDTO
                {
                    IdModule = module.IdModule,
                    ModuleName = module.ModuleName,
                    ModuleKey = module.ModuleKey,
                    DisplayOrder = module.DisplayOrder
                };

                var modulePerms = allPermissions.Where(p => p.IdModule == module.IdModule).ToList();
                foreach (var perm in modulePerms)
                {
                    var isAllowed = rolePermissions.Any(rp => rp.IdPermission == perm.IdPermission && rp.IsAllowed);
                    
                    moduleDto.Permissions.Add(new PermissionItemDTO
                    {
                        IdPermission = perm.IdPermission,
                        PermissionKey = perm.PermissionKey,
                        PermissionName = perm.PermissionName,
                        IsAllowed = isAllowed,
                        IsOverridden = false
                    });
                }

                matrix.Add(moduleDto);
            }

            return matrix;
        }

        public async Task<bool> SaveRolePermissionsAsync(SaveRolePermissionsDTO model)
        {
            foreach (var item in model.Permissions)
            {
                var rp = new tblRolePermissionTO
                {
                    RoleId = model.RoleId,
                    IdPermission = item.IdPermission,
                    IsAllowed = item.IsAllowed
                };
                await _permissionDAL.SaveRolePermissionAsync(rp);
            }
            return true;
        }

        public async Task<List<tblUserPermissionTO>> GetUserOverridesAsync(int userId)
        {
            return await _permissionDAL.GetUserPermissionsAsync(userId);
        }

        public async Task<bool> SaveUserOverridesAsync(SaveUserOverridesDTO model)
        {
            // First, delete ALL existing overrides for this user
            // We can do this by getting them and deleting, or adding a DeleteAll method.
            // Since we have DeleteUserOverrideAsync which deletes by permissionId, 
            // and GetUserOverridesAsync to get them, let's fetch and clear:
            var existing = await _permissionDAL.GetUserPermissionsAsync(model.UserId);
            foreach (var item in existing)
            {
                await _permissionDAL.DeleteUserPermissionAsync(model.UserId, item.IdPermission);
            }

            // Then insert the new overrides (if any)
            foreach (var item in model.Overrides)
            {
                var up = new tblUserPermissionTO
                {
                    UserId = model.UserId,
                    IdPermission = item.IdPermission,
                    IsAllowed = item.IsAllowed
                };
                await _permissionDAL.SaveUserPermissionAsync(up);
            }
            return true;
        }

        public async Task<bool> DeleteUserOverrideAsync(int userId, int permissionId)
        {
            await _permissionDAL.DeleteUserPermissionAsync(userId, permissionId);
            return true;
        }

        public async Task<List<PermissionItemDTO>> GetMyEffectivePermissionsAsync(int userId, int roleId)
        {
            return await _permissionDAL.GetUserEffectivePermissionsAsync(userId, roleId);
        }

        public async Task<List<tblModuleTO>> GetMyAllowedMenuAsync(int userId, int roleId)
        {
            // 1. Get all modules
            var allModules = await _permissionDAL.GetAllModulesAsync();
            
            // 2. Get effective permissions
            var effectivePerms = await _permissionDAL.GetUserEffectivePermissionsAsync(userId, roleId);

            // 3. Filter modules where the user has the module's View permission (e.g. 'Orders.View')
            // Using a simple convention: the View permission key is usually ModuleKey + ".View"
            // Let's find modules where they have ANY allowed permission for that module.
            
            var allowedModules = new List<tblModuleTO>();
            var allowedPermissionIds = effectivePerms.Where(p => p.IsAllowed).Select(p => p.IdPermission).ToList();
            
            var allPermissionsList = await _permissionDAL.GetAllPermissionsAsync();
            
            foreach (var module in allModules)
            {
                // Only show module in menu if the user has the specific ".View" permission
                // Convention: View permission key = ModuleKey + ".View"
                var viewPermissionKey = $"{module.ModuleKey}.View";
                
                var viewPermission = allPermissionsList
                    .FirstOrDefault(p => p.IdModule == module.IdModule && 
                                        p.PermissionKey == viewPermissionKey);
                
                bool hasViewAccess = false;
                if (viewPermission != null)
                {
                    hasViewAccess = effectivePerms
                        .Any(ep => ep.IdPermission == viewPermission.IdPermission && ep.IsAllowed);
                }
                
                if (hasViewAccess)
                {
                    allowedModules.Add(module);
                }
            }

            return allowedModules;
        }
        public async Task<object> GetUserProfileAsync(int userId)
        {
            return await _permissionDAL.GetUserProfileAsync(userId);
        }
    }
}
