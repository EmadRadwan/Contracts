# Contracts ERP — CLAUDE.md

## Project Overview
OFBiz-inspired enterprise ERP built with ASP.NET Core C# (Clean Architecture) backend
and React.js + TypeScript frontend. The solution manages orders, quotes, inventory,
accounting, HR, CRM, manufacturing, and facilities.

---

## Solution Structure (Contracts.sln)

| Project | Role |
|---|---|
| **Domain** | POCO entity classes only — no logic, no DI |
| **Application** | CQRS handlers, DTOs, validators, service interfaces |
| **Persistence** | EF Core `DataContext`, migrations, seed data |
| **Infrastructure** | Email, PDF (QuestPDF), JWT user accessor |
| **API** | Controllers, middleware, OData setup, SignalR, Program.cs |

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
- Organized into domain modules: `Accounting/`, `CRM/`, `Catalog/`, `Common/`, `Facilities/`,
  `HumanResources/`, `Manufacturing/`, `Order/`, `Parties/`, `Projects/`, `Shipments/`,
  `WorkEfforts/`, `_Base/`
- Handlers return `Result<T>` or `Results<T>` — never throw from handlers
- `PagedList<T>` + `PaginationParams` for paginated endpoints
- `BaseService` abstract class provides `_context`, `_logger`, `_utilityService`
- AutoMapper for entity→DTO; FluentValidation for input validation
- **No repository layer** — `DataContext` is injected directly into handlers

### Persistence
- Single `DataContext : IdentityDbContext<AppUserLogin, ApplicationRole, ...>`
- Lazy loading **disabled** — always use explicit joins or `.Include()`
- `SeedContracts.cs` handles initial data seeding

### Infrastructure
- `UserAccessor.cs` — extracts current user from JWT claims
- `EmailSender.cs` — email notifications
- `PdfGenerationService.cs` — PDF generation via QuestPDF
- `Contents/` — content/file access helpers

### API
- `BaseApiController` — all controllers inherit this
  - Resolves `IMediator` lazily from `HttpContext`
  - `HandleResult<T>`, `HandleResults<T>`, `HandlePagedResult<T>` → maps to HTTP status codes
  - `GetLanguage()` → reads `Accept-Language` header for multilingual support
- Route convention: `[Route("api/[controller]")]`
- OData list endpoints at `/odata/` (e.g., `OrderRecords`, `QuoteRecords`, `PartyRecords`)
- REST endpoints for CRUD operations
- Global `ExceptionMiddleware` handles unhandled exceptions
- Serilog logging (Warning level minimum; 7-day rolling files in `logs/`)
- SignalR hubs in `API/SignalR/`
- JWT authentication; global `[Authorize]` filter applied to all controllers
- Response compression (GZip) enabled

### Environments
- Development: HTTP only on port 5001
- Production: HTTP:5100, HTTPS:8544 (Kestrel + Docker)

---

## Frontend Architecture (client-app/)

### Stack
- **Vite** + TypeScript + React 18
- **KendoReact** v6 — primary UI (Grid, Form, DateInputs, DropDowns, Menus)
- **MUI v6** — secondary UI (layout, Paper, Typography, Button, Grid)
- **React Router v6**
- **Dual state management** (both coexist):
  - **RTK Query** (`src/app/store/apis/`) — all API calls for modern features
  - **MobX** (`src/app/stores/`) — legacy stores (`userStore`, `modalStore`, etc.)
- **Axios** (`src/app/api/agent.ts`) — legacy HTTP client, still used alongside RTK Query

### src/ Folder Structure
```
src/
  app/
    api/          # Legacy Axios agent (agent.ts)
    common/       # Shared form fields, validators, modals
    components/   # Generic reusable components
    contexts/     # React contexts
    hooks/        # Custom hooks (useTranslationHelper, etc.)
    layout/       # App shell, LoadingComponent, styles
    models/       # TypeScript interfaces organized by domain
    router/       # React Router config
    slice/        # Shared Redux slices
    store/
      apis/       # RTK Query API slices (one file per domain area)
      configureStore.ts
    stores/       # MobX stores
  features/       # Domain-organized feature modules
    accounting/
    catalog/
    CRM/
    facilities/
    home/
    humanResources/
    manufacturing/
    orders/
    parties/
    Projects/
    services/
```

