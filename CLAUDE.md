# Contracts ERP — CLAUDE.md

## Project Overview
OFBiz-inspired enterprise ERP built with ASP.NET Core C# (Clean Architecture) backend
and React.js + TypeScript frontend. The solution manages orders, quotes, inventory,
accounting, HR, CRM, manufacturing, and facilities.

---

## Backend Architecture

### Domain
- All entities live flat in `Domain/` (no subfolders) — mirrors the OFBiz entity model
- Pure POCOs: string IDs (OFBiz heritage — not int/Guid), nullable properties, nav collections initialized in constructor
- JSON serialized with snake_case: `[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]`

### Application (CQRS with MediatR)
- Each feature is a **nested-class file**: `Query`/`Command` + `Handler` all in one file
  ```
  ListSalesOrder.cs  →  ListSalesOrder.Query + ListSalesOrder.Handler
  CreateSalesOrder.cs  →  CreateSalesOrder.Command + CreateSalesOrder.Handler
  ```
- Handlers return `Result<T>` or `Results<T>` — never throw from handlers
- **No repository layer** — `DataContext` is injected directly into handlers

### Persistence
- Lazy loading **disabled** — always use explicit joins or `.Include()`

### API
- `BaseApiController` — all controllers inherit this
  - Resolves `IMediator` lazily from `HttpContext`
  - `HandleResult<T>`, `HandleResults<T>`, `HandlePagedResult<T>` → maps to HTTP status codes
  - `GetLanguage()` → reads `Accept-Language` header for multilingual support
- OData list endpoints at `/odata/` (e.g., `OrderRecords`, `QuoteRecords`, `PartyRecords`)
- Serilog logging (Warning level minimum; 7-day rolling files in `logs/`)

### Environments
- Development: HTTP only on port 5001
- Production: HTTP:5100, HTTPS:8544 (Kestrel + Docker)

---

## Frontend (client-app/)

Frontend-specific stack notes, naming conventions, and UI patterns (Menu-Exit, Actions Menu, Status Ribbon, RBAC) live in `client-app/CLAUDE.md` — loaded automatically when working under that directory.

---

## Naming Conventions — Backend
| Thing | Convention | Example |
|---|---|---|
| CQRS file | `VerbEntity.cs` | `ListSalesOrder.cs`, `CreatePurchaseOrder.cs` |
| DTO | `EntityDto` | `OrderDto`, `OrderItemDto`, `OrderDto2` |
| OData record | `EntityRecord` | `OrderRecord`, `QuoteRecord`, `PartyRecord` |
| Handler result | `Result<T>` / `Results<T>` | `Result<OrderDto2>.Success(...)` |

---

## Database Migrations

- NEVER run `dotnet ef database update` or any command that applies a migration to a database.
- NEVER run `dotnet ef migrations remove` against an already-applied migration.
- You MAY run `dotnet ef migrations add <Name>` to generate new migration files for review.
- You MAY run `dotnet ef migrations script` to generate a SQL script for review — this does not touch the database.
- Applying migrations (dev, staging, or prod) is a manual, human-only step. Always stop after generating files and tell me what changed so I can review and apply it myself.

## Important Conventions & Gotchas
- `// REFACTOR:` comments mark known tech debt — do not remove without addressing the underlying issue
- Snake_case JSON applies only to Domain entities; DTOs use standard C# PascalCase
- OData endpoints must be registered in `Program.cs` `GetEdmModel()` — add new entity sets there when introducing OData-backed list views
- `Accept-Language` header drives multilingual label resolution server-side; pass it via `GetLanguage()` in controllers
- `DataContext` has `LazyLoadingEnabled = false` — always write explicit joins or `.Include()` chains
- Both RTK Query and Axios (`agent.ts`) coexist — prefer RTK Query for new features
- Both MobX and Redux coexist — prefer Redux slices + RTK Query for new features
- `QuestPDF.Settings.UseEnvironmentFonts = false` is set globally — do not rely on system fonts in PDF templates

- Do not spawn subagents unless explicitly asked. Work sequentially.

## Build Rules
- NEVER run `dotnet build`, `dotnet run`, `dotnet watch`, or any build/compile command after modifying files
- NEVER run `npm run build`, `npm start`, or any frontend build commands after modifying files
- After making code changes, stop and report what was changed — do not attempt to verify by building
- The developer will manually re-run/re-debug after all changes are complete

## Local Dev Login (browser verification only)
- Only use when the developer explicitly asks for a live browser check (e.g. via chrome-devtools tools) against the already-running local dev server (`http://localhost:3000`, API on `:5001`). Never start the dev server yourself — see Build Rules above.
- Email: `eradwan1967@gmail.com`
- Password: `Pa$$w0rd`