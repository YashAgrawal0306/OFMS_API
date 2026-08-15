using DTO.Models.Master.ThemeMaster;

namespace Repository.DAL.Interface.Master.ThemeMaster
{
    public interface IThemeMasterDAL
    {
        Task<TblThemeResponseTO?> GetUserTheme(int userId);
        Task<int> SaveUserTheme(int userId, TblThemeTO themeTO);
        Task<int> ResetUserTheme(int userId);
    }
}
