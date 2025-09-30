-- =====================================================
-- GameSpace MiniGame Area 資料庫結構及種子資料
-- 重新組織版本 - 更易於閱讀和理解
-- 創建日期: 2025/09/30 20:50:53
-- =====================================================

USE [GameSpacedatabase]
GO

-- =====================================================
-- 1. 管理者權限系統 (Manager Permission System)
-- =====================================================

-- 1.1 管理者資料表
-- 用途: 儲存管理者的基本資訊和登入相關資料
CREATE TABLE [dbo].[ManagerData](
    [Manager_Id] [int] NOT NULL,
    [Manager_Name] [nvarchar](30) COLLATE Chinese_Taiwan_Stroke_CI_AS NULL,
    [Manager_Account] [varchar](30) COLLATE Chinese_Taiwan_Stroke_CI_AS NULL,
    [Manager_Password] [nvarchar](200) COLLATE Chinese_Taiwan_Stroke_CI_AS NULL,
    [Administrator_registration_date] [datetime2](7) NULL,
    [Manager_Email] [nvarchar](255) COLLATE Chinese_Taiwan_Stroke_CI_AS NOT NULL,
    [Manager_EmailConfirmed] [bit] NOT NULL,
    [Manager_AccessFailedCount] [int] NOT NULL,
    [Manager_LockoutEnabled] [bit] NOT NULL,
    [Manager_LockoutEnd] [datetime2](7) NULL,
    CONSTRAINT [PK_ManagerData] PRIMARY KEY CLUSTERED ([Manager_Id] ASC)
) ON [PRIMARY]
GO

-- 1.2 管理者角色權限表
-- 用途: 定義各種管理角色的權限設定
CREATE TABLE [dbo].[ManagerRolePermission](
    [ManagerRole_Id] [int] NOT NULL,
    [role_name] [nvarchar](50) COLLATE Chinese_Taiwan_Stroke_CI_AS NOT NULL,
    [AdministratorPrivilegesManagement] [bit] NULL,      -- 管理者權限管理
    [UserStatusManagement] [bit] NULL,                   -- 使用者狀態管理
    [ShoppingPermissionManagement] [bit] NULL,            -- 購物權限管理
    [MessagePermissionManagement] [bit] NULL,             -- 訊息權限管理
    [Pet_Rights_Management] [bit] NULL,                    -- 寵物權限管理
    [customer_service] [bit] NULL,                        -- 客服權限
    CONSTRAINT [PK_ManagerRolePermission] PRIMARY KEY CLUSTERED ([ManagerRole_Id] ASC)
) ON [PRIMARY]
GO

-- 1.3 管理者角色關聯表
-- 用途: 關聯管理者和其擁有的角色
CREATE TABLE [dbo].[ManagerRole](
    [Manager_Id] [int] NOT NULL,
    [ManagerRole_Id] [int] NOT NULL,
    CONSTRAINT [PK_ManagerRole] PRIMARY KEY CLUSTERED ([Manager_Id] ASC, [ManagerRole_Id] ASC)
) ON [PRIMARY]
GO

-- 1.4 客服代理表
-- 用途: 管理客服代理的基本資訊
CREATE TABLE [dbo].[CS_Agent](
    [agent_id] [int] IDENTITY(1,1) NOT NULL,
    [manager_id] [int] NOT NULL,
    [is_active] [bit] NOT NULL,
    [max_concurrent] [tinyint] NOT NULL,
    [created_at] [datetime2](7) NOT NULL,
    [created_by_manager] [int] NULL,
    [updated_at] [datetime2](7) NULL,
    [updated_by_manager] [int] NULL,
    CONSTRAINT [PK_CS_Agent] PRIMARY KEY CLUSTERED ([agent_id] ASC)
) ON [PRIMARY]
GO

-- 1.5 客服代理權限表
-- 用途: 定義客服代理的具體權限
CREATE TABLE [dbo].[CS_Agent_Permission](
    [agent_permission_id] [int] IDENTITY(1,1) NOT NULL,
    [agent_id] [int] NOT NULL,
    [can_assign] [bit] NOT NULL,           -- 可分配
    [can_transfer] [bit] NOT NULL,         -- 可轉移
    [can_accept] [bit] NOT NULL,          -- 可接受
    [can_edit_mute_all] [bit] NOT NULL,   -- 可編輯全域靜音
    CONSTRAINT [PK_CS_Agent_Permission] PRIMARY KEY CLUSTERED ([agent_permission_id] ASC)
) ON [PRIMARY]
GO

-- =====================================================
-- 2. 使用者系統 (User System)
-- =====================================================

