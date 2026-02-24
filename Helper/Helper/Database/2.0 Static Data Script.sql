INSERT INTO [dbo].[dimImageType] (IdImageType, ImageType, ImageTypeName, [Description], IsActive, CreatedAt, CreatedBy)
VALUES
    (1, 'GROUP',       'Group Image',       'Image associated with a group master',        1, GETDATE(), 1),
    (2, 'CATEGORY',    'Category Image',    'Image associated with a category master',     1, GETDATE(), 1),
    (3, 'SUBCATEGORY', 'Sub Category Image','Image associated with a sub category master', 1, GETDATE(), 1),
    (4, 'ITEM',        'Item Image',        'Image associated with an item master',        1, GETDATE(), 1);
GO