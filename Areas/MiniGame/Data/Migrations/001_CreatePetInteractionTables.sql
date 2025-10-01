-- 建立寵物互動狀態增益規則表
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='PetInteractionBonusRules' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[PetInteractionBonusRules] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [InteractionType] nvarchar(50) NOT NULL,
        [InteractionName] nvarchar(100) NOT NULL,
        [PointsCost] int NOT NULL,
        [HappinessGain] int NOT NULL,
        [ExpGain] int NOT NULL,
        [CooldownMinutes] int NOT NULL,
        [IsActive] bit NOT NULL DEFAULT 1,
        [Description] nvarchar(500) NULL,
        [CreatedAt] datetime2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
        [UpdatedAt] datetime2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT [PK_PetInteractionBonusRules] PRIMARY KEY ([Id])
    );
    
    CREATE INDEX [IX_PetInteractionBonusRules_InteractionType] ON [dbo].[PetInteractionBonusRules] ([InteractionType]);
    CREATE INDEX [IX_PetInteractionBonusRules_IsActive] ON [dbo].[PetInteractionBonusRules] ([IsActive]);
END

-- 建立寵物互動歷史表
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='PetInteractionHistories' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[PetInteractionHistories] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [PetID] int NOT NULL,
        [UserID] int NOT NULL,
        [InteractionType] nvarchar(50) NOT NULL,
        [InteractionName] nvarchar(100) NOT NULL,
        [PointsCost] int NOT NULL,
        [ExpGained] int NOT NULL,
        [HappinessGained] int NOT NULL,
        [InteractionTime] datetime2(7) NOT NULL,
        [CreatedAt] datetime2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
        [Description] nvarchar(500) NULL,
        CONSTRAINT [PK_PetInteractionHistories] PRIMARY KEY ([Id])
    );
    
    CREATE INDEX [IX_PetInteractionHistories_PetID] ON [dbo].[PetInteractionHistories] ([PetID]);
    CREATE INDEX [IX_PetInteractionHistories_UserID] ON [dbo].[PetInteractionHistories] ([UserID]);
    CREATE INDEX [IX_PetInteractionHistories_InteractionType] ON [dbo].[PetInteractionHistories] ([InteractionType]);
    CREATE INDEX [IX_PetInteractionHistories_InteractionTime] ON [dbo].[PetInteractionHistories] ([InteractionTime]);
END

-- 插入預設的互動規則
IF NOT EXISTS (SELECT * FROM [dbo].[PetInteractionBonusRules] WHERE [InteractionType] = 'feed')
BEGIN
    INSERT INTO [dbo].[PetInteractionBonusRules] 
    ([InteractionType], [InteractionName], [PointsCost], [HappinessGain], [ExpGain], [CooldownMinutes], [IsActive], [Description])
    VALUES 
    ('feed', '餵食', 10, 15, 20, 30, 1, '餵食寵物，增加快樂度和經驗值'),
    ('play', '玩耍', 15, 20, 25, 60, 1, '與寵物玩耍，大幅增加快樂度'),
    ('groom', '梳毛', 5, 10, 15, 15, 1, '為寵物梳毛，增加整潔度'),
    ('train', '訓練', 25, 5, 50, 120, 1, '訓練寵物，大幅增加經驗值'),
    ('heal', '治療', 20, 30, 10, 180, 1, '治療寵物，恢復健康狀態');
END