-- 2.1 使用者基本資料表
-- 用途: 儲存使用者的基本資訊和登入相關資料
CREATE TABLE [dbo].[Users](
    [User_ID] [int] IDENTITY(1,1) NOT NULL,
    [User_name] [nvarchar](30) COLLATE Chinese_Taiwan_Stroke_CI_AS NOT NULL,
    [User_Account] [varchar](30) COLLATE Chinese_Taiwan_Stroke_CI_AS NOT NULL,
    [User_Password] [nvarchar](200) COLLATE Chinese_Taiwan_Stroke_CI_AS NOT NULL,
    [User_EmailConfirmed] [bit] NOT NULL,
    [User_PhoneNumberConfirmed] [bit] NOT NULL,
    [User_TwoFactorEnabled] [bit] NOT NULL,
    [User_AccessFailedCount] [int] NOT NULL,
    [User_LockoutEnabled] [bit] NOT NULL,
    [User_LockoutEnd] [datetime2](7) NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ([User_ID] ASC)
) ON [PRIMARY]
GO

-- 2.2 使用者介紹表
-- 用途: 儲存使用者的個人介紹和詳細資訊
CREATE TABLE [dbo].[User_Introduce](
    [User_ID] [int] NOT NULL,
    [User_Introduce] [nvarchar](500) COLLATE Chinese_Taiwan_Stroke_CI_AS NULL,
    [User_Avatar] [nvarchar](200) COLLATE Chinese_Taiwan_Stroke_CI_AS NULL,
    [User_Birthday] [date] NULL,
    [User_Gender] [nvarchar](10) COLLATE Chinese_Taiwan_Stroke_CI_AS NULL,
    [User_Location] [nvarchar](100) COLLATE Chinese_Taiwan_Stroke_CI_AS NULL,
    CONSTRAINT [PK_User_Introduce] PRIMARY KEY CLUSTERED ([User_ID] ASC)
) ON [PRIMARY]
GO

-- 2.3 使用者權限表
-- 用途: 管理使用者的特殊權限和狀態
CREATE TABLE [dbo].[User_Rights](
    [User_ID] [int] NOT NULL,
    [User_Status] [nvarchar](20) COLLATE Chinese_Taiwan_Stroke_CI_AS NOT NULL,
    [User_VIP_Level] [int] NOT NULL,
    [User_VIP_ExpireDate] [datetime2](7) NULL,
    [User_Ban_Reason] [nvarchar](200) COLLATE Chinese_Taiwan_Stroke_CI_AS NULL,
    [User_Ban_ExpireDate] [datetime2](7) NULL,
    CONSTRAINT [PK_User_Rights] PRIMARY KEY CLUSTERED ([User_ID] ASC)
) ON [PRIMARY]
GO

-- 2.4 使用者銷售資訊表
-- 用途: 記錄使用者的銷售相關資訊
CREATE TABLE [dbo].[User_Sales_Information](
    [User_ID] [int] NOT NULL,
    [User_Total_Purchase] [decimal](18, 2) NOT NULL,
    [User_Total_Spent] [decimal](18, 2) NOT NULL,
    [User_Last_Purchase_Date] [datetime2](7) NULL,
    [User_Favorite_Category] [nvarchar](50) COLLATE Chinese_Taiwan_Stroke_CI_AS NULL,
    CONSTRAINT [PK_User_Sales_Information] PRIMARY KEY CLUSTERED ([User_ID] ASC)
) ON [PRIMARY]
GO

-- =====================================================
-- 3. 錢包系統 (Wallet System)
-- =====================================================

-- 3.1 使用者錢包表
-- 用途: 管理使用者的點數餘額和錢包狀態
CREATE TABLE [dbo].[User_Wallet](
    [User_ID] [int] NOT NULL,
    [Wallet_Balance] [int] NOT NULL,
    [Wallet_Last_Updated] [datetime2](7) NOT NULL,
    [Wallet_Status] [nvarchar](20) COLLATE Chinese_Taiwan_Stroke_CI_AS NOT NULL,
    CONSTRAINT [PK_User_Wallet] PRIMARY KEY CLUSTERED ([User_ID] ASC)
) ON [PRIMARY]
GO

-- 3.2 錢包歷史記錄表
-- 用途: 記錄所有錢包交易歷史
CREATE TABLE [dbo].[WalletHistory](
    [HistoryID] [int] IDENTITY(1,1) NOT NULL,
    [UserID] [int] NOT NULL,
    [TransactionType] [nvarchar](20) COLLATE Chinese_Taiwan_Stroke_CI_AS NOT NULL,
    [Amount] [int] NOT NULL,
    [Balance] [int] NOT NULL,
    [Description] [nvarchar](200) COLLATE Chinese_Taiwan_Stroke_CI_AS NULL,
    [TransactionDate] [datetime2](7) NOT NULL,
    CONSTRAINT [PK_WalletHistory] PRIMARY KEY CLUSTERED ([HistoryID] ASC)
) ON [PRIMARY]
GO

-- =====================================================
-- 4. 優惠券系統 (Coupon System)
-- =====================================================

