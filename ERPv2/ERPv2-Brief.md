# ERPv2 — Modernization Brief

**Compiled:** 2026-08-23
**Purpose:** a single self-contained context document for starting a fresh session on the ERPv2 effort — the "bigger follow-on project" applying UI/UX, React, and ASP.NET best practices to this OFBiz-derived ERP.

> **How to use this:** open a new session in `/Users/emadradwan/Documents/ERP/Contracts` and point it at this file. Everything below is either quoted from the original sessions or re-verified against the codebase on 2026-08-23 (each section says which). No other session context is needed.

---

## 0. Provenance — where this came from

Three prior sessions produced the material summarized here.

| # | Session | Date | Subject | Transcript |
|---|---|---|---|---|
| 1 | `1a7ede26-519d-458f-9fec-0b3697d98b32` | 2026-07-17/18 | Frontend audit + five-track UI/UX modernization plan (HIS-integration driven) | **Deleted — no longer on disk** |
| 2 | `232af1de-8441-417e-ac5f-23b015c1731b` | 2026-07-25/26 | Feature-coverage audit v1: this ERP vs. OFBiz vs. Odoo | On disk (405 KB) |
| 3 | `45499d39-3292-4360-9fdb-98770f69ea1d` | 2026-08-06 | Backend architecture review — DDD question, god-services, test story | On disk (324 KB) |

Session 1's transcript has been rotated off disk. What survives of it is the memory note
`~/.claude/projects/-Users-emadradwan-Documents-ERP-Contracts/memory/project_ui_ux_modernization_plan.md`,
reconstructed from the transcript on 2026-08-06 before it was lost. The original deliverables
(a HIS-integration readiness audit with gap matrix and phased roadmap) lived in scratchpad
directories that no longer exist. Section 2 below is the surviving record of that work.

To reopen the two that still exist:
```
claude --resume 232af1de-8441-417e-ac5f-23b015c1731b
claude --resume 45499d39-3292-4360-9fdb-98770f69ea1d
```

---

## 1. The stated goal

From session 1 (2026-07-17), verbatim:

> "we have a plan to have this ERP integrate with one of the HIS systems… we need a plan to modernize the UI to use best practices in UI/UX and data integrations"

and from session 2 (2026-07-25), the wider framing:

> "another big project aiming to apply best practices of UI/UX, React frontend and ASP.NET backend"

So: **HIS integration is one driver, not the only one.** The repo is meant to become the basis
for a larger project built to current best practices on both ends of the stack. That framing is
why the plan below is consolidation-and-strangle rather than rewrite — see §5.

---

## 2. Frontend — the five-track modernization plan (session 1, 2026-07-17)

### Track A — Consolidate, don't replace
Enforce the Kendo/MUI split that CLAUDE.md already states, rather than changing it. Both are
genuinely load-bearing; picking a winner would be a rewrite. Instead: **write down which kit owns
which job** — Kendo for grids, form fields, dropdowns; MUI for layout, typography, dialogs,
buttons — so new screens stop guessing. Then remove the strays: delete Mantine (zero usages),
retire Semantic UI and jQuery, finish Formik→react-hook-form, pick one of lodash/underscore.

### Track B — One data layer, enforced going forward
RTK Query exclusively for new features. Migrate MobX and `agent.ts` call sites feature-by-feature
as those features get touched for other reasons — explicitly **no dedicated big-bang migration**.

### Track C — Wire up real-time on a low-stakes feature first
`AddSignalR()` is registered but nothing consumes it. Build one real hub end-to-end (order-status
or approval-queue updates: hub → RTK Query cache invalidation over the socket → UI updates with
no manual refresh) **before** any HIS event stream needs that same pattern under deadline pressure.

### Track D — A design-token and component layer, not a rewrite
Menu-Exit, Actions-Menu, and the Status Ribbon are well-specified in CLAUDE.md but exist only as
documentation plus a few reference implementations. Promote them to actual shared components with
one implementation each, plus a small design-token set (spacing scale, one date-picker, one
confirmation-dialog pattern) layered on the retained Kendo+MUI pair. Lightweight Storybook instance
for just these shared pieces.

