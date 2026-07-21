# Trial Balance Web Data Source — Projects-21-Jul Power BI Report

## Goal

Add a live web data source to the `Projects-21-Jul` Power BI project (PBIP format) that
reads the ERP's Trial Balance report for a given company directly over HTTP, instead of
going through the MySQL replica used by every other table in this model.

Company used throughout: `6010` (hardcoded in the M query — see "Known limitations" below).

## Status: uncommitted

All changes below are **uncommitted local changes** on this machine. Before switching to
another machine, commit and push this branch, or copy the working tree — otherwise the
other machine won't see any of this.

```
 M API/Controllers/Accounting/CustomTimePeriodsController.cs
A  Application/Accounting/OrganizationGlSettings/GetCurrentTimePeriod.cs
AM Projects-21-Jul/Projects-21-Jul.SemanticModel/definition/model.tmdl
AM Projects-21-Jul/Projects-21-Jul.SemanticModel/definition/relationships.tmdl
?? Projects-21-Jul/Projects-21-Jul.SemanticModel/definition/tables/Fact_TrialBalance.tmdl
```

No EF Core migration was needed — the new backend endpoint only reads existing tables
(`CustomTimePeriods`), it doesn't add/change any schema.

---

## 1. Why a new backend endpoint was needed

The original ask was to hit:

```
http://129.146.22.240:5100/api/trialBalance/Company/6010/getTrialBalanceReport
```

That URL doesn't match any real route — it's missing a required path segment. The actual
controller is `API/Controllers/Accounting/TrialBalanceController.cs`, with two relevant
actions:

```
GET api/TrialBalance/{selectedAccountingCompanyId}/{customTimePeriodId}/getTrialBalanceReport      [Authorize]
GET api/TrialBalance/{selectedAccountingCompanyId}/{customTimePeriodId}/generateTrialBalanceReport [AllowAnonymous]
```

Two problems for a Power BI source:
1. Every route requires a `customTimePeriodId`, not just a company id.
2. The "real" action is `[Authorize]` (needs a JWT bearer token) — Power Query has no
   interactive login step, so scheduled/unattended refresh can't use it. The sibling
   `generateTrialBalanceReport` action returns identical data and is `[AllowAnonymous]`,
   so that's the one the M query calls.

There was no existing way to resolve "the current period for company X" without auth
either — `CustomTimePeriod` has no `IsCurrent` flag (only `IsClosed` "Y"/"N" +
`FromDate`/`ThruDate`), and the only listing endpoints
(`listCustomTimePeriodsLov`, OData `customTimePeriodRecords`) are authenticated and not
even filtered by organization. So a second small anonymous endpoint was added.

## 2. Backend changes

### New CQRS query — `Application/Accounting/OrganizationGlSettings/GetCurrentTimePeriod.cs`

Namespace `Application.Shipments.OrganizationGlSettings` (matches the existing sibling
files in this same folder — `ListCustomTimePeriodsLov.cs`, `GetTrialBalanceReport.cs` —
which is a pre-existing naming quirk in this codebase, not something introduced here).

Picks the org's open period: `IsClosed != "Y"`, ordered by `FromDate` descending, first
match. Returns the existing `CustomTimePeriodDto` (reused, not a new DTO).

### New controller action — `API/Controllers/Accounting/CustomTimePeriodsController.cs`

```csharp
[HttpGet("{organizationPartyId}/getCurrentTimePeriod")]
[AllowAnonymous]
public async Task<IActionResult> GetCurrentTimePeriod(string organizationPartyId)
{
    return HandleResult(await Mediator.Send(new GetCurrentTimePeriod.Query { OrganizationPartyId = organizationPartyId }));
}
```

Full route: `GET api/CustomTimePeriods/{organizationPartyId}/getCurrentTimePeriod`

### Response shapes (System.Text.Json, PascalCase — no camelCase policy configured in `Program.cs`)

`getCurrentTimePeriod` → `CustomTimePeriodDto`:
```
CustomTimePeriodId, ParentPeriodId, PeriodTypeId, PeriodTypeDescription,
PeriodNum, PeriodName, FromDate, ThruDate, IsClosed
```

`generateTrialBalanceReport` → `TrialBalanceContext`:
```
TrialBalanceContext
├── PartyNameList        : string[]
├── PostedDebitsTotal     : decimal
├── PostedCreditsTotal    : decimal
└── AccountBalances       : AccountBalance[]   (flat list, no parent/child nesting)
      ├── GlAccountId
      ├── AccountCode
      ├── AccountName
      ├── OpeningBalance
      ├── PostedDebits
      ├── PostedCredits
      └── EndingBalance
```

Only accounts with non-zero `EndingBalance`/`PostedDebits`/`PostedCredits` are included;
ordered by `AccountCode`.

### Security note

This adds a **second** `[AllowAnonymous]` surface (alongside the pre-existing
`generateTrialBalanceReport`). Both now leak org-scoped GL data with no auth check to
anyone who can reach the host — acceptable here because it mirrors a pattern the codebase
already had and was an explicit tradeoff for unattended Power BI refresh, but worth
knowing if this endpoint's exposure is ever reconsidered.

---

## 3. Power BI semantic model changes

New table: `Projects-21-Jul.SemanticModel/definition/tables/Fact_TrialBalance.tmdl`

Columns: `GlAccountId, AccountCode, AccountName, OpeningBalance, PostedDebits,
PostedCredits, EndingBalance` (the four balance columns are `double`, `summarizeBy: sum`).

