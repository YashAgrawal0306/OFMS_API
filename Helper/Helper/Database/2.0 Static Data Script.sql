INSERT INTO [dbo].[dimImageType] (IdImageType, ImageType, ImageTypeName, [Description], IsActive, CreatedAt, CreatedBy)
VALUES
    (1, 'GROUP',       'Group Image',       'Image associated with a group master',        1, GETDATE(), 1),
    (2, 'CATEGORY',    'Category Image',    'Image associated with a category master',     1, GETDATE(), 1),
    (3, 'SUBCATEGORY', 'Sub Category Image','Image associated with a sub category master', 1, GETDATE(), 1),
    (4, 'ITEM',        'Item Image',        'Image associated with an item master',        1, GETDATE(), 1);
GO



INSERT INTO dimTransactionType(TransactionTypeName)
VALUES
('Order Status'),
('Payment Status'),
('Delivery Status');


INSERT INTO dimStatus
(IdTransactionType, StatusName, Description, ColorCode, SequenceNo)
VALUES
(1,'New','Order placed by customer','#17A2B8',1),
(1,'Accepted','Order accepted by manager','#007BFF',2),
(1,'Cook Assigned','Order assigned to cook','#6F42C1',3),
(1,'Ready','Order is ready for delivery','#20C997',4),
(1,'Delivery Assigned','Delivery boy assigned','#FD7E14',5),
(1,'Completed','Order completed successfully','#28A745',6),
(1,'Cancelled','Order cancelled','#DC3545',7);


INSERT INTO dimStatus
(IdTransactionType, StatusName, Description, ColorCode, SequenceNo)
VALUES
(2,'Assigned','Order assigned to cook','#007BFF',1),
(2,'Accepted','Cook accepted the order','#6F42C1',2),
(2,'Preparing','Food preparation in progress','#FFC107',3),
(2,'Ready','Food preparation completed','#28A745',4);


INSERT INTO dimStatus
(IdTransactionType, StatusName, Description, ColorCode, SequenceNo)
VALUES
(3,'Assigned','Order assigned to delivery boy','#007BFF',1),
(3,'Accepted','Delivery boy accepted assignment','#6F42C1',2),
(3,'Picked Up','Order picked up from restaurant','#FD7E14',3),
(3,'Delivered','Order delivered to customer','#28A745',4);

INSERT INTO dimStatus
(IdTransactionType, StatusName, Description, ColorCode, SequenceNo)
VALUES
(4,'Pending','Payment is pending','#FFC107',1),
(4,'Paid','Payment completed successfully','#28A745',2),
(4,'Failed','Payment failed','#DC3545',3),
(4,'Refunded','Payment refunded','#17A2B8',4);



INSERT INTO dimOrderType
(
    OrderTypeName,
    Description,
    IsActive,
    CreatedBy
)
VALUES
('Online', 'Order placed through customer website', 1, 1),

('Walk-In', 'Customer visits restaurant and places order directly', 1, 1),

('Phone', 'Order placed through phone call', 1, 1),

('WhatsApp', 'Order received through WhatsApp', 1, 1),

('Counter', 'Order created by manager at billing counter', 1, 1);





 INSERT INTO dimPaymentType (PaymentTypeName, IsActive, CreatedOn)
VALUES
('Cash', 1, GETDATE()),
('UPI', 1, GETDATE()),
('Card', 1, GETDATE()),
('Online Payment', 1, GETDATE());


INSERT INTO dimAddressType(AddressTypeName,Description)
VALUES
('Home','Home Address'),
('Office','Office Address'),
('Other','Other Address');


INSERT INTO dimEntityType
(
    EntityTypeCode,
    EntityTypeName,
    Description,
    IsActive,
    CreatedOn,
    CreatedBy
)
VALUES
('CUSTOMER', 'Customer', 'Registered customer address mapping', 1, GETDATE(), 1),
('GUEST', 'Guest Customer', 'Guest customer address mapping', 1, GETDATE(), 1),
('EMPLOYEE', 'Employee', 'Employee address mapping', 1, GETDATE(), 1),
('SUPPLIER', 'Supplier', 'Supplier address mapping', 1, GETDATE(), 1),
('BRANCH', 'Branch', 'Restaurant branch address mapping', 1, GETDATE(), 1);