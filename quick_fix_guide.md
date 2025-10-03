# GameSpace 錯誤修復指南 - 快速參考

## 📋 實體屬性對照速查表

### Pet 實體
```csharp
// 檔案: Models/Pet.cs
✅ 正確屬性名稱：
- PetId
- UserId
- PetName
- Level
- Experience
- Hunger        // ✅ 飽食度
- Mood          // ✅ 心情
- Stamina       // ✅ 體力
- Cleanliness   // ✅ 乾淨度
- Health        // ✅ 健康度
- SkinColor
- BackgroundColor

❌ 錯誤使用：
- Happiness → Mood
- Energy → 可能是 Stamina 或 Hunger
```

### 所有實體屬性名稱錯誤對照

| 錯誤使用 ❌ | 正確名稱 ✅ | 實體 |
|------------|-----------|------|
| User_Email | User_email | User |
| User_LockoutEnd | UserLockoutEnd | User |
| User_Point | UserPoint | UserWallet |
| User_Id | UserId | UserWallet |
| UserID | UserId | WalletHistory, Coupon |
| HistoryID | LogId | WalletHistory |
| ChangeAmount | PointsChanged | WalletHistory |
| RelatedID | ItemCode | WalletHistory |
| CouponID | CouponId | Coupon |
| CouponTypeID | CouponTypeId | Coupon |
| UsedInOrderID | UsedInOrderId | Coupon |
| Happiness | Mood | Pet |
| Energy | Stamina/Hunger | Pet |

### DbContext DbSet 名稱對照

| 錯誤使用 ❌ | 正確名稱 ✅ |
|------------|-----------|
| WalletHistory | WalletHistories |
| EVouchers | Evouchers |
| EVoucherTypes | EvoucherTypes |
| Managers | ManagerData |
| ManagerDatum | ManagerData |

---

## 🔧 檔案級修復清單

### 需要修復的檔案清單（按優先級）

#### P0 - Razor Views（必須優先）
1. ✅ `Areas/MiniGame/Views/EVouchers/Edit.cshtml` - 編碼錯誤
2. ✅ `Areas/MiniGame/Views/Shared/_AdminLayout.cshtml` - 語法錯誤

#### P1 - Services 層（屬性名稱錯誤）
3. ✅ `Areas/MiniGame/Services/WalletService.cs`
4. ✅ `Areas/MiniGame/Services/UserWalletService.cs`
5. ✅ `Areas/MiniGame/Services/UserService.cs`
6. ✅ `Areas/MiniGame/Services/CouponService.cs`
7. ✅ `Areas/MiniGame/Services/EVoucherService.cs`
8. ✅ `Areas/MiniGame/Services/MiniGameAdminService.cs`
9. ✅ `Areas/MiniGame/Services/MiniGameAdminAuthService.cs`
10. ✅ `Areas/MiniGame/Services/SignInService.cs`

#### P1 - Filters 層
11. ✅ `Areas/MiniGame/Filters/MiniGameAdminAuthorizeAttribute.cs`

#### P1 - Models 層（導覽屬性）
12. ✅ `Models/Coupon.cs` - 需要添加 CouponType 導覽屬性

---

## 🚀 快速修復命令

### 步驟 1: 還原損壞的 Razor 檔案
```powershell
cd C:\Users\n2029\Desktop\GameSpace
git checkout HEAD -- GameSpace/GameSpace/Areas/MiniGame/Views/EVouchers/Edit.cshtml
git checkout HEAD -- GameSpace/GameSpace/Areas/MiniGame/Views/Shared/_AdminLayout.cshtml
```

### 步驟 2: 批次搜尋取代（建議手動逐一確認）

#### 在 Visual Studio 中：
1. 按 `Ctrl+Shift+H` 開啟全域搜尋取代
2. 搜尋範圍選擇：`Areas\MiniGame\Services`
3. 使用正規表達式：**啟用**
4. 依序替換以下模式：

