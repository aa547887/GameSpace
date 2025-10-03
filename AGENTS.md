# Repository Guidelines

## Project Structure & Module Organization
- Solution: `GameSpace/GameSpace.sln`
- Web app (ASP.NET Core MVC, .NET 8): `GameSpace/GameSpace/GameSpace.csproj`
- MVC Areas and features: `GameSpace/GameSpace/Areas/*`
- Views and shared layout: `GameSpace/GameSpace/Views`, `.../Views/Shared`
- Static assets: `GameSpace/GameSpace/wwwroot` (managed via `libman.json`)
- Data and EF Core migrations: `GameSpace/GameSpace/Data`
- App settings: `GameSpace/GameSpace/appsettings.json` and `appsettings.Development.json`
- Database/schema docs: `GameSpace/schema`

## Build, Test, and Development Commands
- Restore: `dotnet restore GameSpace/GameSpace.sln`
- Build (Debug): `dotnet build GameSpace/GameSpace.sln -c Debug`
- Run (Dev): `dotnet watch run --project GameSpace/GameSpace/GameSpace.csproj`
- Client libs: from the project directory, `libman restore`
- EF migrate (optional): `dotnet ef database update` (requires `dotnet-ef`)

## Coding Style & Naming Conventions
- Language: C# (.NET 8). Indent with 4 spaces; no tabs.
- Braces on new lines (Allman). One class per file.
- Names: PascalCase for types/methods; camelCase for locals/params; prefer `_camelCase` for private fields.
- Organize by feature (Areas) and keep controllers thin; move logic to services.
- Run `dotnet format` before committing to enforce conventions.

## Testing Guidelines
- No standalone test project is present. Prefer xUnit in `GameSpace.Tests` alongside the solution.
- Naming: `ClassNameTests`, methods like `Method_Should_DoExpected`.
- Run tests: `dotnet test` (once a test project exists). Target ≥80% coverage for new code.

## Commit & Pull Request Guidelines
- Commits: concise, imperative subject; include scope (e.g., `Forum: ...`). The history commonly groups work by phases (e.g., `🔧 Phase 2: ...`).
- Reference issues (e.g., `Fixes #123`) and summarize changes + rationale.
- PRs: clear description, steps to reproduce/verify, screenshots for UI, migration notes when DB changes are included.

## Security & Configuration
- Do not commit secrets. Use `appsettings.Development.json` or User Secrets (`dotnet user-secrets`) for local credentials.
- Set environment: PowerShell `($env:ASPNETCORE_ENVIRONMENT='Development')`; Bash `export ASPNETCORE_ENVIRONMENT=Development`.
- Review CORS, authentication, and role policies under `Areas/*` when adding endpoints.