### Track E — Give BI/EPM somewhere to render
No charting library exists in the project at all. Add `@progress/kendo-react-charts` — it leverages
the existing Kendo license and grid investment rather than introducing a sixth UI dependency.
Needed for FIN-BUD / EPM-CPM work regardless of HIS timing.

### Sequencing as originally proposed
Tracks A and B are foundation-clearing work meant to start **immediately and in parallel with
backend work**, not gated on any HIS decision. Phase 0 (~4–6 weeks) was: delete Mantine, scope the
Semantic UI / jQuery retirement, publish the Kendo-vs-MUI ownership rule, build the first real
SignalR hub, stand up Storybook for the three shared components.

---

## 3. Frontend — verified current state (re-checked 2026-08-23)

The July numbers have moved. These are counts taken today against `client-app/src` (922 `.ts`/`.tsx` files):

| Item | Jul 17 | Aug 23 | Note |
|---|---|---|---|
| KendoReact | 429 files | **474 files** | Load-bearing, growing |
| MUI | 463 files | **464 files** | Load-bearing |
| Semantic UI | 14 files | **15 files** | Still in `package.json` |
| Mantine | 0 usages | **0 usages** | Now upgraded to `^9.0.2` in `package.json` while still unused |
| jQuery | present | **0 imports in `src`** | Still in `package.json` + `@types/jquery` — safe to drop |
| Formik | 1 file | **1 file** | `app/common/form/MyTextInput.tsx` |
| react-hook-form | 6 files | **7 files** | |
| MobX | — | **9 files** | 5 stores + 4 components |
| RTK / RTK Query | — | **145 files** (96 with `createApi`) | Clear majority |
| `agent.ts` (Axios) | — | **~78 files** | The real Track B backlog |
| lodash / underscore | both installed | **lodash 3 files, underscore 0** | Drop underscore outright |
| Charting library | none | **none** | Track E untouched |
| `aria-*` coverage | 93/597 (~16%) | **93/922 (~10%)** | Coverage fell as the codebase grew |
| SignalR | `AddSignalR()`, no consumer | **`AddSignalR()` at `API/Extensions/ApplicationServiceExtensions.cs:127`, zero `MapHub` calls anywhere** | Track C untouched |

### The exact Track A work list (verified file paths)

**Semantic UI — 15 files.** Five are error/utility pages, six are Menu components, the rest form inputs:
```
index.tsx
app/common/form/MyTextInput.tsx
features/errors/{TestError,ValidationErrors,NotFound,ServerError}.tsx
features/accounting/fixedAssets/CreateFixedAssetMenu.tsx
features/accounting/globalGlSetting/chartOfAccounts/form/ChartOfAccountsSearchForm.tsx
features/accounting/invoice/menu/InvoiceMenu.tsx
features/parties/form/PartyContactForm.tsx
features/parties/dashboard/{PartyMenu,CreatePartyMenu}.tsx
features/orders/menu/{CreateCustomerRequestMenu,CreateOrderMenu,CreateQuoteMenu}.tsx
```
Note the overlap: the five `*Menu.tsx` files are exactly the Menu-Exit/Actions-Menu pattern from
Track D. **Retiring Semantic UI and building the shared Menu component are the same task** — do
them together rather than converting these twice.

**MobX — 9 files** (`app/stores/{commonBusinessStore,modalStore,geoStore,commonAppStore,userStore}.ts`
plus `ProductListMenu.tsx`, `PartyMenu.tsx`, `CreatePartyMenu.tsx`, `ServerError.tsx`). Three of
those four components are also on the Semantic UI list — the same handful of legacy screens carries
all three debts at once.

**Formik — 1 file** (`MyTextInput.tsx`, which is also Semantic UI). One file closes out that migration.

**lodash — 3 files** (`PurchaseOrderForm.tsx`, `SalesOrderForm.tsx`, `useSalesRequestCalculations.ts`).
`underscore` has zero usages — it and `@types/underscore` can be removed with no code change.

**Zero-risk deletions available today:** `@mantine/core`, `@mantine/dates`, `@mantine/hooks`,
`jquery`, `@types/jquery`, `underscore`, `@types/underscore`.

---

## 4. Backend — architecture review (session 3, 2026-08-06)

Asked as: *"deep, honest advice on the backend architecture I'm currently using and how it compares
to best practices… I hear about DDD and other design patterns."* Context given at the time: a single
developer, a paying client, and a steady stream of bug tickets.

