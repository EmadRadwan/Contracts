# GL Account Transaction Details — Broader Impact Scan (All Accounts, FY2026)

Follow-on to `gl-account-transaction-details-endingbalance-bug.md`, which fixed
`Application/Accounting/Services/GlAccountTransactionDetails.cs` for GL 110100 (CIB). This
document checks every other GL account in the database for the same or a related issue, using
a direct read-only query against production (`erp_contracts` on `129.146.22.240:3308`) rather
than relying on manual Excel exports.

## Method

The confirmed bug (see the other doc) is: `postedDebits`/`postedCredits`/`endingBalance` were
computed from cumulative totals with no lower date bound, then partially corrected by
subtracting only `OPENING_BALANCE`-typed entries. For a report period starting **2026-01-01**
(the fiscal year used in the original CIB export), the amount that leaked into any account's
reported totals is:

```sql
SUM(amount) WHERE TRANSACTION_DATE < '2026-01-01'
              AND ACCTG_TRANS_TYPE_ID <> 'OPENING_BALANCE'
              AND IS_POSTED = 'Y' AND GL_FISCAL_TYPE_ID = 'ACTUAL'
GROUP BY GL_ACCOUNT_ID
```

Any account with a nonzero result here was showing an inflated Debit/Credit Carried and Ending
Balance in the "Transaction Details" modal/export for FY2026, under the pre-fix code.

**62 GL accounts** show nonzero leakage for FY2026. They split into two groups with very
different implications.

## Group 1 — Same bug as CIB (has an `OPENING_BALANCE` entry, still leaked pre-period rows)

| GL Account | Name | Leaked Debit | Leaked Credit |
|---|---|---:|---:|
| 110100 | البنك التجاري الدولى (CIB) | 12,270,624 | 434,550 |
| 111010 | النقدية (Cash) | 61,150,489 | 267,468 |
| 110300 | بنك أبو ظبي الإسلامي | 17,040,927 | 8,043,430 |
| 110200 | بيت التمويل الكويتي | 501,983 | 0 |

These are exactly the four bank/cash accounts the Banks page KPI cards summarize. They have a
proper reset (`OPENING_BALANCE`) entry, so the fix already applied to
`GlAccountTransactionDetails.cs` resolves all four the same way it resolved CIB — no further
code change needed. **Action:** after rebuild, re-pull each account's transaction-detail export
for FY2026 and confirm Debit/Credit Carried drop by the amounts above.

## Group 2 — No `OPENING_BALANCE` entry at all (58 accounts, different implication)

For these accounts, `openingDebits`/`openingCredits` were always 0 under the old code, so
`postedDebits = endingDebits − 0` accidentally returned the account's **entire lifetime
balance** rather than a period-bounded one. This isn't "double-counting" in the CIB sense — it's
that the report was never period-bounded for these accounts to begin with, because there was
never a reset entry to bound it against.

**This is where the fix changes behavior, not just corrects a number.** After the fix, viewing
one of these accounts for FY2026 will show **Opening Balance = 0** and Ending Balance = only
2026 activity — any balance carried in from before 2026 disappears from the display.

Breakdown of the 58:

- **1 account directly on the Banks page:** `124410` (شيكات تحت التحصيل / Checks under
  collection) — one of the five KPI cards on the Banks page. Leaked debit 113,781,604 / credit
  12,832,491, the single largest leak found, larger than CIB's. Worth flagging specifically since
  it's already in scope of the original investigation.
- **50 accounts** are individual customer receivable sub-ledgers (`مدينون - <name>`), e.g.
  `1211001` (مدينون - احمد صالح, leak 26.8M/6.7M), `1211002` (اشرف كمال, 19.5M/18.2M), and ~48
  more, mostly in the 5,000–20,000,000 range. These almost certainly represent running
  "how much does this customer still owe" balances — plausibly meant to show lifetime totals by
  design, not a fiscal-year-bounded figure.
- **2 large liability accounts:** `250120` (دفعات مقدمة من العملاء / Advance payments from
  customers, 277,078,505 credit leak) and `250130` (ودائع صيانة العملاء / Customer maintenance
  deposits, 20,804,040 credit leak).
- **5 smaller/miscellaneous accounts:** `124430` (نسيم - الثروة الخضراء, 8,043,430 debit),
  `601000` (الرواتب والأجور / Payroll, 419,550), `100072` (عهدة مستديمة / Permanent custody,
  156,400), `124424` (قرية السدة, 100,000), `601280` (مصاريف برامج وأنظمة, 15,000).

