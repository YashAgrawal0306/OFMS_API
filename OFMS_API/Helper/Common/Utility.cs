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
    }
}
