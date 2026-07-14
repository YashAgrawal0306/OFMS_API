using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OFMS_API.BL.Interface.Permission;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OFMS_API.Attributes
{
    public class HasPermissionAttribute : TypeFilterAttribute
    {
        public HasPermissionAttribute(string permissionKey) : base(typeof(HasPermissionFilter))
        {
            Arguments = new object[] { permissionKey };
        }
    }

    public class HasPermissionFilter : IAsyncAuthorizationFilter
    {
        private readonly string _permissionKey;
        private readonly IPermissionBL _permissionBL;

        public HasPermissionFilter(string permissionKey, IPermissionBL permissionBL)
        {
            _permissionKey = permissionKey;
            _permissionBL = permissionBL;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;
            if (!user.Identity.IsAuthenticated)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Extract User ID and Role ID from JWT token claims
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                           ?? user.FindFirst("Id")?.Value 
                           ?? user.FindFirst("userId")?.Value;
                           
            var roleIdClaim = user.FindFirst(ClaimTypes.Role)?.Value 
                           ?? user.FindFirst("RoleId")?.Value 
                           ?? user.FindFirst("roleId")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                context.Result = new ForbidResult();
                return;
            }

            // If RoleId claim doesn't exist, default to 0 (no role)
            int roleId = 0;
            if (!string.IsNullOrEmpty(roleIdClaim))
            {
                int.TryParse(roleIdClaim, out roleId);
            }

            // Get effective permissions for user
            var permissions = await _permissionBL.GetMyEffectivePermissionsAsync(userId, roleId);

            // Check if user has the required permission
            var requiredPerm = permissions.FirstOrDefault(p => p.PermissionKey == _permissionKey);

            if (requiredPerm == null || !requiredPerm.IsAllowed)
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
