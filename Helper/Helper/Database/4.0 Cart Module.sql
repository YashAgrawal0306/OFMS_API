-- ============================================================
-- 4.0 Cart Module
-- Cart table for customer item management before order placement
-- No foreign keys — application-level validation is used
-- ============================================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'tblCart')
BEGIN
    CREATE TABLE tblCart
    (
        IdCart        INT IDENTITY(1,1) PRIMARY KEY,
        CustomerId    INT NOT NULL,
        IdItemMaster  INT NOT NULL,
        Quantity      INT NOT NULL,
        UnitPrice     DECIMAL(18,2) NOT NULL,
        TotalPrice    DECIMAL(18,2) NOT NULL,
        Remarks       NVARCHAR(500) NULL,
        IsActive      BIT NOT NULL DEFAULT 1,
        CreatedOn     DATETIME NOT NULL DEFAULT GETDATE(),
        CreatedBy     INT NOT NULL,
        UpdatedOn     DATETIME NULL,
        UpdatedBy     INT NULL
    );

    -- Unique index: a customer cannot have the same item twice
    SET QUOTED_IDENTIFIER ON;
    CREATE UNIQUE INDEX UX_tblCart_Customer_Item
        ON tblCart(CustomerId, IdItemMaster)
        WHERE IsActive = 1;
END