-- 4.1 優惠券類型表
-- 用途: 定義各種優惠券的類型和規則
CREATE TABLE [dbo].[CouponType](
    [CouponTypeID] [int] IDENTITY(1,1) NOT NULL,
    [Name] [nvarchar](50) COLLATE Chinese_Taiwan_Stroke_CI_AS NOT NULL,
    [DiscountType] [nvarchar](20) COLLATE Chinese_Taiwan_Stroke_CI_AS NOT NULL,
    [DiscountValue] [decimal](18, 2) NOT NULL,
    [MinSpend] [decimal](18, 2) NOT NULL,
    [ValidFrom] [datetime2](7) NOT NULL,
    [ValidTo] [datetime2](7) NOT NULL,
    [PointsCost] [int] NOT NULL,
    [Description] [nvarchar](600) COLLATE Chinese_Taiwan_Stroke_CI_AS NULL,
    CONSTRAINT [PK_CouponType] PRIMARY KEY CLUSTERED ([CouponTypeID] ASC)
) ON [PRIMARY]
GO

-- 4.2 優惠券表
-- 用途: 儲存使用者擁有的優惠券實例
CREATE TABLE [dbo].[Coupon](
    [CouponID] [int] IDENTITY(1,1) NOT NULL,
    [CouponCode] [nvarchar](50) COLLATE Chinese_Taiwan_Stroke_CI_AS NOT NULL,
    [CouponTypeID] [int] NOT NULL,
    [UserID] [int] NOT NULL,
    [IsUsed] [bit] NOT NULL,
    [AcquiredTime] [datetime2](7) NOT NULL,
    [UsedTime] [datetime2](7) NULL,
    [UsedInOrderID] [int] NULL,
    CONSTRAINT [PK_Coupon] PRIMARY KEY CLUSTERED ([CouponID] ASC)
) ON [PRIMARY]
GO

-- =====================================================
-- 5. 電子禮券系統 (E-Voucher System)
-- =====================================================

-- 5.1 電子禮券類型表
-- 用途: 定義各種電子禮券的類型和規則
CREATE TABLE [dbo].[EVoucherType](
    [EVoucherTypeID] [int] IDENTITY(1,1) NOT NULL,
    [Name] [nvarchar](50) COLLATE Chinese_Taiwan_Stroke_CI_AS NOT NULL,
    [Value] [decimal](18, 2) NOT NULL,
    [ValidFrom] [datetime2](7) NOT NULL,
    [ValidTo] [datetime2](7) NOT NULL,
    [PointsCost] [int] NOT NULL,
    [Description] [nvarchar](600) COLLATE Chinese_Taiwan_Stroke_CI_AS NULL,
    CONSTRAINT [PK_EVoucherType] PRIMARY KEY CLUSTERED ([EVoucherTypeID] ASC)
) ON [PRIMARY]
GO

-- 5.2 電子禮券表
-- 用途: 儲存使用者擁有的電子禮券實例
CREATE TABLE [dbo].[EVoucher](
    [EVoucherID] [int] IDENTITY(1,1) NOT NULL,
    [EVoucherCode] [nvarchar](50) COLLATE Chinese_Taiwan_Stroke_CI_AS NOT NULL,
    [EVoucherTypeID] [int] NOT NULL,
    [UserID] [int] NOT NULL,
    [IsUsed] [bit] NOT NULL,
    [AcquiredTime] [datetime2](7) NOT NULL,
    [UsedTime] [datetime2](7) NULL,
    CONSTRAINT [PK_EVoucher] PRIMARY KEY CLUSTERED ([EVoucherID] ASC)
) ON [PRIMARY]
GO

-- 5.3 電子禮券代幣表
-- 用途: 管理電子禮券的 QR Code 代幣
CREATE TABLE [dbo].[EVoucherToken](
    [TokenID] [int] IDENTITY(1,1) NOT NULL,
    [EVoucherID] [int] NOT NULL,
    [Token] [nvarchar](64) COLLATE Chinese_Taiwan_Stroke_CI_AS NOT NULL,
    [ExpiresAt] [datetime2](7) NOT NULL,
    [IsRevoked] [bit] NOT NULL,
    CONSTRAINT [PK_EVoucherToken] PRIMARY KEY CLUSTERED ([TokenID] ASC)
) ON [PRIMARY]
GO

-- 5.4 電子禮券兌換記錄表
-- 用途: 記錄電子禮券的兌換歷史
CREATE TABLE [dbo].[EVoucherRedeemLog](
    [RedeemID] [int] IDENTITY(1,1) NOT NULL,
    [EVoucherID] [int] NOT NULL,
    [TokenID] [int] NULL,
    [UserID] [int] NOT NULL,
    [ScannedAt] [datetime2](7) NOT NULL,
    [Status] [nvarchar](20) COLLATE Chinese_Taiwan_Stroke_CI_AS NOT NULL,
    CONSTRAINT [PK_EVoucherRedeemLog] PRIMARY KEY CLUSTERED ([RedeemID] ASC)
) ON [PRIMARY]
GO

-- =====================================================
-- 6. 簽到系統 (Sign-in System)
-- =====================================================

