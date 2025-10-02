# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

GameSpace is an ASP.NET Core 8.0 MVC web application with a multi-area architecture for managing an online gaming platform. The project includes identity management, an online store, forums, social features, and a mini-game system with virtual pets and rewards.

**Key Technologies:**
- ASP.NET Core 8.0 MVC
- Entity Framework Core 8.0 (SQL Server)
- ASP.NET Core Identity
- SignalR (real-time chat)
- ClosedXML (Excel exports)

## Solution Structure

```
GameSpace/
├── GameSpace/
│   ├── GameSpace.sln
│   └── GameSpace/
│       ├── Program.cs              # Application entry point and DI configuration
│       ├── Areas/                  # Feature-based organization
│       │   ├── Forum/             # Forum system
│       │   ├── Identity/          # User authentication/registration
│       │   ├── MemberManagement/  # Member administration
│       │   ├── MiniGame/          # Mini-game system (active development)
│       │   ├── OnlineStore/       # E-commerce functionality
│       │   └── social_hub/        # Social features and chat
│       ├── Controllers/           # Root-level controllers
│       ├── Data/                  # ApplicationDbContext (Identity)
│       ├── Infrastructure/        # Cross-cutting concerns
│       │   ├── Login/            # Unified login system
│       │   └── Time/             # Time zone handling
│       ├── Models/                # Shared EF models and GameSpacedatabaseContext
│       ├── Views/                 # Shared views and layouts
│       ├── Partials/             # Reusable partial views
│       └── wwwroot/              # Static files (SB Admin, Bootstrap, etc.)
└── schema/                        # Database documentation and specs
```

## Common Development Commands

### Build and Run
```bash
# Build the solution
dotnet build GameSpace/GameSpace/GameSpace.sln

# Run the application
dotnet run --project GameSpace/GameSpace/GameSpace/GameSpace.csproj

# Run in watch mode (auto-reload on file changes)
dotnet watch --project GameSpace/GameSpace/GameSpace/GameSpace.csproj
```

### Database Operations
**IMPORTANT:** This project uses **database-first** approach with manual SQL Server management.
- **DO NOT** use Entity Framework migrations (`dotnet ef migrations`)
- Database schema is managed directly in SQL Server (SSMS)
- Connection strings are in `appsettings.json`

### Health Check
```bash
# Verify database connectivity
curl http://localhost:5000/healthz/db
```

## Architecture and Key Patterns

### 1. Dual Authentication System

The application uses **two authentication schemes**:

1. **Identity Cookie** (`Identity.Application`): For regular users
   - Managed by ASP.NET Core Identity
   - Used in public-facing areas

2. **AdminCookie** (`GameSpace.Areas.social_hub.Auth.AuthConstants.AdminCookieScheme`): For administrators
   - Custom cookie scheme for admin features
   - 4-hour sliding expiration
   - Configured in `Program.cs:127-170`

**Login System:**
- Centralized login at `/Login` (see `Controllers/LoginController.cs`)
- Uses `Infrastructure/Login/ILoginIdentity` interface for abstraction
- Implementation: `CookieAndAdminCookieLoginIdentity` handles both schemes
- Areas should redirect to centralized login, not implement their own

### 2. Area-Based Organization

