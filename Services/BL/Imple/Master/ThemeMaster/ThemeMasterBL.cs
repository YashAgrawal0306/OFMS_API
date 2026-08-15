using DTO.Models.Master.ThemeMaster;
using Repository.DAL.Interface.Master.ThemeMaster;
using Services.BL.Interface.Master.ThemeMaster;
using System.Text.RegularExpressions;

namespace Services.BL.Imple.Master.ThemeMaster
{
    public class ThemeMasterBL : IThemeMasterBL
    {
        private readonly IThemeMasterDAL _dal;

        public ThemeMasterBL(IThemeMasterDAL dal)
        {
            _dal = dal;
        }

        public async Task<TblThemeResponseTO?> GetUserTheme(int userId)
            => await _dal.GetUserTheme(userId);

        public async Task<int> SaveUserTheme(int userId, TblThemeTO themeTO)
        {
            // Validate and sanitize color — only allow valid hex colors or null
            if (!string.IsNullOrWhiteSpace(themeTO.PrimaryColor))
            {
                if (!IsValidHexColor(themeTO.PrimaryColor))
                    themeTO.PrimaryColor = "#ffc107"; // fallback to default
            }

            // Validate mode
            if (themeTO.Mode != "Dark" && themeTO.Mode != "Light")
                themeTO.Mode = "Dark";

            // Validate font size
            var validSizes = new[] { "Small", "Medium", "Large", "XLarge" };
            if (!validSizes.Contains(themeTO.FontSize))
                themeTO.FontSize = "Medium";

            // Validate font family (allow known safe fonts only)
            var validFonts = new[] { "Outfit", "Inter", "Roboto", "Poppins", "Lato", "Nunito", "Open Sans", "Montserrat" };
            if (!validFonts.Contains(themeTO.FontFamily))
                themeTO.FontFamily = "Outfit";

            return await _dal.SaveUserTheme(userId, themeTO);
        }

        public async Task<int> ResetUserTheme(int userId)
            => await _dal.ResetUserTheme(userId);