### Feature Module Structure (e.g., `features/orders/`)
```
orders/
  dashboard/    # List/grid views
  form/         # Form components
    order/
      SalesOrder/
      PurchaseOrder/
  hook/         # Feature-specific hooks (useSalesOrder, etc.)
  menu/         # Action menus
  slice/        # Redux UI slices (orderItemsUiSlice.ts, ordersUiSlice.ts, ...)
```

### API Call Pattern (RTK Query — use for new features)
```typescript
// src/app/store/apis/ordersApi.ts
const ordersApi = createApi({
  reducerPath: "salesOrders",
  baseQuery: fetchBaseQuery({ baseUrl: import.meta.env.VITE_API_URL, ... }),
  tagTypes: ["orders", "PurchaseOrders"],
  endpoints(builder) { ... }
});
export const { useFetchOrdersQuery, useAddSalesOrderMutation } = ordersApi;
```

---

## Naming Conventions

### Backend
| Thing | Convention | Example |
|---|---|---|
| CQRS file | `VerbEntity.cs` | `ListSalesOrder.cs`, `CreatePurchaseOrder.cs` |
| DTO | `EntityDto` | `OrderDto`, `OrderItemDto`, `OrderDto2` |
| OData record | `EntityRecord` | `OrderRecord`, `QuoteRecord`, `PartyRecord` |
| Handler result | `Result<T>` / `Results<T>` | `Result<OrderDto2>.Success(...)` |

### Frontend
| Thing | Convention | Example |
|---|---|---|
| RTK API file | `entityNameApi.ts` | `ordersApi.ts`, `partiesApi.ts` |
| RTK hook | `use[Verb][Entity][Query\|Mutation]` | `useFetchOrdersQuery`, `useAddSalesOrderMutation` |
| Redux UI slice | `featureNameUiSlice.ts` | `orderItemsUiSlice.ts`, `ordersUiSlice.ts` |
| Model file | camelCase TypeScript interface | `order.ts`, `orderPaymentPreference.ts` |
| Component | PascalCase `.tsx` | `SalesOrderForm.tsx`, `OrderTotals.tsx` |

---

## Key Libraries

### Backend NuGet
- `MediatR` — CQRS dispatcher
- `AutoMapper` — entity→DTO mapping
- `FluentValidation` — input validation
- `Microsoft.AspNetCore.OData` 8.x — OData query support for list views
- `Serilog` — structured logging
- `QuestPDF` — PDF generation
- `System.Linq.Dynamic.Core` — dynamic LINQ queries

### Frontend npm
- `@progress/kendo-react-*` v6 — primary UI component suite
- `@mui/material` v6 — secondary UI
- `@reduxjs/toolkit` — RTK + RTK Query
- `mobx` + `mobx-react-lite` — legacy store layer
- `react-router-dom` v6
- `axios` — legacy HTTP client
- `react-toastify` — toast notifications

---

## Important Conventions & Gotchas
- `// REFACTOR:` comments mark known tech debt — do not remove without addressing the underlying issue
- Snake_case JSON applies only to Domain entities; DTOs use standard C# PascalCase
- OData endpoints must be registered in `Program.cs` `GetEdmModel()` — add new entity sets there when introducing OData-backed list views
- `Accept-Language` header drives multilingual label resolution server-side; pass it via `GetLanguage()` in controllers
- `DataContext` has `LazyLoadingEnabled = false` — always write explicit joins or `.Include()` chains
- Both RTK Query and Axios (`agent.ts`) coexist — prefer RTK Query for new features
- Both MobX and Redux coexist — prefer Redux slices + RTK Query for new features
- `QuestPDF.Settings.UseEnvironmentFonts = false` is set globally — do not rely on system fonts in PDF templates