Each major feature is isolated in its own Area with:
- **Controllers/** - Area-specific MVC controllers
- **Views/** - Razor views (usually includes `_ViewStart.cshtml` and `Shared/_Sidebar.cshtml`)
- **Models/** or **ViewModels/** - DTOs and view models
- **Services/** - Business logic
- **Data/** - Area-specific DbContext (if needed)
- **config/** - Dependency injection setup (e.g., `ServiceExtensions.cs`)

**MiniGame Area** is the most complete example of this pattern.

### 3. Permission System

Two permission models coexist:

**A. Policy-Based Authorization** (Standard ASP.NET Core)
- Defined in `Program.cs:173-181`
- Policies: `CanManageShopping`, `CanAdmin`, `CanMessage`, `CanUserStatus`, `CanPet`, `CanCS`
- Based on claims like `perm:Shopping`, `perm:Admin`, etc.
- Used with `[Authorize(Policy="PolicyName")]`

**B. Manager Permission System** (Custom)
- Database tables: `ManagerData`, `ManagerRole`, `ManagerRolePermission`
- Service: `Areas/social_hub/Permissions/ManagerPermissionService.cs`
- Attribute: `RequireManagerPermissionsAttribute`
- Used primarily in social_hub and MiniGame areas
- Provides fine-grained, role-based permissions stored in database

### 4. Database Context Strategy

**Two DbContexts:**
1. `ApplicationDbContext` (`Data/ApplicationDbContext.cs`): ASP.NET Identity tables only
2. `GameSpacedatabaseContext` (`Models/GameSpacedatabaseContext.cs`): All business tables

**Connection Strings** (from `appsettings.json`):
```json
{
  "DefaultConnection": "...",  // For ApplicationDbContext
  "GameSpace": "..."           // For GameSpacedatabaseContext
}
```

### 5. SignalR Real-Time Features

- Hub: `Areas/social_hub/Hubs/ChatHub.cs`
- Endpoint: `/social_hub/chatHub` (configured in `Program.cs:234-240`)
- Transports: WebSockets, Server-Sent Events, Long Polling
- Services:
  - `IMuteFilter` / `MuteFilter`: Profanity filtering with caching
  - `INotificationService` / `NotificationService`: In-app notifications

### 6. MiniGame Area Architecture

The MiniGame area is actively developed and follows strict boundaries:

**Service Registration:**
- All services registered via `Areas/MiniGame/config/ServiceExtensions.cs`
- Uses extension method: `services.AddMiniGameServices(configuration)`
- Called from `Program.cs`

**Key Services:**
- `IMiniGameAdminService` - Admin operations
- `IMiniGamePermissionService` - Permission checks
- `IUserWalletService` - Wallet and points management
- `ICouponService` / `IEVoucherService` - Reward management
- `ISignInStatsService` - Daily check-in tracking
- `IMiniGameService` - Game session management
- Pet-related services (color options, backgrounds, leveling, costs)
- `IDailyGameLimitService` - Daily play limit enforcement

**Controllers:** All admin controllers use:
- `[Area("MiniGame")]`
- `[Authorize(AuthenticationSchemes = AuthConstants.AdminCookieScheme)]`
- Base controller: `MiniGameBaseController.cs` (common functionality)

**Database Tables:**
- Wallet: `User_Wallet`, `WalletHistory`
- Coupons: `CouponType`, `Coupon`, `EVoucherType`, `EVoucher`, `EVoucherToken`, `EVoucherRedeemLog`
- Sign-in: `UserSignInStats`
- Pets: `Pet` (includes attributes, levels, appearance)
- Games: `MiniGame` (game sessions and rewards)

## UI Framework and Styling

**Admin Backend:** SB Admin template
- Located in: `wwwroot/lib/sb-admin/`
- **DO NOT modify vendor files**
- Shared layout likely in `Views/Shared/_Layout.cshtml`

**Public Frontend:** Bootstrap-based
- Standard Bootstrap components
- Located in: `wwwroot/lib/bootstrap/`

**Sidebars:**
- Global: `Views/Shared/_Sidebar.cshtml`
- Area-specific: `Areas/{AreaName}/Views/Shared/_Sidebar.cshtml`
- Permission-based visibility (show/hide based on user claims/roles)

## Critical Development Rules

### Boundaries and Constraints

1. **Area Isolation:**
   - Each area should be self-contained
   - Cross-area references allowed for reading (interfaces, models)
   - **NEVER** modify code in other areas
   - Exception: `Program.cs` for DI registration only (append, don't modify existing)

2. **Database Schema:**
   - **Database-first** approach
   - Schema managed in SQL Server directly
   - **DO NOT** run `dotnet ef migrations add/update`
   - Models should match existing database schema exactly
   - Reference: `schema/` directory contains documentation

3. **Vendor Files:**
   - Located in `wwwroot/lib/`
   - **NEVER** modify these files
   - Use custom CSS/JS files for customizations

4. **Encoding:**
   - All source files: **UTF-8 with BOM**
   - Especially important for Chinese language content

### When Working on MiniGame Area

If developing in `Areas/MiniGame/`:

1. **Scope Restriction:**
   - Only create/modify files within `Areas/MiniGame/`
   - Exception: Add service registration to `Program.cs` (use `ServiceExtensions.cs` pattern)

2. **Authentication:**
   - Always redirect to `/Login` for authentication
   - Never implement custom login flows
   - Use `AdminCookie` scheme for admin features

3. **Implementation Order:**
   - View → Model → Controller → Service → Filter → Config
   - Complete all structures before fixing compilation errors

4. **No Placeholders:**
   - All features must be fully implemented
   - No dummy data or placeholder text
   - Must connect to actual database tables

## Session and Cookie Configuration

- **Session:** 30-minute idle timeout, HttpOnly, Essential, SameSite=Lax
- **AdminCookie:** 4-hour sliding expiration, custom login/logout paths
- **AJAX Handling:** Returns 401/403 status codes instead of redirects for AJAX requests

## Time Zone Handling

- Centralized via `Infrastructure/Time/IAppClock`
- Implementation: `AppClock` with Taipei timezone
- Configured in `Program.cs:85`

## CORS (Optional)

- Configured for chat functionality if `Cors:Chat:Origins` is set
- Policy name: "chat"
- Allows credentials, any header, any method

## Anti-Forgery Protection

- Enabled globally via `AutoValidateAntiforgeryTokenAttribute`
- Header name: `RequestVerificationToken`
- AJAX requests should include this header

## Status Code Pages

- Custom error handling: `/Home/Http{0}` (e.g., `/Home/Http404`)
- Development: Developer exception page
- Production: `/Home/Maintenance`

## Testing and Health Checks

**Database Health Check:**
- Endpoint: `/healthz/db` (see `Controllers/HealthController.cs`)
- Should return: `{"status":"ok"}` or error details
- Use this to verify database connectivity

## Common Pitfalls to Avoid

1. **Don't** use EF migrations (database is manually managed)
2. **Don't** modify vendor files in `wwwroot/lib/`
3. **Don't** cross area boundaries for code modifications
4. **Don't** implement separate login systems per area
5. **Don't** forget to register services in the appropriate `ServiceExtensions.cs`
6. **Don't** use Singleton for `IUserContextReader` or services depending on `HttpContext`
7. **Don't** mix authentication schemes without understanding the dual-cookie system
8. **Don't** forget UTF-8 BOM encoding for files with Chinese content

## Key Files to Reference

- `Program.cs` - Complete DI configuration, middleware pipeline, routing
- `Areas/social_hub/Auth/AuthConstants.cs` - Authentication scheme names
- `Areas/social_hub/Permissions/ManagerPermissionService.cs` - Permission checking logic
- `Infrastructure/Login/CookieAndAdminCookieLoginIdentity.cs` - Login implementation
- `Areas/MiniGame/config/ServiceExtensions.cs` - Example of proper service registration
- `Models/GameSpacedatabaseContext.cs` - Complete database schema (scaffolded from DB)

## Git Workflow

**Current Branch:** `minigame`
**Main Branch:** `main`

Recent commits show focus on MiniGame area development and audit fixes. Follow existing commit message patterns.