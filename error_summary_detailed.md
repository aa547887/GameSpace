# GameSpace 建置錯誤詳細摘要

**建置時間**: 2025-10-03
**錯誤總數**: 1466 個錯誤
**警告總數**: 159 個警告

---

## 📊 核心問題根源分析

### 實體類別屬性名稱對照表

經檢查，發現 Services 層使用了**錯誤的屬性名稱**。以下是實際的實體定義：

#### ✅ User 實體（正確定義）
```csharp
// 檔案: Models/User.cs
public partial class User
{
    public int UserId { get; set; }                    // ✅ 正確
    public string UserName { get; set; }               // ✅ 正確
    public DateTime? UserLockoutEnd { get; set; }      // ✅ 正確
    public string User_email { get; set; }             // ✅ 正確（實際存在）

    // 錯誤使用案例:
    // ❌ User_Email (大寫E) - 實際應為 User_email (小寫e)
    // ❌ User_LockoutEnd - 實際應為 UserLockoutEnd (無底線)
}
```

#### ✅ UserWallet 實體（正確定義）
```csharp
// 檔案: Models/UserWallet.cs
public partial class UserWallet
{
    public int UserId { get; set; }          // ✅ 正確
    public int UserPoint { get; set; }       // ✅ 正確
    public virtual User User { get; set; }   // ✅ 導覽屬性
}
```

**錯誤使用清單**:
- ❌ `User_Point` → ✅ `UserPoint`
- ❌ `User_Id` → ✅ `UserId`

#### ✅ WalletHistory 實體（正確定義）
```csharp
// 檔案: Models/WalletHistory.cs
public partial class WalletHistory
{
    public int LogId { get; set; }           // ✅ 主鍵
    public int UserId { get; set; }          // ✅ 使用者ID
    public string ChangeType { get; set; }   // ✅ 變更類型
    public int PointsChanged { get; set; }   // ✅ 積分變更量
    public string? ItemCode { get; set; }    // ✅ 項目代碼（關聯ID）
    public string? Description { get; set; } // ✅ 描述
    public DateTime ChangeTime { get; set; } // ✅ 變更時間
    public virtual User User { get; set; }   // ✅ 導覽屬性
}
```

**錯誤使用清單**:
- ❌ `HistoryID` → ✅ `LogId`
- ❌ `UserID` (大寫ID) → ✅ `UserId` (駝峰)
- ❌ `ChangeAmount` → ✅ `PointsChanged`
- ❌ `RelatedID` → ✅ `ItemCode`

#### ✅ Coupon 實體（正確定義）
```csharp
// 檔案: Models/Coupon.cs
public partial class Coupon
{
    public int CouponId { get; set; }              // ✅ 主鍵
    public string CouponCode { get; set; }         // ✅ 優惠券代碼
    public int CouponTypeId { get; set; }          // ✅ 類型ID
    public int UserId { get; set; }                // ✅ 使用者ID
    public bool IsUsed { get; set; }               // ✅ 是否已使用
    public DateTime AcquiredTime { get; set; }     // ✅ 取得時間
    public DateTime? UsedTime { get; set; }        // ✅ 使用時間
    public int? UsedInOrderId { get; set; }        // ✅ 訂單ID
    public virtual OrderInfo? UsedInOrder { get; set; }  // ✅ 導覽屬性
    public virtual User User { get; set; }         // ✅ 導覽屬性
}
```

**錯誤使用清單**:
- ❌ `CouponID` (大寫ID) → ✅ `CouponId` (駝峰)
- ❌ `CouponTypeID` → ✅ `CouponTypeId`
- ❌ `UserID` → ✅ `UserId`
- ❌ `UsedInOrderID` → ✅ `UsedInOrderId`
- ❌ `CouponType` 導覽屬性 → ⚠️ **未定義**（Coupon 實體缺少 CouponType 導覽屬性）

#### ✅ GameSpacedatabaseContext DbSet 定義
```csharp
// 檔案: Models/GameSpacedatabaseContext.cs
public virtual DbSet<Coupon> Coupons { get; set; }             // ✅ 存在
public virtual DbSet<CouponType> CouponTypes { get; set; }     // ✅ 存在
public virtual DbSet<Evoucher> Evouchers { get; set; }         // ✅ 存在（注意：Evoucher 不是 EVoucher）
public virtual DbSet<EvoucherType> EvoucherTypes { get; set; } // ✅ 存在
public virtual DbSet<UserWallet> UserWallets { get; set; }     // ✅ 存在
public virtual DbSet<WalletHistory> WalletHistories { get; set; } // ✅ 存在
public virtual DbSet<ManagerDatum> ManagerData { get; set; }   // ✅ 存在（複數形式）
public virtual DbSet<Pet> Pets { get; set; }                   // ✅ 存在
```

