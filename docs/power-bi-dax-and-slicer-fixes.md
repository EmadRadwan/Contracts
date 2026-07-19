# Power BI: Fact_GL_Transactions Filter, Total_TTD Double-Counting Fix, and Custom Time Period Slicer

Follow-on to `banks-trial-balance-vs-powerbi-findings.md` and the two
`gl-account-transaction-details-*` documents. Those covered the C# side (Trial Balance /
GL Account Transaction Details modal). This document covers the Power BI side — the
`Projects-06-Jul` report/semantic model — including the same double-counting bug found and
fixed in DAX form, plus a new slicer designed to match Trial Balance's exact reporting periods.

## 1. `Fact_GL_Transactions.sql` — posted-only filter added

`API/Sql/Production/Fact_GL_Transactions.sql` originally had no `WHERE` clause at all (see
`banks-trial-balance-vs-powerbi-findings.md` §3) — every entry line, posted or not, ACTUAL or
not. Two changes made across this and the prior session:

1. Added `t.IS_POSTED` to the SELECT list (so the model could see it).
2. Added `WHERE t.IS_POSTED = 'Y'` — matches the same filter the C# handlers already use
   (`act.IsPosted == "Y"`). Requested explicitly once the Power BI visuals that had been relying
   on unposted transactions being present were updated to no longer need them.

**Not yet run against the database** — this is a `DROP VIEW`/`CREATE VIEW` script; applying it
and refreshing the Power BI dataset is a manual step.

## 2. `Total_TTD` / `Total_TTD_Raw` had the exact same double-counting bug as the C# fix — now fixed

### The bug

```dax
Total_TTD = CALCULATE([Total_FTP], DATESBETWEEN(DateTbl[Date], [MinDateAcross], [MaxDate]))
```

`MinDateAcross` ignores all filters and resolves to the literal earliest date in `DateTbl`
(2024-01-01, per the table's partition — the table actually extends to 2040-12-31). So
`Total_TTD` summed *every* row for an account, including the `OPENING_BALANCE` entry's own
amount **and** every individual pre-reset transaction it was meant to summarize (the view has no
date truncation). Reconstructing CIB's numbers by hand confirmed this exactly reproduces the
original buggy Excel export figure:

```
+11,118,958.00  (the OPENING_BALANCE entry itself, counted as an ordinary row)
+11,836,074.00  (the 41 other pre-reset transactions, net debit)
 -8,493,539.10  (true FY2026 activity, net credit)
= 14,461,492.90   ← matches the original wrong Excel header exactly
```

Since the Banks page's KPI cards aggregate four such accounts (110100, 111010, 110300, 110200),
this bug compounded across all four — very likely the root cause of the entire discrepancy that
started this investigation.

### The fix

New helper measure and two measures redefined **in place** (same names, same `lineageTag`s, so
everything already referencing `[Total_TTD]`/`[Total_TTD_Raw]` inherits the fix automatically —
`Total_TTD_Opening`, `Total_TTD_Average`, `Total_TTD_PP`, `PoP_Growth_TTD`, `Total_TTD_Opening_Raw`,
`'Opening Balance Label'`, `'Closing Balance Label'`, and most of the Balance Sheet page's
measures — `Current Assets`, `Current Liabilities`, `Inventory`, `Total Debt`, `Total Equity`,
`Total Assets`, `Capital Employed`, `Receivables`, `Payables`, `BalanceSheetValue`, and related
ratios — all inherit this fix, not just the Banks page):

```dax
// New, in Fact_GL_Transactions.tmdl
ResetDate =
CALCULATE(
    MAX(Fact_GL_Transactions[transaction_date]),
    Fact_GL_Transactions[ACCTG_TRANS_TYPE_ID] = "OPENING_BALANCE",
    ALL(DateTbl),
    Fact_GL_Transactions[transaction_date] <= [MaxDate]
)

// Redefined, same name/lineageTag
Total_TTD =
VAR ResetDate = [ResetDate]
VAR OpeningAmount =
    IF(ISBLANK(ResetDate), 0,
        CALCULATE([Total_FTP], ALL(DateTbl),
            Fact_GL_Transactions[ACCTG_TRANS_TYPE_ID] = "OPENING_BALANCE",
            Fact_GL_Transactions[transaction_date] = ResetDate))
VAR PeriodStart = IF(ISBLANK(ResetDate), [MinDateAcross], ResetDate)
VAR PeriodActivity =
    CALCULATE([Total_FTP], ALL(DateTbl),
        Fact_GL_Transactions[ACCTG_TRANS_TYPE_ID] <> "OPENING_BALANCE",
        Fact_GL_Transactions[transaction_date] >= PeriodStart,
        Fact_GL_Transactions[transaction_date] <= [MaxDate])
RETURN OpeningAmount + PeriodActivity

// Total_TTD_Raw redefined identically, substituting Total_FTP_Raw for Total_FTP
```

Same shape as the C# fix: bound to `[since last reset, MaxDate]` when a reset entry exists;
falls back unchanged to the original lifetime-cumulative sum when it doesn't (correct for
accounts with no `OPENING_BALANCE` entry, same fallback logic as the C# side).

**Validated by hand against CIB:** `ResetDate` → 2025-12-30, `OpeningAmount` → +11,118,958.00,
`PeriodActivity` → -8,493,539.10, total → **2,625,418.90** — matches the C#-side fix exactly.

**Status:** applied to `Fact_GL_Transactions.tmdl`. Not yet opened/refreshed in Power BI
Desktop — verify the Banks page's CIB Closing Balance Label reads 2,625,418.90 (not
14,461,492.90), and spot-check one Balance Sheet figure, since this touches more than just the
Banks page.