```
搜尋: \.User_Point\b
取代: .UserPoint
檔案類型: *.cs

搜尋: \.User_Id\b
取代: .UserId
檔案類型: *.cs

搜尋: \bUserID\b
取代: UserId
檔案類型: *.cs
附註: 小心全域替換，確認不會影響字串常數

搜尋: \bCouponID\b
取代: CouponId
檔案類型: *.cs

搜尋: \bCouponTypeID\b
取代: CouponTypeId
檔案類型: *.cs

搜尋: \bUsedInOrderID\b
取代: UsedInOrderId
檔案類型: *.cs

搜尋: \bHistoryID\b
取代: LogId
檔案類型: *.cs

搜尋: \bChangeAmount\b
取代: PointsChanged
檔案類型: *.cs

搜尋: \bRelatedID\b
取代: ItemCode
檔案類型: *.cs

搜尋: \.User_Email\b
取代: .User_email
檔案類型: *.cs

搜尋: \.User_LockoutEnd\b
取代: .UserLockoutEnd
檔案類型: *.cs

搜尋: _context\.WalletHistory\b
取代: _context.WalletHistories
檔案類型: *.cs

搜尋: _context\.EVouchers\b
取代: _context.Evouchers
檔案類型: *.cs

搜尋: _context\.EVoucherTypes\b
取代: _context.EvoucherTypes
檔案類型: *.cs

搜尋: _context\.Managers\b
取代: _context.ManagerData
檔案類型: *.cs

搜尋: _context\.ManagerDatum\b
取代: _context.ManagerData
檔案類型: *.cs

搜尋: \bHappiness\b
取代: Mood
檔案類型: *.cs
範圍: Areas\MiniGame\Services\MiniGameAdminService.cs
附註: 僅限 Pet 相關程式碼，需手動確認
```

### 步驟 3: 手動修正特殊案例

#### 檔案：`Models/Coupon.cs`
在 Coupon 類別中添加導覽屬性：

```csharp
public partial class Coupon
{
    // ... 現有屬性 ...

    public virtual OrderInfo? UsedInOrder { get; set; }
    public virtual User User { get; set; } = null!;

    // ⬇️ 新增此行
    public virtual CouponType CouponType { get; set; } = null!;
}
```

#### 檔案：`Areas/MiniGame/Services/MiniGameAdminService.cs`
Pet 的 Energy 屬性需要手動判斷應該對應到 Stamina 還是 Hunger：

```csharp
// 行 430-435 附近，需要確認業務邏輯
// ❌ 如果原本是：
if (model.Energy.HasValue)
{
    pet.Energy = model.Energy.Value;
}

// ✅ 改為（假設 Energy 對應 Stamina）：
if (model.Energy.HasValue)
{
    pet.Stamina = model.Energy.Value;  // 或 pet.Hunger，視業務需求
}
```

---

## 📝 逐檔案修復詳細說明

### 檔案：WalletService.cs

**位置**: `Areas/MiniGame/Services/WalletService.cs`

**需要修正的行**:
```csharp
// 第 230, 234, 259, 260, 303, 331 行附近
❌ wallet.User_Point += points;
✅ wallet.UserPoint += points;

// 第 234, 264 行附近
❌ var history = new WalletHistory
   {
       UserID = userId,
       ChangeAmount = points,
       RelatedID = relatedId,
       ...
   };
✅ var history = new WalletHistory
   {
       UserId = userId,
       PointsChanged = points,
       ItemCode = relatedId,
       ...
   };

// 第 241, 271, 284 行附近
❌ await _context.WalletHistory.AddAsync(history);
✅ await _context.WalletHistories.AddAsync(history);
```

---

### 檔案：UserWalletService.cs

**位置**: `Areas/MiniGame/Services/UserWalletService.cs`

**需要修正的區塊**:

