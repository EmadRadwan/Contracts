# Banks Page: Cheques Under Collection Separation & Bank-Selection Consistency Fix

Session on `Projects-17-Jul` (the current/latest Power BI project — semantic model + report already
had the `Dim_CustomTimePeriod` slicer, `ResetDate`, the fixed `Total_TTD`/`Total_TTD_Raw`, and a
bank-picker slicer built on top of the work documented in `power-bi-dax-and-slicer-fixes.md`,
which was done on `Projects-06-Jul`). This document covers what changed in *this* session, on top
of that existing state.

## 1. Starting question: does the Banks page actually match its title?

Asked for an honest read on whether everything shown on the page is consistent with "Banks."
Pulled the real chart-of-accounts classification (`Dim_gl_account`) for every account on the page
rather than going by name:

| GL Account | Name | `SUBACCOUNT_AR` | `REPORT_AR` |
|---|---|---|---|
| 110100 | البنك التجاري الدولى (CIB) | نقدية في البنوك | الميزانية العمومية (Balance Sheet) |
| 110300 | بنك أبو ظبي الإسلامي | نقدية في البنوك | الميزانية العمومية |
| 110200 | بيت التمويل الكويتي | نقدية في البنوك | الميزانية العمومية |
| 124410 | شيكات تحت التحصيل | نقدية في البنوك | الميزانية العمومية |
| 111010 | النقدية (Cash) | **نقدية في الصندوق** | الميزانية العمومية |
| 440000 | ايرادات ارباح بنكية - حسابات استثمارية | — (classified as Sales Revenue) | **قائمة الدخل** (Income Statement) → Trading Account → Sales |
| 600940 | رسوم بنكية | — (Other Expenses) | **قائمة الدخل** → Operating Account → Other Expenses |

Findings:
- **`111010` "Cash on Hand" contradicts the title by definition** — `نقدية في الصندوق` literally means
  cash *not* in a bank. (Correction to earlier docs: `power-bi-dax-and-slicer-fixes.md`/
  `gl-account-transaction-details-broader-impact-scan.md` had grouped `111010` with "the four bank
  accounts the page summarizes" — that was wrong; it's on the page but in its own separate card
  group, not part of the bank total.)
- **`440000` is chart-of-accounts-classified as ordinary Sales Revenue**, not anything
  bank/interest-related — that's inferred purely from its Arabic name. Likely a COA data-quality
  issue worth flagging to accounting separately (it would also be quietly inflating whatever
  "Sales" figure the Income Statement report shows).
- **`600940` is generically classified as "Other Expenses"** — nothing marks it as bank-specific
  except its name.
- **`124410` (Cheques Under Collection) is legitimately classified under Cash in Banks**, but
  conceptually it's money *not yet* in any bank account — see §2.

Decision made: narrow/clarify rather than rename the page — keep Cash on Hand / Investment
Profits / Bank Fees as supplementary cards (out of scope for this pass), but fix `124410`'s
relationship to the "real" bank total, which turned out to be an actual, provable bug, not just a
naming quibble.

## 2. Cheques Under Collection — what it is and why it was a real bug, not just labeling