## 3. `Dim_CustomTimePeriod`-backed slicer — matching Power BI's periods to Trial Balance's exactly

### Why

Checked what Trial Balance can actually be run against: `CUSTOM_TIME_PERIOD` has exactly **6
rows** — `FISCAL_YEAR` 2025, `FISCAL_YEAR` 2026, and `FISCAL_QUARTER` 2026/Q1–Q4 (2025 has no
quarterly breakdown; there are **zero** `FISCAL_MONTH` records anywhere). The Banks page's
existing date slicer is a generic `DateTbl` Year/Quarter/Month/Date hierarchy — drilling it to
Month or Date produces a Power BI number with **no Trial Balance equivalent to check it
against**, and the alignment between calendar quarters and fiscal quarters is currently only
*coincidental* (it works because the fiscal calendar happens to be standard Jan–Dec).

### What was built (semantic model layer — done)

- **New table `Dim_CustomTimePeriod`** (`Dim_CustomTimePeriod.tmdl`) — sourced directly from the
  same `CUSTOM_TIME_PERIOD` MySQL table Trial Balance reads (`PERIOD_NAME`, `FROM_DATE`,
  `THRU_DATE`, filtered to `FISCAL_YEAR`/`FISCAL_QUARTER`, sorted by `FROM_DATE`). Deliberately
  **disconnected** — no relationship to `DateTbl` — because year and quarter records cover
  overlapping date ranges, which a normal 1:many relationship can't represent.
- **`SelectedPeriodFromDate` / `SelectedPeriodThruDate`** — `SELECTEDVALUE(...)`, blank unless
  exactly one period is selected in a slicer bound to this table.
- **`MaxDate` (DateTbl.tmdl), redefined**: uses the selected period's `THRU_DATE` when active,
  else falls back to its original `MAX(DateTbl[Date])` behavior — safe everywhere else in the
  report, since `Dim_CustomTimePeriod` stays blank on any page that doesn't have the new slicer.
