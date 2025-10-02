# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**GameSpace** is an ASP.NET Core 8.0 MVC web application featuring a multi-tenant gaming platform with admin management systems. The primary development focus is on the **MiniGame Area** - an admin backend system for managing user wallets, pets, mini-games, sign-in rewards, coupons, and e-vouchers.

## Build & Development Commands

### Building the Project
```powershell
# Build the solution
dotnet build GameSpace/GameSpace.sln

# Build specific configuration
dotnet build GameSpace/GameSpace.sln --configuration Release

# Run the application
dotnet run --project GameSpace/GameSpace/GameSpace.csproj
```

### Database Operations
```powershell
# Connect to SQL Server and query data
sqlcmd -S "DESKTOP-8HQIS1S\SQLEXPRESS" -d "GameSpacedatabase" -E -Q "SELECT * FROM ManagerData"

# Query specific tables (use TOP 10 to limit results)
sqlcmd -S "DESKTOP-8HQIS1S\SQLEXPRESS" -d "GameSpacedatabase" -E -Q "SELECT TOP 10 * FROM WalletHistory"
sqlcmd -S "DESKTOP-8HQIS1S\SQLEXPRESS" -d "GameSpacedatabase" -E -Q "SELECT TOP 10 * FROM UserSignInStats"
sqlcmd -S "DESKTOP-8HQIS1S\SQLEXPRESS" -d "GameSpacedatabase" -E -Q "SELECT TOP 10 * FROM Pet"
```

## Architecture Overview

### Multi-Context Database Architecture
The application uses **two separate DbContexts**:

1. **ApplicationDbContext** (`DefaultConnection`)
   - ASP.NET Identity for user authentication
   - Connection: `aspnet-GameSpace-38e0b594-8684-40b2-b330-7fb94b733c73`

2. **GameSpacedatabaseContext** (`GameSpace`)
   - All business domain models (Users, Pets, MiniGames, Wallets, etc.)
   - Connection: `GameSpacedatabase`
   - Located at: `Models/GameSpacedatabaseContext.cs`

### Area-Based Architecture
The project follows ASP.NET Areas pattern with strict boundaries:

```
Areas/
├── Forum/              # Forum management
├── Identity/           # ASP.NET Identity pages
├── MemberManagement/   # Member management
├── MiniGame/          # ⭐ PRIMARY WORK AREA - Admin backend system
├── OnlineStore/       # E-commerce features
└── social_hub/        # Social features & SignalR chat
```

### MiniGame Area Structure (Primary Development Focus)

**⚠️ IMPORTANT CONSTRAINT**: All MiniGame development MUST stay within `Areas/MiniGame/`. Only add registrations to `Program.cs` - never modify other areas or vendor files.

```
Areas/MiniGame/
├── Controllers/          # Admin controllers (all inherit from MiniGameBaseController)
│   ├── MiniGameBaseController.cs    # Base controller with auth & utilities
│   ├── AdminHomeController.cs       # Dashboard & landing
│   ├── AdminWalletController.cs     # Wallet & points management
│   ├── AdminCouponController.cs     # Coupon management
│   ├── AdminEVoucherController.cs   # E-voucher management
│   ├── AdminPetController.cs        # Pet system management
│   ├── AdminMiniGameController.cs   # Game records & rules
│   ├── AdminSignInController.cs     # Sign-in rewards & stats
│   ├── AdminUserController.cs       # User management
│   └── AdminManagerController.cs    # Manager permissions
│
├── Services/            # Business logic layer (all registered in ServiceExtensions.cs)
│   ├── Interface files (I*.cs)      # Service contracts
│   ├── Implementation files         # Business logic
│   ├── *ValidationService.cs        # Validation logic
│   └── *RulesService.cs            # Rules & configuration
│
├── Models/              # ViewModels & DTOs (NOT EF entities)
│   ├── ViewModels/                  # Request/Response models
│   ├── Settings/                    # Configuration models
│   └── ValidationResult.cs          # Validation results
│
├── Filters/             # Authorization & request filters
│   ├── MiniGameAdminAuthorizeAttribute.cs      # Basic admin auth check
│   ├── MiniGameModulePermissionAttribute.cs    # Fine-grained permissions
│   ├── MiniGameAdminOnlyAttribute.cs           # Admin-only access
│   ├── IdempotencyFilter.cs                    # Prevent duplicate operations
│   └── MiniGameProblemDetailsFilter.cs         # Error handling
│
├── config/              # Startup configuration
│   ├── ServiceExtensions.cs         # ALL service registrations
│   └── StartupExtensions.cs         # Additional startup config
│
├── Views/               # Razor views (SB Admin template - DO NOT modify vendor)
├── docs/                # Documentation & audit reports
└── wwwroot/            # Static assets
```

