-- 1. Modify tblCookAssignment to support merged items
IF COL_LENGTH('tblCookAssignment', 'IdItemMaster') IS NULL
BEGIN
    ALTER TABLE tblCookAssignment ADD IdItemMaster INT NULL;
    ALTER TABLE tblCookAssignment ADD TotalQuantity INT NULL;
    ALTER TABLE tblCookAssignment ADD IsMerged BIT DEFAULT 0;
END
GO

-- 2. Make IdOrderMaster and IdOrderDetails nullable
ALTER TABLE tblCookAssignment ALTER COLUMN IdOrderMaster INT NULL;
GO
ALTER TABLE tblCookAssignment ALTER COLUMN IdOrderDetails INT NULL;
GO

-- 3. Create tblCookAssignmentMapping if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tblCookAssignmentMapping]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[tblCookAssignmentMapping](
        [IdMapping] [int] IDENTITY(1,1) NOT NULL,
        [IdCookAssignment] [int] NOT NULL,
        [IdOrderMaster] [int] NOT NULL,
        [IdOrderDetails] [int] NOT NULL,
        [Quantity] [int] NOT NULL,
        [IdStatus] [int] NOT NULL,
        [CreatedOn] [datetime] NOT NULL DEFAULT GETDATE(),
        [UpdatedOn] [datetime] NULL,
        CONSTRAINT [PK_tblCookAssignmentMapping] PRIMARY KEY CLUSTERED 
        (
            [IdMapping] ASC
        )
    )

    -- Note: Foreign keys can be added if needed, but keeping it simple based on existing structure.
END
GO
