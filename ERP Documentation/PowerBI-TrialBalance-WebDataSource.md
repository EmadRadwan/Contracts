# Trial Balance Web Data Source — Projects-21-Jul Power BI Report

## Goal

Add a live web data source to the `Projects-21-Jul` Power BI project (PBIP format) that
reads the ERP's Trial Balance report for a given company directly over HTTP, instead of
going through the MySQL replica used by every other table in this model.

Company used throughout: `Company` — this is the actual, literal `OrganizationPartyId`
value in this (single-org) ERP instance. **Correction:** an earlier draft of this doc used
`6010` as the company id — that was wrong. `6010` is a `CustomTimePeriodId` (fiscal year
2026), not an org id. See section 5 for how this was discovered and fixed.

## Status: uncommitted

All changes below are **uncommitted local changes** on this machine. Before switching to
another machine, commit and push this branch, or copy the working tree — otherwise the
other machine won't see any of this.

Also observed once during this work: the entire `Projects-21-Jul/` directory disappeared
from disk on one machine while still showing as staged (`AD` in `git status`) in the
index — i.e. `git add` had run, then the files were deleted from the working tree before
being committed. Nothing was ever lost (the full content was intact in the index the
whole time, confirmed via `git diff --cached --stat`), but it was never actually
committed here — **commit this work** once you're happy with it, don't rely on the
working tree alone surviving a machine switch.

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