**錯誤使用清單**:
- ❌ `WalletHistory` (單數) → ✅ `WalletHistories` (複數)
- ❌ `EVouchers` (大寫EV) → ✅ `Evouchers` (只有E大寫)
- ❌ `EVoucherTypes` → ✅ `EvoucherTypes`
- ❌ `Managers` → ✅ `ManagerData`
- ❌ `ManagerDatum` (單數) → ✅ `ManagerData` (複數)

---

## 🎯 關鍵修復清單

### P0 - 緊急（Razor Views 編碼問題）

#### 1. EVouchers/Edit.cshtml 編碼錯誤
```
檔案: Areas/MiniGame/Views/EVouchers/Edit.cshtml
錯誤行: 100, 103, 110, 111, 112, 147
問題: RZ1005 - 無效的字元 "�"
修復: 從 git 還原或重新以 UTF-8 編碼儲存
```

#### 2. _AdminLayout.cshtml 語法錯誤
```
檔案: Areas/MiniGame/Views/Shared/_AdminLayout.cshtml
錯誤行: 419
問題: CS1003 - 缺少逗號
修復: 檢查行 419 的 C# 語法
```

---

### P1 - 高優先級（Services 層屬性名稱錯誤）

#### 1. WalletService.cs 錯誤修正
**檔案**: `Areas/MiniGame/Services/WalletService.cs`

需要進行以下替換：
```csharp
// 第 230, 234, 259, 260, 303, 331 行
❌ wallet.User_Point
✅ wallet.UserPoint

// 第 234, 264 行
❌ WalletHistory { UserID = ... }
✅ WalletHistory { UserId = ... }

// 第 234, 264 行等
❌ ChangeAmount = ...
✅ PointsChanged = ...

// 第 234, 264 行等
❌ RelatedID = ...
✅ ItemCode = ...

// 第 241, 271, 284 行
❌ _context.WalletHistory
✅ _context.WalletHistories
```

#### 2. UserWalletService.cs 錯誤修正
**檔案**: `Areas/MiniGame/Services/UserWalletService.cs`

需要進行以下替換：
```csharp
// Coupon 屬性錯誤（行 109-127）
❌ CouponTypeID    ✅ CouponTypeId
❌ UserID          ✅ UserId
❌ UsedInOrderID   ✅ UsedInOrderId
❌ CouponID        ✅ CouponId

// WalletHistory 屬性錯誤（行 122-171）
❌ UserID          ✅ UserId
❌ ChangeAmount    ✅ PointsChanged
❌ RelatedID       ✅ ItemCode
❌ HistoryID       ✅ LogId

// DbContext 錯誤（行 148, 161）
❌ _context.EVoucherTypes  ✅ _context.EvoucherTypes
❌ _context.EVouchers      ✅ _context.Evouchers

// 型別錯誤（行 151）
❌ EVoucher  ✅ Evoucher
```

#### 3. UserService.cs 錯誤修正
**檔案**: `Areas/MiniGame/Services/UserService.cs`

需要進行以下替換：
```csharp
// User 屬性錯誤（行 40, 176）
❌ user.User_Email       ✅ user.User_email  // 注意：小寫 e

// User 屬性錯誤（行 143, 161）
❌ user.User_LockoutEnd  ✅ user.UserLockoutEnd  // 注意：無底線

// UserWallet 屬性錯誤（行 55-56）
❌ User_Id     ✅ UserId
❌ User_Point  ✅ UserPoint
```

#### 4. CouponService.cs 錯誤修正
**檔案**: `Areas/MiniGame/Services/CouponService.cs`

需要進行以下替換：
```csharp
// Coupon 屬性錯誤
❌ CouponID      ✅ CouponId
❌ CouponTypeID  ✅ CouponTypeId
❌ UserID        ✅ UserId
// ... 檔案中所有出現的地方
```

#### 5. MiniGameAdminAuthService.cs 錯誤修正
**檔案**: `Areas/MiniGame/Services/MiniGameAdminAuthService.cs`

需要進行以下替換：
```csharp
// DbContext 錯誤（行 17, 31, 41）
❌ _context.ManagerDatum  ✅ _context.ManagerData
```

#### 6. MiniGameAdminAuthorizeAttribute.cs 錯誤修正
**檔案**: `Areas/MiniGame/Filters/MiniGameAdminAuthorizeAttribute.cs`

需要進行以下替換：
```csharp
// DbContext 錯誤（行 35）
❌ context.Managers  ✅ context.ManagerData
```

---

### P2 - 中優先級（Coupon 實體缺少導覽屬性）

#### 需要在 Coupon 實體中添加導覽屬性
**檔案**: `Models/Coupon.cs`

```csharp
public partial class Coupon
{
    // ... 現有屬性 ...

    // ⚠️ 需要新增：
    public virtual CouponType CouponType { get; set; } = null!;
}
```