## Classification of the 58 Group-2 accounts

To judge whether "period-only, Opening = 0" is a reasonable outcome per account, each of the 58
was classified by how far back its history actually goes and how much of its activity sits before
2026:

- **Bucket 1 — Multi-year (pre-2025 history):** earliest transaction dated before 2025-01-01.
  These accounts existed across at least one prior fiscal year boundary already.
- **Bucket 2 — Recently-opened 2025 account:** earliest transaction in 2025, and pre-2026 rows
  are a meaningful share (≥5%) of the account's total activity — i.e. the account has a real,
  substantial chunk of its life sitting before the FY2026 cutoff, just not multiple years of it.
- **Bucket 3 — Boundary straggler:** 2 or fewer pre-2026 rows, or pre-2026 rows are under 5% of
  total activity — almost all of the account's life is inside 2026, with only one or two entries
  dated just before the New Year line.

| Bucket | Accounts | Total Leaked Debit | Total Leaked Credit |
|---|---:|---:|---:|
| 1 — Multi-year | 12 | 52,396,150 | 339,597,715 |
| 2 — Recently-opened 2025 | 37 | 358,630,439 | 158,648,925 |
| 3 — Boundary straggler | 9 | 9,371,940 | 4,370,464 |

### Bucket 1: Multi-year (pre-2025 history) (12 accounts)

| GL Account | Name | Earliest Txn | Pre-2026 Rows / Total | Leaked Debit | Leaked Credit |
|---|---|---|---|---:|---:|
| 250120 | دفعات مقدمة من العملاء | 2023-02-01 | 56/118 | 0 | 277,078,505 |
| 250130 | ودائع صيانة العملاء | 2023-02-01 | 59/119 | 0 | 20,804,040 |
| 120019 | مدينون - جورج يوسف وديع | 2024-07-30 | 4/6 | 5,371,000 | 3,121,000 |
| 120008 | مدينون - تميم عبدالكريم | 2024-10-23 | 8/10 | 6,573,250 | 4,515,750 |
| 120009 | مدينون - حاتم صلاح الدين | 2024-10-06 | 8/8 | 5,390,000 | 3,615,000 |
| 120017 | مدينون - شريف محمد عبدالخالق | 2024-07-27 | 9/11 | 9,060,000 | 7,360,000 |
| 120004 | مدينون - دولت حسين عبدالخالق | 2024-04-16 | 4/6 | 4,367,000 | 2,889,500 |
| 120013 | مدينون - سمر احمد حامد | 2023-02-01 | 5/8 | 2,403,500 | 1,739,920 |
| 120015 | مدينون - نادي محمد الصغير | 2024-12-01 | 3/5 | 4,429,400 | 3,842,500 |
| 120010 | مدينون - محمد محمود سليمان | 2024-03-13 | 4/6 | 5,420,000 | 5,249,500 |
| 120012 | مدينون - حسين عبدالرحمن الاغبري | 2024-02-04 | 4/4 | 4,765,000 | 4,765,000 |
| 120011 | مدينون - نيفن محمد طلعت / ولاء محمد عبد الوهاب | 2024-12-24 | 6/6 | 4,617,000 | 4,617,000 |

These are the strongest candidates for needing a real `OPENING_BALANCE` reset (or a lifetime-
balance fallback) — they've already crossed at least one fiscal year boundary with no reset
mechanism, so the same leakage pattern would recur every year going forward regardless of which
FY is selected.

### Bucket 2: Recently-opened 2025 account (37 accounts)

