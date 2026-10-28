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
- 191 of them carry `[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]`. This is a
  **Newtonsoft** attribute and it only affects Newtonsoft — which in this solution means one thing:
  deserializing the OFBiz-exported seed files (`API/Json/*.json`, read by `Persistence/SeedContracts.cs`),
  whose keys are `MIME_TYPE_ID`-style. It does **not** affect API responses — see the gotcha below.

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
- **API responses are camelCase — everything, Domain entities included.** The MVC pipeline is
  System.Text.Json (`AddControllers().AddJsonOptions(...)` in `Program.cs`; there is no
  `AddNewtonsoftJson` and no `Microsoft.AspNetCore.Mvc.NewtonsoftJson` package reference), and it
  ignores Newtonsoft's `[JsonObject(SnakeCaseNamingStrategy)]` entirely. So a handler returning a raw
  Domain entity — e.g. `GlAccountTrialBalanceResult.GlAccount` — reaches the client as `accountCode`,
  not `account_code`. There is not a single snake_case property read in `client-app/`. Newtonsoft is
  referenced only by `Domain` (for the attribute) and `Persistence` (for seed deserialization).
- OData endpoints must be registered in `Program.cs` `GetEdmModel()` — add new entity sets there when introducing OData-backed list views
- `Accept-Language` header drives multilingual label resolution server-side; pass it via `GetLanguage()` in controllers
- `DataContext` has `LazyLoadingEnabled = false` — always write explicit joins or `.Include()` chains
- Both RTK Query and Axios (`agent.ts`) coexist — prefer RTK Query for new features
- Both MobX and Redux coexist — prefer Redux slices + RTK Query for new features
- PDFs are rendered server-side with Telerik Reporting (`API/Reporting/`), code-defined reports using the literal-TextBox-per-cell technique — do not use `Telerik.Reporting.List`/`Fields.X` binding (it silently rendered zero rows). QuestPDF was removed 2026-09-17.

- Do not spawn subagents unless explicitly asked. Work sequentially.

## Phase Two — Contracts & Certificates design (2026-10-08)

Phase two (client list in `second phase/`, business case `Phase-Two-Business-Case.source.html`) is built on this
code first. The contractor-certificate items (مستخلصات, sheet p.6, last in Batch 2) follow this design, taken from how
Odoo and its community model the same problem. Copy behaviour and ideas only — never code (Odoo industry packages
are OEEL, Apps Store modules are proprietary, OCA modules are AGPL).

**Today (as built):** a certificate is a `WorkEffort` (`PROJECT_CERTIFICATE`) with `CERTIFICATE_ITEM` children
(`Application/Projects/CreateProjectCertificate.cs`); `AchievementPercent` is typed per certificate (no memory of the
previous total); approval creates **a new purchase order per certificate**, then an invoice; retention ("تأمين" /
additional insurance) is added as extra order lines; serials run per party (`partyId-0001`); there is no contract, so
contracted / paid / remaining and over-certification cannot be answered.

**Target design:**
1. **Contract first; every certificate belongs to a contract.** Contract = OFBiz `Agreement` (header: contractor or
   supplier, project/building, value, retention %, advance %, dates, status, contract type) + `AgreementItem` (the
   bill of quantities: item, unit, qty, rate, cost code), linked to the project by `AgreementWorkEffortApplic` — all
   already in `Domain/` (new columns/tables still need the developer's OK; migrations are human-applied). Supply
   framework contracts may instead be a purchase order (Odoo's blanket order: agreed qty vs ordered).
2. **Cumulative certificates.** Each certificate line stores the cumulative qty / % to date; the screen shows previous
   / this certificate / to date. The user types this period's increment as a % **or** a quantity (each fills the
   other); a section-level % applies to every line in the section, capped at what each line has left. Approval posts
   the increment into the running total (Odoo "Property Developer": Progress Increment → Cumulated Progress →
   Update Quantity Delivered).
3. **Net = to-date minus previously certified:** (gross to date − retention to date − advance recovered to date −
   deductions/penalties to date) − net already certified. Errors self-correct in the next certificate; approved
   certificates are never edited.
4. **Contract type sets the over-100% rule:** fixed price → blocked above 100% / contract qty unless an approved
   variation raises it; re-measured (open book) → warning.
5. **One supplier invoice per approved certificate** for its net — not one purchase order per certificate. Payments link
   one-to-many; the certificate shows net / paid / remaining / status (unpaid, partial, paid). Nothing is billed
   without an approved certificate (OCA `purchase_work_acceptance`).
6. **Retention** is held in a retention-payable account per contract (running total on the contract) and released by
   a release certificate (typically half at handover, the rest after the defects period) — replaces the insurance
   order lines (OCA `account_invoice_payment_retention`).
7. **Advance payment** to a contractor is recorded on the contract and recovered automatically from each certificate,
   optionally capped at the contract total (OCA `purchase_deposit`).
8. **Delay penalties** from a due date on the certificate: late days × rate, adjustable (OCA
   `purchase_work_acceptance_late_fines`).
9. **Variations (change orders)** are separate documents with their own approval; contract value = original +
   approved variations (Odoo adds a second order to the same project).
10. **Numbering per contract** ("C-012 / certificate 4"), which removes the per-party serial problem.
11. **Status bar + approval queue:** Draft → Submitted (site engineer) → Verified (QS/technical) → Approved (PM/finance)
    → Billed → Paid; reject with a reason; locked after approval; feeds Approval Q (sheet #21). Printed progress
    report for signature (Odoo's Progress Approver + Print Progress Report).
12. **Commitments for budget vs actual:** contract value not yet certified = committed; certificates = actual. Show
    contracted / certified / paid / remaining per project before the budget module exists (also answers the project
    credit limit, #34). Budget check rule (client): < 90% pass, 90–100% warn + PM/FM approval, > 100% block — between
    Odoo Enterprise (warn only, red line) and OCA `budget_control` (blocks; balance = allocated − actual − commitments).
13. **Unit-rate build-up and cost nature:** work items composed of material / labour cost lines (cost, price, margin;
    reusable templates) and a cost-nature (material / labour) analysis — feeds "budget vs actual by item".
14. **Site remarks / snag list** per project or unit with stages (handover to buyers) — not on the client list yet.

**Unit sales side (same study):** installments due on a construction milestone, not only a date (Odoo milestone
billing); the payment plan built and previewed before posting (#38; no Apps Store module documents restructuring an
existing plan); unit status bar available / reserved / sold with smart buttons to sales request, payments and
commissions.

Sources: Odoo `github.com/odoo/industry` 19.0 (`construction`, `construction_developer` "Property Developer",
`real_estate`, `industry_real_estate`); OCA `purchase-workflow`, `account-invoicing`; Ecosoft `budgeting`; Apps Store
IPC modules (`ipc_interim_payment_certificates_odoo`, `bb_ipc_contracts`) and property modules
(`property_management_rental_sales`, `in_payment_plane`).

## Build Rules
- NEVER run `dotnet build`, `dotnet run`, `dotnet watch`, or any build/compile command after modifying files
- NEVER run `npm run build`, `npm start`, or any frontend build commands after modifying files
- After making code changes, stop and report what was changed — do not attempt to verify by building
- The developer will manually re-run/re-debug after all changes are complete

## Local Dev Login (browser verification only)
- Only use when the developer explicitly asks for a live browser check (e.g. via chrome-devtools tools) against the already-running local dev server (`http://localhost:3000`, API on `:5001`). Never start the dev server yourself — see Build Rules above.
- Email: `eradwan1967@gmail.com`
- Password: `Pa$$w0rd`