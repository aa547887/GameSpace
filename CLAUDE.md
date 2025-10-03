# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

GameSpace (branded as "GamiPort") is an ASP.NET Core 8.0 MVC web application built with Areas architecture. It's a gaming community platform combining forums, e-commerce, mini-games, social features, and user management.

**Technology Stack**: .NET 8.0, Entity Framework Core 8.0, SQL Server, ASP.NET Core Identity, SignalR, Bootstrap 5

## Build & Run Commands

```bash
# Navigate to project directory
cd GameSpace/GameSpace

# Restore packages
dotnet restore

# Build project
dotnet build

# Run application (development)
dotnet run

# Run with specific profile
dotnet run --launch-profile https

# Apply Identity migrations
dotnet ef database update --context ApplicationDbContext
```

**Default URLs**:
- HTTPS: https://localhost:7042
- HTTP: http://localhost:5211

**Note**: Main database is database-first. Schema scripts are in `GameSpace/schema/`.

## Architecture Overview

### Areas-Based Organization

The application uses **ASP.NET Core Areas** to organize features into modules:

1. **MiniGame** - Largest area with wallet system, sign-in rewards, pet management, and mini-games
2. **social_hub** - SignalR-based real-time chat, notifications, content filtering
3. **OnlineStore** - E-commerce with products, orders, suppliers
4. **Forum** - Gaming forums, threads, posts, reactions
5. **MemberManagement** - User and manager administration
6. **Identity** - ASP.NET Core Identity scaffolded pages

### Dual Authentication System

**Important**: This application has TWO separate authentication systems:

1. **ASP.NET Core Identity** - For regular users
   - Uses `ApplicationDbContext`
   - Standard cookie authentication
   - Email confirmation required

2. **Custom Admin Authentication** - For managers/staff
   - Scheme: `"AdminCookie"`
   - Cookie name: `GameSpace.Admin`
   - Login path: `/Login`
   - 4-hour sliding expiration
   - Uses `GameSpacedatabaseContext`

**Authorization Policies**:
- `AdminOnly` - Requires AdminCookie
- `CanManageShopping`, `CanAdmin`, `CanMessage`, `CanUserStatus`, `CanPet`, `CanCS` - Fine-grained permissions via claims

### Dual Database Context

**Critical**: Two separate DbContexts exist:

1. **ApplicationDbContext** (`Data/ApplicationDbContext.cs`)
   - Handles ASP.NET Core Identity (users, roles, claims)
   - Connection string: `DefaultConnection`
   - Code-first with migrations

2. **GameSpacedatabaseContext** (`Models/GameSpacedatabaseContext.cs`)
   - Main application database (84+ entity models)
   - Connection string: `GameSpacedatabaseContext`
   - Database-first approach (scaffolded from existing DB)
   - 100,000+ line auto-generated file
   - Use `GameSpacedatabaseContext.Partial.cs` for custom extensions

**Always verify which context a service/controller should use.**

### Directory Structure

```
GameSpace/
├── Areas/
│   ├── MiniGame/         - Controllers (22), Services (30+), extensive admin features
│   ├── social_hub/       - ChatHub (SignalR), MuteFilter, NotificationService
│   ├── OnlineStore/      - ProductInfoes, OrderInfoes, Suppliers
│   ├── Forum/            - Posts, Threads, Metrics, Reports
│   ├── MemberManagement/ - User/Manager admin
│   └── Identity/         - ASP.NET Identity pages
├── Controllers/          - Root controllers (Home, Login, Health)
├── Data/                 - ApplicationDbContext & Identity migrations
├── Infrastructure/       - Cross-cutting concerns
│   ├── Login/           - Unified ILoginIdentity abstraction
│   └── Time/            - IAppClock (Taipei timezone handling)
├── Models/               - 84+ entity models (GameSpacedatabaseContext)
├── Partials/             - DbContext partial extensions
├── Views/                - 270+ Razor views
├── wwwroot/              - Static assets (CSS, JS, images)
├── schema/               - Database scripts, seed data, documentation
└── Program.cs            - DI configuration, middleware pipeline
```

## Key Patterns & Conventions

### Base Controller Pattern

Areas use base controllers for shared functionality:

**MiniGame Example** (`Areas/MiniGame/Controllers/MiniGameBaseController.cs`):
```csharp
[Area("MiniGame")]
[Authorize(AuthenticationSchemes = "AdminCookie", Policy = "AdminOnly")]
public class MiniGameBaseController : Controller
{
    protected readonly GameSpacedatabaseContext _context;
    protected readonly IMiniGameAdminService _adminService;
    protected readonly IManagerPermissionService _permissionService;

    protected async Task<int> GetCurrentManagerId();
    protected async Task<ManagerDatum?> GetCurrentManagerAsync();
    protected async Task<bool> HasPermissionAsync(string gate);
}
```

**When creating controllers**: Inherit from area base controller and call `base(context, adminService, permissionService)` constructor.

### Service Layer Pattern

Services follow interface-based dependency injection:

```csharp
// Interface
public interface IUserWalletService
{
    Task<UserWallet?> GetWalletAsync(int userId);
    Task<bool> AddPointsAsync(int userId, int points, string reason);
}

// Implementation
public class UserWalletService : IUserWalletService
{
    private readonly GameSpacedatabaseContext _context;

    public UserWalletService(GameSpacedatabaseContext context)
    {
        _context = context;
    }
}
```

**Registration**: Services registered in `Program.cs` or area-specific `ServiceExtensions.cs` (MiniGame area).

### Permission System

**MiniGame Area** uses fine-grained permissions:
- Stored in `ManagerRolePermission` table
- Loaded as claims during admin login
- Checked via `IManagerPermissionService.HasPermissionAsync(managerId, gate)`
- Available "gates": `Shopping`, `Admin`, `Message`, `UserStat`, `Pet`, `CS`

**Usage in controllers**:
```csharp
if (!await HasPermissionAsync("Pet"))
{
    return Forbid();
}
```

### Idempotency Filter

**MiniGame Area** has `IdempotencyFilter` for POST/PUT/PATCH/DELETE:
- Checks `X-Idempotency-Key` header
- 60-second deduplication window
- Memory cache-based
- Applied globally to MiniGame controllers

### SignalR Chat (social_hub)

**ChatHub** (`/social_hub/chatHub`):
- Real-time direct messaging
- Methods: `SendDirect`, `MarkAsRead`, `GetUnreadCount`
- Client events: `ReceiveDirect`, `ReadReceipt`, `UnreadUpdate`, `Error`
- **MuteFilter**: Automatically masks profanity in messages

**Authentication**: Supports both regular users and admin cookies via `UserContextReader`.

### Time Zone Handling

**Critical**: Application uses **Taipei timezone** (UTC+8):
- `IAppClock` service provides timezone-aware DateTime
- Use `_appClock.Now` instead of `DateTime.Now`
- View helpers: `@Html.TaipeiTime(datetime)`, `@Html.TaipeiTimeShort(datetime)`

## Database Schema

### Key Entity Models (in `/Models/`)

**Users**: `User`, `UserWallet`, `UserToken`, `UserIntroduce`, `UserRight`
**Managers**: `ManagerDatum`, `ManagerRole`, `ManagerRolePermission`
**MiniGame**: `MiniGame`, `Pet`, `DailyGameLimit`, `SignInRecord`
**Wallet**: `Coupon`, `CouponType`, `Evoucher`, `EvoucherType`, `WalletHistory`
**Social**: `DmConversation`, `DmMessage`, `Group`, `GroupChat`, `Notification`
**Forum**: `Forum`, `Thread`, `ThreadPost`, `Post`, `Reaction`
**Store**: `ProductInfo`, `OrderInfo`, `OrderItem`, `PaymentTransaction`
**Support**: `SupportTicket`, `SupportTicketMessage`, `CsAgent`

### DbSet Naming Convention

**Inconsistent**: Some tables use singular (`User`), others plural (`ProductInfoes`, `OrderInfoes`).
**Important**: Check `GameSpacedatabaseContext.cs` for exact DbSet names before querying.

### Seed Data

Manager test accounts (ID 30000001-30000010) with various permissions in `/schema/` SQL scripts.

**Primary test account**:
- Username: `zhang_zhiming_01`
- Password: `AdminPass001@`
- Role: Full permissions

## Common Coding Patterns

### Controller Method Pattern

```csharp
[HttpGet]
public async Task<IActionResult> Index()
{
    if (!await HasPermissionAsync("RequiredGate"))
        return Forbid();

    var data = await _service.GetDataAsync();
    return View(data);
}

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(CreateViewModel model)
{
    if (!ModelState.IsValid)
        return View(model);

    await _service.CreateAsync(model);
    return RedirectToAction(nameof(Index));
}
```

### Service Method Pattern