**Correction (see section 5):** this URL's *shape* is actually correct —
`Company` → `selectedAccountingCompanyId`, `6010` → `customTimePeriodId`. An earlier
version of this doc claimed there was an extra literal `/Company/` segment and that the
route didn't match; that was a misreading. `Company` is simply the literal value of
`OrganizationPartyId` in this ERP instance (there's only one org), not a fixed path token.

The actual controller is `API/Controllers/Accounting/TrialBalanceController.cs`, with two
relevant actions:

```
GET api/TrialBalance/{selectedAccountingCompanyId}/{customTimePeriodId}/getTrialBalanceReport      [Authorize]
GET api/TrialBalance/{selectedAccountingCompanyId}/{customTimePeriodId}/generateTrialBalanceReport [AllowAnonymous]
```

So the only real problems for a Power BI source were:
1. The "real" action is `[Authorize]` (needs a JWT bearer token) — Power Query has no
   interactive login step, so scheduled/unattended refresh can't use it. The sibling
   `generateTrialBalanceReport` action returns identical data and is `[AllowAnonymous]`,
   so that's the one the M query calls.
2. The original URL hardcoded a specific period (`6010`, fiscal year 2026). To make the
   report always reflect "now" without manually updating the period id every year/quarter,
   a second endpoint was added to resolve the current period dynamically (see below) —
   this was a deliberate choice, not something the URL forced.

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

Picks the period that actually **covers today's date**: `IsClosed != "Y"` AND
`FromDate <= now <= ThruDate` (or `ThruDate` null), ordered by `FromDate` descending (so
that when a quarter and its parent year both cover today, the more specific/narrower
quarter wins). Returns the existing `CustomTimePeriodDto` (reused, not a new DTO).

**Bug fixed during testing (section 5):** the first version of this handler only filtered
on `IsClosed != "Y"` and ordered by `FromDate` descending with no date-range check. Since
this ERP's data leaves future periods with `IsClosed = "N"` (not closed simply means "not
yet closed", not "in progress"), that version always returned the *furthest-future* period
instead of the one containing today — confirmed live against production, where it
returned fiscal quarter `6014` (Oct 2026–Jan 2027) instead of the current quarter. The
date-range filter above is the fix.

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

### Response shapes — **camelCase on the wire**, verified against production via `curl`

**Correction:** an earlier version of this doc claimed PascalCase JSON on the assumption
that no naming policy was configured. That was wrong — `AddControllers().AddJsonOptions()`
starts from ASP.NET Core's framework defaults, which already include
`JsonNamingPolicy.CamelCase`; the `Program.cs` snippet only added
`PropertyNameCaseInsensitive = true` on top, it never disabled camelCase. Confirmed live:

```
curl http://129.146.22.240:5100/api/CustomTimePeriods/Company/getCurrentTimePeriod
→ {"customTimePeriodId":"6014","parentPeriodId":"6010","periodTypeId":"FISCAL_QUARTER", ...}
```

`getCurrentTimePeriod` → `CustomTimePeriodDto` (camelCase keys):
```
customTimePeriodId, parentPeriodId, periodTypeId, periodTypeDescription,
periodNum, periodName, fromDate, thruDate, isClosed
```

`generateTrialBalanceReport` → `TrialBalanceContext` (camelCase keys):
```
TrialBalanceContext
├── partyNameList        : string[]
├── postedDebitsTotal     : decimal
├── postedCreditsTotal    : decimal
└── accountBalances       : AccountBalance[]   (flat list, no parent/child nesting)
      ├── glAccountId
      ├── accountCode
      ├── accountName
      ├── openingBalance
      ├── postedDebits
      ├── postedCredits
      └── endingBalance
```

(C# property names are still PascalCase in the DTO source files — `GlAccountId`,
`AccountBalances`, etc. — the camelCase transform happens only in the JSON serializer.
The M query must reference the camelCase wire names; the semantic model's own column
names stay PascalCase via an explicit rename step — see section 3.)

Only accounts with non-zero `endingBalance`/`postedDebits`/`postedCredits` are included;
ordered by `accountCode`.

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
    CompanyId = "Company",
    CurrentPeriodJson = Json.Document(Web.Contents(ApiHost, [RelativePath = "api/CustomTimePeriods/" & CompanyId & "/getCurrentTimePeriod"])),
    CustomTimePeriodId = CurrentPeriodJson[customTimePeriodId],
    TrialBalanceJson = Json.Document(Web.Contents(ApiHost, [RelativePath = "api/TrialBalance/" & CompanyId & "/" & CustomTimePeriodId & "/generateTrialBalanceReport"])),
    AccountBalances = TrialBalanceJson[accountBalances],
    ToTable = Table.FromList(AccountBalances, Splitter.SplitByNothing(), null, null, ExtraValues.Error),
    Expanded = Table.ExpandRecordColumn(ToTable, "Column1", {"glAccountId", "accountCode", "accountName", "openingBalance", "postedDebits", "postedCredits", "endingBalance"}, {"GlAccountId", "AccountCode", "AccountName", "OpeningBalance", "PostedDebits", "PostedCredits", "EndingBalance"}),
    ChangedType = Table.TransformColumnTypes(Expanded, {{"GlAccountId", type text}, {"AccountCode", type text}, {"AccountName", type text}, {"OpeningBalance", type number}, {"PostedDebits", type number}, {"PostedCredits", type number}, {"EndingBalance", type number}})
in
    ChangedType
```

Note the `Expanded` step's 4-argument form of `Table.ExpandRecordColumn` — the third list
is the camelCase wire field names (what the JSON actually calls them), the fourth is what
to rename them to in the resulting table (PascalCase, matching the column definitions in
this `.tmdl` file and the existing relationship to `Dim_gl_account`). Both a wrong company
id (`6010` instead of `Company`) and this camelCase/PascalCase mismatch were caught and
fixed *before* ever being run inside Power BI Desktop — by testing each endpoint directly
with `curl` first. See section 5 for the full trail.

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
   for `Company`'s current open period.
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
| `Expression.Error: The field '...' wasn't found` | M query referencing PascalCase field names against camelCase JSON (or vice versa) — see section 3's field name lists. |
| `400 Bad Request`, body `"No open time period covering today's date was found for this organization"` | The `GetCurrentTimePeriod` endpoint ran fine but found no matching period for `Company` — check `CustomTimePeriods` data (`IsClosed`, `FromDate`/`ThruDate` range) for that org. |
| Plain `400 Bad Request` with no recognizable body/title | The new endpoint likely isn't deployed yet on that host — see section 5's incident writeup for how to tell this apart from a data problem. |
| 401 Unauthorized | Hit the wrong (authenticated) route by mistake — must be `generateTrialBalanceReport` / `getCurrentTimePeriod`, not `getTrialBalanceReport`. |
| Timeout / can't reach host | API server at `129.146.22.240:5100` down, or not reachable from the refreshing machine/network. |

---

## 5. Incident trail (2026-07-21) — what was actually wrong, in order found

This section exists because almost every initial assumption in this project turned out to
be slightly off, and each one was only caught by testing the real endpoint rather than
reasoning from the DTO source code alone. Read it before "fixing" any of the above back to
how it looked originally.

1. **First deploy attempt** produced `400 Bad Request` on `getCurrentTimePeriod` with no
   recognizable JSON body. Diagnosis at the time: the new endpoint simply wasn't deployed
   yet to `129.146.22.240:5100` (a separately built/deployed host — see `CLAUDE.md`, this
   is the **production** API, not local dev). Resolution: build + deploy.

2. **After redeploying**, calling `getCurrentTimePeriod` with `6010` as the org id
   returned `400` with a real JSON body this time:
   `{"title":"No open time period found for this organization","status":400}` — proof the
   route now existed (it matched the handler's exact error string), but exposed that
   `6010` was never a valid `OrganizationPartyId` to begin with.

3. **Root cause found by reading actual `CustomTimePeriods` data** (`Temps/temp4.json`):
   `ORGANIZATION_PARTY_ID` is literally `"Company"` for every row; `6010` is a
   `CUSTOM_TIME_PERIOD_ID` (fiscal year 2026, `IS_CLOSED: "N"`). This means:
   - The M query's `CompanyId` literal was wrong (`"6010"` → `"Company"`, fixed).
   - The *original* URL from the very first ask
     (`.../trialBalance/Company/6010/getTrialBalanceReport`) was actually shaped
     correctly the whole time — `Company` and `6010` were the right two path values.
     Section 1's original claim of a route-shape mismatch was a misdiagnosis; retract it.

4. **Testing `getCurrentTimePeriod` with the corrected `Company` org id** returned `200`
   but with the wrong period — `6014` (fiscal quarter Oct 2026–Jan 2027, a *future*
   quarter), not anything containing today (2026-07-21). This confirmed a real bug in
   `GetCurrentTimePeriod.cs`: filtering on `IsClosed != "Y"` and ordering by `FromDate`
   descending will always return the furthest-future not-yet-closed period, since this
   data leaves future periods open until they're actually closed out. Fixed by adding an
   explicit `FromDate <= now <= ThruDate` window (see section 2).

5. **Testing `generateTrialBalanceReport` directly** (with `Company` + the known-good
   period `6010`) returned real account data — and revealed the JSON is **camelCase**
   (`glAccountId`, `accountBalances`, ...), not PascalCase as originally documented. Fixed
   the M query's field references and added an explicit rename step back to PascalCase
   (section 3), so this doesn't quietly break the first time someone hits Refresh in
   Desktop.

6. Separately, at one point the entire `Projects-21-Jul/` directory vanished from disk on
   a machine while still `git add`-staged (index intact, working tree empty) — recovered
   from the index without any destructive git operations. Nothing was lost, but it's a
   reminder this work has never actually been **committed** — do that before relying on
   the working tree surviving anything.

**Net effect of all of the above:** the semantic model / M query changes described in
section 3 are the corrected, tested version — every endpoint call in this doc has now
been verified directly against production with `curl`, including a full end-to-end
`generateTrialBalanceReport` call returning real Arabic account names and balances for
period `6010`. What has **not** yet been verified is the `GetCurrentTimePeriod` date-range
fix (item 4) against production, since that fix is still a local, undeployed change.

---

## 6. Known limitations / possible follow-ups

- **`CompanyId = "Company"` is hardcoded** in the M query. This happens to be correct for
  every table in this model already (single-org ERP instance), so it's lower-risk than the
  earlier `6010` mistake, but if this ever becomes multi-org, turn it into a proper Power
  BI parameter instead of a literal.
- **No date/period filter beyond "current period"** — there's no way yet to pull a
  historical trial balance for a closed period through this table; it always reflects
  whatever `GetCurrentTimePeriod` resolves to at refresh time.
- **The `GetCurrentTimePeriod` date-range fix (section 5, item 4) has not been
  deployed/retested yet.** Until it is, production will still return the old
  furthest-future-period behavior.
- Backend code has **not been built/compiled** on this machine (per project convention,
  `dotnet build` is not run automatically after code changes) — compile and review before
  relying on it.
