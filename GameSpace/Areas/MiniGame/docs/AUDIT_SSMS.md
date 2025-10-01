# SSMS 資料庫結構審計報告

## 資料庫連線資訊
- 伺服器：DESKTOP-8HQIS1S\SQLEXPRESS
- 資料庫：GameSpacedatabase
- 總表數：82 個

## 關鍵資料表結構

### 1. User_Wallet (會員錢包)
- User_Id: int, NOT NULL
- User_Point: int, NOT NULL, DEFAULT 0
- 種子資料：5 筆測試資料

### 2. Pet (寵物系統)
- PetID: int, NOT NULL, PK
- UserID: int, NOT NULL
- PetName: nvarchar(50), NOT NULL
- Level: int, NOT NULL
- Experience: int, NOT NULL
- 五維屬性：Hunger, Mood, Stamina, Cleanliness, Health
- 外觀設定：SkinColor, BackgroundColor
- 時間記錄：LevelUpTime, SkinColorChangedTime, BackgroundColorChangedTime
- 點數記錄：PointsChanged_SkinColor, PointsChanged_BackgroundColor, PointsGained_LevelUp

### 3. ManagerData (管理員資料)
- Manager_Id: int, NOT NULL, PK
- Manager_Name: nvarchar(30)
- Manager_Account: varchar(30)
- Manager_Password: nvarchar(200)
- Manager_Email: nvarchar(255), NOT NULL
- 其他認證相關欄位
