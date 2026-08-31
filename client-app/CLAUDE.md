# client-app — CLAUDE.md

Frontend-specific conventions and patterns. See the root `CLAUDE.md` for backend architecture and repo-wide rules.

## Stack notes
- **Dual state management** (both coexist):
  - **RTK Query** (`src/app/store/apis/`) — all API calls for modern features
  - **MobX** (`src/app/stores/`) — legacy stores (`userStore`, `modalStore`, etc.)
- **Axios** (`src/app/api/agent.ts`) — legacy HTTP client, still used alongside RTK Query
- Localization to Arabic must alwayes be supported via getTranslatedLabel and @ar.json

## Naming Conventions — Frontend
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

## Role-Based Access Control

Roles are plain strings (ASP.NET Identity `ApplicationRole.Name`).

**Frontend gating is the default, and it is UX only — not a security boundary.** The global filter
(`Program.cs`) requires an authenticated user and nothing more, so for most endpoints any signed-in user
can call the API directly regardless of what the UI shows them. Assume this when reasoning about what a
role actually protects.

**A small number of endpoints are enforced server-side** with `[Authorize(Roles=...)]`. These are the
exception, not the rule:

| Endpoint(s) | Role |
|---|---|
| `LeadsController` — create / edit / assign (6 actions) | `CRM_Leads_Create`, `CRM_Leads_Edit`, `CRM_Leads_Assign` (see `LeadAssignmentConstants`) |
| `PartiesController` — 2 lead-related actions | `CRM_Leads_Create` |
| `AuditActivityRecordsController`, `EntityAuditLogRecordsController` | `Admin` |

Role claims travel on the JWT as `ClaimTypes.Role` (`TokenService`), and nothing overrides
`RoleClaimType` or the inbound claim map — so `[Authorize(Roles=...)]` works as written. Adding it to a
controller is a valid way to make a gate real; the audit endpoints did so because a leak there is not
scoped to one module, it exposes what every user did across the system.

When you add server-side enforcement, keep the role string identical to the one the UI checks with
`<Can perform="...">`, or the menu and the API will disagree.

### Two gating layers per module
Most modules use **two separate role strings**, not one:
1. **Module-level `_View` role** (e.g. `Sales_View`, `Projects_View`, `Accounting_View`) — gates whether the top-nav module link appears at all. Checked in `client-app/src/app/layout/Header.tsx` via a `moduleRoleMap` + `<Can>`.
2. **Feature-level role(s)** (e.g. `CreateSalesRequest`, `ViewSalesRequest`) — gates the specific route/sub-menu once inside the module. Checked via `<RequireRole allowedRoles={[...]} />` (route protection, `client-app/src/app/router/Routes.tsx`) and `<Can perform={...}>` (menu/UI visibility, e.g. `SalesRequestMenu.tsx`).

A user needs **both** roles to actually use a feature. Granting only the feature-level role hides the module from the top nav entirely. Granting only the module-level role shows the nav link, but the inner route silently redirects to `/not-found` when clicked — this looks like a routing bug but isn't; check the user's exact role list first.

### Read-only vs. full-access convention
For read-only access to a feature, add a separate `ViewX` role (e.g. `ViewSalesRequest`) alongside the existing `CreateX` role — don't overload `CreateX`. Enforce read-only client-side by reusing the form's existing highest read-only `editMode` (e.g. `editMode = 3` in `SalesRequestForm`, already used for locked/approved records) rather than adding a parallel code path. Explicitly wrap any status-changing action menu (approve/reset/delete) in `<Can perform="CreateX">` — action-menu components (e.g. `SalesRequestActionsMenu`) are separate from the form and are **not** automatically covered by form-level read-only state.

### Seeding roles — production/staging caveat
In `Persistence/SeedContracts.cs`:
- The `requiredRoles` string array + role-creation loop runs **unconditionally on every startup** — adding a new role name there is enough for the role to exist in any environment (including production) after a deploy.
- The `userRoles` dictionary + `AssignRoles(...)` call that grants specific roles to specific seeded emails only runs **when `AppUserLogins` is completely empty** (fresh DB). On any environment with existing users, editing this dictionary has **no effect** — it never re-runs against existing users.
- **Consequence:** to grant a new role to an existing user outside a fresh DB (staging/production), assign it through the live Users admin UI (`/users` → edit user → add role) — not by editing `SeedContracts.cs`, which only seeds brand-new databases.
