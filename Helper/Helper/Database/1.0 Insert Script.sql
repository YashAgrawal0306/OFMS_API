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


CREATE TABLE tblUserRoleMapping
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    RoleId INT NOT NULL,
    CreatedOn DATETIME NOT NULL DEFAULT GETDATE()
);


CREATE TABLE dimTransactionType
(
    IdTransactionType INT IDENTITY(1,1) PRIMARY KEY,
    TransactionTypeName VARCHAR(100) NOT NULL,
    Description VARCHAR(500) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedOn DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedBy INT NULL,
    UpdatedOn DATETIME NULL,
    UpdatedBy INT NULL
);

CREATE TABLE dimStatus
(
    IdStatus INT IDENTITY(1,1) PRIMARY KEY,
    IdTransactionType INT NOT NULL,
    StatusName VARCHAR(100) NOT NULL,
    Description VARCHAR(MAX) NULL,
    ColorCode VARCHAR(20) NULL,
    SequenceNo INT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedOn DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedBy INT NULL,
    UpdatedOn DATETIME NULL,
    UpdatedBy INT NULL
);


CREATE TABLE tblOrderMaster
(
    IdOrderMaster INT IDENTITY(1,1) PRIMARY KEY,
    OrderNo VARCHAR(50) NOT NULL,
    CustomerId INT NOT NULL,

    IdStatus INT NOT NULL, -- New, Accepted, Completed etc.

    SubTotal DECIMAL(18,2) NOT NULL DEFAULT 0,
    TaxAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    DeliveryCharge DECIMAL(18,2) NOT NULL DEFAULT 0,
    DiscountAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    GrandTotal DECIMAL(18,2) NOT NULL DEFAULT 0,

    Remarks NVARCHAR(500) NULL,

    IsActive BIT NOT NULL DEFAULT 1,
    CreatedOn DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedBy INT NULL,
    UpdatedOn DATETIME NULL,
    UpdatedBy INT NULL
);

CREATE TABLE tblOrderDetails
(
    IdOrderDetails INT IDENTITY(1,1) PRIMARY KEY,
    IdOrderMaster INT NOT NULL,
    IdItemMaster INT NOT NULL,

    Quantity INT NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    TotalPrice DECIMAL(18,2) NOT NULL,

    CreatedOn DATETIME NOT NULL DEFAULT GETDATE()
);


CREATE TABLE dimOrderType
(
    IdOrderType INT IDENTITY(1,1) PRIMARY KEY,
    OrderTypeName VARCHAR(100) NOT NULL,
    Description VARCHAR(500) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedOn DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedBy INT NULL,
    UpdatedOn DATETIME NULL,
    UpdatedBy INT NULL
);

    CREATE TABLE tblCookAssignment
(
    IdCookAssignment INT IDENTITY(1,1) PRIMARY KEY,
    IdOrderDetails INT NOT NULL,
    CookUserId INT NOT NULL,
    IdStatus INT NOT NULL,
    AssignedOn DATETIME NOT NULL DEFAULT GETDATE(),
    AssignedBy INT NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedOn DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedBy INT NULL,
    UpdatedOn DATETIME NULL,
    UpdatedBy INT NULL
);


CREATE TABLE tblDeliveryAssignment
(
    IdDeliveryAssignment INT IDENTITY(1,1) PRIMARY KEY,
    IdOrderMaster INT NOT NULL,
    DeliveryBoyId INT NOT NULL,
    IdStatus INT NOT NULL,
    AssignedOn DATETIME NOT NULL DEFAULT GETDATE(),
    AssignedBy INT NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedOn DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedBy INT NULL,
    UpdatedOn DATETIME NULL,
    UpdatedBy INT NULL
);


CREATE TABLE tblPayment
(
    IdPayment INT IDENTITY(1,1) PRIMARY KEY,
    IdOrderMaster INT NOT NULL,

    Amount DECIMAL(18,2) NOT NULL,
    PaymentMethod VARCHAR(50) NULL,
    TransactionNo VARCHAR(100) NULL,
    TransactionTypeId VARCHAR(100) NULL,
    IdStatus INT NOT NULL,

    IsActive BIT NOT NULL DEFAULT 1,
    CreatedOn DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedBy INT NULL,
    UpdatedOn DATETIME NULL,
    UpdatedBy INT NULL
);


 CREATE TABLE dimPaymentType
(
    PaymentTypeId INT IDENTITY(1,1) PRIMARY KEY,
    PaymentTypeName VARCHAR(50) NOT NULL,
    IsActive BIT DEFAULT 1,
    CreatedOn DATETIME DEFAULT GETDATE()
);

CREATE TABLE dimCountry
(
    IdCountry INT IDENTITY(1,1) PRIMARY KEY,
    CountryCode VARCHAR(10) NOT NULL,
    CountryName VARCHAR(150) NOT NULL,

    IsActive BIT NOT NULL DEFAULT 1,

    CreatedOn DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedBy INT NULL,

    UpdatedOn DATETIME NULL,
    UpdatedBy INT NULL
);

CREATE TABLE dimState
(
    IdState INT IDENTITY(1,1) PRIMARY KEY,

    IdCountry INT NOT NULL,
    StateCode VARCHAR(20) NOT NULL,
    StateName VARCHAR(150) NOT NULL,

    IsActive BIT NOT NULL DEFAULT 1,

    CreatedOn DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedBy INT NULL,

    UpdatedOn DATETIME NULL,
    UpdatedBy INT NULL,

    CONSTRAINT FK_dimState_dimCountry
    FOREIGN KEY(IdCountry)
    REFERENCES dimCountry(IdCountry)
);

