# GameSpace MiniGame Area 完整稽核報告

**稽核日期**: 2025-10-03
**稽核範圍**: Areas/MiniGame 完整程式碼
**稽核版本**: commit 60bf026
**稽核工具**: Claude Code AI Auditor

---

## 執行摘要

本次稽核針對 GameSpace 專案的 MiniGame Area 進行全面檢查，涵蓋 Controllers、Services、Models/ViewModels 三大層面。稽核發現 **14 個 Critical（嚴重）問題**、**18 個 High（高優先級）問題**、**22 個 Medium（中等）問題** 以及 **12 個 Low（低優先級）問題**。

### 關鍵發現

1. **架構問題**: Entity 和 ViewModel 混淆，部分 Entity 放錯位置
2. **依賴注入錯誤**: 多個 Controllers 未正確調用基類建構函式
3. **資料存取不一致**: 混用 EF Core 和 ADO.NET，部分使用錯誤的 DbContext
4. **DbSet 名稱錯誤**: 多處使用單數而非複數形式（如 `Coupon` 應為 `Coupons`）
5. **命名空間混亂**: ViewModels 命名空間不統一
6. **安全性問題**: 密碼雜湊演算法不安全，缺少細粒度權限檢查

### 優先修復建議

**立即修復（今天內）**:
- 8 個 Critical 級別的 Controller 依賴注入問題
- 4 個 Critical 級別的 Service DbContext 問題
- 2 個 Critical 級別的 Models 命名空間問題

**本週修復**:
- 所有 High 級別問題（18個）
- DbSet 名稱統一修正
- Entity 位置重組

**本月修復**:
- Medium 級別問題（22個）
- 完善錯誤處理和日誌記錄

---

## 第一部分：Controllers 層稽核

### 稽核範圍
- **檔案數量**: 22 個 Controller
- **程式碼行數**: 約 8,000+ 行
- **檢查項目**: 依賴注入、路由配置、認證授權、業務邏輯分離、錯誤處理

### Critical 問題清單（Controllers）

#### 1. AdminPetController - 缺少 GameSpacedatabaseContext 注入
**檔案**: `AdminPetController.cs` (Line 17-21)
**嚴重程度**: 🔴 Critical

**問題描述**:
```csharp
public AdminPetController(IPetService petService, IPetRulesService petRulesService)
{
    _petService = petService;
    _petRulesService = petRulesService;
}
```
Controller 繼承自 `MiniGameBaseController`，但 constructor 未注入 `GameSpacedatabaseContext` 且未調用 `base(context)`，導致基類的 `_context` 為 null。

**影響**:
- Runtime NullReferenceException
- 基類所有依賴 `_context` 的方法都會失敗
- 無法使用權限檢查、日誌記錄等基類功能

**修復方案**:
```csharp
private readonly IPetService _petService;
private readonly IPetRulesService _petRulesService;

public AdminPetController(
    GameSpacedatabaseContext context,
    IPetService petService,
    IPetRulesService petRulesService)
    : base(context)
{
    _petService = petService;
    _petRulesService = petRulesService;
}
```

---

#### 2. AdminManagerController - 重複 _context 宣告
**檔案**: `AdminManagerController.cs` (Line 15-19)
**嚴重程度**: 🔴 Critical

**問題描述**:
```csharp
public class AdminManagerController : MiniGameBaseController
{
    private readonly GameSpacedatabaseContext _context;  // 重複宣告，基類已有

    public AdminManagerController(GameSpacedatabaseContext context)
    {
        _context = context;  // 未調用 base(context)
    }
}
```

**影響**:
- 基類的 `_context` 仍為 null
- 子類的 `_context` 會遮蔽基類的 `protected _context`
- 使用基類方法時會發生 NullReferenceException

**修復方案**:
```csharp
public class AdminManagerController : MiniGameBaseController
{
    // 移除重複的 _context 宣告

    public AdminManagerController(GameSpacedatabaseContext context)
        : base(context)
    {
        // 可以在這裡初始化其他依賴
    }
}
```

---