-- 6.1 使用者簽到統計表
-- 用途: 記錄使用者的簽到統計和獎勵
CREATE TABLE [dbo].[UserSignInStats](
    [UserID] [int] NOT NULL,
    [SignInDate] [date] NOT NULL,
    [SignInTime] [datetime2](7) NOT NULL,
    [RewardType] [nvarchar](20) COLLATE Chinese_Taiwan_Stroke_CI_AS NOT NULL,
    [RewardAmount] [int] NOT NULL,
    [ConsecutiveDays] [int] NOT NULL,
    CONSTRAINT [PK_UserSignInStats] PRIMARY KEY CLUSTERED ([UserID] ASC, [SignInDate] ASC)
) ON [PRIMARY]
GO

-- =====================================================
-- 7. 寵物系統 (Pet System)
-- =====================================================

-- 7.1 寵物表
-- 用途: 儲存使用者的寵物資訊和狀態
CREATE TABLE [dbo].[Pet](
    [PetID] [int] IDENTITY(1,1) NOT NULL,
    [UserID] [int] NOT NULL,
    [PetName] [nvarchar](30) COLLATE Chinese_Taiwan_Stroke_CI_AS NOT NULL,
    [PetLevel] [int] NOT NULL,
    [PetExperience] [int] NOT NULL,
    [PetSkin] [nvarchar](20) COLLATE Chinese_Taiwan_Stroke_CI_AS NOT NULL,
    [PetBackground] [nvarchar](20) COLLATE Chinese_Taiwan_Stroke_CI_AS NOT NULL,
    [PetHunger] [int] NOT NULL,
    [PetHappiness] [int] NOT NULL,
    [PetHealth] [int] NOT NULL,
    [PetEnergy] [int] NOT NULL,
    [PetCleanliness] [int] NOT NULL,
    [LastInteractionTime] [datetime2](7) NOT NULL,
    CONSTRAINT [PK_Pet] PRIMARY KEY CLUSTERED ([PetID] ASC)
) ON [PRIMARY]
GO

-- =====================================================
-- 8. 小遊戲系統 (Mini-Game System)
-- =====================================================

-- 8.1 小遊戲表
-- 用途: 記錄使用者的遊戲記錄和結果
CREATE TABLE [dbo].[MiniGame](
    [GameID] [int] IDENTITY(1,1) NOT NULL,
    [UserID] [int] NOT NULL,
    [GameType] [nvarchar](20) COLLATE Chinese_Taiwan_Stroke_CI_AS NOT NULL,
    [StartTime] [datetime2](7) NOT NULL,
    [EndTime] [datetime2](7) NULL,
    [GameResult] [nvarchar](10) COLLATE Chinese_Taiwan_Stroke_CI_AS NULL,
    [Score] [int] NULL,
    [RewardPoints] [int] NULL,
    [RewardPetExp] [int] NULL,
    [RewardCouponID] [int] NULL,
    CONSTRAINT [PK_MiniGame] PRIMARY KEY CLUSTERED ([GameID] ASC)
) ON [PRIMARY]
GO

-- =====================================================
-- 9. 排行榜系統 (Leaderboard System)
-- =====================================================

-- 9.1 排行榜快照表
-- 用途: 定期記錄排行榜的快照資料
CREATE TABLE [dbo].[leaderboard_snapshots](
    [snapshot_id] [int] IDENTITY(1,1) NOT NULL,
    [snapshot_date] [datetime2](7) NOT NULL,
    [leaderboard_type] [nvarchar](20) COLLATE Chinese_Taiwan_Stroke_CI_AS NOT NULL,
    [user_id] [int] NOT NULL,
    [rank] [int] NOT NULL,
    [score] [int] NOT NULL,
    CONSTRAINT [PK_leaderboard_snapshots] PRIMARY KEY CLUSTERED ([snapshot_id] ASC)
) ON [PRIMARY]
GO

-- =====================================================
-- 10. 使用者代幣表 (User Tokens)
-- =====================================================

-- 10.1 使用者代幣表
-- 用途: 管理使用者的各種代幣和認證
CREATE TABLE [dbo].[UserTokens](
    [TokenID] [int] IDENTITY(1,1) NOT NULL,
    [UserID] [int] NOT NULL,
    [TokenType] [nvarchar](20) COLLATE Chinese_Taiwan_Stroke_CI_AS NOT NULL,
    [Token] [nvarchar](200) COLLATE Chinese_Taiwan_Stroke_CI_AS NOT NULL,
    [ExpiresAt] [datetime2](7) NOT NULL,
    [IsRevoked] [bit] NOT NULL,
    CONSTRAINT [PK_UserTokens] PRIMARY KEY CLUSTERED ([TokenID] ASC)
) ON [PRIMARY]
GO

-- =====================================================
-- 11. 外鍵約束 (Foreign Key Constraints)
-- =====================================================