CREATE TABLE dimCity
(
    IdCity INT IDENTITY(1,1) PRIMARY KEY,

    IdState INT NOT NULL,
    CityCode VARCHAR(20) NOT NULL,
    CityName VARCHAR(150) NOT NULL,

    IsActive BIT NOT NULL DEFAULT 1,

    CreatedOn DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedBy INT NULL,

    UpdatedOn DATETIME NULL,
    UpdatedBy INT NULL,

    CONSTRAINT FK_dimCity_dimState
    FOREIGN KEY(IdState)
    REFERENCES dimState(IdState)
);


CREATE TABLE dimAddressType
(
    IdAddressType INT IDENTITY(1,1) PRIMARY KEY,

    AddressTypeName VARCHAR(100) NOT NULL,
    Description VARCHAR(500) NULL,

    IsActive BIT NOT NULL DEFAULT 1,

    CreatedOn DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedBy INT NULL,

    UpdatedOn DATETIME NULL,
    UpdatedBy INT NULL
);


CREATE TABLE tblAddress
(
    IdAddress INT IDENTITY(1,1) PRIMARY KEY,

    AddressLine1 VARCHAR(500) NOT NULL,
    AddressLine2 VARCHAR(500) NULL,

    Landmark VARCHAR(250) NULL,
    Area VARCHAR(250) NULL,
    Locality VARCHAR(250) NULL,

    IdCity INT NOT NULL,
    IdState INT NOT NULL,
    IdCountry INT NOT NULL,

    Pincode VARCHAR(20) NOT NULL,

    Latitude DECIMAL(18,8) NULL,
    Longitude DECIMAL(18,8) NULL,

    IsActive BIT NOT NULL DEFAULT 1,

    CreatedOn DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedBy INT NULL,

    UpdatedOn DATETIME NULL,
    UpdatedBy INT NULL,

    FOREIGN KEY(IdCountry) REFERENCES dimCountry(IdCountry),
    FOREIGN KEY(IdState) REFERENCES dimState(IdState),
    FOREIGN KEY(IdCity) REFERENCES dimCity(IdCity)
);


CREATE TABLE tblAddressMapping
(
    IdAddressMapping INT IDENTITY(1,1) PRIMARY KEY,

    EntityType VARCHAR(50) NOT NULL,
    EntityId INT NOT NULL,

    IdAddress INT NOT NULL,
    IdAddressType INT NOT NULL,

    IsDefault BIT NOT NULL DEFAULT 0,

    IsActive BIT NOT NULL DEFAULT 1,

    CreatedOn DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedBy INT NULL,

    UpdatedOn DATETIME NULL,
    UpdatedBy INT NULL,

    FOREIGN KEY(IdAddress)
    REFERENCES tblAddress(IdAddress),

    FOREIGN KEY(IdAddressType)
    REFERENCES dimAddressType(IdAddressType)
);


CREATE TABLE dimEntityType
(
    IdEntityType INT IDENTITY(1,1) PRIMARY KEY,

    EntityTypeCode VARCHAR(50) NOT NULL,
    EntityTypeName VARCHAR(100) NOT NULL,
    Description VARCHAR(500) NULL,

    IsActive BIT NOT NULL DEFAULT 1,

    CreatedOn DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedBy INT NULL,

    UpdatedOn DATETIME NULL,
    UpdatedBy INT NULL
);




CREATE TABLE tblCookAssignment
(
    IdCookAssignment INT IDENTITY(1,1) PRIMARY KEY,

    IdOrderMaster INT NOT NULL,
    IdOrderDetails INT NULL,
    -- NULL = Whole Order Assignment
    -- Value = Specific Item Assignment

    CookUserId INT NOT NULL,
    -- User assigned as Cook

    IdStatus INT NOT NULL,
    -- Assigned
    -- Accepted
    -- Preparing
    -- Ready

    AssignedOn DATETIME NOT NULL DEFAULT GETDATE(),
    AcceptedOn DATETIME NULL,
    StartCookingOn DATETIME NULL,
    ReadyOn DATETIME NULL,

    EstimatedPreparationTime INT NULL,
    -- In Minutes

    ActualPreparationTime INT NULL,
    -- In Minutes

    Remarks NVARCHAR(500) NULL,

    IsActive BIT NOT NULL DEFAULT 1,

    CreatedOn DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedBy INT NULL,

    UpdatedOn DATETIME NULL,
    UpdatedBy INT NULL
);

CREATE TABLE tblDeliveryAssignment
(
    IdDeliveryAssignment INT IDENTITY(1,1) PRIMARY KEY,

    IdOrderMaster INT NOT NULL,

    DeliveryBoyUserId INT NOT NULL,
    -- User assigned for delivery

    IdStatus INT NOT NULL,
    -- Assigned
    -- Accepted
    -- Picked Up
    -- Delivered
    -- Cancelled

    AssignedOn DATETIME NOT NULL DEFAULT GETDATE(),

    AcceptedOn DATETIME NULL,

    PickedUpOn DATETIME NULL,

    DeliveredOn DATETIME NULL,

    EstimatedDeliveryTime INT NULL,
    -- Minutes

    ActualDeliveryTime INT NULL,
    -- Minutes

    DeliveryRemarks NVARCHAR(500) NULL,

    IsActive BIT NOT NULL DEFAULT 1,

    CreatedOn DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedBy INT NULL,

    UpdatedOn DATETIME NULL,
    UpdatedBy INT NULL
);