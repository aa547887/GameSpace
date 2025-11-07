-- ==================================================================================
-- 寵物升級和經驗值記錄查詢 - SQL查詢文件
-- ==================================================================================
-- 執行時間: 2025-11-07
-- 數據庫: GameSpacedatabase
-- 服務器: (local)\SQLEXPRESS

-- ==================================================================================
-- 查詢1: 獲取 Pet 表結構信息
-- ==================================================================================
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Pet'
ORDER BY ORDINAL_POSITION;

-- 結果: 26 個欄位 (已驗證)

-- ==================================================================================
-- 查詢2: 統計所有寵物記錄
-- ==================================================================================
SELECT COUNT(*) as TotalRecords
FROM Pet;

-- 結果: 200 筆記錄

-- ==================================================================================
-- 查詢3: 獲取最新更新的 10 筆寵物記錄（所有欄位）
-- ==================================================================================
SELECT TOP 10
    PetID,
    UserID,
    PetName,
    Level,
    LevelUpTime,
    Experience,
    Hunger,
    Mood,
    Stamina,
    Cleanliness,
    Health,
    SkinColor,
    SkinColorChangedTime,
    BackgroundColor,
    BackgroundColorChangedTime,
    PointsChanged_SkinColor,
    PointsChanged_BackgroundColor,
    PointsGained_LevelUp,
    PointsGainedTime_LevelUp,
    IsDeleted,
    DeletedAt,
    DeletedBy,
    DeleteReason,
    CurrentExperience,
    ExperienceToNextLevel,
    TotalPointsGained_LevelUp
FROM Pet
ORDER BY LevelUpTime DESC;

-- 結果: 10 筆記錄，按最後升級時間降序排列
-- 最新: PetID 1 (2025-11-07 11:46:44.4287362)

-- ==================================================================================
-- 查詢4: 獲取等級最高的 10 筆寵物記錄
-- ==================================================================================
SELECT TOP 10
    PetID,
    UserID,
    PetName,
    Level,
    Experience,
    LevelUpTime,
    CurrentExperience,
    ExperienceToNextLevel,
    Hunger,
    Mood,
    Stamina,
    Cleanliness,
    Health,
    SkinColor,
    BackgroundColor,
    PointsGained_LevelUp,
    TotalPointsGained_LevelUp
FROM Pet
ORDER BY Level DESC, Experience DESC;

-- 結果: 10 筆記錄，等級為 50 和 48
-- 最高等級寵物: PetID 109, 66, 184, 102, 73, 121, 10, 40 (所有為Level 50)

-- ==================================================================================
-- 查詢5: 統計信息 - 等級和經驗值統計
-- ==================================================================================
SELECT
    COUNT(*) as TotalPets,
    COUNT(DISTINCT UserID) as TotalUsers,
    AVG(CAST(Level as FLOAT)) as AvgLevel,
    MAX(Level) as MaxLevel,
    MIN(Level) as MinLevel,
    AVG(CAST(Experience as FLOAT)) as AvgExperience,
    MAX(Experience) as MaxExperience,
    MIN(Experience) as MinExperience,
    SUM(CASE WHEN IsDeleted = 1 THEN 1 ELSE 0 END) as DeletedPets
FROM Pet;

-- 結果:
-- TotalPets: 200
-- TotalUsers: 200
-- AvgLevel: 25.75
-- MaxLevel: 50
-- MinLevel: 1
-- AvgExperience: 3155.855
-- MaxExperience: 9134
-- MinExperience: 122
-- DeletedPets: 0

-- ==================================================================================
-- 查詢6: 獲取最新的 5 筆寵物記錄（簡化版）
-- ==================================================================================
SELECT TOP 5
    PetID,
    UserID,
    PetName,
    Level,
    Experience,
    CurrentExperience,
    ExperienceToNextLevel,
    LevelUpTime,
    IsDeleted
FROM Pet
ORDER BY LevelUpTime DESC;

-- 結果: 最新5筆記錄詳情

-- ==================================================================================
-- 查詢7: 按等級分佈統計
-- ==================================================================================
SELECT
    Level,
    COUNT(*) as PetCount,
    CAST(CAST(COUNT(*) as FLOAT) / 200 * 100 as DECIMAL(5,2)) as Percentage,
    MIN(Experience) as MinExp,
    MAX(Experience) as MaxExp,
    AVG(CAST(Experience as FLOAT)) as AvgExp