**影響檔案**:
- `Areas/MiniGame/Controllers/AdminCouponController.cs` (行 21, 86, 200)

---

### P3 - 中優先級（Pet 實體屬性名稱確認）

需要檢查 Pet 實體的實際定義，確認以下屬性：
- `Happiness` vs `Mood` （心情）
- `Energy` vs `Hunger` （飽食度）

**影響檔案**:
- `Areas/MiniGame/Services/MiniGameAdminService.cs` (行 430-435)

---

### P4 - 中優先級（ViewModel 缺失問題）

這些是獨立的問題，需要補充 ViewModel 定義和屬性。詳見完整報告 `build_analysis_report.md`。

---

## 🔧 快速修復腳本

### 腳本 1: 從 Git 還原損壞的 Razor Views
```powershell
# 還原編碼損壞的檔案
git checkout HEAD -- GameSpace/GameSpace/Areas/MiniGame/Views/EVouchers/Edit.cshtml
git checkout HEAD -- GameSpace/GameSpace/Areas/MiniGame/Views/Shared/_AdminLayout.cshtml
```

### 腳本 2: 批次替換屬性名稱（PowerShell）
```powershell
# 注意：執行前請先備份！
# 建議使用 IDE 的全域搜尋取代功能，並逐一確認

# WalletService.cs
$file = "GameSpace/GameSpace/Areas/MiniGame/Services/WalletService.cs"
(Get-Content $file) `
    -replace '\.User_Point', '.UserPoint' `
    -replace '\.UserID\s*=', '.UserId =' `
    -replace '\.ChangeAmount', '.PointsChanged' `
    -replace '\.RelatedID', '.ItemCode' `
    -replace '_context\.WalletHistory', '_context.WalletHistories' |
Set-Content $file

# UserService.cs
$file = "GameSpace/GameSpace/Areas/MiniGame/Services/UserService.cs"
(Get-Content $file) `
    -replace '\.User_Email', '.User_email' `
    -replace '\.User_LockoutEnd', '.UserLockoutEnd' `
    -replace '\.User_Id\s*=', '.UserId =' `
    -replace '\.User_Point', '.UserPoint' |
Set-Content $file

# ... 其他檔案類似處理
```

---

## 📊 錯誤統計

| 錯誤類型 | 數量 | 優先級 |
|---------|------|--------|
| Razor Views 編碼/語法錯誤 | 8 | P0 |
| 實體屬性名稱錯誤 | ~400 | P1 |
| DbContext DbSet 名稱錯誤 | ~30 | P1 |
| ViewModel 缺失/屬性缺失 | ~300 | P2-P4 |
| Service 介面方法缺失 | ~200 | P3 |
| Controllers 依賴錯誤 | ~400 | P4 |
| 其他錯誤 | ~128 | P4 |
| **總計** | **1466** | |

---

## ✅ 建議修復流程

### 階段 1：解除編譯阻斷（15 分鐘）
1. 還原 `EVouchers/Edit.cshtml`
2. 還原 `_AdminLayout.cshtml`
3. 執行建置確認 Razor 錯誤已清除

### 階段 2：修正實體屬性名稱（2-3 小時）
1. 使用 IDE 的全域搜尋取代功能
2. 批次替換錯誤的屬性名稱：
   - `User_Point` → `UserPoint`
   - `User_Id` → `UserId`
   - `UserID` → `UserId`
   - `CouponID` → `CouponId`
   - `CouponTypeID` → `CouponTypeId`
   - `ChangeAmount` → `PointsChanged`
   - `RelatedID` → `ItemCode`
   - `HistoryID` → `LogId`
   - `User_Email` → `User_email`
   - `User_LockoutEnd` → `UserLockoutEnd`
3. 批次替換錯誤的 DbSet 名稱：
   - `WalletHistory` → `WalletHistories`
   - `EVouchers` → `Evouchers`
   - `EVoucherTypes` → `EvoucherTypes`
   - `Managers` → `ManagerData`
   - `ManagerDatum` → `ManagerData`

### 階段 3：補充導覽屬性（30 分鐘）
1. 在 `Coupon` 實體中添加 `CouponType` 導覽屬性
2. 檢查 Pet 實體定義，確認屬性名稱

### 階段 4：補充 ViewModel（2-3 小時）
1. 補充現有 ViewModel 的缺失屬性
2. 建立缺失的 ViewModel 類別
3. 詳見 `build_analysis_report.md` 的 P2-P4 章節

### 階段 5：重新建置並驗證（1 小時）
1. 執行完整建置
2. 修復殘餘錯誤
3. 處理警告

---

**預計總修復時間**: 6-8 小時
**關鍵點**: 屬性名稱的大小寫和駝峰命名必須完全一致
