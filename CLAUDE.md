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
- Localization to Arabic must alwayes be supported via getTranslatedLabel and @ar.json

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

## Menu-Exit Pattern for Form/List Switching

When a feature renders a form in place of a list (via state, not a route change), the form must handle menu clicks so the user can return to the list by clicking the active menu item.

### How it works

1. The list component controls `editMode` state. When `editMode > 0` it renders the form instead of the grid. `cancelEdit()` resets `editMode` to 0 and returns to the grid.
2. The form receives `cancelEdit` as a prop and passes an `onMenuSelect` callback to `SalesRequestMenu`:

```tsx
<SalesRequestMenu
  selectedMenuItem="my-feature"
  onMenuSelect={(key) => {
    if (key === "salesRequest.menu.myFeature") cancelEdit();
  }}
/>
```

3. The menu component calls `onMenuSelect(link.key)` on every click; the form ignores keys it doesn't own.

### Rules
- The key passed to `onMenuSelect` is the `translationKey` / `key` field defined in the links array inside `SalesRequestMenu`.
- Never rely on route change alone to exit a form rendered this way; the route does not change when list and form share the same path.

### Example implementations
- `SalesRequestForm` → watches `"salesRequest.menu.salesRequests"`
- `SalesCommissionForm` → watches `"salesRequest.menu.salesCommissions"`

---

## Actions Menu Pattern (Form-Level)

When a form needs status-changing actions (approve, reset, delete, etc.), encapsulate them in a dedicated `*ActionsMenu` component rather than scattering buttons across the form.

**Reference files:** `menu/SalesCommissionActionsMenu.tsx` (component), `form/SalesCommissionForm.tsx` (usage), `menu/SalesRequestActionsMenu.tsx` (more complex example with reset + delete).

### Component structure
- Accepts `entityId`, `currentStatusId`, `disabled`, and one `onXxx` callback per action.
- Renders a single MUI `Button` ("Actions") that opens a MUI `Menu`.
- Each `MenuItem` is disabled when the current status makes the action invalid — disabled logic lives **inside** the menu component, not in the caller.
- Each destructive/irreversible action opens a `Dialog` for confirmation before firing the mutation.
- The button carries no top/bottom margin — the parent controls spacing.

### Form header layout (title row + actions menu + status ribbon)

The form header row uses a MUI `Grid` to place the title/actions on the left columns and the status ribbon in the last `xs={1}` column. Reference: `SalesCommissionForm.tsx`.

```tsx
<Grid container alignItems="center" sx={{ mb: 2 }}>
  <Grid item xs={editMode === 2 && record?.id ? 11 : 12}>
    <Box sx={{ display: "flex", alignItems: "center", justifyContent: "space-between" }}>
      <Typography variant="h4">
        {title}
        {record?.id && (
          <Typography component="span" variant="h5" color="grey" sx={{ ml: 1 }}>
            ({record.id})
          </Typography>
        )}
      </Typography>

      {/* Only rendered in edit mode, never in create mode */}
      {editMode === 2 && record?.id && (
        <EntityActionsMenu
          entityId={record.id}
          currentStatusId={record.statusId}
          disabled={false}
          onActionDone={cancelEdit}
        />
      )}
    </Box>
  </Grid>

  {editMode === 2 && record?.id && (
    <Grid item xs={1}>
      <RibbonContainer>
        <Ribbon
          side="left"
          type="corner"
          size="large"
          backgroundColor={ribbonBg[record.statusId] ?? "#757575"}
          color="#ffffff"
          fontFamily="sans-serif"
        >
          {ribbonLabel[record.statusId] ?? record.statusId}
        </Ribbon>
      </RibbonContainer>
    </Grid>
  )}
</Grid>
```

### Rules
- `justifyContent: "space-between"` on the inner `Box` puts the title at the RTL start and the Actions button at the RTL end (left side).
- Gate both the menu and ribbon on `editMode === 2 && record?.id` — neither appears in create mode.
- `onActionDone` is `cancelEdit` in form context and `refetch` in list/grid context.
- The `xs={1}` ribbon column only renders in edit mode; the title column expands to `xs={12}` in create mode.

## Status Ribbon Pattern

Use `react-ribbons` (`RibbonContainer` + `Ribbon`) to show entity status in the top corner of a form. `side="left"` for Arabic RTL layout.

Define status → color and status → label maps above the `return`:

```tsx
const ribbonBg: Record<string, string> = {
  MY_STATUS_PENDING:  "#ff9800",  // orange
  MY_STATUS_APPROVED: "#4caf50",  // green
  MY_STATUS_PAID:     "#1976d2",  // blue
};
const ribbonLabels: Record<string, string> = {
  MY_STATUS_PENDING:  getTranslatedLabel("entity.status.pending",  "قيد الانتظار"),
  MY_STATUS_APPROVED: getTranslatedLabel("entity.status.approved", "معتمد"),
  MY_STATUS_PAID:     getTranslatedLabel("entity.status.paid",     "مدفوع"),
};
const statusId = record?.statusId ?? "";
```

Then render inside the `xs={1}` grid column as shown in the Actions Menu layout above.

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
- `react-ribbons` — corner status ribbons on form headers

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