### Authentication & Authorization System

**Dual Cookie Scheme Architecture:**
- **Identity Cookie**: Front-end user authentication (ASP.NET Identity)
- **AdminCookie**: Backend manager authentication (custom claims-based)

#### Manager Authentication Flow
1. Login via `LoginController` with `ManagerData` table credentials
2. Claims populated from `ManagerRolePermission` table
3. Cookie issued with 4-hour sliding expiration
4. Email verification via OTP if first login

#### Permission Model
Manager permissions stored in `ManagerRolePermission` with boolean flags:
- `AdministratorPrivilegesManagement` - Full system access (superuser)
- `UserStatusManagement` - User.View, User.Edit
- `ShoppingPermissionManagement` - Wallet.*, Coupon.*, EVoucher.*
- `PetRightsManagement` - Pet.View, Pet.Edit
- `MessagePermissionManagement` - Message.View, Message.Edit
- `CustomerService` - CustomerService access

**Permission Checking:**
- Controllers inherit from `MiniGameBaseController`
- Use `[Authorize(AuthenticationSchemes = "AdminCookie", Policy = "AdminOnly")]` attribute
- Fine-grained checks via `MiniGameModulePermissionAttribute(string permission)`
- Runtime checks: `await HasPermissionAsync("Module.Action")`

### Service Registration Pattern

All MiniGame services MUST be registered in `Areas/MiniGame/config/ServiceExtensions.cs`:

```csharp
services.AddScoped<IServiceInterface, ServiceImplementation>();
```

Then in `Program.cs`, add ONE line:
```csharp
builder.Services.AddMiniGameServices(builder.Configuration);
```

### Database Entities vs ViewModels

**CRITICAL DISTINCTION:**
- **EF Core Entities**: Located in `Models/` (root level) - e.g., `Models/User.cs`, `Models/Pet.cs`
- **ViewModels**: Located in `Areas/MiniGame/Models/ViewModels/` - for data transfer only
- **Never mix**: Controllers use ViewModels, Services work with Entities

Key tables in `GameSpacedatabaseContext`:
```
Users                   # User accounts
ManagerData            # Admin accounts
ManagerRolePermission  # Admin permission matrix
User_Wallet            # User points balance
WalletHistory          # Transaction log (Point/Coupon/EVoucher changes)
Pet                    # Pet system (5 attributes: 健康/飽食/心情/乾淨/忠誠)
MiniGame               # Game play records (win/lose/abort, rewards)
UserSignInStats        # Daily sign-in tracking
Coupon                 # User-owned coupons
CouponType             # 20 coupon types (discount rules)
EVoucher               # User-owned e-vouchers
EVoucherType           # 20 e-voucher types (redemption rules)
```

### Shared Infrastructure

**Login System** (`Infrastructure/Login/`):
- `ILoginIdentity` - Unified login interface
- `CookieAndAdminCookieLoginIdentity` - Dual cookie implementation
- Registered globally in `Program.cs`

**Time Management** (`Infrastructure/Time/`):
- `IAppClock` - Timezone-aware clock abstraction
- `AppClock(TimeZones.Taipei)` - Taiwan timezone (UTC+8)

**Social Hub Integration** (`Areas/social_hub/`):
- SignalR `ChatHub` at `/social_hub/chatHub`
- Mute filter service (profanity detection)
- User context reader for social features