        /// <summary>Returns the 100+ professionally curated predefined theme colors.</summary>
        public List<ThemeColorItem> GetThemeList()
        {
            return new List<ThemeColorItem>
            {
                // Amber / Gold
                new() { Id = 1,   Name = "Amber Gold",        Primary = "#F59E0B" },
                new() { Id = 2,   Name = "Deep Amber",         Primary = "#D97706" },
                new() { Id = 3,   Name = "Warm Yellow",        Primary = "#EAB308" },
                new() { Id = 4,   Name = "Gold",               Primary = "#CA8A04" },
                new() { Id = 5,   Name = "Honey",              Primary = "#F97316" },

                // Blue
                new() { Id = 6,   Name = "Ocean Blue",         Primary = "#2563EB" },
                new() { Id = 7,   Name = "Sky Blue",           Primary = "#0EA5E9" },
                new() { Id = 8,   Name = "Cornflower",         Primary = "#3B82F6" },
                new() { Id = 9,   Name = "Cobalt",             Primary = "#1D4ED8" },
                new() { Id = 10,  Name = "Navy",               Primary = "#1E3A5F" },
                new() { Id = 11,  Name = "Steel Blue",         Primary = "#4A90D9" },
                new() { Id = 12,  Name = "Cerulean",           Primary = "#0891B2" },
                new() { Id = 13,  Name = "Royal Blue",         Primary = "#4169E1" },

                // Indigo / Purple
                new() { Id = 14,  Name = "Royal Indigo",       Primary = "#4F46E5" },
                new() { Id = 15,  Name = "Deep Indigo",        Primary = "#4338CA" },
                new() { Id = 16,  Name = "Violet",             Primary = "#7C3AED" },
                new() { Id = 17,  Name = "Purple",             Primary = "#9333EA" },
                new() { Id = 18,  Name = "Royal Purple",       Primary = "#7E22CE" },
                new() { Id = 19,  Name = "Grape",              Primary = "#A855F7" },
                new() { Id = 20,  Name = "Mauve",              Primary = "#C084FC" },
                new() { Id = 21,  Name = "Lavender",           Primary = "#818CF8" },
                new() { Id = 22,  Name = "Amethyst",           Primary = "#8B5CF6" },

                // Green / Teal
                new() { Id = 23,  Name = "Emerald",            Primary = "#059669" },
                new() { Id = 24,  Name = "Forest Green",       Primary = "#16A34A" },
                new() { Id = 25,  Name = "Lime",               Primary = "#65A30D" },
                new() { Id = 26,  Name = "Sage",               Primary = "#4ADE80" },
                new() { Id = 27,  Name = "Teal",               Primary = "#0D9488" },
                new() { Id = 28,  Name = "Seafoam",            Primary = "#10B981" },
                new() { Id = 29,  Name = "Mint",               Primary = "#34D399" },
                new() { Id = 30,  Name = "Jade",               Primary = "#008080" },
                new() { Id = 31,  Name = "Pine",               Primary = "#166534" },
                new() { Id = 32,  Name = "Olive",              Primary = "#65A30D" },

                // Red / Rose / Pink
                new() { Id = 33,  Name = "Crimson Red",        Primary = "#DC2626" },
                new() { Id = 34,  Name = "Rose",               Primary = "#E11D48" },
                new() { Id = 35,  Name = "Hot Pink",           Primary = "#EC4899" },
                new() { Id = 36,  Name = "Fuchsia",            Primary = "#D946EF" },
                new() { Id = 37,  Name = "Flamingo",           Primary = "#FB7185" },
                new() { Id = 38,  Name = "Scarlet",            Primary = "#EF4444" },
                new() { Id = 39,  Name = "Ruby",               Primary = "#9B1C1C" },
                new() { Id = 40,  Name = "Blush",              Primary = "#F43F5E" },

                // Orange
                new() { Id = 41,  Name = "Orange",             Primary = "#EA580C" },
                new() { Id = 42,  Name = "Burnt Orange",       Primary = "#C2410C" },
                new() { Id = 43,  Name = "Tangerine",          Primary = "#FB923C" },
                new() { Id = 44,  Name = "Peach",              Primary = "#FDBA74" },
                new() { Id = 45,  Name = "Pumpkin",            Primary = "#D97706" },

                // Cyan / Aqua
                new() { Id = 46,  Name = "Cyan",               Primary = "#06B6D4" },
                new() { Id = 47,  Name = "Aqua",               Primary = "#22D3EE" },
                new() { Id = 48,  Name = "Ice Blue",           Primary = "#67E8F9" },
                new() { Id = 49,  Name = "Turquoise",          Primary = "#14B8A6" },
                new() { Id = 50,  Name = "Electric Cyan",      Primary = "#00BCD4" },

                // Neutral / Gray
                new() { Id = 51,  Name = "Slate",              Primary = "#64748B" },
                new() { Id = 52,  Name = "Cool Gray",          Primary = "#6B7280" },
                new() { Id = 53,  Name = "Zinc",               Primary = "#71717A" },
                new() { Id = 54,  Name = "Stone",              Primary = "#78716C" },
                new() { Id = 55,  Name = "Graphite",           Primary = "#374151" },
                new() { Id = 56,  Name = "Charcoal",           Primary = "#4B5563" },

                // Specialty
                new() { Id = 57,  Name = "Midnight",           Primary = "#1E293B" },
                new() { Id = 58,  Name = "Deep Space",         Primary = "#0F172A" },
                new() { Id = 59,  Name = "Sunset",             Primary = "#F97316" },
                new() { Id = 60,  Name = "Dawn",               Primary = "#FB923C" },

                // More Blues
                new() { Id = 61,  Name = "Powder Blue",        Primary = "#93C5FD" },
                new() { Id = 62,  Name = "Denim",              Primary = "#2563EB" },
                new() { Id = 63,  Name = "Arctic",             Primary = "#BAE6FD" },
                new() { Id = 64,  Name = "Sapphire",           Primary = "#1a5276" },
                new() { Id = 65,  Name = "Lagoon",             Primary = "#0E7490" },

                // More Greens
                new() { Id = 66,  Name = "Avocado",            Primary = "#84CC16" },
                new() { Id = 67,  Name = "Fern",               Primary = "#22C55E" },
                new() { Id = 68,  Name = "Evergreen",          Primary = "#14532D" },
                new() { Id = 69,  Name = "Spearmint",          Primary = "#6EE7B7" },
                new() { Id = 70,  Name = "Eucalyptus",         Primary = "#047857" },

                // More Purples / Pinks
                new() { Id = 71,  Name = "Orchid",             Primary = "#DA77F2" },
                new() { Id = 72,  Name = "Lilac",              Primary = "#C4B5FD" },
                new() { Id = 73,  Name = "Thistle",            Primary = "#9B59B6" },
                new() { Id = 74,  Name = "Wisteria",           Primary = "#8E44AD" },
                new() { Id = 75,  Name = "Magenta",            Primary = "#DB2777" },
                new() { Id = 76,  Name = "Deep Rose",          Primary = "#BE123C" },
                new() { Id = 77,  Name = "Bubblegum",          Primary = "#F472B6" },

                // More Oranges / Reds
                new() { Id = 78,  Name = "Papaya",             Primary = "#FBBF24" },
                new() { Id = 79,  Name = "Salsa",              Primary = "#B91C1C" },
                new() { Id = 80,  Name = "Brick",              Primary = "#991B1B" },
                new() { Id = 81,  Name = "Copper",             Primary = "#B45309" },
                new() { Id = 82,  Name = "Rust",               Primary = "#92400E" },

                // More Teals / Cyan
                new() { Id = 83,  Name = "Peacock",            Primary = "#0369A1" },
                new() { Id = 84,  Name = "Petrol",             Primary = "#155E75" },
                new() { Id = 85,  Name = "Verdigris",          Primary = "#0F766E" },
                new() { Id = 86,  Name = "Malachite",          Primary = "#009966" },

                // Earth Tones
                new() { Id = 87,  Name = "Mocha",              Primary = "#795548" },
                new() { Id = 88,  Name = "Caramel",            Primary = "#A0522D" },
                new() { Id = 89,  Name = "Tan",                Primary = "#D2691E" },
                new() { Id = 90,  Name = "Sienna",             Primary = "#A0522D" },
                new() { Id = 91,  Name = "Walnut",             Primary = "#5D4037" },

                // Vivid / Neon
                new() { Id = 92,  Name = "Electric Blue",      Primary = "#0000FF" },
                new() { Id = 93,  Name = "Neon Green",         Primary = "#39FF14" },
                new() { Id = 94,  Name = "Neon Purple",        Primary = "#BC13FE" },
                new() { Id = 95,  Name = "Electric Pink",      Primary = "#FF44CC" },

                // Premium Gradients (single solid)
                new() { Id = 96,  Name = "Bordeaux",           Primary = "#722F37" },
                new() { Id = 97,  Name = "Plum",               Primary = "#673147" },
                new() { Id = 98,  Name = "Maroon",             Primary = "#800000" },
                new() { Id = 99,  Name = "Burgundy",           Primary = "#8B1E3F" },
                new() { Id = 100, Name = "Oxblood",            Primary = "#4A0010" },
                new() { Id = 101, Name = "Forest Dew",         Primary = "#2E8B57" },
                new() { Id = 102, Name = "Ocean Mist",         Primary = "#007BA7" },
                new() { Id = 103, Name = "Thunder",            Primary = "#6A0DAD" },
                new() { Id = 104, Name = "Twilight",           Primary = "#4B0082" },
                new() { Id = 105, Name = "Stellar Blue",       Primary = "#1976D2" },
            };
        }

        private static bool IsValidHexColor(string color)
        {
            return Regex.IsMatch(color.Trim(), @"^#([0-9A-Fa-f]{3}|[0-9A-Fa-f]{6})$");
        }
    }
}
