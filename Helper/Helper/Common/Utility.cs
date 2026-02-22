using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace OFMS_API.Helper.Common
{
    public class Utility
    {
        private readonly HttpContextAccessor _httpContextAccessor ;
        public Utility(HttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public static string FormatExceptionMessage(Exception ex)
        {
            if (ex == null)
                return "An unknown error occurred.";
            return $"An error occurred: {ex.Message}";
        }

        public static string StoreFileInLocalFolder(IFormFile file)
        {
            var folderPath = @"D:\Project\OFMS\OFMS_API\OFMS_API\Images\UserProfileImages\";
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(folderPath, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }
            return fileName;
        }

        //public static int? GetUserId()
        //{ 

        //    var httpContext = _httpContextAccessor.HttpContext;

        //    if (httpContext == null)
        //        return null;

        //    var user = httpContext.User;

        //    if (user == null || !user.Identity.IsAuthenticated)
        //        return null;

        //    var userIdClaim = user.FindFirst("userId");

        //    if (userIdClaim == null)
        //        return null;

        //    if (int.TryParse(userIdClaim.Value, out var id))
        //        return id;

        //    return null;
        //}
    }
}
