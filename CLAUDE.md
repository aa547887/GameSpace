# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

GameSpace is an ASP.NET Core 8.0 web application serving as a gaming forum platform with modular functionality organized into Areas. The project includes social networking, e-commerce, gaming features, and a comprehensive admin backend.

## Build and Development Commands

### Prerequisites
- .NET 9.0 SDK
- SQL Server (DESKTOP-8HQISIS\SQLEXPRESS)
- Windows Integrated Authentication for database access

### Core Commands

```bash
# Restore NuGet packages
dotnet restore GameSpace/GameSpace/GameSpace.csproj

# Build the solution
dotnet build GameSpace/GameSpace/GameSpace.sln

# Build specific project
dotnet build GameSpace/GameSpace/GameSpace.csproj

# Run the application
dotnet run --project GameSpace/GameSpace/GameSpace.csproj

# Database connection test (via sqlcmd)
sqlcmd -S "DESKTOP-8HQISIS\SQLEXPRESS" -d "GameSpacedatabase" -E -Q "SELECT TOP 1 * FROM ManagerData"
```

## Architecture

### Areas-based Modular Structure

The application uses ASP.NET Areas for organization:

1. **MiniGame** - Primary development area with gaming features, pet system, wallet management
2. **social_hub** - Real-time chat (SignalR), notifications, profanity filtering
3. **OnlineStore** - E-commerce functionality
4. **Forum** - Discussion board
5. **MemberManagement** - User administration
6. **Identity** - Authentication/authorization

### MiniGame Area Architecture (Primary Focus)

**Layered Structure:**
```
Areas/MiniGame/
├── Controllers/      # 24 admin controllers (Admin*, Pet*, Wallet*, etc.)
├── Models/          # Domain models, ViewModels, Settings
├── Views/           # Razor views using SB Admin template
├── Services/        # Business logic layer (50+ services)
├── Filters/         # Authorization attributes, problem details
├── Data/            # MiniGameDbContext
├── config/          # DI registration (ServiceExtensions.cs)
└── docs/            # Development tracking docs (WIP_RUN.md, PROGRESS.json, etc.)
```

**Key Service Categories:**
- Admin services: `IMiniGameAdminService`, `IMiniGamePermissionService`, `IMiniGameAdminAuthService`
- Pet system: Color/background options, level rewards, interaction bonuses
- Wallet: Point management, transaction history
- Game limits: Daily game limit settings

### Database Architecture

**Two DbContexts:**
- `ApplicationDbContext` - ASP.NET Identity (connection: DefaultConnection)
- `GameSpacedatabaseContext` - Business data (connection: GameSpace)
- `MiniGameDbContext` - MiniGame-specific models (uses GameSpace connection)

**Connection Strings** (appsettings.json):
- DefaultConnection: `Server=DESKTOP-8HQISIS\SQLEXPRESS;Database=aspnet-GameSpace-...`
- GameSpace: `Server=DESKTOP-8HQISIS\SQLEXPRESS;Database=GameSpacedatabase;...`

**Important Database Tables (MiniGame Area):**
- `ManagerData` - Admin accounts (102 seed records)
- `ManagerRolePermission` - Role definitions (8 roles)
- `ManagerRole` - Admin-role assignments
- `User_Wallet` - Member points/coupons/vouchers
- `WalletHistory` - Transaction logs
- `Pet` - Pet养成 system
- `MiniGame` - Game records
- `Coupon`, `CouponType` - Coupon management
- `EVoucher`, `EVoucherType` - E-voucher management
- `UserSignInStats` - Daily check-in records

### Authentication & Authorization

**Dual Authentication Scheme:**
1. **ASP.NET Identity** - Default user authentication
2. **AdminCookie** - Custom admin backend authentication
   - Scheme name: `"AdminCookie"` (from `AuthConstants.AdminCookieScheme`)
   - Cookie: `GameSpace.Admin`
   - Login: `/Login`, Logout: `/Login/Logout`
   - Timeout: 4 hours sliding expiration

**Authorization Policies:**
- `AdminOnly` - MiniGame Area exclusive (requires AdminCookie)
- `CanManageShopping` - Shopping management (perm:Shopping claim)
- `CanAdmin` - Admin access (perm:Admin claim)
- `CanMessage` - Messaging (perm:Message claim)
- `CanUserStatus` - User status (perm:UserStat claim)
- `CanPet` - Pet management (perm:Pet claim)
- `CanCS` - Customer service (perm:CS claim)