| GL Account | Name | Earliest Txn | Pre-2026 Rows / Total | Leaked Debit | Leaked Credit |
|---|---|---|---|---:|---:|
| 124410 | شيكات تحت التحصيل | 2025-08-25 | 75/275 | 113,781,604 | 12,832,491 |
| 1211001 | مدينون - احمد صالح | 2025-08-01 | 20/31 | 26,833,995 | 6,687,600 |
| 1211004 | مدينون - رضا فتحى | 2025-08-01 | 9/15 | 16,743,360 | 3,129,600 |
| 120127 | مدينون - منه الله علاء الدين | 2025-12-20 | 3/6 | 6,917,702 | 338,884 |
| 1211009 | مدينون - علاء خاطر | 2025-08-01 | 6/12 | 7,065,210 | 850,000 |
| 120076 | مدينون - محمد عبدالشفيع | 2025-12-16 | 3/4 | 6,814,384 | 631,000 |
| 120081 | مدينون - حبيبة قطب احمد | 2025-12-18 | 3/5 | 5,629,758 | 260,637 |
| 120030 | مدينون - احمد يحيي حسني | 2025-12-20 | 3/6 | 5,382,027 | 250,000 |
| 120138 | مدينون - محمد عبدالعزيز عيد | 2025-12-23 | 3/5 | 5,367,600 | 300,000 |
| 1211006 | مدينون - احمد عبد الرحمن ابراهيم | 2025-10-15 | 3/3 | 5,585,400 | 1,040,000 |
| 1211012 | مدينون - محمد القاضي | 2025-09-10 | 3/4 | 5,585,400 | 1,044,000 |
| 120020 | مدينون - محمد محمد ابراهيم | 2025-09-01 | 3/6 | 5,275,000 | 1,200,000 |
| 120041 | مدينون - سامي احمد محمد علي | 2025-12-25 | 3/3 | 4,419,830 | 409,244 |
| 120028 | مدينون - وفاء احمد محمد علي | 2025-12-25 | 3/3 | 2,991,919 | 277,030 |
| 120153 | مدينون - شيماء مصطفي عبدالصبور مصطفي | 2025-12-21 | 3/3 | 2,694,435 | 130,000 |
| 120021 | مدينون - احمد ابراهيم الدسوقي | 2025-01-01 | 3/5 | 0 | 2,320,000 |
| 120005 | مدينون - هاني عبدالرحمن زكى | 2025-06-04 | 4/5 | 5,097,200 | 3,164,500 |
| 120014 | مدينون - عبدالرحمن علي القاضي | 2025-08-20 | 3/5 | 4,565,000 | 2,800,000 |
| 120058 | مدينون - سمير سعد خضري | 2025-12-15 | 9/9 | 17,620,874 | 16,315,624 |
| 1211002 | مدينون - اشرف كمال | 2025-08-25 | 12/12 | 19,457,415 | 18,184,500 |
| 1211003 | مدينون - محمد أبو جامع | 2025-09-10 | 3/7 | 377,580 | 1,408,900 |
| 120059 | مدينون - محمد الصاوي | 2025-12-03 | 5/14 | 0 | 904,503 |
| 120118 | مدينون - صفاء محمد ابراهيم | 2025-12-28 | 3/3 | 7,077,575 | 6,553,310 |
| 120082 | مدينون - محمود مصطفي محمد حجازي | 2025-12-18 | 3/3 | 7,046,404 | 6,524,448 |
| 120088 | مدينون - احمد يحيي توفيق حسانين | 2025-12-20 | 3/3 | 6,973,244 | 6,456,707 |
| 120069 | مدينون - احمد ناجي مبروك | 2025-12-20 | 3/3 | 6,861,324 | 6,353,078 |
| 120073 | مدينون - احمد ضياء | 2025-12-20 | 3/5 | 6,659,007 | 6,165,747 |
| 120090 | مدينون - يؤانس حنا عزيز يوسف نسيم | 2025-12-20 | 3/3 | 6,647,372 | 6,154,974 |
| 120071 | مدينون - رانيا رجب | 2025-12-20 | 3/3 | 6,426,231 | 5,950,214 |
| 120074 | مدينون - محمد عبدالله زلط | 2025-12-20 | 3/3 | 6,087,531 | 5,636,603 |
| 120080 | مدينون - بسنت هشام محمود | 2025-12-20 | 3/3 | 5,975,321 | 5,532,705 |
| 120075 | مدينون - احمد محي | 2025-12-20 | 3/3 | 5,500,448 | 5,093,007 |
| 120079 | مدينون - اسلام محمد محمود | 2025-12-31 | 3/3 | 5,472,531 | 5,067,158 |
| 120119 | مدينون - محمد محمود محروس | 2025-12-20 | 3/3 | 5,456,658 | 5,052,461 |
| 1211005 | مدينون - احمد سراج | 2025-10-05 | 3/3 | 4,718,700 | 4,410,000 |
| 1211008 | مدينون - محمد الصعيدى | 2025-09-10 | 3/3 | 4,622,400 | 4,320,000 |
| 120018 | مدينون - هشام عاطف عبد النعيم | 2025-05-07 | 4/4 | 4,900,000 | 4,900,000 |

