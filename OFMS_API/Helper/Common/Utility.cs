namespace OFMS_API.Helper.Common
{
    public class Utility
    {
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
    }
}