-- 管理者系統外鍵
ALTER TABLE [dbo].[ManagerRole] 
ADD CONSTRAINT [FK_ManagerRole_ManagerData] 
FOREIGN KEY ([Manager_Id]) REFERENCES [dbo].[ManagerData] ([Manager_Id])
GO

ALTER TABLE [dbo].[ManagerRole] 
ADD CONSTRAINT [FK_ManagerRole_ManagerRolePermission] 
FOREIGN KEY ([ManagerRole_Id]) REFERENCES [dbo].[ManagerRolePermission] ([ManagerRole_Id])
GO

ALTER TABLE [dbo].[CS_Agent] 
ADD CONSTRAINT [FK_CS_Agent_ManagerData] 
FOREIGN KEY ([manager_id]) REFERENCES [dbo].[ManagerData] ([Manager_Id])
GO

ALTER TABLE [dbo].[CS_Agent_Permission] 
ADD CONSTRAINT [FK_CS_Agent_Permission_CS_Agent] 
FOREIGN KEY ([agent_id]) REFERENCES [dbo].[CS_Agent] ([agent_id])
GO

-- 使用者系統外鍵
ALTER TABLE [dbo].[User_Introduce] 
ADD CONSTRAINT [FK_User_Introduce_Users] 
FOREIGN KEY ([User_ID]) REFERENCES [dbo].[Users] ([User_ID])
GO

ALTER TABLE [dbo].[User_Rights] 
ADD CONSTRAINT [FK_User_Rights_Users] 
FOREIGN KEY ([User_ID]) REFERENCES [dbo].[Users] ([User_ID])
GO

ALTER TABLE [dbo].[User_Sales_Information] 
ADD CONSTRAINT [FK_User_Sales_Information_Users] 
FOREIGN KEY ([User_ID]) REFERENCES [dbo].[Users] ([User_ID])
GO

-- 錢包系統外鍵
ALTER TABLE [dbo].[User_Wallet] 
ADD CONSTRAINT [FK_User_Wallet_Users] 
FOREIGN KEY ([User_ID]) REFERENCES [dbo].[Users] ([User_ID])
GO

ALTER TABLE [dbo].[WalletHistory] 
ADD CONSTRAINT [FK_WalletHistory_Users] 
FOREIGN KEY ([UserID]) REFERENCES [dbo].[Users] ([User_ID])
GO

-- 優惠券系統外鍵
ALTER TABLE [dbo].[Coupon] 
ADD CONSTRAINT [FK_Coupon_CouponType] 
FOREIGN KEY ([CouponTypeID]) REFERENCES [dbo].[CouponType] ([CouponTypeID])
GO

ALTER TABLE [dbo].[Coupon] 
ADD CONSTRAINT [FK_Coupon_Users] 
FOREIGN KEY ([UserID]) REFERENCES [dbo].[Users] ([User_ID])
GO

-- 電子禮券系統外鍵
ALTER TABLE [dbo].[EVoucher] 
ADD CONSTRAINT [FK_EVoucher_EVoucherType] 
FOREIGN KEY ([EVoucherTypeID]) REFERENCES [dbo].[EVoucherType] ([EVoucherTypeID])
GO

ALTER TABLE [dbo].[EVoucher] 
ADD CONSTRAINT [FK_EVoucher_Users] 
FOREIGN KEY ([UserID]) REFERENCES [dbo].[Users] ([User_ID])
GO

ALTER TABLE [dbo].[EVoucherToken] 
ADD CONSTRAINT [FK_EVoucherToken_EVoucher] 
FOREIGN KEY ([EVoucherID]) REFERENCES [dbo].[EVoucher] ([EVoucherID])
GO

ALTER TABLE [dbo].[EVoucherRedeemLog] 
ADD CONSTRAINT [FK_EVoucherRedeemLog_EVoucher] 
FOREIGN KEY ([EVoucherID]) REFERENCES [dbo].[EVoucher] ([EVoucherID])
GO

ALTER TABLE [dbo].[EVoucherRedeemLog] 
ADD CONSTRAINT [FK_EVoucherRedeemLog_EVoucherToken] 
FOREIGN KEY ([TokenID]) REFERENCES [dbo].[EVoucherToken] ([TokenID])
GO

ALTER TABLE [dbo].[EVoucherRedeemLog] 
ADD CONSTRAINT [FK_EVoucherRedeemLog_Users] 
FOREIGN KEY ([UserID]) REFERENCES [dbo].[Users] ([User_ID])
GO

-- 簽到系統外鍵
ALTER TABLE [dbo].[UserSignInStats] 
ADD CONSTRAINT [FK_UserSignInStats_Users] 
FOREIGN KEY ([UserID]) REFERENCES [dbo].[Users] ([User_ID])
GO

-- 寵物系統外鍵
ALTER TABLE [dbo].[Pet] 
ADD CONSTRAINT [FK_Pet_Users] 
FOREIGN KEY ([UserID]) REFERENCES [dbo].[Users] ([User_ID])
GO

