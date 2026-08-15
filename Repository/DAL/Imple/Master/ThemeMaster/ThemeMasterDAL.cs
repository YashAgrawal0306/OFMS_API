using Dapper;
using DTO.Models.Master.ThemeMaster;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Repository.DAL.Interface.Master.ThemeMaster;

namespace Repository.DAL.Imple.Master.ThemeMaster
{
    public class ThemeMasterDAL : IThemeMasterDAL
    {
        private readonly string _connq;

        public ThemeMasterDAL(IConfiguration configuration)
        {
            _connq = configuration.GetConnectionString("DefaultConnection") ?? "";
        }

        /// <summary>Returns the active theme record for the given user, or null if none exists.</summary>
        public async Task<TblThemeResponseTO?> GetUserTheme(int userId)
        {
            using var conn = new SqlConnection(_connq);
            const string sql = @"
                SELECT IdThemeSetting, IdUser, ThemeName, PrimaryColor, Mode, FontFamily, FontSize
                FROM   tblUserThemeSettings
                WHERE  IdUser = @UserId AND IsActive = 1";

            return await conn.QueryFirstOrDefaultAsync<TblThemeResponseTO>(sql, new { UserId = userId });
        }

        /// <summary>
        /// Upserts the user's theme: deactivates any existing record, then inserts a fresh one.
        /// Returns the new IdThemeSetting on success, 0 on failure.
        /// </summary>
        public async Task<int> SaveUserTheme(int userId, TblThemeTO themeTO)
        {
            using var conn = new SqlConnection(_connq);
            await conn.OpenAsync();

            using var tran = conn.BeginTransaction();
            try
            {
                // Step 1 – deactivate any existing active record for this user
                const string deactivateSql = @"
                    UPDATE tblUserThemeSettings
                    SET    IsActive  = 0,
                           UpdatedOn = GETDATE(),
                           UpdatedBy = @UserId
                    WHERE  IdUser  = @UserId AND IsActive = 1";

                await conn.ExecuteAsync(deactivateSql, new { UserId = userId }, tran);

                // Step 2 – insert the new record
                const string insertSql = @"
                    INSERT INTO tblUserThemeSettings
                        (IdUser, ThemeName, PrimaryColor, Mode, FontFamily, FontSize, IsActive, CreatedOn, CreatedBy)
                    VALUES
                        (@UserId, @ThemeName, @PrimaryColor, @Mode, @FontFamily, @FontSize, 1, GETDATE(), @UserId);
                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

                int newId = await conn.ExecuteScalarAsync<int>(insertSql, new
                {
                    UserId       = userId,
                    ThemeName    = themeTO.ThemeName,
                    PrimaryColor = themeTO.PrimaryColor,
                    Mode         = themeTO.Mode,
                    FontFamily   = themeTO.FontFamily,
                    FontSize     = themeTO.FontSize
                }, tran);

                tran.Commit();
                return newId;
            }
            catch
            {
                tran.Rollback();
                return 0;
            }
        }

        /// <summary>Deactivates all active theme records for the user (restores default).</summary>
        public async Task<int> ResetUserTheme(int userId)
        {
            using var conn = new SqlConnection(_connq);
            const string sql = @"
                UPDATE tblUserThemeSettings
                SET    IsActive  = 0,
                       UpdatedOn = GETDATE(),
                       UpdatedBy = @UserId
                WHERE  IdUser = @UserId AND IsActive = 1";

            return await conn.ExecuteAsync(sql, new { UserId = userId });
        }
    }
}
