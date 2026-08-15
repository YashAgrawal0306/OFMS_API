using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace OFMS_API.Helper.Common
{
    public class CustomUserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            var user = connection.User;
            if (user == null) return null;

            var claim = user.FindFirst("userId")
                     ?? user.FindFirst("UserId")
                     ?? user.FindFirst("id")
                     ?? user.FindFirst("user_id")
                     ?? user.FindFirst(ClaimTypes.NameIdentifier)
                     ?? user.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")
                     ?? user.FindFirst("nameid")
                     ?? user.FindFirst("sub");

            return claim?.Value;
        }
    }
}
