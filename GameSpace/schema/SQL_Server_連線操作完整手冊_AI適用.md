# SQL Server 連線與資料讀取完整操作手冊（AI 適用版）

> 本手冊專為 AI 助手設計，提供從零開始連接 SQL Server 並讀取資料的完整步驟。
> 
> **目標**: 讓任何 AI 都能按照本手冊快速上手 GameSpace 資料庫操作

**最後更新**: 2025-10-03  
**驗證狀態**: ✅ 已在實際環境中成功驗證

---

## 📚 目錄

1. [環境資訊](#環境資訊)
2. [連線前準備](#連線前準備)
3. [基本連線命令](#基本連線命令)
4. [實戰讀取步驟](#實戰讀取步驟)
5. [所有核心表的讀取命令](#所有核心表的讀取命令)
6. [常見錯誤與解決方案](#常見錯誤與解決方案)
7. [資料驗證清單](#資料驗證清單)
8. [最佳實踐](#最佳實踐)

---

## 環境資訊

### 系統環境
```
作業系統: Windows 11 家用版 (Build 26100)
Shell: PowerShell
資料庫引擎: SQL Server Express
工具: sqlcmd (SQL Server 命令列工具)
認證方式: Windows 整合認證
```

### 資料庫連接資訊
```
伺服器名稱: DESKTOP-8HQIS1S\SQLEXPRESS
資料庫名稱: GameSpacedatabase
連接字串名稱: GameSpace
認證: Integrated Security=True (Windows 認證)
```

### 工作目錄
```powershell
C:\Users\n2029\Desktop\GameSpace
```

---

## 連線前準備

### Step 1: 確認 sqlcmd 可用

**執行命令**:
```powershell
sqlcmd -?
```

**預期結果**: 顯示 sqlcmd 的幫助訊息

**如果失敗**: 需要安裝 SQL Server 命令列工具
```powershell
# 檢查是否已安裝
Get-Command sqlcmd
```

---

### Step 2: 確認資料庫連接資訊

**需要確認的資訊**:
1. ✅ 伺服器名稱（Server Name）
2. ✅ 資料庫名稱（Database Name）
3. ✅ 認證方式（Windows 認證 or SQL 認證）

**從專案中查找連接資訊**:

```powershell
# 讀取 appsettings.json 以確認連接字串
Get-Content GameSpace/GameSpace/appsettings.json | Select-String -Pattern "ConnectionStrings" -Context 0,5
```

**預期輸出**:
```json
"ConnectionStrings": {
  "GameSpace": "Server=DESKTOP-8HQIS1S\\SQLEXPRESS;Database=GameSpacedatabase;Integrated Security=True;"
}
```

---

## 基本連線命令

### 命令格式

```powershell
sqlcmd -S "伺服器名稱" -d "資料庫名稱" -E -Q "SQL查詢語句"
```

### 參數說明

| 參數 | 說明 | 範例 |
|-----|------|------|
| `-S` | 伺服器名稱 | `"DESKTOP-8HQIS1S\SQLEXPRESS"` |
| `-d` | 資料庫名稱 | `"GameSpacedatabase"` |
| `-E` | 使用 Windows 整合認證 | 無需額外參數 |
| `-Q` | 執行 SQL 查詢後退出 | `"SELECT * FROM Users"` |
| `-W` | 移除尾隨空白（可選） | 無需額外參數 |

### 重要注意事項

⚠️ **反斜線轉義**: 伺服器名稱中的反斜線要使用雙反斜線 `\\` 或用引號包起來

✅ **正確寫法**:
```powershell
-S "DESKTOP-8HQIS1S\SQLEXPRESS"
```

❌ **錯誤寫法**:
```powershell
-S DESKTOP-8HQIS1S\SQLEXPRESS  # 沒有引號會出錯
```

---

## 實戰讀取步驟

### 🎯 Phase 1: 測試基本連線

#### Step 1.1: 列出所有資料表

**目的**: 確認可以連接到資料庫並查看資料庫結構

**命令**:
```powershell
sqlcmd -S "DESKTOP-8HQIS1S\SQLEXPRESS" -d "GameSpacedatabase" -E -Q "SELECT name FROM sys.tables ORDER BY name" -W
```

**預期結果**: 顯示所有資料表名稱（應該有 82 個表）

**關鍵表名**:
```
ManagerData
ManagerRolePermission
Users
Pet
User_Wallet
WalletHistory
UserSignInStats
MiniGame
Coupon
CouponType
EVoucher
EVoucherType
```

**如果成功**: 繼續下一步  
**如果失敗**: 參考「常見錯誤與解決方案」章節

---

#### Step 1.2: 統計記錄總數

**目的**: 快速確認每個核心表有資料

**命令模板**:
```powershell
sqlcmd -S "DESKTOP-8HQIS1S\SQLEXPRESS" -d "GameSpacedatabase" -E -Q "SELECT COUNT(*) AS TotalCount FROM [表名]"
```

**實際執行命令** (依序執行):

```powershell
# 1. 統計管理員數量
sqlcmd -S "DESKTOP-8HQIS1S\SQLEXPRESS" -d "GameSpacedatabase" -E -Q "SELECT COUNT(*) AS TotalCount FROM ManagerData"

# 2. 統計使用者數量
sqlcmd -S "DESKTOP-8HQIS1S\SQLEXPRESS" -d "GameSpacedatabase" -E -Q "SELECT COUNT(*) AS TotalUsers FROM Users"

# 3. 統計寵物數量
sqlcmd -S "DESKTOP-8HQIS1S\SQLEXPRESS" -d "GameSpacedatabase" -E -Q "SELECT COUNT(*) AS TotalPets FROM Pet"

# 4. 統計簽到記錄
sqlcmd -S "DESKTOP-8HQIS1S\SQLEXPRESS" -d "GameSpacedatabase" -E -Q "SELECT COUNT(*) AS TotalSignIns FROM UserSignInStats"

# 5. 統計優惠券
sqlcmd -S "DESKTOP-8HQIS1S\SQLEXPRESS" -d "GameSpacedatabase" -E -Q "SELECT COUNT(*) AS TotalCoupons FROM Coupon"

# 6. 統計電子禮券
sqlcmd -S "DESKTOP-8HQIS1S\SQLEXPRESS" -d "GameSpacedatabase" -E -Q "SELECT COUNT(*) AS TotalEVouchers FROM EVoucher"
```

**預期結果**:
```
TotalCount: 102  (ManagerData)
TotalUsers: 200  (Users)
TotalPets: 200   (Pet)
TotalSignIns: 2400 (UserSignInStats)
TotalCoupons: 698  (Coupon)
TotalEVouchers: 355 (EVoucher)
```

---

### 🎯 Phase 2: 讀取核心業務資料

#### Step 2.1: 讀取管理員資料

**目的**: 獲取測試帳號和權限資訊

**命令 1: 讀取前 3 個管理員**
```powershell
sqlcmd -S "DESKTOP-8HQIS1S\SQLEXPRESS" -d "GameSpacedatabase" -E -Q "SELECT TOP 3 * FROM ManagerData"
```

**預期輸出**: 包含 Manager_Id, Manager_Name, Manager_Account, Manager_Password 等欄位

**測試帳號**:
- `zhang_zhiming_01` / `AdminPass001@` (最高權限)
- `li_xiaohua_02` / `SecurePass002#` (使用者管理)
- `wang_meiling_03` / `StrongPwd003!` (商城寵物管理)

**命令 2: 讀取所有角色權限**
```powershell
sqlcmd -S "DESKTOP-8HQIS1S\SQLEXPRESS" -d "GameSpacedatabase" -E -Q "SELECT * FROM ManagerRolePermission"
```

**預期輸出**: 8 個角色，包含各種權限布林值

---

#### Step 2.2: 讀取使用者資料

**命令**:
```powershell
sqlcmd -S "DESKTOP-8HQIS1S\SQLEXPRESS" -d "GameSpacedatabase" -E -Q "SELECT TOP 5 User_ID, User_name, User_Account, User_Password, User_EmailConfirmed FROM Users"
```

**預期輸出**: 200 個使用者中的前 5 筆

---

#### Step 2.3: 讀取寵物資料（五維屬性）

**命令**:
```powershell
sqlcmd -S "DESKTOP-8HQIS1S\SQLEXPRESS" -d "GameSpacedatabase" -E -Q "SELECT TOP 5 PetID, UserID, PetName, Level, Experience, Health, Hunger, Mood, Stamina, Cleanliness FROM Pet"
```

**重要**: 實際欄位是 `Stamina` (體力)，不是 `Loyalty` (忠誠度)

**五維屬性**:
1. Health (健康值)
2. Hunger (飽食度)
3. Mood (心情值)
4. Stamina (體力值) ← 注意這個
5. Cleanliness (清潔度)

---

#### Step 2.4: 讀取錢包資料

**命令 1: 讀取錢包餘額**
```powershell
sqlcmd -S "DESKTOP-8HQIS1S\SQLEXPRESS" -d "GameSpacedatabase" -E -Q "SELECT TOP 5 * FROM User_Wallet"
```

**命令 2: 讀取錢包交易歷史**
```powershell
sqlcmd -S "DESKTOP-8HQIS1S\SQLEXPRESS" -d "GameSpacedatabase" -E -Q "SELECT TOP 10 LogID, UserID, ChangeType, PointsChanged, ItemCode, Description, ChangeTime FROM WalletHistory ORDER BY ChangeTime DESC"
```

**ChangeType 類型**:
- `Point`: 點數變更
- `Coupon`: 優惠券變更
- `EVoucher`: 電子禮券變更

---

#### Step 2.5: 讀取簽到記錄

**命令**:
```powershell
sqlcmd -S "DESKTOP-8HQIS1S\SQLEXPRESS" -d "GameSpacedatabase" -E -Q "SELECT TOP 10 LogID, SignTime, UserID, PointsGained, ExpGained, CouponGained FROM UserSignInStats ORDER BY SignTime DESC"
```

---

#### Step 2.6: 讀取小遊戲記錄

**命令**:
```powershell
sqlcmd -S "DESKTOP-8HQIS1S\SQLEXPRESS" -d "GameSpacedatabase" -E -Q "SELECT TOP 5 PlayID, UserID, PetID, Level, Result, ExpGained, PointsGained, StartTime, EndTime FROM MiniGame"
```

**Result 類型**:
- `Win`: 勝利
- `Lose`: 失敗
- `Aborted`: 中途放棄

---

#### Step 2.7: 讀取優惠券類型

**命令**:
```powershell
sqlcmd -S "DESKTOP-8HQIS1S\SQLEXPRESS" -d "GameSpacedatabase" -E -Q "SELECT TOP 5 CouponTypeID, Name, DiscountType, DiscountValue, MinSpend, PointsCost FROM CouponType"
```

**DiscountType**:
- `Amount`: 固定金額折扣
- `Percent`: 百分比折扣

---

#### Step 2.8: 讀取電子禮券類型

**命令**:
```powershell
sqlcmd -S "DESKTOP-8HQIS1S\SQLEXPRESS" -d "GameSpacedatabase" -E -Q "SELECT TOP 5 EVoucherTypeID, Name, ValueAmount, ValidFrom, ValidTo, PointsCost, TotalAvailable FROM EVoucherType"
```

---

### 🎯 Phase 3: 進階查詢（業務邏輯驗證）

#### 查詢範例 1: 獲取特定使用者的完整資訊

**命令**:
```powershell
sqlcmd -S "DESKTOP-8HQIS1S\SQLEXPRESS" -d "GameSpacedatabase" -E -Q "SELECT u.User_ID, u.User_name, u.User_Account, w.User_Point, p.PetName, p.Level FROM Users u LEFT JOIN User_Wallet w ON u.User_ID = w.User_Id LEFT JOIN Pet p ON u.User_ID = p.UserID WHERE u.User_ID = 10000001"
```

---

#### 查詢範例 2: 獲取最近的交易記錄

**命令**:
```powershell
sqlcmd -S "DESKTOP-8HQIS1S\SQLEXPRESS" -d "GameSpacedatabase" -E -Q "SELECT TOP 20 wh.LogID, u.User_name, wh.ChangeType, wh.PointsChanged, wh.Description, wh.ChangeTime FROM WalletHistory wh INNER JOIN Users u ON wh.UserID = u.User_ID ORDER BY wh.ChangeTime DESC"
```

---

#### 查詢範例 3: 統計每個使用者的簽到次數

**命令**:
```powershell
sqlcmd -S "DESKTOP-8HQIS1S\SQLEXPRESS" -d "GameSpacedatabase" -E -Q "SELECT u.User_name, COUNT(s.LogID) AS SignInCount, SUM(s.PointsGained) AS TotalPoints FROM Users u LEFT JOIN UserSignInStats s ON u.User_ID = s.UserID GROUP BY u.User_name ORDER BY SignInCount DESC"
```

---

## 所有核心表的讀取命令

### 完整命令清單（可直接複製執行）

```powershell
# ========== 管理員相關 ==========

# 1. ManagerData - 管理員基本資料
sqlcmd -S "DESKTOP-8HQIS1S\SQLEXPRESS" -d "GameSpacedatabase" -E -Q "SELECT TOP 10 * FROM ManagerData"

# 2. ManagerRolePermission - 角色權限
sqlcmd -S "DESKTOP-8HQIS1S\SQLEXPRESS" -d "GameSpacedatabase" -E -Q "SELECT * FROM ManagerRolePermission"

# 3. ManagerRole - 管理員角色分配
sqlcmd -S "DESKTOP-8HQIS1S\SQLEXPRESS" -d "GameSpacedatabase" -E -Q "SELECT TOP 10 * FROM ManagerRole"


# ========== 使用者相關 ==========

# 4. Users - 使用者帳號
sqlcmd -S "DESKTOP-8HQIS1S\SQLEXPRESS" -d "GameSpacedatabase" -E -Q "SELECT TOP 10 * FROM Users"

# 5. User_Wallet - 使用者錢包
sqlcmd -S "DESKTOP-8HQIS1S\SQLEXPRESS" -d "GameSpacedatabase" -E -Q "SELECT TOP 10 * FROM User_Wallet"

# 6. WalletHistory - 錢包交易歷史
sqlcmd -S "DESKTOP-8HQIS1S\SQLEXPRESS" -d "GameSpacedatabase" -E -Q "SELECT TOP 20 * FROM WalletHistory ORDER BY ChangeTime DESC"


# ========== 寵物系統 ==========

# 7. Pet - 寵物資料
sqlcmd -S "DESKTOP-8HQIS1S\SQLEXPRESS" -d "GameSpacedatabase" -E -Q "SELECT TOP 10 * FROM Pet"

# 8. PetColorOptions - 寵物顏色選項
sqlcmd -S "DESKTOP-8HQIS1S\SQLEXPRESS" -d "GameSpacedatabase" -E -Q "SELECT * FROM PetColorOptions"

# 9. PetSkinColorPointSettings - 寵物換色點數設定
sqlcmd -S "DESKTOP-8HQIS1S\SQLEXPRESS" -d "GameSpacedatabase" -E -Q "SELECT * FROM PetSkinColorPointSettings"

# 10. PetBackgroundPointSettings - 寵物背景點數設定
sqlcmd -S "DESKTOP-8HQIS1S\SQLEXPRESS" -d "GameSpacedatabase" -E -Q "SELECT * FROM PetBackgroundPointSettings"


# ========== 小遊戲系統 ==========

# 11. MiniGame - 遊戲記錄
sqlcmd -S "DESKTOP-8HQIS1S\SQLEXPRESS" -d "GameSpacedatabase" -E -Q "SELECT TOP 20 * FROM MiniGame ORDER BY StartTime DESC"


# ========== 簽到系統 ==========

# 12. UserSignInStats - 簽到統計
sqlcmd -S "DESKTOP-8HQIS1S\SQLEXPRESS" -d "GameSpacedatabase" -E -Q "SELECT TOP 20 * FROM UserSignInStats ORDER BY SignTime DESC"


# ========== 優惠券系統 ==========

# 13. CouponType - 優惠券類型
sqlcmd -S "DESKTOP-8HQIS1S\SQLEXPRESS" -d "GameSpacedatabase" -E -Q "SELECT * FROM CouponType"

# 14. Coupon - 使用者優惠券
sqlcmd -S "DESKTOP-8HQIS1S\SQLEXPRESS" -d "GameSpacedatabase" -E -Q "SELECT TOP 20 * FROM Coupon"


# ========== 電子禮券系統 ==========

# 15. EVoucherType - 電子禮券類型
sqlcmd -S "DESKTOP-8HQIS1S\SQLEXPRESS" -d "GameSpacedatabase" -E -Q "SELECT * FROM EVoucherType"

# 16. EVoucher - 使用者電子禮券
sqlcmd -S "DESKTOP-8HQIS1S\SQLEXPRESS" -d "GameSpacedatabase" -E -Q "SELECT TOP 20 * FROM EVoucher"


# ========== 統計命令 ==========

# 統計所有核心表的記錄數
sqlcmd -S "DESKTOP-8HQIS1S\SQLEXPRESS" -d "GameSpacedatabase" -E -Q "SELECT 'ManagerData' AS TableName, COUNT(*) AS RecordCount FROM ManagerData UNION ALL SELECT 'Users', COUNT(*) FROM Users UNION ALL SELECT 'Pet', COUNT(*) FROM Pet UNION ALL SELECT 'UserSignInStats', COUNT(*) FROM UserSignInStats UNION ALL SELECT 'Coupon', COUNT(*) FROM Coupon UNION ALL SELECT 'EVoucher', COUNT(*) FROM EVoucher"
```

---

## 常見錯誤與解決方案

### 錯誤 1: "找不到表或視圖"

**錯誤訊息**:
```
錯誤 207, 層級 16, 狀態 1
無效的欄位名稱 'FieldName'
```

**原因**: 欄位名稱不正確

**解決方案**:
1. 使用 `SELECT * FROM 表名` 先查看實際欄位
2. 注意 Snake_Case 命名（如 `User_Id` 而非 `UserId`）
3. 使用方括號包住欄位名 `[User_Id]`

---

### 錯誤 2: "連線失敗"

**錯誤訊息**:
```
Sqlcmd: Error: Microsoft ODBC Driver for SQL Server : 
Login timeout expired
```

**原因**: 
1. 伺服器名稱錯誤
2. SQL Server 服務未啟動
3. 網路連線問題

**解決方案**:
```powershell
# 1. 檢查 SQL Server 服務狀態
Get-Service | Where-Object {$_.Name -like "*SQL*"}

# 2. 啟動 SQL Server 服務（如果已停止）
Start-Service MSSQL$SQLEXPRESS

# 3. 測試基本連線
sqlcmd -S "DESKTOP-8HQIS1S\SQLEXPRESS" -E -Q "SELECT @@VERSION"
```

---

### 錯誤 3: "Windows 認證失敗"

**錯誤訊息**:
```
Login failed for user 'DOMAIN\USERNAME'
```

**原因**: 
1. 使用者沒有資料庫存取權限
2. SQL Server 不允許 Windows 認證

**解決方案**:
```powershell
# 改用 SQL 認證（如果有 SQL 帳號）
sqlcmd -S "DESKTOP-8HQIS1S\SQLEXPRESS" -d "GameSpacedatabase" -U "sa" -P "密碼" -Q "SELECT * FROM Users"
```

---

### 錯誤 4: "中文亂碼"

**症狀**: 查詢結果中的中文顯示為亂碼或問號

**原因**: PowerShell 的編碼問題

**解決方案**:
```powershell
# 臨時設定 PowerShell 編碼為 UTF-8
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

# 然後再執行查詢
sqlcmd -S "DESKTOP-8HQIS1S\SQLEXPRESS" -d "GameSpacedatabase" -E -Q "SELECT TOP 5 * FROM Pet"
```

---

### 錯誤 5: "結果被截斷"

**症狀**: 查詢結果的文字欄位被截斷

**原因**: sqlcmd 預設欄位寬度限制

**解決方案**:
```powershell
# 使用 -y 參數設定可變長度欄位的最大寬度
sqlcmd -S "DESKTOP-8HQIS1S\SQLEXPRESS" -d "GameSpacedatabase" -E -y 0 -Q "SELECT * FROM CouponType"
```

---

## 資料驗證清單

### 在讀取資料後，應該驗證以下項目：

#### ✅ 基本驗證

- [ ] 每個核心表都有資料（COUNT > 0）
- [ ] 管理員帳號有 102 筆
- [ ] 使用者帳號有 200 筆
- [ ] 寵物有 200 筆
- [ ] 至少有 3 個測試管理員帳號可用

#### ✅ 資料完整性驗證

- [ ] 每個 User 都有對應的 User_Wallet
- [ ] 每個 Pet 都有對應的 UserID
- [ ] WalletHistory 中的 ChangeType 只有 Point/Coupon/EVoucher
- [ ] MiniGame 的 Result 只有 Win/Lose/Aborted

#### ✅ 業務邏輯驗證

- [ ] 簽到記錄的 PointsGained > 0
- [ ] 優惠券類型有 Amount 和 Percent 兩種
- [ ] 寵物的五維屬性都在 0-100 範圍內
- [ ] 管理員至少有一個具有最高權限（AdministratorPrivilegesManagement = 1）

#### ✅ 關聯性驗證

```powershell
# 驗證使用者和錢包的關聯
sqlcmd -S "DESKTOP-8HQIS1S\SQLEXPRESS" -d "GameSpacedatabase" -E -Q "SELECT COUNT(*) AS OrphanWallets FROM User_Wallet w WHERE NOT EXISTS (SELECT 1 FROM Users u WHERE u.User_ID = w.User_Id)"

# 應該返回 0（表示沒有孤兒記錄）
```

---

## 最佳實踐

### 1. 逐步讀取，從簡單到複雜

**推薦順序**:
1. 先列出所有表名（確認連線）
2. 統計每個表的記錄數（了解資料規模）
3. 讀取簡單表的完整記錄（如 ManagerRolePermission）
4. 讀取大表的部分記錄（使用 TOP N）
5. 執行關聯查詢（JOIN）

---

### 2. 使用 TOP N 限制結果

**為什麼**: 避免輸出過多資料導致終端機卡頓

**範例**:
```powershell
# ✅ 好的做法
sqlcmd -S "..." -d "..." -E -Q "SELECT TOP 10 * FROM Users"

# ❌ 不好的做法（可能返回數千筆）
sqlcmd -S "..." -d "..." -E -Q "SELECT * FROM WalletHistory"
```

---

### 3. 先查看表結構，再指定欄位

**步驟**:
```powershell
# Step 1: 查看前幾筆完整記錄
sqlcmd -S "DESKTOP-8HQIS1S\SQLEXPRESS" -d "GameSpacedatabase" -E -Q "SELECT TOP 1 * FROM Pet"

# Step 2: 確認欄位名稱後，再指定需要的欄位
sqlcmd -S "DESKTOP-8HQIS1S\SQLEXPRESS" -d "GameSpacedatabase" -E -Q "SELECT PetID, UserID, PetName, Level FROM Pet"
```

---

### 4. 使用 ORDER BY 獲取最新/最舊的記錄

**範例**:
```powershell
# 獲取最新的 10 筆簽到記錄
sqlcmd -S "DESKTOP-8HQIS1S\SQLEXPRESS" -d "GameSpacedatabase" -E -Q "SELECT TOP 10 * FROM UserSignInStats ORDER BY SignTime DESC"

# 獲取最早註冊的 10 個使用者
sqlcmd -S "DESKTOP-8HQIS1S\SQLEXPRESS" -d "GameSpacedatabase" -E -Q "SELECT TOP 10 * FROM Users ORDER BY User_ID ASC"
```

---

### 5. 保存查詢結果到檔案

**命令**:
```powershell
# 方法 1: 使用 PowerShell 重導向
sqlcmd -S "DESKTOP-8HQIS1S\SQLEXPRESS" -d "GameSpacedatabase" -E -Q "SELECT * FROM ManagerData" > ManagerData_Export.txt

# 方法 2: 使用 sqlcmd 的 -o 參數
sqlcmd -S "DESKTOP-8HQIS1S\SQLEXPRESS" -d "GameSpacedatabase" -E -Q "SELECT * FROM ManagerData" -o ManagerData_Export.txt
```

---

### 6. 批次執行多個查詢

**建立 SQL 腳本檔案** (`queries.sql`):
```sql
-- queries.sql
SELECT COUNT(*) AS TotalUsers FROM Users;
SELECT COUNT(*) AS TotalPets FROM Pet;
SELECT COUNT(*) AS TotalManagers FROM ManagerData;
```

**執行腳本**:
```powershell
sqlcmd -S "DESKTOP-8HQIS1S\SQLEXPRESS" -d "GameSpacedatabase" -E -i queries.sql
```

---

## 快速參考表

### 核心表與欄位對照

| 表名 | 主鍵 | 常用欄位 | 記錄數 |
|-----|------|---------|--------|
| **ManagerData** | Manager_Id | Manager_Name, Manager_Account, Manager_Password | 102 |
| **ManagerRolePermission** | ManagerRole_Id | role_name, AdministratorPrivilegesManagement | 8 |
| **Users** | User_ID | User_name, User_Account, User_Password | 200 |
| **User_Wallet** | User_Id | User_Point | 200 |
| **Pet** | PetID | PetName, Level, Health, Hunger, Mood, Stamina, Cleanliness | 200 |
| **WalletHistory** | LogID | UserID, ChangeType, PointsChanged, Description | 大量 |
| **UserSignInStats** | LogID | UserID, SignTime, PointsGained, ExpGained | 2400 |
| **MiniGame** | PlayID | UserID, PetID, Result, ExpGained, PointsGained | 大量 |
| **CouponType** | CouponTypeID | Name, DiscountType, DiscountValue | 20+ |
| **Coupon** | CouponID | UserID, CouponCode, IsUsed | 698 |
| **EVoucherType** | EVoucherTypeID | Name, ValueAmount | 20+ |
| **EVoucher** | EVoucherID | UserID, EVoucherCode, IsUsed | 355 |

---

### 常用 SQL 語法速查

| 需求 | SQL 語法 |
|-----|---------|
| 查看所有表 | `SELECT name FROM sys.tables ORDER BY name` |
| 查看表結構 | `EXEC sp_columns [表名]` |
| 統計記錄數 | `SELECT COUNT(*) FROM [表名]` |
| 查看前N筆 | `SELECT TOP N * FROM [表名]` |
| 排序查詢 | `SELECT * FROM [表名] ORDER BY [欄位] DESC` |
| 條件查詢 | `SELECT * FROM [表名] WHERE [條件]` |
| JOIN查詢 | `SELECT * FROM [表1] INNER JOIN [表2] ON [條件]` |
| GROUP BY | `SELECT [欄位], COUNT(*) FROM [表名] GROUP BY [欄位]` |

---

## 進階技巧

### 技巧 1: 使用變數簡化命令

```powershell
# 定義連線變數
$Server = "DESKTOP-8HQIS1S\SQLEXPRESS"
$Database = "GameSpacedatabase"
$BaseCmd = "sqlcmd -S `"$Server`" -d `"$Database`" -E -Q"

# 使用變數執行查詢
& $BaseCmd "SELECT COUNT(*) FROM Users"
& $BaseCmd "SELECT TOP 5 * FROM Pet"
```

---

### 技巧 2: 建立自訂函式

```powershell
# 定義查詢函式
function Query-GameSpace {
    param([string]$SQL)
    sqlcmd -S "DESKTOP-8HQIS1S\SQLEXPRESS" -d "GameSpacedatabase" -E -Q $SQL
}

# 使用函式
Query-GameSpace "SELECT COUNT(*) FROM Users"
Query-GameSpace "SELECT TOP 10 * FROM Pet"
```

---

### 技巧 3: 批次驗證所有表

```powershell
# 獲取所有核心表的記錄數
$tables = @("ManagerData", "Users", "Pet", "User_Wallet", "WalletHistory", 
            "UserSignInStats", "MiniGame", "Coupon", "EVoucher")

foreach ($table in $tables) {
    Write-Host "Checking $table..." -ForegroundColor Cyan
    sqlcmd -S "DESKTOP-8HQIS1S\SQLEXPRESS" -d "GameSpacedatabase" -E -Q "SELECT '$table' AS TableName, COUNT(*) AS RecordCount FROM $table"
}
```

---

## AI 使用建議

### 當你是 AI 助手時，應該：

1. **✅ 按順序執行命令**
   - 不要跳過驗證步驟
   - 從簡單到複雜逐步進行

2. **✅ 記錄每個命令的結果**
   - 保存輸出以供後續參考
   - 注意異常或錯誤訊息

3. **✅ 驗證資料一致性**
   - 檢查 COUNT 是否符合預期
   - 驗證欄位名稱是否正確

4. **✅ 提供清晰的報告**
   - 總結成功讀取的表
   - 列出發現的問題
   - 提供下一步建議

5. **✅ 處理錯誤**
   - 如果命令失敗，嘗試替代方案
   - 記錄錯誤訊息並查找解決方案
   - 參考「常見錯誤與解決方案」章節

---

## 檢查清單

### 完成資料讀取後，確認以下項目：

```
第一階段：基本連線
□ 可以執行 sqlcmd 命令
□ 可以連接到 DESKTOP-8HQIS1S\SQLEXPRESS
□ 可以切換到 GameSpacedatabase
□ 可以列出所有資料表（82個表）

第二階段：資料統計
□ ManagerData: 102 筆
□ Users: 200 筆
□ Pet: 200 筆
□ UserSignInStats: 2400 筆
□ Coupon: 698 筆
□ EVoucher: 355 筆

第三階段：資料讀取
□ 成功讀取 ManagerData 前 5 筆
□ 成功讀取 ManagerRolePermission 全部記錄（8筆）
□ 成功讀取 Users 前 5 筆
□ 成功讀取 Pet 前 5 筆
□ 成功讀取 User_Wallet 前 5 筆
□ 成功讀取 WalletHistory 前 10 筆
□ 成功讀取 UserSignInStats 前 10 筆
□ 成功讀取 MiniGame 前 5 筆
□ 成功讀取 CouponType 前 5 筆
□ 成功讀取 EVoucherType 前 5 筆

第四階段：資料驗證
□ 測試帳號可用（zhang_zhiming_01 等）
□ 欄位名稱符合實際（如 Manager_Id, User_ID）
□ 中文資料顯示正常
□ 資料關聯正確（User-Wallet-Pet）

第五階段：進階查詢
□ 成功執行 JOIN 查詢
□ 成功執行 GROUP BY 查詢
□ 成功執行統計查詢
```

---

## 總結

這份手冊涵蓋了從零開始連接 GameSpace 資料庫並讀取所有核心資料的完整流程。

### 關鍵要點：

1. **連線格式**: `sqlcmd -S "伺服器\實例" -d "資料庫" -E -Q "SQL"`
2. **認證方式**: Windows 整合認證（-E 參數）
3. **核心表數**: 82 個表，其中 12 個是核心業務表
4. **資料規模**: 102 管理員、200 使用者、200 寵物、2400+ 簽到記錄
5. **欄位命名**: 使用 Snake_Case（如 `Manager_Id`, `User_Point`）
6. **讀取策略**: 使用 TOP N 限制結果，從簡單到複雜逐步進行

### 完成這份手冊後，你應該能夠：

✅ 成功連接到 SQL Server 資料庫  
✅ 讀取所有核心業務資料  
✅ 理解資料庫結構和關聯  
✅ 執行基本和進階 SQL 查詢  
✅ 處理常見錯誤  
✅ 驗證資料完整性  
✅ 為開發 MiniGame Area 做好準備  

---

**手冊版本**: 1.0  
**最後驗證**: 2025-10-03  
**適用環境**: Windows 11 + SQL Server Express  
**維護者**: Claude Code  
**狀態**: ✅ 已在實際環境中完整驗證

