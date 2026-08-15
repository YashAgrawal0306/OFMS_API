using DTO.Models.Master.ThemeMaster;

namespace Services.BL.Interface.Master.ThemeMaster
{
    public interface IThemeMasterBL
    {
        Task<TblThemeResponseTO?> GetUserTheme(int userId);
        Task<int> SaveUserTheme(int userId, TblThemeTO themeTO);
        Task<int> ResetUserTheme(int userId);
        List<ThemeColorItem> GetThemeList();
    }
}