`124410` (Checks under collection) tops this bucket and belongs to the Banks page — see Group 1
note above. The rest are customer sub-accounts opened partway through 2025; they carry real,
material balances into 2026 even without a multi-year history.

### Bucket 3: Boundary straggler (9 accounts)

| GL Account | Name | Earliest Txn | Pre-2026 Rows / Total | Leaked Debit | Leaked Credit |
|---|---|---|---|---:|---:|
| 124430 | نسيم - الثروة الخضراء | 2025-12-22 | 1/139 | 8,043,430 | 0 |
| 1211007 | مدينون - فاطمة نبيل | 2025-12-22 | 2/14 | 378,000 | 3,925,000 |
| 601000 | الرواتب والأجور | 2025-12-31 | 1/53 | 419,550 | 0 |
| 120133 | مدينون - شبل احمد محمود شبل | 2025-12-25 | 2/7 | 0 | 260,000 |
| 1211010 | مدينون - رامى حسين | 2025-12-31 | 1/1 | 259,560 | 0 |
| 120001 | مدينون - اسلام وحيد بسيونى | 2025-12-16 | 1/7 | 0 | 185,464 |
| 100072 | محمد مختار-عهدة مستديمة | 2025-12-24 | 2/130 | 156,400 | 0 |
| 124424 | قرية السدة - الصحراوى 2 فدان | 2025-12-24 | 1/175 | 100,000 | 0 |
| 601280 | مصاريف برامج وأنظمة | 2025-12-24 | 1/5 | 15,000 | 0 |

Note `124430` has a large 8,043,430 leak from just a *single* stray row despite 139 total rows —
worth a quick manual look (possibly a single misdated entry rather than a structural issue). The
rest are low-value, low-row-count stragglers right at the year boundary; lowest priority of the
three buckets.

## Resolution — Option 2 implemented (fallback for accounts without an `OPENING_BALANCE` entry)

**Real-world confirmation before the fix was extended:** the initial period-bound-only fix was
deployed to dev but not yet to production. Comparing two exports of GL `1211001` (مدينون - احمد
صالح) — one from each environment, same underlying data — showed exactly the Group 2 risk in
practice:

| | Production (pre-fix) | Dev (period-bound-only fix) |
|---|---:|---:|
| Debit Carried | 26,833,995 | 0 |
| Credit Carried | 10,038,586 | 3,350,986 |
| Ending Balance | **16,795,409** | **-3,350,986** |

The account's own row grid (unaffected by either version of the bug/fix, since it just walks the
transaction list from an opening of 0) independently confirms **16,795,409** is correct — the
customer was charged 26,833,995 across 5 apartments in Aug 2025 and has paid back 10,038,586 to
date. Dev's period-bound-only fix zeroed the pre-2026 charges out of the header, producing a
negative "Ending Balance" that contradicted the last row of its own grid within the same file —
i.e. exactly the regression this section warned about, now caught concretely, before reaching
production.

**Fix implemented:** `GlAccountTransactionDetails.cs` now checks whether the account has *any*
posted, `ACTUAL` `OPENING_BALANCE`-typed entry at all (regardless of date). If yes, `periodDebits`/
`periodCredits` are bounded to `[periodStart, periodEnd]` (the CIB-style fix, unchanged for Group
1 accounts). If no, the `periodStart` lower bound is dropped and the account falls back to its
lifetime-to-date total (bounded only by `periodEnd`) — reproducing the old, correct-for-these-
accounts production behavior. This is Option 2 above, applied automatically per account rather
than requiring an `OPENING_BALANCE` backfill across all 58 Group 2 accounts.

Option 1 (posting real `OPENING_BALANCE` entries for the Bucket 1 multi-year accounts, so they
too become properly period-bounded year over year) remains a separate, optional follow-up — not
required for correctness now, since the fallback already gives them their correct lifetime total.

## Caveat on scope

This scan used FY2026 (`FromDate = 2026-01-01`) as the reference period, matching the CIB export
already diagnosed. "Leakage" is inherently period-relative — an account with no leak against
2026-01-01 could still leak against a different period start if it has activity dated before
that other start. Re-run the query with a different `FromDate` if you need to check a specific
period other than FY2026.
