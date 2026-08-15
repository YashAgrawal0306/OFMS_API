using System;

namespace DTO.Models.Master.ThemeMaster
{
    /// <summary>Input DTO for saving a user theme.</summary>
    public class TblThemeTO
    {
        public string? ThemeName { get; set; }
        public string? PrimaryColor { get; set; }
        public string Mode { get; set; } = "Dark";       // "Dark" | "Light"
        public string FontFamily { get; set; } = "Outfit";
        public string FontSize { get; set; } = "Medium";  // "Small"|"Medium"|"Large"|"XLarge"
    }

    /// <summary>Response DTO returned to the frontend.</summary>
    public class TblThemeResponseTO
    {
        public int IdThemeSetting { get; set; }
        public int IdUser { get; set; }
        public string? ThemeName { get; set; }
        public string PrimaryColor { get; set; } = "#ffc107";
        public string Mode { get; set; } = "Dark";
        public string FontFamily { get; set; } = "Outfit";
        public string FontSize { get; set; } = "Medium";
    }

    /// <summary>One entry in the predefined theme list.</summary>
    public class ThemeColorItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Primary { get; set; } = string.Empty;
    }
}