#### 3. AdminMiniGameController - 重複 _context 宣告
**檔案**: `AdminMiniGameController.cs` (Line 15-19)
**嚴重程度**: 🔴 Critical
**問題**: 同上 (#2)

---

#### 4. AdminEVoucherController - 重複 _context 宣告
**檔案**: `AdminEVoucherController.cs` (Line 15-19)
**嚴重程度**: 🔴 Critical
**問題**: 同上 (#2)

---

#### 5. AdminWalletController - 缺少 AuthenticationSchemes
**檔案**: `AdminWalletController.cs` (Line 11)
**嚴重程度**: 🔴 Critical

**問題描述**:
```csharp
[Authorize(Policy = "AdminOnly")]  // 缺少 AuthenticationSchemes
```

**影響**:
- 可能使用錯誤的認證 Scheme（預設為 Identity Cookie 而非 AdminCookie）
- 認證失敗或權限檢查不正確
- 與其他 Controllers 不一致

**修復方案**:
```csharp
using GameSpace.Areas.social_hub.Auth;  // 引入 AuthConstants

[Area("MiniGame")]
[Authorize(AuthenticationSchemes = AuthConstants.AdminCookieScheme, Policy = "AdminOnly")]
public class AdminWalletController : MiniGameBaseController
```

---

#### 6. AdminCouponController - 使用錯誤的 DbSet 名稱
**檔案**: `AdminCouponController.cs` (Multiple lines)
**嚴重程度**: 🔴 Critical

**問題描述**:
```csharp
// 錯誤：使用單數形式
var query = _context.Coupon.AsQueryable();  // Line 44
var couponType = await _context.CouponType.FindAsync(id);  // Line 84

// 正確：應使用複數形式（根據 GameSpacedatabaseContext 定義）
var query = _context.Coupons.AsQueryable();
var couponType = await _context.CouponTypes.FindAsync(id);
```

**影響**:
- 編譯錯誤：DbSet 屬性不存在
- 系統無法運行

**所有需要修正的位置**:
- Line 44: `_context.Coupon` → `_context.Coupons`
- Line 48: `_context.CouponType` → `_context.CouponTypes`
- Line 84: `_context.CouponType` → `_context.CouponTypes`
- Line 109, 151, 295, 367: 所有 `Coupon` → `Coupons`

---

#### 7. AdminCouponController - ViewModel 類型不匹配
**檔案**: `AdminCouponController.cs` (Line 44-72)
**嚴重程度**: 🔴 Critical

**問題描述**:
```csharp
var viewModel = new AdminWalletIndexViewModel  // 錯誤：使用錢包的 ViewModel
{
    Coupons = new PagedResult<UserCouponModel>  // 屬性不存在於 AdminWalletIndexViewModel
    {
        Items = coupons,
        // ...
    }
};
```

**影響**:
- 編譯錯誤：屬性不存在
- 邏輯錯誤：使用了錯誤的 ViewModel

**修復方案**:
創建正確的 ViewModel（應該已存在於 ViewModels 中）:
```csharp
// 使用 AdminCouponIndexViewModel 或類似的正確 ViewModel
var viewModel = new AdminCouponIndexViewModel
{
    Coupons = new PagedResult<CouponViewModel>
    {
        Items = coupons,
        TotalCount = totalCount,
        PageNumber = page,
        PageSize = pageSize
    },
    CouponTypes = couponTypes
};
```

---

#### 8. AdminHomeController - ViewModel 定義在 Controller 內
**檔案**: `AdminHomeController.cs` (Line 394-440)
**嚴重程度**: 🔴 Critical

**問題描述**:
Controller 檔案內部定義了多個 ViewModel 類別：
- `MiniGameAdminDashboardViewModel`
- `DashboardStatisticsModel`
- `RecentActivityModel`
- `SystemStatusModel`
- 等等...

**影響**:
- 違反單一職責原則
- 這些 ViewModels 無法在其他地方重用
- 檔案過長，難以維護
- 違反 MVC 架構規範

**修復方案**:
1. 將所有 ViewModel 移至 `Models/ViewModels/AdminHomeViewModels.cs`
2. 更新命名空間為 `GameSpace.Areas.MiniGame.Models.ViewModels`
3. 在 Controller 中引用：`using GameSpace.Areas.MiniGame.Models.ViewModels;`

---

### High 級別問題（Controllers - 選錄重要項目）

#### 9. AdminUserController - 直接使用 Entity 而非 ViewModel
**檔案**: `AdminUserController.cs` (Line 29, 144-156)
**嚴重程度**: 🟠 High

**問題描述**:
```csharp
IEnumerable<User> users;  // 直接使用 Entity
var user = new User { ... };  // POST 接收 Entity
```

**影響**:
- 違反 MVC 最佳實踐
- 可能暴露不應該暴露的屬性
- Over-posting 安全風險
- 循環引用導致 JSON 序列化失敗

**修復方案**:
創建並使用 ViewModel:
```csharp
// UserViewModel
public class AdminUserListViewModel
{
    public int UserId { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public bool IsActive { get; set; }
    // ... 只包含需要的屬性
}

// Controller
IEnumerable<AdminUserListViewModel> users = await _userService.GetUsersAsync(query);
```

---

#### 10. 缺少細粒度權限檢查
**影響檔案**: AdminMiniGameController, AdminPetController, AdminWalletController 等
**嚴重程度**: 🟠 High

**問題描述**:
部分敏感操作（Create, Edit, Delete）未使用 `HasPermissionAsync` 進行細粒度權限檢查，僅依賴 Controller 級別的 `[Authorize]` attribute。

**範例 - AdminPetController**:
```csharp
[HttpPost]
public async Task<IActionResult> DeletePet(int petId)
{
    // ❌ 缺少權限檢查
    await _petService.DeletePetAsync(petId);
    return Ok();
}
```

**修復方案**:
```csharp
[HttpPost]
public async Task<IActionResult> DeletePet(int petId)
{
    // ✅ 加入權限檢查
    if (!await HasPermissionAsync("Pet.Delete"))
    {
        return Forbid();
    }

    await _petService.DeletePetAsync(petId);
    await LogOperationAsync("DeletePet", $"Deleted pet {petId}");
    return Ok(new { success = true });
}
```

---

### Controllers 層統計摘要

| 項目 | 數量 |
|-----|------|
| 總 Controllers | 22 |
| Critical 問題 | 8 |
| High 問題 | 12 |
| Medium 問題 | 15 |
| Low 問題 | 8 |

---

## 第二部分：Services 層稽核

### 稽核範圍
- **服務數量**: 30+ 個 Service (Interface + Implementation)
- **程式碼行數**: 約 6,000+ 行
- **檢查項目**: 介面實作、資料存取方式、錯誤處理、業務邏輯正確性

### Critical 問題清單（Services）

#### 11. MiniGameAdminAuthService - 未實作介面
**檔案**: `MiniGameAdminAuthService.cs` (Line 6)
**嚴重程度**: 🔴 Critical

**問題描述**:
```csharp
// 錯誤
public class MiniGameAdminAuthService
{
    // ...
}

// 正確
public class MiniGameAdminAuthService : IMiniGameAdminAuthService
{
    // ...
}
```

**影響**:
- DI 容器無法正確注入
- 可能導致 Runtime 類型轉換錯誤

---

#### 12. UserWalletService - 使用不存在的 DbContext
**檔案**: `UserWalletService.cs` (Line 9)
**嚴重程度**: 🔴 Critical

**問題描述**:
```csharp
private readonly MiniGameDbContext _context;  // ❌ 此 DbContext 不存在

public UserWalletService(MiniGameDbContext context)
{
    _context = context;
}
```

**影響**:
- 編譯錯誤或 Runtime 錯誤
- 服務無法運作

**修復方案**:
```csharp
private readonly GameSpacedatabaseContext _context;

public UserWalletService(GameSpacedatabaseContext context)
{
    _context = context;
}
```

---

#### 13. MiniGameService - 使用 ADO.NET 而非 EF Core
**檔案**: `MiniGameService.cs`
**嚴重程度**: 🔴 Critical

**問題描述**:
```csharp
using (var connection = new SqlConnection(_connectionString))
{
    await connection.OpenAsync();
    using (var command = new SqlCommand(sql, connection))
    {
        // ... 原生 SQL 查詢
    }
}
```

**影響**:
- 與其他服務不一致
- 無法利用 EF Core 的變更追蹤、快取等功能
- SQL Injection 風險較高
- 無法參與 DbContext 的 Transaction
- 維護困難

**修復方案**:
改用 EF Core:
```csharp
private readonly GameSpacedatabaseContext _context;

public MiniGameService(GameSpacedatabaseContext context)
{
    _context = context;
}

public async Task<List<MiniGame>> GetGameRecordsAsync(int userId, int page, int pageSize)
{
    return await _context.MiniGames
        .Where(g => g.UserId == userId)
        .OrderByDescending(g => g.StartTime)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .AsNoTracking()
        .ToListAsync();
}
```

---

#### 14. PetService - 實體類別命名空間錯誤
**檔案**: `PetService.cs`
**嚴重程度**: 🔴 Critical

**問題描述**:
```csharp
using GameSpace.Areas.MiniGame.Models;  // ❌ 錯誤：Entity 在這裡

// 使用時：
Pet pet = await _context.Pet.FindAsync(id);  // Pet 類型來自 MiniGame.Models
```

但實際上 `GameSpacedatabaseContext` 的 Entity 來自 `GameSpace.Models`:
```csharp
// GameSpacedatabaseContext.cs
namespace GameSpace.Models;

public virtual DbSet<Pet> Pets { get; set; }  // Pet 來自 GameSpace.Models
```

**影響**:
- 類型不匹配
- 編譯錯誤或執行時錯誤
- DbSet 名稱也錯誤（應為 `Pets` 不是 `Pet`）

**修復方案**:
```csharp
using GameSpace.Models;  // ✅ 正確的命名空間

public async Task<Pet> GetPetByIdAsync(int petId)
{
    return await _context.Pets  // ✅ 使用複數 Pets
        .AsNoTracking()
        .FirstOrDefaultAsync(p => p.PetId == petId);
}
```

---

### High 級別問題（Services - 選錄重要項目）

#### 15. 缺少錯誤處理和日誌記錄
**影響檔案**: 大部分 Services
**嚴重程度**: 🟠 High

**問題描述**:
```csharp
public async Task<bool> HasManagerPermissionAsync(int managerId, string permission)
{
    try
    {
        // 業務邏輯
        return true;
    }
    catch
    {
        return false;  // ❌ 吞掉所有例外，沒有記錄
    }
}
```

**影響**:
- 無法追蹤和診斷問題
- 隱藏真實錯誤
- 除錯困難

**修復方案**:
```csharp
private readonly ILogger<MiniGamePermissionService> _logger;

public async Task<bool> HasManagerPermissionAsync(int managerId, string permission)
{
    try
    {
        // 業務邏輯
        return true;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex,
            "Error checking permission {Permission} for manager {ManagerId}",
            permission, managerId);
        throw;  // 或返回 false，視業務需求
    }
}
```

---

#### 16. ManagerService - 不安全的密碼雜湊
**檔案**: `ManagerService.cs` (Line 371-375)
**嚴重程度**: 🟠 High

**問題描述**:
```csharp
using (var sha256 = SHA256.Create())
{
    var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
    return Convert.ToBase64String(hashedBytes);
}
```

**影響**:
- SHA256 不適合用於密碼雜湊（無 salt、計算速度太快）
- 容易受到彩虹表攻擊
- 不符合現代安全標準

**修復方案**:
使用 ASP.NET Core Identity 的 PasswordHasher 或 BCrypt:
```csharp
using Microsoft.AspNetCore.Identity;

private readonly IPasswordHasher<ManagerDatum> _passwordHasher;

public string HashPassword(ManagerDatum manager, string password)
{
    return _passwordHasher.HashPassword(manager, password);
}

public bool VerifyPassword(ManagerDatum manager, string hashedPassword, string providedPassword)
{
    var result = _passwordHasher.VerifyHashedPassword(
        manager, hashedPassword, providedPassword);
    return result != PasswordVerificationResult.Failed;
}
```

---

### Services 層統計摘要

| 項目 | 數量 |
|-----|------|
| 總 Services | 30+ |
| 完整配對 (Interface + Implementation) | 15 |
| 僅 Interface | 8 |
| Critical 問題 | 4 |
| High 問題 | 4 |
| Medium 問題 | 4 |
| Low 問題 | 3 |

---

## 第三部分：Models/ViewModels 層稽核

### 稽核範圍
- **檔案數量**: 35 個
- **Entity 檔案**: 13 個（錯誤位置）
- **ViewModel 檔案**: 18 個
- **檢查項目**: 檔案組織、命名規範、Entity/ViewModel 區分

### Critical 問題清單（Models）

#### 17. ViewModels/AdminViewModels.cs - 命名空間錯誤
**檔案**: `ViewModels/AdminViewModels.cs`
**嚴重程度**: 🔴 Critical

**問題描述**:
```csharp
// ❌ 錯誤
namespace GameSpace.Areas.MiniGame.Models
{
    public class AdminUserIndexViewModel { ... }
}

// ✅ 正確
namespace GameSpace.Areas.MiniGame.Models.ViewModels
{
    public class AdminUserIndexViewModel { ... }
}
```

**影響**:
- 破壞命名空間層級結構
- 與其他 ViewModels 不一致
- 可能導致名稱衝突

---

#### 18. Entity 與 ViewModel 混合定義
**檔案**: `UserRight.cs`, `UserSalesInformation.cs`
**嚴重程度**: 🔴 Critical

**問題描述**:
```csharp
// UserRight.cs 包含：
public class UserRight { }  // Entity
public class UserRightQueryModel { }  // ViewModel
public class UserRightStatistics { }  // ViewModel
```

**影響**:
- 違反單一職責原則
- Entity 和 ViewModel 職責混淆
- 難以維護

**修復方案**:
1. `UserRight` Entity 保留在原檔案（或移至主 Models）
2. 創建 `ViewModels/UserRightViewModels.cs`:
```csharp
namespace GameSpace.Areas.MiniGame.Models.ViewModels
{
    public class UserRightQueryModel { }
    public class UserRightStatistics { }
}
```

---

### High 級別問題（Models）

#### 19. Entity 放錯位置
**影響檔案**: 13 個 Entity 檔案
**嚴重程度**: 🟠 High

**問題描述**:
以下 Entity 應該在 `GameSpace.Models` 而非 `GameSpace.Areas.MiniGame.Models`:

1. DailyGameLimit.cs
2. ManagerData.cs
3. ManagerRole.cs
4. ManagerRolePermission.cs
5. MiniGame.cs
6. Pet.cs
7. PetColorOption.cs
8. PetCostSettings.cs
9. PetLevelRewardSetting.cs
10. Sender.cs
11. UserIntroduce.cs
12. UserSignInStats.cs
13. WalletHistory.cs

**影響**:
- 架構混亂
- Entity 與 ViewModel 無法清楚區分
- 可能導致命名空間衝突
- 違反專案架構規範

**修復方案**:
需要檢查這些 Entity 是否已存在於 `GameSpace.Models`。如果已存在，刪除 MiniGame.Models 中的重複定義；如果不存在，移動這些檔案到主 Models 目錄。

---

### Models 層統計摘要

| 項目 | 數量 |
|-----|------|
| 總檔案數 | 35 |
| Entity 檔案（錯誤位置） | 13 |
| ViewModel 檔案 | 18 |
| 空檔案 | 1 |
| Critical 問題 | 2 |
| High 問題 | 2 |
| Medium 問題 | 3 |
| Low 問題 | 1 |

---

## 第四部分：整體架構建議

### 正確的架構層次

```
GameSpace/
├── Models/                           # 所有 EF Core Entity
│   ├── GameSpacedatabaseContext.cs
│   ├── User.cs
│   ├── Pet.cs
│   ├── MiniGame.cs
│   ├── ManagerData.cs
│   └── ...
│
└── Areas/
    └── MiniGame/
        ├── Controllers/              # MVC Controllers
        │   ├── MiniGameBaseController.cs
        │   ├── AdminHomeController.cs
        │   └── ...
        │
        ├── Services/                 # 業務邏輯層
        │   ├── Interfaces/           # 建議：分離到子目錄
        │   │   ├── IPetService.cs
        │   │   └── ...
        │   └── Implementations/      # 建議：分離到子目錄
        │       ├── PetService.cs
        │       └── ...
        │
        ├── Models/                   # ViewModels, DTOs, ReadModels
        │   ├── ViewModels/           # 所有 ViewModels
        │   │   ├── AdminViewModels.cs
        │   │   ├── ManagerViewModels.cs
        │   │   └── ...
        │   ├── Settings/             # Settings ViewModels
        │   │   └── ...
        │   ├── ReadModels.cs         # 唯讀模型
        │   ├── StatisticsModels.cs
        │   └── ValidationResult.cs
        │
        ├── Filters/                  # 認證授權 Filters
        │   └── ...
        │
        ├── config/                   # 啟動配置
        │   ├── ServiceExtensions.cs
        │   └── StartupExtensions.cs
        │
        └── Views/                    # Razor Views
            └── ...
```

### 命名空間規範

```csharp
// Entity - 全域共用
namespace GameSpace.Models;

// ViewModel - MiniGame 專用
namespace GameSpace.Areas.MiniGame.Models.ViewModels;

// Service Interface
namespace GameSpace.Areas.MiniGame.Services;

// Service Implementation
namespace GameSpace.Areas.MiniGame.Services;

// Controller
namespace GameSpace.Areas.MiniGame.Controllers;
```

---

## 第五部分：立即修復計劃

### 階段 1: Critical 問題修復（今天內完成）

#### 1.1 Controllers 依賴注入修復（預計 30 分鐘）
- [ ] AdminPetController: 加入 GameSpacedatabaseContext 注入
- [ ] AdminManagerController: 移除重複 _context，調用 base(context)
- [ ] AdminMiniGameController: 移除重複 _context，調用 base(context)
- [ ] AdminEVoucherController: 移除重複 _context，調用 base(context)

#### 1.2 DbSet 名稱修正（預計 20 分鐘）
- [ ] AdminCouponController: 所有 `Coupon` → `Coupons`, `CouponType` → `CouponTypes`
- [ ] AdminWalletController: 加入 AuthenticationSchemes
- [ ] 檢查其他 Controllers 是否有類似問題

#### 1.3 Services DbContext 修復（預計 30 分鐘）
- [ ] MiniGameAdminAuthService: 加入 `: IMiniGameAdminAuthService`
- [ ] UserWalletService: `MiniGameDbContext` → `GameSpacedatabaseContext`
- [ ] PetService: 修正命名空間和 DbSet 名稱
- [ ] MiniGameService: 改用 EF Core（較複雜，可能需要更多時間）

#### 1.4 Models 命名空間修復（預計 15 分鐘）
- [ ] AdminViewModels.cs: 修正命名空間為 `GameSpace.Areas.MiniGame.Models.ViewModels`
- [ ] UserRight.cs: 分離 ViewModels 到獨立檔案
- [ ] UserSalesInformation.cs: 分離 ViewModels 到獨立檔案

**階段 1 預計總時間**: 1.5 - 2 小時

---

### 階段 2: High 問題修復（本週完成）

#### 2.1 Entity 位置重組（預計 2 小時）
1. 檢查主 Models 目錄中是否已有這些 Entity
2. 如有重複，刪除 MiniGame.Models 中的版本
3. 如無，移動 Entity 到主 Models 目錄
4. 更新所有引用的命名空間

#### 2.2 權限檢查完善（預計 3 小時）
為所有敏感操作添加細粒度權限檢查：
- AdminPetController: Create, Edit, Delete
- AdminWalletController: AdjustPoints, IssueCoupon
- AdminMiniGameController: EditRules, DeleteRecord
- 等等...

#### 2.3 日誌記錄加入（預計 2 小時）
為所有 Services 加入 ILogger:
- 注入 ILogger<TService>
- 在關鍵操作記錄日誌
- 在錯誤處理記錄詳細資訊

#### 2.4 ViewModel 使用規範化（預計 3 小時）
- AdminUserController: Entity → ViewModel
- 其他直接使用 Entity 的地方
- 創建缺少的 ViewModels

**階段 2 預計總時間**: 10 小時

---

### 階段 3: Medium 問題修復（本月完成）

- 統一錯誤處理模式
- 加入 Transaction 管理
- 替換不安全的密碼雜湊
- 將業務規則移至設定
- 統一分頁參數
- 等等...

**階段 3 預計總時間**: 20 小時

---

## 第六部分：測試與驗證計劃

### 編譯驗證
```powershell
dotnet build GameSpace/GameSpace.sln
```
預期：0 errors, 0 warnings

### 功能測試清單

#### 登入與認證
- [ ] 管理員登入（AdminCookie）
- [ ] 權限檢查（不同角色）
- [ ] 登出功能

#### 會員錢包系統
- [ ] 查詢會員點數
- [ ] 發放會員點數
- [ ] 查詢會員優惠券
- [ ] 發放優惠券
- [ ] 查詢電子禮券
- [ ] 發放/調整電子禮券
- [ ] 查看收支明細

#### 會員簽到系統
- [ ] 查看簽到記錄
- [ ] 簽到規則設定

#### 寵物系統
- [ ] 查看寵物清單
- [ ] 寵物詳細資料
- [ ] 手動調整寵物資料
- [ ] 寵物規則設定
- [ ] 換色記錄查詢
- [ ] 換背景記錄查詢

#### 小遊戲系統
- [ ] 查看遊戲記錄
- [ ] 遊戲規則設定
- [ ] 每日次數限制設定

#### 管理員系統
- [ ] 管理員清單
- [ ] 管理員權限設定
- [ ] 角色管理

---

## 第七部分：風險評估

### 高風險項目

1. **Entity 移動**: 可能影響現有功能，需要全面測試
2. **DbContext 修改**: 錯誤可能導致資料存取失敗
3. **認證授權變更**: 可能影響登入和權限檢查

### 緩解措施

1. **備份**: 在修復前建立完整備份
2. **分階段**: 不要一次修改太多，每階段後驗證
3. **測試**: 每次修改後執行完整測試
4. **版本控制**: 每階段提交 Git commit，方便回滾

---

## 第八部分：長期改進建議

### 架構改進
1. 引入 AutoMapper 處理 Entity ↔ ViewModel 轉換
2. 建立統一的 Result<T> 模式處理成功/失敗
3. 使用 MediatR 實作 CQRS 模式
4. 加入 Repository Pattern（可選）

### 開發流程改進
1. 建立 Code Review Checklist
2. 使用 Static Analysis Tools（Roslyn Analyzers, SonarQube）
3. 建立 Coding Standards 文件
4. 加入 Pre-commit Git Hooks 檢查程式碼品質

### 測試改進
1. 加入單元測試（xUnit）
2. 加入整合測試
3. 建立自動化測試流程（CI/CD）

### 文件改進
1. API 文檔（Swagger/OpenAPI）
2. 架構圖和流程圖
3. 資料庫 Schema 文檔

---

## 附錄 A: 完整問題清單

### Critical 問題（14 個）
1. AdminPetController - 缺少 Context 注入
2. AdminManagerController - 重複 _context 宣告
3. AdminMiniGameController - 重複 _context 宣告
4. AdminEVoucherController - 重複 _context 宣告
5. AdminWalletController - 缺少 AuthenticationSchemes
6. AdminCouponController - 錯誤的 DbSet 名稱
7. AdminCouponController - 錯誤的 ViewModel
8. AdminHomeController - ViewModel 定義在 Controller 內
9. MiniGameAdminAuthService - 未實作介面
10. UserWalletService - 使用不存在的 DbContext
11. MiniGameService - 使用 ADO.NET 而非 EF Core
12. CouponService - 使用 ADO.NET 而非 EF Core
13. SignInStatsService - 使用 ADO.NET 而非 EF Core
14. PetService - 實體類別命名空間錯誤

（完整清單請見各章節詳細說明）

---

## 結論

本次稽核發現的問題雖然數量較多，但大部分都是可以系統化修復的架構和規範問題。最關鍵的是 **14 個 Critical 問題必須立即修復**，否則系統無法正常運行。

建議優先處理 Critical 和 High 級別問題，確保系統的基本功能正常運作，然後逐步改善 Medium 和 Low 級別問題，提升程式碼品質和可維護性。

透過本次稽核和後續的修復工作，MiniGame Area 將能夠達到：
- ✅ 0 編譯錯誤
- ✅ 架構清晰、職責分明
- ✅ 安全性符合標準
- ✅ 可維護性良好
- ✅ 所有功能正常運作

---

**報告結束**