**MiniGame Authorization Attributes:**
- `[MiniGameAdminOnly]` - Requires AdminCookie authentication
- `[MiniGameAdminAuthorize]` - Custom authorization filter
- `[MiniGameModulePermission]` - Module-level permission check

### Frontend Architecture

**Template System:**
- **SB Admin** template for all admin backend (`wwwroot/lib/sb-admin/`)
  - **CRITICAL**: Never modify vendor files in `sb-admin/`
- Bootstrap, jQuery, jQuery Validation
- Razor views with server-side rendering
- Vue.js for social_hub interactive features

**Admin Navigation Flow:**
1. Login → Shared admin home (with sidebar)
2. Click "小遊戲管理系統" tab
3. Enter MiniGame Area home (MiniGame sidebar)
4. Sidebar dynamically shows authorized modules based on Role/Claim/Policy

## Development Guidelines

### MiniGame Area Development (CRITICAL RULES)

**Scope Restrictions:**
- **ONLY work within** `Areas/MiniGame/` directory
- **ONLY modify** `Program.cs` to add necessary DI registrations
- **NEVER modify** other Areas or vendor files (`wwwroot/lib/sb-admin/`)

**Audit-First Approach:**
Before making ANY changes:
1. Connect to SSMS database and verify schema/seed data
2. Audit existing View/Model/Controller/Service/Filter/Config files
3. Check 100% coverage with database tables/columns
4. **IF audit passes** → DO NOT modify anything
5. **IF audit fails** → Apply minimal necessary fixes only

**Reentrant Development (State Files):**
Track progress in `Areas/MiniGame/docs/`:
- `WIP_RUN.md` - Timestamped run log (start/complete/errors/recovery)
- `PROGRESS.json` - Task states (todo/doing/done/error)
- `AUDIT_SSMS.md` - Database structure verification log
- `COVERAGE_MATRIX.json` - Feature × (View/Model/Controller/Service/Filter/Config) × DB tables coverage matrix (target: 100%)

**Minimal Change Principle:**
- Preserve existing structure
- No large-scale refactoring
- Only add missing features or fix errors
- Use real data, no placeholder/fake returns
- All views must bind to actual backend data
- All controller actions must apply correct Policy

**Database Rules:**
- **NEVER use EF Migrations to modify schema**
- Database schema truth source: SSMS (SQL Server Management Studio)
- Connection via: `sqlcmd -S "DESKTOP-8HQISIS\SQLEXPRESS" -d "GameSpacedatabase" -E`
- Schema reference files in: `GameSpace/schema/`

### Service Registration Pattern

All MiniGame services registered via extension method in `Program.cs`:
```csharp
builder.Services.AddMiniGameServices(builder.Configuration);
```

Defined in: `Areas/MiniGame/config/ServiceExtensions.cs`

### Permission Implementation Pattern

**Controller Level:**
```csharp
[Area("MiniGame")]
[Authorize(Policy = "AdminOnly")] // AdminCookie required
[MiniGameAdminOnly] // Custom filter
public class AdminPetController : Controller
{
    // Actions inherit policy
}
```

**View Level (Sidebar):**
- Show/hide menu items based on User claims/roles
- Unauthorized requests return 403 or redirect to access denied page

**Shared 403 Page:**
- Project has global 403 error page
- Use for unauthorized access attempts

## Key Integration Points

### Program.cs Middleware Pipeline
Critical order:
1. Session (`UseSession()`) - BEFORE Authentication
2. Authentication (`UseAuthentication()`)
3. Authorization (`UseAuthorization()`)

### SignalR Hub
- ChatHub at `/social_hub/chatHub`
- Supports WebSockets, SSE, Long Polling

### Session Configuration
- Timeout: 30 minutes
- HttpOnly, Essential cookies
- SameSite: Lax

## Schema Reference Files

Located in `GameSpace/schema/`:
- `資料庫連線與讀取流程_詳細版.md` - Database connection guide
- `MiniGame_Area_完整描述文件.md` - Complete MiniGame spec
- `這裡有MinGame Area和管理者權限相關資料庫結構及種子資料.sql` - DB schema + seed data
- `管理者權限相關描述.txt` - Manager permission description

## Current Branch Status

- Current branch: `minigame`
- Main branch: `main`
- Recent work: Pet appearance changes, wallet management UI, admin controller updates
- Untracked files: WalletController.cs, PetAppearanceChangeLog.cs, Wallet views