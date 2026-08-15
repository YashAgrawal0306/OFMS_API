-- =============================================
-- Create tblUserThemeSettings table
-- Run this in SSMS / SQL Server connected to the OFMS database
-- =============================================
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLES 
    WHERE TABLE_NAME = 'tblUserThemeSettings'
)
BEGIN
    CREATE TABLE tblUserThemeSettings (
        IdThemeSetting  INT           NOT NULL IDENTITY(1,1) PRIMARY KEY,
        IdUser          INT           NOT NULL,
        ThemeName       NVARCHAR(100) NOT NULL,
        PrimaryColor    NVARCHAR(20)  NOT NULL,
        Mode            NVARCHAR(10)  NOT NULL DEFAULT 'Light',
        FontFamily      NVARCHAR(100) NOT NULL DEFAULT 'Outfit',
        FontSize        NVARCHAR(20)  NOT NULL DEFAULT 'Medium',
        IsActive        BIT           NOT NULL DEFAULT 1,
        CreatedOn       DATETIME      NOT NULL DEFAULT GETDATE(),
        CreatedBy       INT               NULL,
        UpdatedOn       DATETIME          NULL,
        UpdatedBy       INT               NULL
    );
    PRINT 'tblUserThemeSettings created successfully.';
END
ELSE
BEGIN
    PRINT 'tblUserThemeSettings already exists.';
END