-- 小遊戲系統外鍵
ALTER TABLE [dbo].[MiniGame] 
ADD CONSTRAINT [FK_MiniGame_Users] 
FOREIGN KEY ([UserID]) REFERENCES [dbo].[Users] ([User_ID])
GO

ALTER TABLE [dbo].[MiniGame] 
ADD CONSTRAINT [FK_MiniGame_Coupon] 
FOREIGN KEY ([RewardCouponID]) REFERENCES [dbo].[Coupon] ([CouponID])
GO

-- 排行榜系統外鍵
ALTER TABLE [dbo].[leaderboard_snapshots] 
ADD CONSTRAINT [FK_leaderboard_snapshots_Users] 
FOREIGN KEY ([user_id]) REFERENCES [dbo].[Users] ([User_ID])
GO

-- 使用者代幣外鍵
ALTER TABLE [dbo].[UserTokens] 
ADD CONSTRAINT [FK_UserTokens_Users] 
FOREIGN KEY ([UserID]) REFERENCES [dbo].[Users] ([User_ID])
GO

-- =====================================================
-- 12. 索引 (Indexes)
-- =====================================================

-- 管理者系統索引
CREATE INDEX [IX_ManagerData_Account] ON [dbo].[ManagerData] ([Manager_Account])
GO

CREATE INDEX [IX_ManagerData_Email] ON [dbo].[ManagerData] ([Manager_Email])
GO

-- 使用者系統索引
CREATE INDEX [IX_Users_Account] ON [dbo].[Users] ([User_Account])
GO

CREATE INDEX [IX_Users_Name] ON [dbo].[Users] ([User_name])
GO

-- 錢包系統索引
CREATE INDEX [IX_WalletHistory_UserID] ON [dbo].[WalletHistory] ([UserID])
GO

CREATE INDEX [IX_WalletHistory_TransactionDate] ON [dbo].[WalletHistory] ([TransactionDate])
GO

-- 優惠券系統索引
CREATE INDEX [IX_Coupon_UserID] ON [dbo].[Coupon] ([UserID])
GO

CREATE INDEX [IX_Coupon_IsUsed] ON [dbo].[Coupon] ([IsUsed])
GO

CREATE INDEX [IX_Coupon_AcquiredTime] ON [dbo].[Coupon] ([AcquiredTime])
GO

-- 電子禮券系統索引
CREATE INDEX [IX_EVoucher_UserID] ON [dbo].[EVoucher] ([UserID])
GO

CREATE INDEX [IX_EVoucher_IsUsed] ON [dbo].[EVoucher] ([IsUsed])
GO

CREATE INDEX [IX_EVoucherToken_Token] ON [dbo].[EVoucherToken] ([Token])
GO

CREATE INDEX [IX_EVoucherToken_ExpiresAt] ON [dbo].[EVoucherToken] ([ExpiresAt])
GO

-- 簽到系統索引
CREATE INDEX [IX_UserSignInStats_UserID] ON [dbo].[UserSignInStats] ([UserID])
GO

CREATE INDEX [IX_UserSignInStats_SignInDate] ON [dbo].[UserSignInStats] ([SignInDate])
GO

-- 寵物系統索引
CREATE INDEX [IX_Pet_UserID] ON [dbo].[Pet] ([UserID])
GO

-- 小遊戲系統索引
CREATE INDEX [IX_MiniGame_UserID] ON [dbo].[MiniGame] ([UserID])
GO

CREATE INDEX [IX_MiniGame_StartTime] ON [dbo].[MiniGame] ([StartTime])
GO

-- 排行榜系統索引
CREATE INDEX [IX_leaderboard_snapshots_Type] ON [dbo].[leaderboard_snapshots] ([leaderboard_type])
GO

CREATE INDEX [IX_leaderboard_snapshots_Date] ON [dbo].[leaderboard_snapshots] ([snapshot_date])
GO

-- =====================================================
-- 13. 視圖 (Views)
-- =====================================================

-- 13.1 客服代理資格視圖
-- 用途: 查詢具有客服權限的管理者
CREATE VIEW [dbo].[vCS_EligibleAgents]
AS
SELECT DISTINCT m.[Manager_Id] AS ManagerId
FROM dbo.ManagerData AS m
JOIN dbo.ManagerRole AS map ON map.[Manager_Id] = m.[Manager_Id]
JOIN dbo.ManagerRolePermission AS perm ON perm.[ManagerRole_Id] = map.[ManagerRole_Id]
WHERE ISNULL(perm.[customer_service], 0) = 1;
GO

-- =====================================================
-- 14. 種子資料 (Seed Data)
-- =====================================================

