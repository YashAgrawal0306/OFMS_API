-- ═══════════════════════════════════════════════════════════════════════════
-- OFMS RBAC Permission System — SQL Script
-- Run this on your existing OFMS database
-- ═══════════════════════════════════════════════════════════════════════════

-- ─────────────────────────────────────────────────────────────────────────
-- 1. tblModule  (App modules / menu items)
-- ─────────────────────────────────────────────────────────────────────────
CREATE TABLE tblModule (
    IdModule        INT             PRIMARY KEY IDENTITY(1,1),
    ModuleName      NVARCHAR(100)   NOT NULL,
    ModuleKey       NVARCHAR(100)   NOT NULL UNIQUE,   -- e.g. 'Orders', 'Kitchen'
    ParentModuleId  INT             NULL,              -- for nested menus
    DisplayOrder    INT             NOT NULL DEFAULT 0,
    Icon            NVARCHAR(100)   NULL,              -- FontAwesome class
    Route           NVARCHAR(200)   NULL,              -- Angular route path
    IsActive        BIT             NOT NULL DEFAULT 1,
    CreatedOn       DATETIME        NOT NULL DEFAULT GETDATE(),
    CreatedBy       INT             NULL,
    UpdatedOn       DATETIME        NULL,
    UpdatedBy       INT             NULL
);

-- ─────────────────────────────────────────────────────────────────────────
-- 2. tblPermission  (All possible actions per module)
-- ─────────────────────────────────────────────────────────────────────────
CREATE TABLE tblPermission (
    IdPermission    INT             PRIMARY KEY IDENTITY(1,1),
    IdModule        INT             NOT NULL,
    PermissionKey   NVARCHAR(150)   NOT NULL UNIQUE,   -- e.g. 'Orders.View', 'Orders.Edit'
    PermissionName  NVARCHAR(150)   NOT NULL,
    PermissionType  NVARCHAR(50)    NOT NULL DEFAULT 'Action', -- Module | Action | DataScope
    IsActive        BIT             NOT NULL DEFAULT 1,
    CreatedOn       DATETIME        NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (IdModule) REFERENCES tblModule(IdModule)
);

-- ─────────────────────────────────────────────────────────────────────────
-- 3. tblRolePermission  (Role → Permission mapping)
-- ─────────────────────────────────────────────────────────────────────────
CREATE TABLE tblRolePermission (
    IdRolePermission    INT     PRIMARY KEY IDENTITY(1,1),
    RoleId              INT     NOT NULL,
    IdPermission        INT     NOT NULL,
    IsAllowed           BIT     NOT NULL DEFAULT 1,
    CreatedOn           DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedBy           INT     NULL,
    UpdatedOn           DATETIME NULL,
    UpdatedBy           INT     NULL,
    UNIQUE (RoleId, IdPermission)
);

-- ─────────────────────────────────────────────────────────────────────────
-- 4. tblUserPermission  (User-level overrides — only store overridden perms)
-- ─────────────────────────────────────────────────────────────────────────
CREATE TABLE tblUserPermission (
    IdUserPermission    INT     PRIMARY KEY IDENTITY(1,1),
    UserId              INT     NOT NULL,
    IdPermission        INT     NOT NULL,
    IsAllowed           BIT     NOT NULL DEFAULT 1,
    CreatedOn           DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedBy           INT     NULL,
    UpdatedOn           DATETIME NULL,
    UpdatedBy           INT     NULL,
    UNIQUE (UserId, IdPermission)
);

-- ═══════════════════════════════════════════════════════════════════════════
-- SEED DATA — Modules
-- ═══════════════════════════════════════════════════════════════════════════
INSERT INTO tblModule (ModuleName, ModuleKey, DisplayOrder, Icon, Route) VALUES
('Dashboard',           'Dashboard',         1,  'fas fa-tachometer-alt',   '/adminDashboard'),
('Orders',              'Orders',            2,  'fas fa-shopping-cart',    '/OrderMaster'),
('Cook Assignment',     'CookAssignment',    3,  'fas fa-utensils',         '/CookAssignment'),
('Delivery Assignment', 'DeliveryAssignment',4,  'fas fa-motorcycle',       '/DeliveryAssignment'),
('Menu Management',     'MenuManagement',    5,  'fas fa-book-open',        '/DisplayMenuList'),
('Item Master',         'ItemMaster',        6,  'fas fa-hamburger',        '/ItemMaster'),
('Group Master',        'GroupMaster',       7,  'fas fa-layer-group',      '/GroupMaster'),
('Category Master',     'CategoryMaster',    8,  'fas fa-tags',             '/CategoryMaster'),
('Member Management',   'MemberManagement',  9,  'fas fa-users',            '/MemberManagement'),
('Customer List',       'CustomerList',      10, 'fas fa-user-friends',     '/CustomerList'),
('Address Master',      'AddressMaster',     11, 'fas fa-map-marker-alt',   '/AddressMaster'),
('Permission Matrix',   'PermissionMatrix',  12, 'fas fa-shield-alt',       '/PermissionMatrix');

