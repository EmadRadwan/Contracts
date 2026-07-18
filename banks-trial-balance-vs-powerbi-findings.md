# Banks: Trial Balance vs. Power BI — Findings

## 1. Power BI "Banks" page (`Projects-06-Jul`)

Page `9f96e84e32eb06c276e5`, displayName **"Banks"** (البنوك).

**Layout:**
- Title textbox: "البنوك"
- Date slicer: Year/Quarter/Month hierarchy from `DateTbl` ("الفترة الزمنية")
- KPI cards grouped by GL sub-account (`Dim_gl_account.SUBACCOUNT_AR` filter on each card):
  - **Cash in Banks** (نقدية في البنوك) — Total, Opening Balance, Closing Balance
  - **Cash on Hand** (نقدية في الصندوق) — Total, Opening Balance, Closing Balance
  - **GL 124410 — Checks under collection** (شيكات تحت التحصيل) — Total, Opening, Closing
  - **GL 440000 — Investment account profits** (أرباح الحسابات الاستثمارية) — `Total_FTP`
  - **GL 600940 — Bank fees** (الرسوم البنكية) — `Total_FTP`
- Clustered bar chart "أرصدة البنوك الحالية" (Current Bank Balances) — `Total_TTD` by `ACCOUNT_NAME_ARABIC`, filtered to Cash-in-Banks. **This is the only visual that breaks a single bank out individually** — the KPI cards aggregate all banks together.
- Detail table "حركات الحسابات البنكية" (Bank Account Transactions) — date, description, entry #, debit/credit flag, amount, account name; filtered to Cash-in-Banks.

**Key measures** (`MeasureTbl.tmdl` / `Fact_GL_Transactions.tmdl`):
- `Total_FTP` = `SUMX(Fact_GL_Transactions, AMOUNT × SIGN_MULTIPLIER × IF(DEBIT_CREDIT_FLAG="C", -1, 1))` — signed period activity over whatever rows are in the current filter context.
- `Total_TTD` = `CALCULATE([Total_FTP], DATESBETWEEN(DateTbl[Date], MinDateAcross, MaxDate))` — forces the date window from the **literal earliest date in the whole calendar table** through `MaxDate` (last date of the current slicer selection). This is a true perpetual running balance built from every transaction row ever loaded into the fact table — **not** dependent on any special "opening balance" entry.
- `'Closing Balance Label'` = formatted `ABS(Total_TTD_Raw)` + مدين/دائن — this is literally the page's "ending balance."
- `'Opening Balance Label'` = `Total_TTD − Total_FTP` (formatted) — opening balance is derived algebraically (closing minus current period activity), not read from a dedicated entry.

## 2. Trial Balance report (`TrialBalance.tsx` → `GetTrialBalanceReport.cs` → `AcctgReportsService`)

Flow: React (`useLazyFetchTrialBalanceReportQuery`) → `GetTrialBalanceReport.Handler` → `AcctgReportsService.ComputeTrialBalance(customTimePeriodId, organizationPartyId)`.

For each GL account linked to the org (via `GlAccountOrganizations`, active during the selected `CustomTimePeriod`), `ComputeGlAccountBalanceForTimePeriod` computes:

- **Opening Balance** = sum of Debits − Credits (or reverse, depending on `IsDebitAccount`), but **only from `AcctgTransEntries` where `AcctgTransTypeId == "OPENING_BALANCE"`**, `IsPosted == "Y"`, `GlFiscalTypeId == "ACTUAL"`, dated on/before `FromDate − 1 tick`.
- **Posted Debits / Posted Credits** = sum of **all** posted, `ACTUAL` entries (any transaction type) dated within `[FromDate, ThruDate]`.
- **Ending Balance** = Opening Balance ± period Debits/Credits (sign per `IsDebitAccount`, based on `GlAccountClass` hierarchy).

**Critical detail:** Opening Balance is *not* "everything that happened before this period" — it only looks at explicit `OPENING_BALANCE`-typed journal entries. The report assumes a proper `OPENING_BALANCE` entry is posted at the start of each fiscal period, equal to the true carried-forward balance.

Also note: Trial Balance explicitly filters `IsPosted == "Y"` and `GlFiscalTypeId == "ACTUAL"` on every query — both for the opening-date sums and the in-period sums.

## 3. Root-cause finding: `Fact_GL_Transactions.sql` had no filtering — and no way to filter

Read `API/Sql/Production/Fact_GL_Transactions.sql`:

```sql
CREATE VIEW Fact_GL_Transactions AS
SELECT ...
FROM ACCTG_TRANS t
JOIN ACCTG_TRANS_ENTRY e ON t.ACCTG_TRANS_ID = e.ACCTG_TRANS_ID
LEFT JOIN (...) ed ON ...;
```

No `WHERE` clause at all — every entry line in the ledger, unconditionally. Two confirmed consequences:

1. **`IS_POSTED` wasn't even selected.** The view exposed `ACCTG_TRANS_TYPE_ID`, `GL_FISCAL_TYPE_ID`, dates, `GL_ACCOUNT_ID`, `DEBIT_CREDIT_FLAG`, `AMOUNT`, etc. — but never `ACCTG_TRANS.IS_POSTED`. The Power BI semantic model therefore had **no way to filter out unposted transactions**, at any layer (view, DAX, or report). Confirmed no compensating filter exists in `report.json` or on the Banks page.
2. **`GL_FISCAL_TYPE_ID` is exposed but never filtered** in the DAX (`Total_FTP` sums every row regardless of this value) or anywhere in the report.

**Net effect:** if a bank's GL account has unposted (draft/pending) journal entries, or entries typed with a non-`ACTUAL` `GL_FISCAL_TYPE_ID`, Power BI's `Total_TTD` includes them while Trial Balance's `EndingBalance` excludes them by design.

## 4. Reconciliation risks, ranked

| # | Cause | Trial Balance | Power BI |
|---|---|---|---|
| 1 | Unposted / non-ACTUAL entries on the account | Excluded (explicit filter) | Included — no column existed to filter them out |
| 2 | Missing/stale `OPENING_BALANCE`-typed entry for the period | Opening balance depends entirely on it | Not needed — accumulates real history from `MinDateAcross` |
| 3 | Period-end date mismatch | `CustomTimePeriod.ThruDate` (ERP fiscal calendar) | `MaxDate` = `MAX(DateTbl[Date])` under the Power BI slicer — a different calendar table |
| 4 | Granularity | One row per GL account — a single bank shows directly | KPI cards aggregate *all* banks under "نقدية في البنوك"; a single bank only shows in the bar chart / detail table |

**Suggested check for one bank:** align `CustomTimePeriod.ThruDate` with the Power BI slicer's `MaxDate`, then compare Trial Balance's Ending Balance to that bank's bar in "أرصدة البنوك الحالية" (not the aggregate KPI cards).

## 5. Change made this session

`API/Sql/Production/Fact_GL_Transactions.sql` — added `t.IS_POSTED` to the SELECT list (line 9). No `WHERE` clause was added, since other existing visuals in the Power BI file currently depend on unposted transactions being present. `GL_FISCAL_TYPE_ID` was already exposed.

```sql
SELECT
    t.ACCTG_TRANS_ID,
    t.ACCTG_TRANS_TYPE_ID,
    t.GL_FISCAL_TYPE_ID,
    t.IS_POSTED,        -- added
    ...
```

This is a `DROP VIEW` / `CREATE VIEW` script, not an EF Core migration — it hasn't been run against any database. The user will decide when to apply it and will handle the posted/fiscal-type filtering logic on the Power BI side later.
