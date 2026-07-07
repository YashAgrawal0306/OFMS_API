BEGIN
    ALTER TABLE tblItemMaster
    ADD Ingredients NVARCHAR(MAX) NULL;
END


ALTER TABLE tblUser
DROP CONSTRAINT FK_tblUser_Roles;

ALTER TABLE tblUser
DROP COLUMN roleId;

Alter table tblUserRoleMapping Add IsActive BIT 
Alter table tblUserRoleMapping Add UpdatedOn Datetime
Alter table tblUserRoleMapping Add CreatedBy Datetime 
Alter table tblUserRoleMapping Add UpdatedBy Datetime


EXEC sp_rename 
    'tblUserRoleMapping.Id',
    'IdUserRoleMapping',
    'COLUMN';


    

ALTER TABLE tblOrderMaster
ADD IdOrderType INT NULL;  

ALTER TABLE tblOrderDetails
ADD IdKitchenStatus INT NULL;


 
EXEC sp_rename 
    'tblpayment.PaymentMethod',
    'PaymentTypeId',
    'COLUMN';

    
 Alter table tblpayment alter column PaymentTypeId INT


    Alter table tblOrderMaster ADD IdAddressMapping int 
  Alter table tblOrderMaster Alter Column IdAddressMapping int not null