## Key Development Rules

### Code Constraints
1. **NEVER modify files outside `Areas/MiniGame/`** except adding service registrations to `Program.cs`
2. **NEVER modify vendor files** (SB Admin template in wwwroot/vendor/)
3. **NEVER modify `Program.cs` beyond service registrations** - do not change existing configuration
4. **Use PowerShell** for all file operations and database queries
5. **Use Traditional Chinese (繁體中文)** for all UI text and comments

### Architecture Patterns
1. **Controllers**: Inherit from `MiniGameBaseController` for auth & utilities
2. **Services**: Register ALL services in `ServiceExtensions.cs`, use scoped lifetime
3. **Permissions**: Use `MiniGameModulePermissionAttribute` for fine-grained access control
4. **Validation**: Create separate `*ValidationService.cs` for complex business rules
5. **ViewModels**: Define in `Areas/MiniGame/Models/ViewModels/`, never reuse entities

### Database Access
1. **Always use `GameSpacedatabaseContext`** (not ApplicationDbContext) for business logic
2. **Use Windows Authentication** (`Integrated Security=True`) for SQL Server
3. **Connection string key**: `"GameSpace"` (not "GameSpacedatabase")
4. **Include navigation properties** when querying for permissions: `.Include(m => m.ManagerRoles)`

### Admin UI Standards
1. **Template**: SB Admin 2 (Bootstrap-based)
2. **Navigation**: Shared admin homepage → "小遊戲管理系統" tab → MiniGame area sidebar
3. **Permission-based UI**: Hide unauthorized menu items and actions
4. **Error pages**: Use global 403 page for access denied, redirect to login for unauthenticated

## Common Pitfalls

1. **Wrong DbContext**: Using `ApplicationDbContext` instead of `GameSpacedatabaseContext`
2. **Missing service registration**: Forgetting to add service in `ServiceExtensions.cs`
3. **Entity/ViewModel confusion**: Passing entities to views or ViewModels to EF
4. **Permission checks**: Not checking module-specific permissions in controllers
5. **Navigation properties**: Forgetting `.Include()` when loading related data
6. **Cookie scheme**: Using wrong authentication scheme (should be "AdminCookie")

## Test Accounts

Admin accounts in `ManagerData` table:
```
zhang_zhiming_01  / AdminPass001@   # Superuser (all permissions)
li_xiaohua_02     / SecurePass002#  # User management
wang_meiling_03   / StrongPwd003!   # Shopping & pet management
```

## Project Specifics

### Game Rules System
- Daily game limit: 3 plays per day (configurable via `DailyGameLimit`)
- Rewards: Points, Pet EXP, or Coupons (configured in game rules)
- Pet system: 5-dimensional attributes (Health, Hunger, Mood, Cleanliness, Loyalty)
- Pet customization: Color change & background change (point-based costs)

### Wallet System
- Three transaction types: `Point`, `Coupon`, `EVoucher`
- All changes logged in `WalletHistory` with timestamp & description
- Coupons: Format `CPN-{YYMM}-{6-char-random}`
- E-Vouchers: Format `EV-{TypeCode}-{4-char-random}-{6-digit-number}`

### Sign-In Rewards
- Daily check-in with configurable reward rules
- Consecutive day tracking for bonus rewards
- Rewards include: Points, Pet EXP, Coupons

## Session & State Management

- **Session**: Enabled with 30-minute idle timeout, used for OTP verification
- **Anti-Forgery**: Auto-validated on POST actions (header: `RequestVerificationToken`)
- **CORS**: Configurable for SignalR chat (section: `Cors:Chat:Origins`)
- **Cache**: Memory cache for mute filter (30-second TTL)

## Middleware Pipeline Order (in Program.cs)

1. Developer Exception Page / Exception Handler
2. Status Code Pages with ReExecute
3. HTTPS Redirection
4. Static Files
5. Routing
6. CORS (if configured)
7. Cookie Policy
8. **Session** (must be before Authentication)
9. Authentication
10. Authorization
11. MapControllers / MapHub / MapRazorPages