-- 14.1 管理者角色權限種子資料
INSERT INTO [dbo].[ManagerRolePermission] 
([ManagerRole_Id], [role_name], [AdministratorPrivilegesManagement], [UserStatusManagement], [ShoppingPermissionManagement], [MessagePermissionManagement], [Pet_Rights_Management], [customer_service])
VALUES
(1, N'超級管理員', 1, 1, 1, 1, 1, 1),
(2, N'一般管理員', 0, 1, 1, 1, 1, 0),
(3, N'客服管理員', 0, 0, 0, 1, 0, 1),
(4, N'寵物管理員', 0, 0, 0, 0, 1, 0),
(5, N'購物管理員', 0, 0, 1, 0, 0, 0);
GO

-- 14.2 優惠券類型種子資料
INSERT INTO [dbo].[CouponType] 
([Name], [DiscountType], [DiscountValue], [MinSpend], [ValidFrom], [ValidTo], [PointsCost], [Description])
VALUES
(N'新用戶優惠券', N'百分比', 10.00, 100.00, '2024-01-01', '2024-12-31', 0, N'新用戶專享10%折扣'),
(N'滿額優惠券', N'固定金額', 50.00, 500.00, '2024-01-01', '2024-12-31', 100, N'滿500元減50元'),
(N'節日優惠券', N'百分比', 20.00, 200.00, '2024-01-01', '2024-12-31', 200, N'節日特惠20%折扣');
GO

-- 14.3 電子禮券類型種子資料
INSERT INTO [dbo].[EVoucherType] 
([Name], [Value], [ValidFrom], [ValidTo], [PointsCost], [Description])
VALUES
(N'咖啡券', 50.00, '2024-01-01', '2024-12-31', 100, N'星巴克咖啡券50元'),
(N'電影券', 200.00, '2024-01-01', '2024-12-31', 400, N'電影票券200元'),
(N'餐飲券', 100.00, '2024-01-01', '2024-12-31', 200, N'餐飲券100元');
GO

-- =====================================================
-- 15. 預存程序 (Stored Procedures)
-- =====================================================

-- 15.1 使用者簽到預存程序
-- 用途: 處理使用者簽到邏輯和獎勵發放
CREATE PROCEDURE [dbo].[sp_UserSignIn]
    @UserID INT,
    @SignInDate DATE,
    @RewardType NVARCHAR(20),
    @RewardAmount INT,
    @ConsecutiveDays INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRANSACTION;
    
    BEGIN TRY
        -- 插入簽到記錄
        INSERT INTO [dbo].[UserSignInStats] 
        ([UserID], [SignInDate], [SignInTime], [RewardType], [RewardAmount], [ConsecutiveDays])
        VALUES 
        (@UserID, @SignInDate, GETDATE(), @RewardType, @RewardAmount, @ConsecutiveDays);
        
        -- 如果是點數獎勵，更新錢包
        IF @RewardType = 'Points'
        BEGIN
            UPDATE [dbo].[User_Wallet] 
            SET [Wallet_Balance] = [Wallet_Balance] + @RewardAmount,
                [Wallet_Last_Updated] = GETDATE()
            WHERE [User_ID] = @UserID;
            
            -- 記錄錢包歷史
            INSERT INTO [dbo].[WalletHistory] 
            ([UserID], [TransactionType], [Amount], [Balance], [Description], [TransactionDate])
            SELECT @UserID, 'SignIn', @RewardAmount, [Wallet_Balance], '簽到獎勵', GETDATE()
            FROM [dbo].[User_Wallet] WHERE [User_ID] = @UserID;
        END
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- 15.2 寵物互動預存程序
-- 用途: 處理寵物互動邏輯和狀態更新
CREATE PROCEDURE [dbo].[sp_PetInteraction]
    @PetID INT,
    @InteractionType NVARCHAR(20),
    @HungerChange INT,
    @HappinessChange INT,
    @HealthChange INT,
    @EnergyChange INT,
    @CleanlinessChange INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRANSACTION;
    
    BEGIN TRY
        -- 更新寵物狀態
        UPDATE [dbo].[Pet] 
        SET [PetHunger] = CASE WHEN [PetHunger] + @HungerChange < 0 THEN 0 
                              WHEN [PetHunger] + @HungerChange > 100 THEN 100 
                              ELSE [PetHunger] + @HungerChange END,
            [PetHappiness] = CASE WHEN [PetHappiness] + @HappinessChange < 0 THEN 0 
                                 WHEN [PetHappiness] + @HappinessChange > 100 THEN 100 
                                 ELSE [PetHappiness] + @HappinessChange END,
            [PetHealth] = CASE WHEN [PetHealth] + @HealthChange < 0 THEN 0 
                              WHEN [PetHealth] + @HealthChange > 100 THEN 100 
                              ELSE [PetHealth] + @HealthChange END,
            [PetEnergy] = CASE WHEN [PetEnergy] + @EnergyChange < 0 THEN 0 
                              WHEN [PetEnergy] + @EnergyChange > 100 THEN 100 
                              ELSE [PetEnergy] + @EnergyChange END,
            [PetCleanliness] = CASE WHEN [PetCleanliness] + @CleanlinessChange < 0 THEN 0 
                                   WHEN [PetCleanliness] + @CleanlinessChange > 100 THEN 100 
                                   ELSE [PetCleanliness] + @CleanlinessChange END,
            [LastInteractionTime] = GETDATE()
        WHERE [PetID] = @PetID;
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- =====================================================
-- 16. 觸發器 (Triggers)
-- =====================================================