### What's genuinely good — keep it
The CQRS/MediatR skeleton. Thin handlers (`CreateSalesOrder.cs` is 91 lines), `Result<T>` instead of
exceptions-as-control-flow, per-handler transaction scoping, FluentValidation wired via
`AddValidatorsFromAssemblyContaining`. **This is not the bug source.**

### The anemic domain model is fine
All 892 `Domain/` entities are pure data bags — a deliberate 1:1 mirror of the OFBiz schema. That's
a legitimate choice for an OFBiz port. The problem isn't that it's anemic; it's *where the logic that
would live on those entities ended up instead*.

### The god-service problem — the actual bug source
Verified again today:
```
Application/Accounting/Services/GeneralLedgerService.cs    6,481 lines
Application/Manufacturing/ProductionRunService.cs          4,398
Application/Accounting/Services/AcctgReportsService.cs     3,337
Application/Accounting/Services/InvoiceHelperService.cs    2,282
Application/Order/Orders/OrderHelperService.cs             2,190
```
These are modules pretending to be classes. Changing one method in a 6,000-line file with no test
around it, to fix a client-reported bug, is exactly the shape of change that breaks something else
three screens away — which matches the reported symptom.

### Circular dependencies
**8 files in `Application/` use `Lazy<T>` injection** to resolve circular service dependencies
(`GeneralLedgerService`, `PaymentHelperService`, `InvoiceUtilityService`, `CostService`,
`InventoryService`, others). That's DI fighting a layering violation — bidirectional calls with no
clear dependency direction. Each is a place where a change in A silently breaks B.

### Testing is effectively zero
`BusinessTest/Order/Orders/CreateSalesOrderTests.cs` is still the **only** test file in the solution.
It builds `DataContext` against a real local MySQL (`server=localhost;user=root;password=…;database=erp`,
hardcoded) and passes `null` for four of `OrderService`'s constructor dependencies. Any path touching
those throws `NullReferenceException` unrelated to the behavior under test. Treat the starting point
as **zero tests**, not one.

### On DDD — the verdict
Full textbook DDD (aggregate roots enforcing invariants, bounded contexts, domain events) is **not**
worth pursuing wholesale here. 892 entities mirroring a foreign schema, one developer, a client
filing bugs in real time — a Clean/Onion rewrite with aggregates is months of high-regression-risk
work at the moment you can least afford instability. It would make the bug bombardment worse before
it got better.

Worth stealing from DDD, cheaply:
- **Value objects for `Money` / `Quantity` / currency pairs** — kills a whole class of bug (wrong
  currency summed, unit mismatch) at compile time instead of at 2am when a GL imbalance surfaces.
  Pure Application-layer addition, no schema change.
- **A thin behavior layer on 2–3 bug-prone areas only** — order status transitions and GL posting
  rules. Not aggregates; just "this transition is valid only from these states" living in one place
  instead of being reimplemented ad hoc across `OrderService`/`OrderHelperService`.

### Priority order as recommended
1. **Fix the test story before refactoring anything.** Real isolated tests (EF Core InMemory or
   SQLite in-memory, fully mocked collaborators — no nulls, no real MySQL) around GL posting, order
   status transitions, and payment application first — highest blast radius.
2. **Characterization-test-before-fixing, as a habit, not a project.** Next bug in
   `GeneralLedgerService` or `OrderHelperService`: pin the surrounding correct behavior in a test
   *before* the fix, then leave the test in. The suite grows for free alongside work already happening.
3. **Add a MediatR `ValidationBehaviour`.** Today `LoggingBehavior` is the only pipeline behavior;
   validation fires only through the MVC auto-validation filter on HTTP-bound requests. Centralizing
   it means every command is validated identically regardless of entry point.
4. **Strangle the god-services opportunistically** — same rule as the MobX→Redux migration in
   CLAUDE.md. When you're in one of these files for a bug anyway, extract the method(s) you touched
   into a smaller named class with a test. Never schedule "break up GeneralLedgerService" as its own
   project; it will never win against client tickets and doesn't need to.
5. **Resolve the `Lazy<T>` circular deps as you touch those 8 files**, not proactively.