-- ═══════════════════════════════════════════════════════════════════════════
-- SEED DATA — Permissions per Module
-- ═══════════════════════════════════════════════════════════════════════════
-- Dashboard
INSERT INTO tblPermission (IdModule, PermissionKey, PermissionName, PermissionType) VALUES
((SELECT IdModule FROM tblModule WHERE ModuleKey='Dashboard'), 'Dashboard.View', 'View Dashboard', 'Action');

-- Orders
INSERT INTO tblPermission (IdModule, PermissionKey, PermissionName, PermissionType) VALUES
((SELECT IdModule FROM tblModule WHERE ModuleKey='Orders'), 'Orders.View',           'View Orders',           'Action'),
((SELECT IdModule FROM tblModule WHERE ModuleKey='Orders'), 'Orders.Add',            'Add Order',             'Action'),
((SELECT IdModule FROM tblModule WHERE ModuleKey='Orders'), 'Orders.Edit',           'Edit Order',            'Action'),
((SELECT IdModule FROM tblModule WHERE ModuleKey='Orders'), 'Orders.Delete',         'Delete Order',          'Action'),
((SELECT IdModule FROM tblModule WHERE ModuleKey='Orders'), 'Orders.AssignCook',     'Assign Cook',           'Action'),
((SELECT IdModule FROM tblModule WHERE ModuleKey='Orders'), 'Orders.AssignDelivery', 'Assign Delivery',       'Action'),
((SELECT IdModule FROM tblModule WHERE ModuleKey='Orders'), 'Orders.ChangeStatus',   'Change Order Status',   'Action'),
((SELECT IdModule FROM tblModule WHERE ModuleKey='Orders'), 'Orders.Export',         'Export Orders',         'Action');

-- Cook Assignment
INSERT INTO tblPermission (IdModule, PermissionKey, PermissionName, PermissionType) VALUES
((SELECT IdModule FROM tblModule WHERE ModuleKey='CookAssignment'), 'CookAssignment.View',   'View Cook Assignments', 'Action'),
((SELECT IdModule FROM tblModule WHERE ModuleKey='CookAssignment'), 'CookAssignment.Assign', 'Assign Cook',           'Action'),
((SELECT IdModule FROM tblModule WHERE ModuleKey='CookAssignment'), 'CookAssignment.Update', 'Update Cook Status',    'Action');

-- Delivery Assignment
INSERT INTO tblPermission (IdModule, PermissionKey, PermissionName, PermissionType) VALUES
((SELECT IdModule FROM tblModule WHERE ModuleKey='DeliveryAssignment'), 'DeliveryAssignment.View',   'View Deliveries',    'Action'),
((SELECT IdModule FROM tblModule WHERE ModuleKey='DeliveryAssignment'), 'DeliveryAssignment.Assign', 'Assign Delivery Boy','Action'),
((SELECT IdModule FROM tblModule WHERE ModuleKey='DeliveryAssignment'), 'DeliveryAssignment.Update', 'Update Delivery Status','Action');

-- Menu Management
INSERT INTO tblPermission (IdModule, PermissionKey, PermissionName, PermissionType) VALUES
((SELECT IdModule FROM tblModule WHERE ModuleKey='MenuManagement'), 'MenuManagement.View',   'View Menu',   'Action'),
((SELECT IdModule FROM tblModule WHERE ModuleKey='MenuManagement'), 'MenuManagement.Add',    'Add Item',    'Action'),
((SELECT IdModule FROM tblModule WHERE ModuleKey='MenuManagement'), 'MenuManagement.Edit',   'Edit Item',   'Action'),
((SELECT IdModule FROM tblModule WHERE ModuleKey='MenuManagement'), 'MenuManagement.Delete', 'Delete Item', 'Action');

-- Item Master
INSERT INTO tblPermission (IdModule, PermissionKey, PermissionName, PermissionType) VALUES
((SELECT IdModule FROM tblModule WHERE ModuleKey='ItemMaster'), 'ItemMaster.View',   'View Items',   'Action'),
((SELECT IdModule FROM tblModule WHERE ModuleKey='ItemMaster'), 'ItemMaster.Add',    'Add Item',     'Action'),
((SELECT IdModule FROM tblModule WHERE ModuleKey='ItemMaster'), 'ItemMaster.Edit',   'Edit Item',    'Action'),
((SELECT IdModule FROM tblModule WHERE ModuleKey='ItemMaster'), 'ItemMaster.Delete', 'Delete Item',  'Action');

