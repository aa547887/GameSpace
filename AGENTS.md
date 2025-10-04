# Repository Guidelines

## Project Structure & Module Organization
- Solution root: `GameSpace/GameSpace`. All .NET 8 MVC code lives in `GameSpace/GameSpace/GameSpace`.
- Feature areas sit under `Areas/*` (Forum, MemberManagement, MiniGame, OnlineStore, social_hub). Shared controllers and views stay under `Controllers/` and `Views/`.
- Domain models are in `Models/`; identity context and migrations in `Data/`. Static assets reside in `wwwroot/` (managed with LibMan).
- Schema documentation lives in `schema/`. Review this when updating EF entities.

## Build, Test, and Development Commands
- Restore packages: `dotnet restore GameSpace/GameSpace.sln`.
- Build locally: `dotnet build GameSpace/GameSpace.sln -c Debug`.
- Run with hot reload: `dotnet watch run --project GameSpace/GameSpace/GameSpace.csproj`.
- Restore client libraries: from `GameSpace/GameSpace/GameSpace`, run `libman restore`.
- Format code: `dotnet format GameSpace/GameSpace.sln` before submitting PRs.

## Coding Style & Naming Conventions
- C#, .NET 8; four-space indentation, Allman braces, one type per file.
- PascalCase for classes/methods, camelCase for locals/parameters, `_camelCase` for private fields.
- Keep nullable reference types enabled; avoid suppressing warnings unless justified.
- Organize `using` directives: framework → third-party → project.

## Testing Guidelines
- Add tests in a sibling `GameSpace.Tests` xUnit project. Reference the main project.
- Name fixtures `FeatureNameTests`; methods as `Scenario_Should_ExpectedOutcome`.
- Run `dotnet test` prior to PRs. Target ≥80% coverage on new code and include coverage summaries if possible.

## Commit & Pull Request Guidelines
- Commit messages should be imperative, optionally scoped: `fix(member): adjust lockout logic`.
- Group logical changes per commit; avoid mixing refactors and fixes.
- PRs must describe intent, list manual validation steps (`dotnet build`, `dotnet test`, UI checks), highlight config or schema updates, link issues, and attach UI screenshots when relevant.

## Security & Configuration Tips
- Never commit secrets. Use `dotnet user-secrets` or environment variables for local credentials.
- Set `ASPNETCORE_ENVIRONMENT=Development` for local runs; document new keys when editing `Program.cs` or auth policies.
- Review dual authentication requirements before touching admin login flows or policies.
