using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace OFMS_API.Helper.Common
{
    public class CustomUserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            return connection.User?.FindFirst("userId")?.Value;
        }
    }
}
