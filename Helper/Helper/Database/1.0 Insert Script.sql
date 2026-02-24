--for the group table of the item

CREATE TABLE tblGroupMaster (
    IdGroupMaster INT PRIMARY KEY IDENTITY(1,1),
    GroupName VARCHAR(100) NOT NULL,
    Description VARCHAR(255),
    IsActive BIT DEFAULT 1,
    CreatedOn DATETIME DEFAULT GETDATE(),
    CreatedBy INT NOT NULL,   
    UpdatedOn DATETIME NULL,
    UpdatedBy INT NULL
);


CREATE TABLE tblCategoryMaster (
    IdCategory INT PRIMARY KEY IDENTITY(1,1),
    IdGroupMaster INT NOT NULL,
    ParentId INT NULL,
    CategoryName NVARCHAR(200) NOT NULL,
    CatDescription NVARCHAR(500) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedBy INT NULL,
    UpdatedAt DATETIME NULL,
    UpdatedBy INT NULL
);

CREATE TABLE tblItemMaster (
    IdItemMaster INT PRIMARY KEY IDENTITY(1,1),
    IdCategory INT NOT NULL,
    IdSubCategory INT NULL,
    IdGroupMaster INT NOT NULL,
    ItemName NVARCHAR(200) NOT NULL,
    ItemDescription NVARCHAR(500) NULL,
    Price DECIMAL(18,2) NOT NULL DEFAULT 0,
    Quantity INT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedBy INT NULL,
    UpdatedAt DATETIME NULL,
    UpdatedBy INT NULL,
);

CREATE TABLE tblItemPriceHistory (
    IdPriceHistory INT PRIMARY KEY IDENTITY(1,1),
    IdItem INT NOT NULL,
    OldPrice DECIMAL(18,2) NOT NULL,
    NewPrice DECIMAL(18,2) NOT NULL,
    EffectiveFrom DATETIME NOT NULL DEFAULT GETDATE(),
    EffectiveTo DATETIME NULL,
    ChangedBy INT NULL,
    ChangeReason NVARCHAR(500) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
);

CREATE TABLE tblItemMasterImage (
    IdItemMasterImage INT PRIMARY KEY IDENTITY(1,1),
    ImageTypeId  INT NOT NULL,    -- FK to ImageTypeMaster
    ReferenceId INT NOT NULL,    -- Id of Group / Category / Item / SubCategory
    ImageUrl NVARCHAR(500) NOT NULL, 
    IsMain BIT NOT NULL DEFAULT 0,
    DisplayOrder INT NOT NULL DEFAULT 0,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedBy INT NULL,
    UpdatedBy INT NULL,
    UpdatedOn DATETIME NULL,
);


CREATE TABLE [dbo].[dimImageType](
    [IdImageType]   [int]           NOT NULL,          -- Manual fixed ID
    [ImageType]     [nvarchar](50)  NOT NULL,          -- CAPS key e.g. ITEM, CATEGORY
    [ImageTypeName] [nvarchar](100) NOT NULL,
    [Description]   [nvarchar](250) NOT NULL,
    [IsActive]      [bit]           NOT NULL DEFAULT (1),
    [CreatedAt]     [datetime]      NOT NULL DEFAULT (GETDATE()),
    [CreatedBy]     [int]           NULL,
    [UpdatedBy]     [int]           NULL,
    [UpdatedOn]     [datetime]      NULL,
PRIMARY KEY CLUSTERED ([IdImageType] ASC)
);
