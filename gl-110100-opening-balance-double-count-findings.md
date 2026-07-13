# GL 110100 (البنك التجاري الدولى) — Opening Balance Double-Counted in Period Totals

## 1. Source files

Two exports of the same GL account (110100, "البنك التجاري الدولى" / CIB), pulled the same day, both showing an identical header summary block:

- `Transactions_110100_____________________.xlsx` — 599 transaction rows, earliest date **2026-01-01**
- `Transactions_110100_____________________ (1).xlsx` — 641 transaction rows, earliest date **2025-09-10**

Header block (identical in both files):

| Field | Value |
|---|---|
| الرصيد الافتتاحي (Opening Balance) | 11,118,958 |
| المدين المرحل (Debit Carried) | 111,494,348.87 |
| الدائن المرحل (Credit Carried) | 108,151,813.97 |
| الرصيد الختامي (Closing Balance) | 14,461,492.90 |

The header is internally self-consistent: `Opening + Debit − Credit = Closing` (11,118,958 + 111,494,348.87 − 108,151,813.97 = 14,461,492.90 ✓). The problem is that the Debit/Credit "carried" totals themselves are wrong.

## 2. Root cause: pre-period transactions folded into the period total, on top of the opening balance that should already represent them

The 641-row file includes 42 transaction lines dated **before** the period start (2025-09-10 through 2025-12-31) that the 599-row file correctly excludes. One of those 42 lines is the account's own `OPENING_BALANCE`-typed journal entry:

```
Trans ID: 10820
Type: OPENING_BALANCE
Date: 2025-12-30
Debit: 11,118,958
Description: الرصيد الافتتاحي (Opening Balance)
```

This is the exact same figure already shown in the header's "الرصيد الافتتاحي" field. It is being counted twice: once as the header's starting balance, and again as an ordinary transaction row inside the body — where it visibly spikes the running "Balance" column from 23,373,382 to 34,493,540 on that single line.

The other 41 pre-period rows (2025-09-10 to 2025-12-29, all dated *before* the OPENING_BALANCE entry itself, which is chronologically the account's own reset point) sum to:

| | Amount |
|---|---|
| Pre-period Debit | 12,270,624.00 |
| Pre-period Credit | 434,550.00 |
| **Net (Debit − Credit)** | **11,836,074.00** |

Confirmed arithmetically:
- 111,494,348.87 (header debit) = 99,223,724.87 (true 2026-only debit, from the 599-row file) + 12,270,624.00 (pre-period debit)
- 108,151,813.97 (header credit) = 107,717,263.97 (true 2026-only credit, from the 599-row file) + 434,550.00 (pre-period credit)

## 3. True closing balance vs. reported closing balance

Using only transactions dated on/after the period start (2026-01-01) against the stated opening balance:

```
11,118,958 + 99,223,724.87 (true period debit) − 107,717,263.97 (true period credit) = 2,625,418.90
```

| | Value |
|---|---|
| Reported closing balance | 14,461,492.90 |
| True closing balance (period-only activity) | 2,625,418.90 |
| **Overstatement** | **11,836,074.00** |

The reported closing balance is roughly **4.5x too high**, entirely due to double-counting the Sept–Dec 2025 activity that predates (and, per the `OPENING_BALANCE` entry, should already be superseded by) the account's stated opening balance.

## 4. Relationship to the earlier findings (`banks-trial-balance-vs-powerbi-findings.md`)

This is a live, concrete instance of risk #2 from that document ("Missing/stale `OPENING_BALANCE`-typed entry for the period"), except here the entry isn't missing — it's present but not being used as the period boundary. Whatever report/query generates the "Debit Carried" / "Credit Carried" header totals is summing transactions from a date range that starts well before the account's own `OPENING_BALANCE` entry, instead of:

- Opening Balance = sum of `OPENING_BALANCE`-typed entries dated on/before the period's `FromDate − 1`
- Period Debit/Credit = sum of entries dated strictly within `[FromDate, ThruDate]`

(This is exactly the logic already documented for `ComputeGlAccountBalanceForTimePeriod` in the Trial Balance report — see `banks-trial-balance-vs-powerbi-findings.md`, section 2. That logic, if applied consistently, would have produced 2,625,418.90, not 14,461,492.90.)

## 5. Suggested fix

Whatever query/report builds this account-statement header (distinct from, but similar to, the Trial Balance `GetTrialBalanceReport` flow) needs the same date-boundary discipline:

1. Exclude `OPENING_BALANCE`-typed entries from the "period debit/credit" sums entirely — they belong only in the opening-balance figure, never in period activity.
2. Bound "period debit/credit" strictly to `transaction_date >= FromDate AND transaction_date <= ThruDate` — do not include any transaction dated before `FromDate`, regardless of type.
3. Spot-check: after the fix, `Opening + Debit − Credit` for GL 110100 over the 2026-01-01 → 2026-07-12 window should equal **2,625,418.90**, not 14,461,492.90.

## 6. Scope / next step

This was checked for GL 110100 only. The same double-counting pattern should be checked against the other bank GL accounts on the Banks page (GL 124410, 440000, 600940, and any other `نقدية في البنوك` / `نقدية في الصندوق` accounts) before assuming it's isolated to this one account.