-- Group Master
INSERT INTO tblPermission (IdModule, PermissionKey, PermissionName, PermissionType) VALUES
((SELECT IdModule FROM tblModule WHERE ModuleKey='GroupMaster'), 'GroupMaster.View',   'View Groups',   'Action'),
((SELECT IdModule FROM tblModule WHERE ModuleKey='GroupMaster'), 'GroupMaster.Add',    'Add Group',     'Action'),
((SELECT IdModule FROM tblModule WHERE ModuleKey='GroupMaster'), 'GroupMaster.Edit',   'Edit Group',    'Action'),
((SELECT IdModule FROM tblModule WHERE ModuleKey='GroupMaster'), 'GroupMaster.Delete', 'Delete Group',  'Action');

-- Category Master
INSERT INTO tblPermission (IdModule, PermissionKey, PermissionName, PermissionType) VALUES
((SELECT IdModule FROM tblModule WHERE ModuleKey='CategoryMaster'), 'CategoryMaster.View',   'View Categories', 'Action'),
((SELECT IdModule FROM tblModule WHERE ModuleKey='CategoryMaster'), 'CategoryMaster.Add',    'Add Category',    'Action'),
((SELECT IdModule FROM tblModule WHERE ModuleKey='CategoryMaster'), 'CategoryMaster.Edit',   'Edit Category',   'Action'),
((SELECT IdModule FROM tblModule WHERE ModuleKey='CategoryMaster'), 'CategoryMaster.Delete', 'Delete Category', 'Action');

-- Member Management
INSERT INTO tblPermission (IdModule, PermissionKey, PermissionName, PermissionType) VALUES
((SELECT IdModule FROM tblModule WHERE ModuleKey='MemberManagement'), 'MemberManagement.View',   'View Members',   'Action'),
((SELECT IdModule FROM tblModule WHERE ModuleKey='MemberManagement'), 'MemberManagement.Add',    'Add Member',     'Action'),
((SELECT IdModule FROM tblModule WHERE ModuleKey='MemberManagement'), 'MemberManagement.Edit',   'Edit Member',    'Action'),
((SELECT IdModule FROM tblModule WHERE ModuleKey='MemberManagement'), 'MemberManagement.Delete', 'Delete Member',  'Action');

-- Customer List
INSERT INTO tblPermission (IdModule, PermissionKey, PermissionName, PermissionType) VALUES
((SELECT IdModule FROM tblModule WHERE ModuleKey='CustomerList'), 'CustomerList.View',   'View Customers', 'Action'),
((SELECT IdModule FROM tblModule WHERE ModuleKey='CustomerList'), 'CustomerList.Add',    'Add Customer',   'Action'),
((SELECT IdModule FROM tblModule WHERE ModuleKey='CustomerList'), 'CustomerList.Edit',   'Edit Customer',  'Action'),
((SELECT IdModule FROM tblModule WHERE ModuleKey='CustomerList'), 'CustomerList.Delete', 'Delete Customer','Action');

-- Address Master
INSERT INTO tblPermission (IdModule, PermissionKey, PermissionName, PermissionType) VALUES
((SELECT IdModule FROM tblModule WHERE ModuleKey='AddressMaster'), 'AddressMaster.View',   'View Addresses', 'Action'),
((SELECT IdModule FROM tblModule WHERE ModuleKey='AddressMaster'), 'AddressMaster.Add',    'Add Address',    'Action'),
((SELECT IdModule FROM tblModule WHERE ModuleKey='AddressMaster'), 'AddressMaster.Edit',   'Edit Address',   'Action'),
((SELECT IdModule FROM tblModule WHERE ModuleKey='AddressMaster'), 'AddressMaster.Delete', 'Delete Address', 'Action');

-- Permission Matrix (Admin Only)
INSERT INTO tblPermission (IdModule, PermissionKey, PermissionName, PermissionType) VALUES
((SELECT IdModule FROM tblModule WHERE ModuleKey='PermissionMatrix'), 'PermissionMatrix.View',   'View Permission Matrix', 'Action'),
((SELECT IdModule FROM tblModule WHERE ModuleKey='PermissionMatrix'), 'PermissionMatrix.Edit',   'Edit Permissions',       'Action');

-- ═══════════════════════════════════════════════════════════════════════════
-- SEED DATA — Role Permissions (ALL ALLOWED for all roles as default)
-- Using existing roles from tblRole. Adjust RoleIds to match your DB.
-- ═══════════════════════════════════════════════════════════════════════════
-- Grant ALL permissions to ALL roles (IsAllowed = 1)
-- This matches user requirement: "for now all have allow"
INSERT INTO tblRolePermission (RoleId, IdPermission, IsAllowed)
SELECT r.RoleId, p.IdPermission, 1
FROM tblPermission p
CROSS JOIN (SELECT DISTINCT RoleId FROM tblUserRoleMapping) r
WHERE NOT EXISTS (
    SELECT 1 FROM tblRolePermission rp
    WHERE rp.RoleId = r.RoleId AND rp.IdPermission = p.IdPermission
);
