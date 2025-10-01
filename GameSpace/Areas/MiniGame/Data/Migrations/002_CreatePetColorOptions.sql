-- 建立寵物顏色選項表
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='PetColorOptions' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[PetColorOptions] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [ColorName] nvarchar(50) NOT NULL,
        [ColorCode] nvarchar(7) NOT NULL,
        [IsActive] bit NOT NULL DEFAULT 1,
        [CreatedAt] datetime2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
        [UpdatedAt] datetime2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT [PK_PetColorOptions] PRIMARY KEY ([Id])
    );
    
    CREATE INDEX [IX_PetColorOptions_ColorName] ON [dbo].[PetColorOptions] ([ColorName]);
    CREATE INDEX [IX_PetColorOptions_IsActive] ON [dbo].[PetColorOptions] ([IsActive]);
END

-- 插入預設的寵物顏色選項
IF NOT EXISTS (SELECT * FROM [dbo].[PetColorOptions] WHERE [ColorName] = '紅色')
BEGIN
    INSERT INTO [dbo].[PetColorOptions] ([ColorName], [ColorCode], [IsActive])
    VALUES 
    ('紅色', '#FF0000', 1),
    ('藍色', '#0000FF', 1),
    ('綠色', '#00FF00', 1),
    ('黃色', '#FFFF00', 1),
    ('紫色', '#800080', 1),
    ('橙色', '#FFA500', 1),
    ('粉色', '#FFC0CB', 1),
    ('黑色', '#000000', 1),
    ('白色', '#FFFFFF', 1),
    ('棕色', '#A52A2A', 1);
END