```csharp
public async Task<Result> DoSomethingAsync(int id)
{
    var entity = await _context.Entities
        .FirstOrDefaultAsync(e => e.Id == id);

    if (entity == null)
        return Result.NotFound();

    // Business logic
    entity.UpdatedAt = _appClock.Now;
    await _context.SaveChangesAsync();

    return Result.Success();
}
```

### ViewModel Separation

**Always** use ViewModels for:
- Form input (`CreateViewModel`, `EditViewModel`)
- Display data (`IndexRowViewModel`, `DetailViewModel`)
- API responses (`ApiResultViewModel`)

**Never** bind entities directly to views or accept entities as form input.

## Known Issues & Technical Debt

From `/Areas/MiniGame/docs/COMPREHENSIVE_AUDIT_REPORT.md`:

### Critical Issues

1. **Dependency Injection Errors** (8 controllers)
   - Some controllers missing `base(context)` constructor calls
   - Results in null `_context` references

2. **DbContext Confusion** (4 services)
   - Some services using wrong context (ApplicationDbContext vs GameSpacedatabaseContext)

3. **Entity/ViewModel Placement**
   - Some entities placed in `/ViewModels/` directory
   - Some ViewModels in `/Models/` directory

### Common Anti-Patterns to Avoid

- **Don't**: Use `ApplicationDbContext` for business entities (use `GameSpacedatabaseContext`)
- **Don't**: Forget to call base class constructors in controllers
- **Don't**: Use `DateTime.Now` (use `IAppClock.Now`)
- **Don't**: Skip permission checks in admin controllers
- **Don't**: Bind entities directly to forms

## MiniGame Area Deep Dive

The most complex area with 4 major subsystems:

### 1. Wallet System
- User points balance tracking
- Exchange points for coupons/vouchers
- Transaction history
- Admin: Issue points, manage coupon/voucher types

### 2. Sign-In System
- Daily check-in calendar
- Rewards: points, pet XP, coupons
- Admin: Configure reward rules

### 3. Pet System
- Pet naming, interactions (feed, bath, play, sleep)
- Skin color changes (costs points)
- Background customization
- Admin: Pet rules, level-up rules, individual pet management

### 4. Mini-Game System
- Adventure game (3 plays/day default)
- Win/lose/abort outcomes with rewards
- Admin: Game rules configuration

**Service Registration**: All MiniGame services registered via `Areas/MiniGame/ServiceExtensions.cs` (30+ services).

## Important Configuration Files

- **appsettings.json / appsettings.Development.json**: Connection strings, CORS, session config
- **libman.json**: Client-side libraries (Font Awesome, Bootstrap)
- **Program.cs**: DI container setup, middleware pipeline, authentication schemes
- **schema/**: Database documentation, seed data scripts

## Documentation Locations

- `/schema/MiniGame_area功能彙整.txt` - MiniGame feature overview (Chinese)
- `/schema/專案規格敘述1.txt`, `專案規格敘述2.txt` - Project specifications
- `/schema/管理者權限相關描述.txt` - Manager permission system docs
- `/Areas/MiniGame/docs/` - Code audit reports, fix progress tracking
- `/Areas/MiniGame/Views/AdminPet/README_NewFeatures.md` - Pet feature docs

## Development Workflow

1. **Starting Development**: Ensure SQL Server is running and databases exist
2. **Adding Features**: Create service interface, implementation, register in DI, create controller/views
3. **Database Changes**: For main DB, update schema scripts in `/schema/` (database-first)
4. **Identity Changes**: Create EF migration for `ApplicationDbContext`
5. **Testing**: Use provided test manager accounts, verify permissions work correctly
6. **Committing**: Follow existing patterns, keep ViewModels separate from entities

## Security Considerations

- **Anti-Forgery**: Global filter validates tokens on all POST/PUT/PATCH/DELETE
- **Authorization**: Always check permissions before admin actions
- **Authentication**: Respect dual auth system, don't mix Identity and Admin cookies
- **Idempotency**: Mutation endpoints in MiniGame area use idempotency keys
- **Content Filtering**: social_hub uses MuteFilter for user-generated content

## Quick Reference

**File Counts**: 56 controllers, 66 services, 84 models, 270 views
**Main Solution**: `GameSpace/GameSpace.sln`
**Main Project**: `GameSpace/GameSpace/GameSpace.csproj`
**Framework**: .NET 8.0
**Database**: SQL Server (LocalDB/Express)
**Server**: `DESKTOP-8HQIS1S\SQLEXPRESS` (development)