`124410` is a standard clearing/transit account: when a customer pays by cheque, the entry debits
`124410` first (the company has the cheque but the bank hasn't confirmed the funds), then a second
entry moves it into the real bank account once the cheque clears. It sits in Cash and Equivalents
for accounting purposes, but it is **not** money a bank would confirm if called right now — which
is specifically what the client uses this page to answer ("how much money is on the bank").

Checked the actual visual filters (not just the classification) and found the aggregate "Cash in
Banks" cards filtered only on `SUBACCOUNT_AR = 'نقدية في البنوك'`, with no exclusion of `124410`.
Since it shares that classification, **it was already being silently blended into the same total
as CIB + ADIB + KFH** — on the exact card the client reads as "total bank balance." It also had
its own separate dedicated card group, so it was represented twice on the page in two different
ways.

## 3. The bank-picker slicer conflict bug (found while fixing #2)

The existing bank-picker slicer (added in the prior session, filters `Dim_gl_account[ACCOUNT_NAME_ARABIC]`,
defaults to CIB selected) filters `GL_ACCOUNT_ID` through the normal Power BI relationship. Every
other fixed-single-account card on the page (Cheques Under Collection, Cash on Hand, Investment
Profits, Bank Fees) also carries its own hardcoded `GL_ACCOUNT_ID` filter. Power BI ANDs all active
filters on a page together — so the moment *any* bank is selected in the slicer, every one of those
other cards' filter (e.g. `GL_ACCOUNT_ID = '124410'`) intersects with the slicer's filter
(e.g. `GL_ACCOUNT_ID = '110300'`) to **zero rows**, silently blanking the card. Since the slicer
already has CIB pre-selected as its saved default, this bug was **already live** on the page before
this session — Cash on Hand/Cheques/Investment/Fees would have been showing blank whenever the
report was opened.

## 4. Changes made

### New DAX measures (`Fact_GL_Transactions.tmdl`)

Each wraps its underlying measure in `CALCULATE(..., REMOVEFILTERS(Dim_gl_account), Dim_gl_account[GL_ACCOUNT_ID] = "<fixed account>")`
— clears whatever `Dim_gl_account` filter is active (from the bank slicer or anywhere else) and
reasserts its own fixed account, so these cards are now immune to the bank-picker slicer:

- `'Cheques Under Collection Opening Balance Label'`, `'... Closing Balance Label'`,
  `'... Ending Balance'` — locked to `124410`; also upgraded to the period-aware "Selected Period"
  logic (`gl-account-transaction-details-endingbalance-bug.md`'s fix pattern) in the same change,
  since they were still on the older generic measures.
- `'Cash On Hand Opening Balance Label'`, `'... Closing Balance Label'`, `'... Total_TTD'` — locked
  to `111010`.
- `'Investment Profits Total_FTP'` — locked to `440000`.
- `'Bank Fees Total_FTP'` — locked to `600940`.
- `'Total Cash Position (Banks + Cheques)'` — the three real banks (explicit list, self-contained)
  plus Cheques Under Collection. **Not yet placed on a card visual** — see §5.

### Report changes (`pages/9f96e84e32eb06c276e5` — the Banks page)

1. Aggregate "Cash in Banks" cards (Opening/Total/Closing): filter changed from
   `SUBACCOUNT_AR = 'نقدية في البنوك'` to an explicit `GL_ACCOUNT_ID IN ('110100','110300','110200')`
   list — excludes `124410` from what the client reads as the bank total.
2. Clustered bar chart ("أرصدة البنوك الحالية"): same filter fix — now shows exactly 3 bars.
3. Bank-picker slicer: extended its existing `NOT IN ('110000')` exclusion to also exclude
   `'124410'`, so Cheques Under Collection can't be selected as if it were a bank.
4. Cheques Under Collection's 3 cards: re-wired to the new immune measures, and relabeled with
   `" (لم تُحصّل بعد)"` — *not yet collected* — appended to each card's display text, so it reads as
   adjacent to bank balance rather than part of it.
5. Cash on Hand, Investment Profits, Bank Fees cards: re-wired to their new immune measures (not
   originally in scope, but they had the identical latent bug from §3 — leaving them unfixed would
   have directly contradicted "other figures stay consistent when a bank is selected").
6. Detail transaction table (`fcb834fe900a3eccc677`) removed entirely — to be rebuilt on a
   dedicated details page later. `page.json` doesn't enumerate visuals by name (folder-discovered),
   so this was a clean deletion.

All edits were done via direct JSON manipulation (`json.load`/`json.dump`, not hand-written
strings) to guarantee syntactic validity; all 16 remaining visuals on the page were re-validated as
parseable JSON after the changes.

## 5. What was deliberately NOT done

No new card visual was hand-authored for `'Total Cash Position (Banks + Cheques)'`. Editing an
*existing* visual in place (swapping a filter value or a measure reference) is low-risk; authoring
a *new* visual from scratch (new GUID, position, full schema) with no way to render/verify it is
meaningfully riskier. The measure is ready — dropping a Card visual onto it in Desktop is a
30-second task.

## 6. Verification checklist for whoever opens this in Power BI Desktop next

1. Refresh — confirm no load errors from the new measures.
2. With nothing selected in the bank slicer: "Cash in Banks" total should reflect only the 3 real
   banks; Cheques Under Collection, Cash on Hand, Investment Profits, and Bank Fees should all show
   real (non-blank) numbers.
3. Select a specific bank (e.g. ADIB): "Cash in Banks" should narrow to just that bank; the other
   four card groups should be **unchanged** — this is the actual test of the fix in §3/§4.5.
4. Confirm `124410` no longer appears in the bank slicer's dropdown or as a bar in the chart.
5. Confirm the Cheques Under Collection cards now show the "(لم تُحصّل بعد)" suffix.
6. Confirm the detail transaction table is gone from the page.
7. Optional: drop a Card visual on `'Total Cash Position (Banks + Cheques)'` if that supplementary
   figure is wanted on the page.

## Related documents (same repo root)

- `power-bi-dax-and-slicer-fixes.md` — the `Total_TTD`/`Total_TTD_Raw` double-counting fix,
  `Dim_CustomTimePeriod` slicer design, and `Fact_GL_Transactions.sql` posted-only filter (the
  foundation this session builds on).
- `gl-account-transaction-details-endingbalance-bug.md` / `gl-account-transaction-details-broader-impact-scan.md` —
  the C#-side sibling bug (`GetGlAccountTransactionDetails.cs`) and its per-account fallback logic,
  which the `'Selected Period ...'` DAX measures mirror.