```csharp
// 行 109-127: Coupon 物件建立
❌ var coupon = new Coupon
   {
       CouponTypeID = typeId,
       UserID = userId,
       UsedInOrderID = null,
       ...
   };
   var history = new WalletHistory
   {
       UserID = userId,
       ChangeAmount = 1,
       RelatedID = coupon.CouponID,
       ...
   };

✅ var coupon = new Coupon
   {
       CouponTypeId = typeId,
       UserId = userId,
       UsedInOrderId = null,
       ...
   };
   var history = new WalletHistory
   {
       UserId = userId,
       PointsChanged = 1,
       ItemCode = coupon.CouponId.ToString(),
       ...
   };

// 行 148, 161: DbContext
❌ var evoucherType = await _context.EVoucherTypes.FindAsync(typeId);
   await _context.EVouchers.AddAsync(evoucher);
✅ var evoucherType = await _context.EvoucherTypes.FindAsync(typeId);
   await _context.Evouchers.AddAsync(evoucher);

// 行 151: 型別名稱
❌ var evoucher = new EVoucher { ... };
✅ var evoucher = new Evoucher { ... };

// 行 195, 250: 查詢條件
❌ .Where(h => h.UserID == userId)
✅ .Where(h => h.UserId == userId)

// 行 307-347: ViewModel 對應
❌ ChangeAmount = h.ChangeAmount ?? 0,
   RelatedId = h.RelatedID,
   HistoryId = h.HistoryID,
✅ ChangeAmount = h.PointsChanged,
   RelatedId = h.ItemCode,
   HistoryId = h.LogId,
```

---

### 檔案：UserService.cs

**位置**: `Areas/MiniGame/Services/UserService.cs`

**需要修正的行**:

```csharp
// 行 40, 176
❌ var user = await _context.Users.FirstOrDefaultAsync(u => u.User_Email == email);
✅ var user = await _context.Users.FirstOrDefaultAsync(u => u.User_email == email);

// 行 55-56
❌ var wallet = new UserWallet
   {
       User_Id = user.UserId,
       User_Point = 0
   };
✅ var wallet = new UserWallet
   {
       UserId = user.UserId,
       UserPoint = 0
   };

// 行 143, 161
❌ user.User_LockoutEnd = DateTime.UtcNow.AddDays(lockoutDays);
✅ user.UserLockoutEnd = DateTime.UtcNow.AddDays(lockoutDays);
```

---

### 檔案：CouponService.cs

**位置**: `Areas/MiniGame/Services/CouponService.cs`

**需要批次替換**:
- 所有 `CouponID` → `CouponId`
- 所有 `CouponTypeID` → `CouponTypeId`
- 所有 `UserID` → `UserId`

建議使用全域搜尋取代功能。

---

### 檔案：MiniGameAdminAuthService.cs

**位置**: `Areas/MiniGame/Services/MiniGameAdminAuthService.cs`

**需要修正的行**:

```csharp
// 行 17, 31, 41
❌ var manager = await _context.ManagerDatum.FindAsync(managerId);
✅ var manager = await _context.ManagerData.FindAsync(managerId);
```

---

### 檔案：MiniGameAdminAuthorizeAttribute.cs

**位置**: `Areas/MiniGame/Filters/MiniGameAdminAuthorizeAttribute.cs`

**需要修正的行**:

```csharp
// 行 35
❌ var manager = await context.Managers
✅ var manager = await context.ManagerData
```

---

## ✅ 修復後驗證清單

完成修復後，依序執行以下命令驗證：

```powershell
# 1. 清理專案
dotnet clean GameSpace/GameSpace.sln

# 2. 還原套件
dotnet restore GameSpace/GameSpace.sln

# 3. 建置專案
dotnet build GameSpace/GameSpace.sln

# 4. 檢查錯誤數量
# 目標：錯誤數應該大幅減少（從 1466 → 預期 < 300）
```

### 預期結果

完成 P0-P1 修復後：
- ✅ Razor Views 錯誤：0
- ✅ 實體屬性名稱錯誤：0
- ✅ DbContext 錯誤：0
- ⚠️ ViewModel 錯誤：仍會存在（需 P2-P4 修復）
- ⚠️ Service 介面方法缺失：仍會存在（需 P3 修復）

**預期錯誤數量**: 約 300-500 個（主要是 ViewModel 和 Service 介面相關）

---

## 📞 如需進一步協助

完成 P0-P1 修復後，如果建置錯誤已大幅減少，可以繼續處理：
1. P2：補充 ViewModel 屬性和類別
2. P3：實作 Service 介面方法
3. P4：修復 Controllers 依賴錯誤

詳見完整報告：
- `build_analysis_report.md` - 完整錯誤分析
- `error_summary_detailed.md` - 詳細錯誤摘要

---

**最後更新**: 2025-10-03
**優先處理**: P0 Razor Views → P1 Services 屬性名稱