- **Four new measures on `Fact_GL_Transactions`**, mirroring Trial Balance's four header fields
  exactly: `'Selected Period Opening Balance'` (balance as of the day before the selected
  period's `FROM_DATE`, same reset-aware/fallback logic as `Total_TTD`), `'Selected Period Debit
  Carried'` / `'Selected Period Credit Carried'` (strictly bounded `[FROM_DATE, THRU_DATE]`,
  excluding `OPENING_BALANCE`-typed entries — same math as the C# `postedDebits`/`postedCredits`),
  `'Selected Period Ending Balance'` (= `Total_TTD_Raw`, now automatically correct), plus
  formatted `'Selected Period Opening/Closing Balance Label'` variants. All `BLANK()` when no
  period is selected.
- Registered in `model.tmdl` (`ref table Dim_CustomTimePeriod` + `PBI_QueryOrder`).

### What's NOT done — a Power BI Desktop step, not more code

Deliberately did not hand-edit the report's visual JSON (slicer placement + card field bindings)
— too easy to get subtly wrong with no way to render/verify it. To finish:

1. Add a **Slicer** visual to the Banks page, field = `Dim_CustomTimePeriod[PERIOD_NAME]`.
2. Point the "Cash in Banks" cards at the new measures: Opening → `'Selected Period Opening
   Balance Label'`, Closing → `'Selected Period Closing Balance Label'`, Total → `'Selected
   Period Debit Carried'`/`'Selected Period Credit Carried'` (confirm which the existing "Total"
   card currently maps to before swapping).

**Not yet migrated:** the clustered bar chart ("أرصدة البنوك الحالية") and the detail
transaction table on the Banks page still follow the *old* calendar Year Hierarchy slicer — they
were not rewired to the new measures. For full-page consistency, either set both slicers to
matching ranges, or migrate those two visuals in a follow-up pass.

## 4. Slicer UX advice (given before building #3, still relevant)

- Cap the visible granularity to Year/Quarter — Month/Date have no Trial Balance counterpart.
- Guard against a full-year selection silently reaching into the future — `DateTbl` extends to
  2040, and a real future-dated posted transaction already exists in production (§ GL 111010 in
  `gl-account-transaction-details-broader-impact-scan.md`), so "Year 2026" as a whole would
  include it.
- Surface the resolved "as of" date on the page (e.g. a text card showing `[MaxDate]` /
  `[SelectedPeriodFromDate]`) so a user can visually cross-check against Trial Balance's own
  `FromDate`/`ThruDate` for the period they ran there.
- Single-select only — the `ResetDate`/`Total_TTD` logic assumes one coherent `MaxDate`.

The `Dim_CustomTimePeriod` slicer (§3) addresses the first two points structurally, once the
Desktop step is completed.

## 5. Summary of file changes this session

| File | Change | Status |
|---|---|---|
| `API/Sql/Production/Fact_GL_Transactions.sql` | Added `IS_POSTED` column, then `WHERE t.IS_POSTED = 'Y'` | Written, not yet run against DB |
| `Fact_GL_Transactions.tmdl` | `ResetDate` measure added; `Total_TTD`/`Total_TTD_Raw` redefined; 6 new `'Selected Period ...'` measures added | Written, not yet opened in Desktop |
| `DateTbl.tmdl` | `MaxDate` redefined to prefer `Dim_CustomTimePeriod` selection | Written, not yet opened in Desktop |
| `Dim_CustomTimePeriod.tmdl` | New table | Written, not yet opened in Desktop |
| `model.tmdl` | New table registered | Written, not yet opened in Desktop |

## 6. Next steps (continue here on the other machine)

1. Open the `.pbip` in Power BI Desktop, refresh, confirm it loads without errors (new table's M
   query, new measures).
2. Verify CIB's Closing Balance Label = 2,625,418.90; spot-check a Balance Sheet figure.
3. Add the `Dim_CustomTimePeriod` slicer to the Banks page and swap the three KPI cards per §3.
4. Decide whether to migrate the bar chart / detail table to the new period measures too, or
   leave them on the calendar slicer for now.
5. Once satisfied, run the `Fact_GL_Transactions.sql` script against the database (§1) and do a
   final full refresh.