Registered in `model.tmdl` (`ref table Fact_TrialBalance` + added to the
`PBI_QueryOrder` annotation list).

Relationship added in `relationships.tmdl`:
```
Fact_TrialBalance.GlAccountId  →  Dim_gl_account.GL_ACCOUNT_ID
```
This lets the new table slice/group by the existing bilingual account hierarchy
(`ACCOUNT_NAME_ARABIC`, `CLASS_AR`, `SUBCLASS_AR`, `REPORT_AR`, ...) already used by the
P&L / balance sheet / trial balance pages, instead of sitting as an isolated table.

### The M query (`Fact_TrialBalance` partition)

```m
let
    ApiHost = "http://129.146.22.240:5100",
    CompanyId = "6010",
    CurrentPeriodJson = Json.Document(Web.Contents(ApiHost, [RelativePath = "api/CustomTimePeriods/" & CompanyId & "/getCurrentTimePeriod"])),
    CustomTimePeriodId = CurrentPeriodJson[CustomTimePeriodId],
    TrialBalanceJson = Json.Document(Web.Contents(ApiHost, [RelativePath = "api/TrialBalance/" & CompanyId & "/" & CustomTimePeriodId & "/generateTrialBalanceReport"])),
    AccountBalances = TrialBalanceJson[AccountBalances],
    ToTable = Table.FromList(AccountBalances, Splitter.SplitByNothing(), null, null, ExtraValues.Error),
    Expanded = Table.ExpandRecordColumn(ToTable, "Column1", {"GlAccountId", "AccountCode", "AccountName", "OpeningBalance", "PostedDebits", "PostedCredits", "EndingBalance"}),
    ChangedType = Table.TransformColumnTypes(Expanded, {{"GlAccountId", type text}, {"AccountCode", type text}, {"AccountName", type text}, {"OpeningBalance", type number}, {"PostedDebits", type number}, {"PostedCredits", type number}, {"EndingBalance", type number}})
in
    ChangedType
```

**Why `Web.Contents(ApiHost, [RelativePath = ...])` instead of `Web.Contents(ApiHost & "...")`:**
Power Query's Formula Firewall blocks a query step that builds a data-source URL out of a
value computed in an earlier step (here, `CustomTimePeriodId` came from the first web
call) — you'd get `Formula.Firewall: Query ... may not directly access a data source`.
Keeping the base URL (`ApiHost`) a static literal and passing the dynamic part through
the `RelativePath` record option is the standard, firewall-safe way to chain two API
calls in Power Query. Don't revert this to string concatenation.

---

## 4. How to use it in Power BI Desktop

1. **Open** `Projects-21-Jul.pbip`. Since the `.tmdl` files were hand-edited outside the
   Desktop UI, Desktop picks them up fresh on open — `Fact_TrialBalance` shows up in the
   Fields pane / Model view with no data loaded yet.
2. **Refresh**: right-click `Fact_TrialBalance` in the Fields pane → Refresh (or Home →
   Refresh for the whole model). This is what triggers the two chained web calls.
3. **First-run prompts**:
   - Connection method for `http://129.146.22.240:5100` → choose **Anonymous** (both
     endpoints are `[AllowAnonymous]`, no credentials needed).
   - Privacy level for that data source → **Organizational** or **Public**, either is
     fine; nothing in this query merges data across sources, so privacy-level conflicts
     with the MySQL source aren't a concern here.
   - Both are one-time per data source; reset them later via **File → Options and
     settings → Data source settings** if needed.
4. **Verify**: `Fact_TrialBalance` should populate with one row per non-zero GL account
   for company `6010`'s current open period.
5. **Build visuals**: drag `AccountCode`/`AccountName` into a Table/Matrix as rows, the
   four balance columns as values. Pull in `Dim_gl_account` fields for Arabic labels and
   grouping via the relationship added above — either extend the existing `ميزان
   المراجعة` (Trial Balance) report page or add a new one.
6. **Publishing**: `129.146.22.240` is a public IP (not localhost), so scheduled refresh
   in the Power BI Service should work without an on-premises data gateway — just
   reconfirm the Anonymous credential + privacy level in the Service's dataset settings
   after first publish.

### Troubleshooting

| Symptom | Likely cause |
|---|---|
| `Formula.Firewall` error | M code reverted to string-concatenated URLs instead of `RelativePath` — check Transform Data / the `.tmdl` source matches section 3 above. |
| Empty table / no error | No open period for company `6010` (`GetCurrentTimePeriod` found nothing — check `CustomTimePeriods.IsClosed`/`OrganizationPartyId` data for that org). |
| 401 Unauthorized | Hit the wrong (authenticated) route by mistake — must be `generateTrialBalanceReport` / `getCurrentTimePeriod`, not `getTrialBalanceReport`. |
| Timeout / can't reach host | API server at `129.146.22.240:5100` down, or not reachable from the refreshing machine/network. |

---

## 5. Known limitations / possible follow-ups

- **`CompanyId = "6010"` is hardcoded** in the M query. To report on a different company,
  either edit that literal in `Fact_TrialBalance.tmdl` and re-import, or (better, not yet
  done) turn it into a proper Power BI parameter so it can be changed from the UI.
- **No date/period filter beyond "current open period"** — there's no way yet to pull a
  historical trial balance for a closed period through this table; it always reflects
  whatever `GetCurrentTimePeriod` resolves to at refresh time.
- Backend code has **not been built/compiled** on this machine (per project convention,
  `dotnet build` is not run automatically after code changes) — compile and review before
  relying on it.
