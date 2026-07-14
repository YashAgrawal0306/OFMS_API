    using System;
using System.Collections.Generic;

namespace DTO.Models.Permission
{
    public class tblModuleTO
    {
        public int IdModule { get; set; }
        public string ModuleName { get; set; }
        public string ModuleKey { get; set; }
        public int? ParentModuleId { get; set; }
        public int DisplayOrder { get; set; }
        public string? Icon { get; set; }
        public string? Route { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        
        // For hierarchical menu mapping
        public List<tblModuleTO> SubModules { get; set; } = new List<tblModuleTO>();
    }

    public class tblPermissionTO
    {
        public int IdPermission { get; set; }
        public int IdModule { get; set; }
        public string PermissionKey { get; set; }
        public string PermissionName { get; set; }
        public string PermissionType { get; set; }
        public bool IsActive { get; set; }
    }

    public class tblRolePermissionTO
    {
        public int IdRolePermission { get; set; }
        public int RoleId { get; set; }
        public int IdPermission { get; set; }
        public bool IsAllowed { get; set; }
    }

    public class tblUserPermissionTO
    {
        public int IdUserPermission { get; set; }
        public int UserId { get; set; }
        public int IdPermission { get; set; }
        public bool IsAllowed { get; set; }
    }

    // Custom response DTOs for the Matrix and API
    
    public class ModulePermissionMatrixDTO
    {
        public int IdModule { get; set; }
        public string ModuleName { get; set; }
        public string ModuleKey { get; set; }
        public int DisplayOrder { get; set; }
        
        public List<PermissionItemDTO> Permissions { get; set; } = new List<PermissionItemDTO>();
    }

    public class PermissionItemDTO
    {
        public int IdPermission { get; set; }
        public string PermissionKey { get; set; }
        public string PermissionName { get; set; }
        public bool IsAllowed { get; set; }
        public bool IsOverridden { get; set; } // Used to indicate if the value comes from User override
    }

    public class SaveRolePermissionsDTO
    {
        public int RoleId { get; set; }
        public List<RolePermissionSaveItem> Permissions { get; set; }
    }

    public class RolePermissionSaveItem
    {
        public int IdPermission { get; set; }
        public bool IsAllowed { get; set; }
    }
    
    public class SaveUserOverridesDTO
    {
        public int UserId { get; set; }
        public List<UserPermissionSaveItem> Overrides { get; set; }
    }

    public class UserPermissionSaveItem
    {
        public int IdPermission { get; set; }
        public bool IsAllowed { get; set; }
    }
}