-- 16.1 錢包餘額觸發器
-- 用途: 確保錢包餘額不會變成負數
CREATE TRIGGER [tr_User_Wallet_Balance_Check]
ON [dbo].[User_Wallet]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    IF EXISTS (SELECT 1 FROM inserted WHERE [Wallet_Balance] < 0)
    BEGIN
        RAISERROR('錢包餘額不能為負數', 16, 1);
        ROLLBACK TRANSACTION;
    END
END
GO

-- 16.2 寵物狀態觸發器
-- 用途: 確保寵物狀態值在合理範圍內
CREATE TRIGGER [tr_Pet_Status_Check]
ON [dbo].[Pet]
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    IF EXISTS (SELECT 1 FROM inserted 
               WHERE [PetHunger] < 0 OR [PetHunger] > 100
                  OR [PetHappiness] < 0 OR [PetHappiness] > 100
                  OR [PetHealth] < 0 OR [PetHealth] > 100
                  OR [PetEnergy] < 0 OR [PetEnergy] > 100
                  OR [PetCleanliness] < 0 OR [PetCleanliness] > 100)
    BEGIN
        RAISERROR('寵物狀態值必須在0-100之間', 16, 1);
        ROLLBACK TRANSACTION;
    END
END
GO

-- =====================================================
-- 17. 函數 (Functions)
-- =====================================================

-- 17.1 計算使用者連續簽到天數
-- 用途: 計算使用者的連續簽到天數
CREATE FUNCTION [dbo].[fn_GetConsecutiveSignInDays](@UserID INT)
RETURNS INT
AS
BEGIN
    DECLARE @ConsecutiveDays INT = 0;
    DECLARE @CurrentDate DATE = CAST(GETDATE() AS DATE);
    
    WHILE EXISTS (SELECT 1 FROM [dbo].[UserSignInStats] 
                  WHERE [UserID] = @UserID 
                    AND [SignInDate] = @CurrentDate)
    BEGIN
        SET @ConsecutiveDays = @ConsecutiveDays + 1;
        SET @CurrentDate = DATEADD(DAY, -1, @CurrentDate);
    END
    
    RETURN @ConsecutiveDays;
END
GO

-- 17.2 計算寵物等級
-- 用途: 根據經驗值計算寵物等級
CREATE FUNCTION [dbo].[fn_CalculatePetLevel](@Experience INT)
RETURNS INT
AS
BEGIN
    DECLARE @Level INT = 1;
    DECLARE @RequiredExp INT = 100;
    
    WHILE @Experience >= @RequiredExp
    BEGIN
        SET @Level = @Level + 1;
        SET @RequiredExp = @RequiredExp + (@Level * 50);
    END
    
    RETURN @Level;
END
GO

-- =====================================================
-- 18. 資料庫完整性檢查
-- =====================================================

-- 18.1 檢查所有外鍵約束
SELECT 
    'Foreign Key Check' AS CheckType,
    COUNT(*) AS TotalConstraints
FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS;
GO

-- 18.2 檢查所有索引
SELECT 
    'Index Check' AS CheckType,
    COUNT(*) AS TotalIndexes
FROM sys.indexes 
WHERE is_primary_key = 0 AND is_unique_constraint = 0;
GO

-- 18.3 檢查所有表
SELECT 
    'Table Check' AS CheckType,
    COUNT(*) AS TotalTables
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE';
GO

-- =====================================================
-- 完成
-- =====================================================

PRINT 'GameSpace MiniGame Area 資料庫結構及種子資料建立完成！';
PRINT '總共建立了 ' + CAST((SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE') AS NVARCHAR(10)) + ' 個資料表';
PRINT '總共建立了 ' + CAST((SELECT COUNT(*) FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS) AS NVARCHAR(10)) + ' 個外鍵約束';
PRINT '總共建立了 ' + CAST((SELECT COUNT(*) FROM sys.indexes WHERE is_primary_key = 0 AND is_unique_constraint = 0) AS NVARCHAR(10)) + ' 個索引';
PRINT '總共建立了 ' + CAST((SELECT COUNT(*) FROM INFORMATION_SCHEMA.VIEWS) AS NVARCHAR(10)) + ' 個視圖';
PRINT '總共建立了 ' + CAST((SELECT COUNT(*) FROM sys.procedures) AS NVARCHAR(10)) + ' 個預存程序';
PRINT '總共建立了 ' + CAST((SELECT COUNT(*) FROM sys.triggers) AS NVARCHAR(10)) + ' 個觸發器';
PRINT '總共建立了 ' + CAST((SELECT COUNT(*) FROM sys.objects WHERE type = 'FN') AS NVARCHAR(10)) + ' 個函數';
GO