FROM Pet
GROUP BY Level
ORDER BY Level DESC;

-- 結果: 顯示各等級寵物分佈情況

-- ==================================================================================
-- 查詢8: 皮膚顏色和背景選項統計
-- ==================================================================================
SELECT
    COUNT(DISTINCT SkinColor) as UniqueSkinColors,
    COUNT(DISTINCT BackgroundColor) as UniqueBackgrounds
FROM Pet;

-- 查詢各皮膚顏色的使用次數
SELECT
    SkinColor,
    COUNT(*) as UsageCount,
    CAST(CAST(COUNT(*) as FLOAT) / 200 * 100 as DECIMAL(5,2)) as Percentage
FROM Pet
GROUP BY SkinColor
ORDER BY UsageCount DESC;

-- 查詢各背景的使用次數
SELECT
    BackgroundColor,
    COUNT(*) as UsageCount,
    CAST(CAST(COUNT(*) as FLOAT) / 200 * 100 as DECIMAL(5,2)) as Percentage
FROM Pet
GROUP BY BackgroundColor
ORDER BY UsageCount DESC;

-- ==================================================================================
-- 查詢9: 寵物狀態條件分析
-- ==================================================================================
SELECT
    'All Pets' as Category,
    COUNT(*) as TotalPets,

    SUM(CASE WHEN Hunger >= 80 THEN 1 ELSE 0 END) as WellFedPets,
    SUM(CASE WHEN Hunger < 30 THEN 1 ELSE 0 END) as HungryPets,

    SUM(CASE WHEN Mood >= 80 THEN 1 ELSE 0 END) as HappyPets,
    SUM(CASE WHEN Mood < 30 THEN 1 ELSE 0 END) as SadPets,

    SUM(CASE WHEN Health >= 80 THEN 1 ELSE 0 END) as HealthyPets,
    SUM(CASE WHEN Health < 30 THEN 1 ELSE 0 END) as SickPets,

    SUM(CASE WHEN Cleanliness >= 80 THEN 1 ELSE 0 END) as CleanPets,
    SUM(CASE WHEN Stamina >= 80 THEN 1 ELSE 0 END) as EnergizedPets
FROM Pet;

-- ==================================================================================
-- 查詢10: 近期活動記錄（最近30天）
-- ==================================================================================
SELECT
    PetID,
    UserID,
    PetName,
    Level,
    LevelUpTime,
    SkinColorChangedTime,
    BackgroundColorChangedTime,
    DATEDIFF(day, LevelUpTime, GETDATE()) as DaysSinceLastLevelUp,
    DATEDIFF(day, SkinColorChangedTime, GETDATE()) as DaysSinceSkinChange,
    DATEDIFF(day, BackgroundColorChangedTime, GETDATE()) as DaysSinceBackgroundChange
FROM Pet
WHERE
    LevelUpTime >= DATEADD(day, -30, GETDATE())
    OR SkinColorChangedTime >= DATEADD(day, -30, GETDATE())
    OR BackgroundColorChangedTime >= DATEADD(day, -30, GETDATE())
ORDER BY LevelUpTime DESC;

-- ==================================================================================
-- 查詢11: 導出完整寵物數據（CSV格式準備）
-- ==================================================================================
SELECT * FROM Pet
ORDER BY PetID;

-- ==================================================================================
-- 查詢12: 特定寵物詳細信息範例
-- ==================================================================================
SELECT * FROM Pet
WHERE PetID = 1;  -- 查詢寵物ID=1的完整信息

-- ==================================================================================
-- 備註
-- ==================================================================================
-- 所有查詢執行時間: 2025-11-07
-- 執行用戶: n2029 (Windows認證)
-- 查詢結果: 均為只讀查詢，無修改操作
-- 數據庫狀態: 正常，無異常
--
-- 主要發現:
-- 1. 共200隻寵物，100%存活（無刪除記錄）
-- 2. 等級分布相對均勻，最高等級為50
-- 3. 最新活動時間: 2025-11-07 11:46:44 (PetID 1)
-- 4. 寵物狀態值波動正常，無明顯異常
-- 5. 外觀系統使用活躍，色彩和背景選項多樣化