### Explicitly rejected
- Retrofitting a repository/UoW layer over `DataContext` — CLAUDE.md rejects this deliberately, and
  it wouldn't touch the actual bug source.
- Chasing a coverage percentage.
- A DDD / Clean Architecture rewrite.

---

## 5. Feature coverage vs. OFBiz and Odoo (session 2, 2026-07-25)

Deliverable: **Contracts ERP — Feature Coverage Audit v1** →
https://claude.ai/code/artifact/41ee014a-04c8-4e30-b240-2664b2ca06b6

Grounded in counts taken directly from the codebase, not estimates: 892 domain entities, 146 API
controllers, 704 CQRS handlers. Contents: an 11-module coverage matrix rated Deep/Solid/Partial/
Minimal/Absent, scored against OFBiz and Odoo separately; per-module detail of what's implemented
vs. what each reference platform has; and a cross-cutting gap list.

**Headline findings:**
- **Near-parity with OFBiz:** Accounting, Order Management.
- **Widest gap vs. Odoo specifically:** Facility/Warehouse — barcode operations, visual routing.
- **Effectively greenfield regardless of benchmark:** Human Resources, Content Management.
- **Two differentiators neither OFBiz nor Odoo has natively:** the real-estate installment-sales
  workflow (apartments, payment certificates, commissions) and the vehicle service-center module.
- **Zero-code categories:** POS, storefront, marketing automation, quality/maintenance, BI layer,
  workflow builder, helpdesk.

Marked v1 deliberately — prioritization and UI direction were **deferred to a later pass**, which is
part of what ERPv2 has to decide.

---

## 6. Ground rules any ERPv2 work must respect

From `CLAUDE.md` (project instructions, enforced):
- **Never** run `dotnet build` / `run` / `watch`, or `npm run build` / `start` after edits. Make
  changes, stop, report. The developer re-runs manually.
- **Never** apply an EF migration. Generating migration files or a SQL script for review is fine;
  applying is human-only.
- No repository layer — `DataContext` injects straight into handlers. Lazy loading is off; use
  explicit joins or `.Include()`.
- Domain entities live flat in `Domain/`, string IDs (OFBiz heritage), snake_case JSON. DTOs stay
  PascalCase.
- Handlers return `Result<T>`/`Results<T>` — never throw.
- OData list endpoints must be registered in `Program.cs` `GetEdmModel()`.
- `// REFACTOR:` comments mark known debt — don't delete without addressing the cause.
- Prefer RTK Query and Redux for new work; MobX and `agent.ts` are legacy-by-design.
- Do not spawn subagents in this repo unless asked.

---

## 7. Open decisions for ERPv2

None of these were settled in the three sessions above:

1. ~~**Is ERPv2 a new solution or an in-place evolution of this one?**~~ **DECIDED 2026-08-23: in-place.**
   ERPv2 starts from the current solution and evolves it — no greenfield rebuild. Every
   recommendation in this brief stands as written.
2. **Is HIS integration still a live driver, and on what timeline?** Track C's urgency depends on it.
3. **Prioritization across the feature gaps in §5** — deliberately deferred in v1. HR and Content
   Management are the two near-greenfield modules; no decision on whether either is in scope.
4. **Frontend end-state:** does ERPv2 keep the Kendo+MUI pair (Track A's premise), or is a
   single-kit rebuild acceptable now that a v2 is on the table?
5. **Test infrastructure choice** — EF InMemory vs. SQLite in-memory vs. Testcontainers/MySQL was
   never picked. Priority #1 in §4 is blocked on it.

---

## 7b. Companion register

`ERPv2/Contracts-ERP-vs-Odoo-Feature-Register.xlsx` — 265 rows, one per business function,
covering all 737 CQRS operations, scored against Odoo. Follows the `Z ERP (1).xlsx` column
template plus four Odoo columns. Sheets: Feature Register, Odoo Gap Summary (live COUNTIFS
rollup), Legend & Method (what was measured vs. proposed vs. left for you).

---

## 8. Suggested opening prompt for the new session

> Read `ERPv2/ERPv2-Brief.md` — it's the consolidated context from three earlier sessions on
> modernizing this ERP. I want to work on **[track/priority]**. Before proposing anything, check
> §7 open decisions and ask me the ones that would change your approach.

