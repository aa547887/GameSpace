# GameSpace Area 註冊架構完整說明

> 本文件說明 GameSpace 專案中各個 Area 的註冊方式、DbContext 使用方式，以及路由設定模式。
> 
> 最後更新：2025-10-03

---

## 📋 目錄
1. [架構總覽](#架構總覽)
2. [資料庫連接架構](#資料庫連接架構)
3. [Area 註冊模式](#area-註冊模式)
4. [控制器實作模式](#控制器實作模式)
5. [服務註冊模式](#服務註冊模式)
6. [路由設定](#路由設定)
7. [最佳實踐總結](#最佳實踐總結)

---

## 架構總覽

### 多 DbContext 架構
GameSpace 採用 **雙 DbContext 架構**，職責清楚分離：

```
ApplicationDbContext (DefaultConnection)
└── ASP.NET Identity 使用者認證
    └── aspnet-GameSpace-38e0b594-8684-40b2-b330-7fb94b733c73

GameSpacedatabaseContext (GameSpace)  ← 主要業務資料庫
└── 所有業務領域 (Users, Pets, MiniGames, Wallets, Orders 等)
    └── GameSpacedatabase
```

### Area 結構
```
Areas/
├── Forum/              # 論壇管理 (簡單 Area)
├── Identity/           # ASP.NET Identity 頁面
├── MemberManagement/   # 會員管理 (簡單 Area)
├── MiniGame/          # ⭐ 小遊戲後台系統 (複雜 Area - 需特殊註冊)
├── OnlineStore/       # 電商功能 (簡單 Area)
└── social_hub/        # ⭐ 社群中心 (複雜 Area - 需特殊註冊)
```

---

## 資料庫連接架構

### Program.cs 中的 DbContext 註冊

```csharp
// ========== DbContexts 註冊 (Program.cs 第 50-53 行) ==========

// 1. Identity DbContext (ASP.NET Identity 專用)
builder.Services.AddDbContext<ApplicationDbContext>(opt => 
    opt.UseSqlServer(identityConn));

// 2. 業務 DbContext (所有 Area 共享)
builder.Services.AddDbContext<GameSpacedatabaseContext>(opt => 
    opt.UseSqlServer(gameSpaceConn));
```

### 連接字串配置 (appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=aspnet-GameSpace-...;Integrated Security=True;",
    "GameSpace": "Server=DESKTOP-8HQIS1S\\SQLEXPRESS;Database=GameSpacedatabase;Integrated Security=True;"
  }
}
```

### ⚠️ 重要原則

| 原則 | 說明 |
|-----|------|
| **共享 DbContext** | 所有 Area 使用同一個 `GameSpacedatabaseContext` 實例 |
| **統一註冊** | DbContext 只在 `Program.cs` 註冊一次 |
| **不重複註冊** | Area 的 `ServiceExtensions.cs` 不再註冊 DbContext |
| **依賴注入** | 控制器和服務通過建構函式注入 DbContext |

---

## Area 註冊模式

### 模式一：簡單 Area（無需特殊註冊）

**適用於**: Forum, OnlineStore, MemberManagement

#### 特徵
- 業務邏輯簡單
- 服務數量少（0-5 個）
- 不需要複雜的依賴注入配置

#### 實作方式
```csharp
// ✅ 不需要在 Program.cs 中註冊任何東西

// 控制器只需要 [Area] 屬性
[Area("Forum")]
public class HomeController : Controller
{
    // 可選：注入共享的 DbContext
    private readonly GameSpacedatabaseContext _context;
    
    public HomeController(GameSpacedatabaseContext context)
    {
        _context = context;
    }
}
```

#### Program.cs 配置
```csharp
// 不需要任何 Area 特定的註冊
// 路由自動處理 (見後面「路由設定」章節)
```

---

### 模式二：複雜 Area（需要特殊註冊）

**適用於**: MiniGame, social_hub

#### 特徵
- 業務邏輯複雜
- 大量服務需要註冊（10+ 個）
- 需要統一管理依賴注入配置

---

### 案例 A：MiniGame Area

#### 目錄結構
```
Areas/MiniGame/
├── Controllers/          # 15+ 控制器
├── Services/            # 32+ 服務
│   ├── Interface/       # 服務介面 (I*.cs)
│   └── Implementation/  # 服務實作
├── Models/              # ViewModels & DTOs
├── config/              # ⭐ 配置目錄
│   ├── ServiceExtensions.cs     # ⭐ 所有服務註冊
│   └── StartupExtensions.cs
└── Views/               # Razor 視圖
```

#### 1. ServiceExtensions.cs（集中註冊所有服務）

```csharp
// Areas/MiniGame/config/ServiceExtensions.cs

namespace GameSpace.Areas.MiniGame.config
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddMiniGameServices(
            this IServiceCollection services, 
            IConfiguration configuration)
        {
            // ⚠️ 不註冊 DbContext（已在 Program.cs 註冊）
            // 註釋說明：使用共享的 GameSpacedatabaseContext
            
            // 註冊核心管理服務
            services.AddScoped<IMiniGameAdminService, MiniGameAdminService>();
            services.AddScoped<IMiniGamePermissionService, MiniGamePermissionService>();
            services.AddScoped<IMiniGameAdminAuthService, MiniGameAdminAuthService>();
            services.AddScoped<IMiniGameAdminGate, MiniGameAdminGate>();

            // 註冊業務服務
            services.AddScoped<IUserWalletService, UserWalletService>();
            services.AddScoped<ICouponService, CouponService>();
            services.AddScoped<IEVoucherService, EVoucherService>();
            services.AddScoped<ISignInStatsService, SignInStatsService>();
            services.AddScoped<IMiniGameService, MiniGameService>();
            services.AddScoped<IPetService, PetService>();
            services.AddScoped<IWalletService, WalletService>();
            services.AddScoped<ISignInService, SignInService>();
            services.AddScoped<IDiagnosticsService, DiagnosticsService>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IManagerService, ManagerService>();
            services.AddScoped<ICouponTypeService, CouponTypeService>();
            services.AddScoped<IEVoucherTypeService, EVoucherTypeService>();
            services.AddScoped<IPetRulesService, PetRulesService>();
            services.AddScoped<IGameRulesService, GameRulesService>();
            
            // 註冊寵物相關服務
            services.AddScoped<IPetColorOptionService, PetColorOptionService>();
            services.AddScoped<IPetBackgroundOptionService, PetBackgroundOptionService>();
            services.AddScoped<IPetLevelExperienceSettingService, PetLevelExperienceSettingService>();
            services.AddScoped<IPetLevelRewardSettingService, PetLevelRewardSettingService>();
            services.AddScoped<IPetLevelUpRuleValidationService, PetLevelUpRuleValidationService>();
            services.AddScoped<IDailyGameLimitService, DailyGameLimitService>();
            services.AddScoped<IPetSkinColorCostSettingService, PetSkinColorCostSettingService>();
            services.AddScoped<IPetBackgroundCostSettingService, PetBackgroundCostSettingService>();
            services.AddScoped<IPetColorChangeSettingsService, PetColorChangeSettingsService>();
            services.AddScoped<IPetLevelUpRuleService, PetLevelUpRuleService>();
            services.AddScoped<IPetBackgroundChangeSettingsService, PetBackgroundChangeSettingsService>();
            services.AddScoped<IPointsSettingsStatisticsService, PointsSettingsStatisticsService>();

            return services;
        }
    }
}
```

#### 2. Program.cs（一行註冊）

```csharp
// Program.cs 第 56 行

using GameSpace.Areas.MiniGame.config;

// ... 其他配置 ...

// ⭐ MiniGame Area 特殊註冊（一行搞定）
builder.Services.AddMiniGameServices(builder.Configuration);

// ... 其他配置 ...
```

#### 3. 控制器實作

```csharp
// Areas/MiniGame/Controllers/MiniGameBaseController.cs

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Authorize(AuthenticationSchemes = "AdminCookie", Policy = "AdminOnly")]
    public abstract class MiniGameBaseController : Controller
    {
        // ⭐ 注入共享的 DbContext
        protected readonly GameSpacedatabaseContext _context;
        protected readonly IMiniGameAdminService _adminService;
        protected readonly IMiniGamePermissionService _permissionService;

        // 建構函式注入
        protected MiniGameBaseController(GameSpacedatabaseContext context)
        {
            _context = context;
        }

        protected MiniGameBaseController(
            GameSpacedatabaseContext context, 
            IMiniGameAdminService adminService) : this(context)
        {
            _adminService = adminService;
        }

        protected MiniGameBaseController(
            GameSpacedatabaseContext context, 
            IMiniGameAdminService adminService, 
            IMiniGamePermissionService permissionService) : this(context, adminService)
        {
            _permissionService = permissionService;
        }
        
        // 共用方法...
    }
}
```

---

### 案例 B：social_hub Area

#### 特殊之處
除了服務註冊，還需要註冊 **SignalR Hub**。

#### Program.cs 配置

```csharp
// ========== 1. 註冊 SignalR ==========
builder.Services.AddSignalR();  // 第 73 行

// ========== 2. 註冊 social_hub 服務 ==========
builder.Services.AddMemoryCache();  // 第 76 行

// 過濾器配置
builder.Services.Configure<GameSpace.Areas.social_hub.Services.MuteFilterOptions>(o =>
{
    o.MaskStyle = GameSpace.Areas.social_hub.Services.MaskStyle.Asterisks;
    o.FixedLabel = "【封鎖】";
    // ... 其他配置
});

// 時鐘服務
builder.Services.AddSingleton<IAppClock>(sp => new AppClock(TimeZones.Taipei));

// 穢語過濾 / 通知
builder.Services.AddScoped<IMuteFilterAlias, MuteFilterAlias>();
builder.Services.AddScoped<INotificationServiceAlias, NotificationServiceAlias>();

// HttpContext 存取
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserContextReader, UserContextReader>();

// 權限服務
builder.Services.AddScoped<IManagerPermissionService, ManagerPermissionServiceAlias>();

// ========== 3. 註冊 SignalR Hub ==========
// 在 app.MapHub 中註冊
app.MapHub<ChatHub>("/social_hub/chatHub", opts =>
{
    opts.Transports =
        HttpTransportType.WebSockets |
        HttpTransportType.ServerSentEvents |
        HttpTransportType.LongPolling;
});
```

---

## 控制器實作模式

### 標準模式（所有 Area 通用）

```csharp
using Microsoft.AspNetCore.Mvc;
using GameSpace.Models;  // 引用共享的 DbContext

namespace GameSpace.Areas.{AreaName}.Controllers
{
    // ⭐ 必須：[Area] 屬性
    [Area("AreaName")]
    
    // ❌ 不需要：[Route] 屬性（路由自動處理）
    
    public class HomeController : Controller
    {
        // ⭐ 注入共享的 DbContext
        private readonly GameSpacedatabaseContext _context;
        
        public HomeController(GameSpacedatabaseContext context)
        {
            _context = context;
        }
        
        public IActionResult Index()
        {
            // 使用 _context 存取資料庫
            var data = _context.Users.ToList();
            return View();
        }
    }
}
```

### ⚠️ 常見錯誤

| 錯誤 | 說明 | 正確做法 |
|-----|------|---------|
| ❌ 註冊自己的 DbContext | 在 Area 的 ServiceExtensions 中註冊 DbContext | ✅ 使用共享的 GameSpacedatabaseContext |
| ❌ 使用 `[Route("AreaName/[controller]")]` | 顯式設定路由 | ✅ 只使用 `[Area("AreaName")]` |
| ❌ 創建 AreaDbContext | 為 Area 創建專屬 DbContext | ✅ 注入共享的 GameSpacedatabaseContext |

---

## 服務註冊模式

### 簡單 Area：直接在 Program.cs 註冊

```csharp
// Program.cs

// 如果只有 1-2 個服務，直接註冊
builder.Services.AddScoped<IForumService, ForumService>();
```

### 複雜 Area：使用 ServiceExtensions 統一管理

```csharp
// Areas/{AreaName}/config/ServiceExtensions.cs

public static class ServiceExtensions
{
    public static IServiceCollection Add{AreaName}Services(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // ⚠️ 不註冊 DbContext
        
        // 註冊所有服務
        services.AddScoped<IService1, Service1>();
        services.AddScoped<IService2, Service2>();
        // ...
        
        return services;
    }
}

// Program.cs
builder.Services.Add{AreaName}Services(builder.Configuration);
```

### 服務生命週期選擇

| 生命週期 | 使用場景 | 範例 |
|---------|---------|------|
| `AddScoped` | 大部分業務服務 | UserService, WalletService |
| `AddSingleton` | 無狀態工具類、配置 | IAppClock, IConfiguration |
| `AddTransient` | 輕量級、每次都建新的 | IEmailSender, IValidator |

---

## 路由設定

### Program.cs 中的路由配置

```csharp
// ========== 路由設定 (Program.cs 第 227-234 行) ==========

// 1. API 路由
app.MapControllers();

// 2. Area 路由（⭐ 這行處理所有 Area）
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

// 3. 預設路由
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// 4. Razor Pages 路由
app.MapRazorPages();
```

### 路由解析範例

| URL | Area | Controller | Action | 處理方式 |
|-----|------|-----------|--------|---------|
| `/MiniGame/AdminHome/Index` | MiniGame | AdminHome | Index | Area 路由 |
| `/Forum/Home/Index` | Forum | Home | Index | Area 路由 |
| `/social_hub/Chat/Room` | social_hub | Chat | Room | Area 路由 |
| `/Home/Index` | - | Home | Index | 預設路由 |

### ⚠️ 路由注意事項

1. **不需要 `[Route]` 屬性**  
   ASP.NET Core 會自動根據 `[Area]` 屬性處理路由

2. **Area 路由優先**  
   如果有 Area 屬性，會先匹配 Area 路由

3. **大小寫不敏感**  
   `/MiniGame/AdminHome` 和 `/minigame/adminhome` 是等價的

---

## 最佳實踐總結

### ✅ 推薦做法

| 項目 | 推薦做法 |
|-----|---------|
| **DbContext** | 所有 Area 共享一個 `GameSpacedatabaseContext` |
| **DbContext 註冊** | 只在 `Program.cs` 註冊一次 |
| **Area 註冊** | 簡單 Area 不需註冊；複雜 Area 使用 `ServiceExtensions` |
| **控制器標記** | 只使用 `[Area("AreaName")]`，不使用 `[Route]` |
| **服務註冊** | 使用 `AddScoped` 作為預設生命週期 |
| **命名空間** | 使用 `GameSpace.Areas.{AreaName}.*` |

### ❌ 避免做法

| 項目 | 避免做法 | 原因 |
|-----|---------|------|
| **重複註冊 DbContext** | 在多處註冊 DbContext | 導致多個實例，違反單一職責 |
| **為 Area 創建專屬 DbContext** | 建立 ForumDbContext, MiniGameDbContext | 增加複雜度，難以維護 |
| **顯式 Route 屬性** | `[Route("MiniGame/[controller]")]` | 破壞 ASP.NET Core 的路由慣例 |
| **在控制器中創建 DbContext** | `new GameSpacedatabaseContext()` | 無法使用依賴注入，難以測試 |

---

## 架構優勢

### 1. 清晰的職責分離
- **ApplicationDbContext**: 只負責身份驗證
- **GameSpacedatabaseContext**: 負責所有業務邏輯

### 2. 統一的資料存取
- 所有 Area 使用相同的 DbContext
- 避免資料不一致問題
- 容易進行跨 Area 的資料查詢

### 3. 靈活的服務註冊
- 簡單 Area：零配置，自動路由
- 複雜 Area：集中管理，易於維護

### 4. 符合 ASP.NET Core 最佳實踐
- 使用內建的 Area 機制
- 遵循依賴注入原則
- 保持配置簡潔明瞭

---

## 實際案例對照表

| Area | 複雜度 | 服務數量 | 特殊需求 | 註冊方式 |
|------|--------|---------|---------|---------|
| **Forum** | 簡單 | 0-2 | 無 | 無需特殊註冊 |
| **OnlineStore** | 簡單 | 0-3 | 無 | 無需特殊註冊 |
| **MemberManagement** | 簡單 | 0-2 | 無 | 無需特殊註冊 |
| **MiniGame** | 複雜 | 32+ | 權限、驗證 | ServiceExtensions |
| **social_hub** | 複雜 | 8+ | SignalR Hub | Program.cs 直接註冊 |

---

## 參考資源

### 相關文件
- [資料庫連線與讀取流程.md](./資料庫連線與讀取流程.md) - 資料庫連線說明
- [MiniGame_Area_完整描述文件.md](./MiniGame_Area_完整描述文件.md) - MiniGame Area 詳細規格
- [管理者權限相關描述.txt](./管理者權限相關描述.txt) - 權限系統說明

### 程式碼位置
- `Program.cs` - 第 50-56, 73-103, 227-244 行
- `Areas/MiniGame/config/ServiceExtensions.cs` - MiniGame 服務註冊
- `Areas/MiniGame/Controllers/MiniGameBaseController.cs` - 基底控制器範例

---

## 版本記錄

| 版本 | 日期 | 說明 |
|-----|------|------|
| 1.0 | 2025-10-03 | 初始版本，基於實際專案架構整理 |

---

**最後更新**: 2025-10-03  
**維護者**: Claude Code  
**狀態**: ✅ 已驗證並與實際程式碼一致

