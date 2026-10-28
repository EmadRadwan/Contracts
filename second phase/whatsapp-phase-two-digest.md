# Phase Two — WhatsApp digest (read 2026-10-07)

Source: local WhatsApp Desktop DB (`ChatStorage.sqlite`), chats **ERP GOLDEN** (group), **Ahmad Agiba** (1:1), **Ashraf Agiba** (1:1, 5 msgs, nothing phase-two).
Window read in full: 2026-07-14 → 2026-10-07 (~870 text/caption lines). Earlier history keyword-searched only.
Limit: most attachments/images before mid-Sep were never downloaded to the Mac (only 175 of 630 images in window are local) —
e.g. the receipt-print sample "from another ERP" (07-27), the Approval-Q example (07-30), Ashraf's cheque docs (07-14) are phone-only.

## Timeline
| Date | Who | Event |
|---|---|---|
| 07-14 | Ashraf | Cheque proposal: on payment-plan approval, 3 cols per installment (cheque no / date / bank), auto serial; receipts serial starts 5000; separate P/R prefixes. Sent `الشيكات المستلمة من العملاء.docx`, `الشيكات تحت التحصيل.docx` |
| 07-14 | Emad | "No time for Power BI amid all these requests" |
| 07-15 | Ahmad | `قيود العمولة.docx` — commission GL rules (600025/26/27 Dr, 250456/57/58 Cr, 250442 tax 14%+5%) → delivered Aug |
| 07-16 | Ashraf | Add customer cheque bank name; maintenance payment in payment plan (split into 3) |
| 07-19 | Ahmad | Upload Watan House + SAWA inventory; status col sold/reserved/available in sales report |
| 07-20 | Emad→Ahmad | Asked for the written list of new requests to justify cost before meeting with الحاج أحمد |
| 07-27 | Ahmad | Receipt print format from another ERP (new accountant dislikes ours). Emad: "add to enhancements list — it has a cost; message is for management" |
| 07-29 10:02 | Emad | "Make sure Ahmad adds the **Contracts module** to the enhancements list" |
| 07-29 14:52 / 15:55 | Ahmad | `2nd phase ERP SYS.pdf` ("preliminary until final agreed") → `…updated.pdf` |
| 07-29 15:38 | Emad | Own additions: **multi-company**, **attachments on all screens** (ID photo etc.), **fingerprint DB integration** |
| 07-29 18:17–18:50 | Ahmad/Emad | PBI daily-balances page + **KPIs**: actual sales vs target, construction % vs plan, collected vs due, actual expenses vs budget. Emad: needs ERP-side KPI target storage first |
| 07-30 | Ahmad | Example of **pending-action screen + notifications for Approval Q** |
| 07-30 | Emad | `accrual_vs_cash_decision_ar` (needs answer ASAP) |
| 08-04 | Emad | Study material for the future contracts module |
| 08-05 | Ahmad | ChatGPT chat "**very important for phase two + PBI for certificates**" (re-sent 10-07) — see below |
| 08-05 | Ahmad | Due-status search buckets (overdue / today / week / month / quarter/half/year) everywhere; land-sale arrears missing from arrears report; **never cancel payments — adjust the entry** (deposit→sales request conversion) |
| 08-05 | Ashraf | Daily offline backup idea — then dropped ("too big, leave it") |
| 08-14 | Emad | `قرارات محاسبية مطلوبة.pdf` |
| 08-15 | Emad | `مقترح ميزة التسوية البنكية.pdf` (bank reconciliation, 8 pp). Ahmad: "I asked for automatic bank rec before" |
| 08-15 / 08-16 | Ahmad / Emad | ChatGPT "**very important: budget**" → Emad's `موديول-الموازنة-لماذا-نحتاج-إدخال-بيانات.pdf` |
| 08-18 | Ahmad | CRM ↔ Facebook campaigns integration (contact CRM admin) |
| 08-23 | Ahmad | Late-penalty payment type; commission reset/edit party/amount/cancel payments |
| 08-26 | Emad→Ahmad | Ask for a payment on account; "we did several second-batch items for free" |
| 09-01 | Ahmad | Payroll: edit invoice/payment, advance type, repay advance by receipt / JE |
| 09-08 | Emad | Re-sent `2nd phase ERP SYS updated` |
| 09-09 | Ahmad | "Enough PBI — focus on system changes" |
| 09-16 | Ahmad | Cost-center screen; move party + cost center from JE header to **line level** |
| 09-19 | Emad→Ahmad | `2nd-phase-ERP-SYS-transcription.xlsx` + `Phase-Two-Business-Case-AR` |
| 09-30 | Ahmad | PBI: contracted 600M = sale price + maintenance; % received of sale price |
| 10-05 | Ahmad | Land-purchase item type (Dr land for investment / Cr 125100) — done |
| **10-06 18:45** | Ahmad | **Returned annotated transcription**: "الموديلز كلها خلصانة ماعدا المستخلصات نخليها آخر حاجة في Batch 2" — lists finalized; certificates go last in Batch 2. File = repo-root `2nd-phase-ERP-SYS-transcription.xlsx` (untracked) |
| 10-06 23:46 | Ahmad | Create multi payments from an invoice |
| 10-07 | Ahmad | "Add **bank reconciliation** too"; full-screen view with search filters; re-sent the phase-two ChatGPT chat |

## Ahmad's 10-06 edits to the transcription (vs Emad's 09-19 version)
- **#1** Report engine: column picker (checkboxes), ordering, subtotals, Excel+PDF; *separate list of required reports to follow*.
- **#2** + financial statements & comparison; Excel/PDF summary+detail; **hyperlink/drill on every number**.
- **#3** Companies: Golden / **Ladris / Sawa** / Holding.
- **#5** PDC statuses + **replaced (مستبدل)**.
- **#6** + professional receipt printing per the sample sent.
- **#9** + show GL account on print; **financial account as payment method** (custody/عهد, partner current account).
- **#13** replaced-cheque status → paid/unpaid (was sent/unpaid).
- **#14** + new payment types wired to correct GL entries.
- **#15 REPLACED**: sales receipts show installment / paid / remaining; overpayment rolls into next installment; handling of remainder. *(old #15 "cancel = keep & colour red" dropped — now covered by the auditor soft-delete work)*
- **#17** + **cost-center tree with levels**.
- **#19** + sales-receipt serial aligned to system auto-serial.
- **#20** fixed formula (+Dr −Cr); + **account statement report for any account** with posted/unposted filter.
- **#21** merged with Approval Q (receipts, payments **and JEs**).
- **#22 REPLACED**: **Rentals screen** — rent due, due date, parties, automated accrual, payments linked to each party's account (sub-accounts roll to a main account).
- **#23** + subtotals by active filters.
- **#29** used leave balance; **#30** + checkbox to pay advance installment in cash instead of salary deduction; **#32** advance screen links outgoing payment + deductions, shows remaining (also on main grid).
- **#33 REPLACED** → **Party credit limit**: raise limit, statement of usage with running balance, grid shows limit/used/remaining.
- **#34 REPLACED** → **Project credit limit**: usage across all certificates/contractors + remaining.
- **#35** + cheque-due alerts to treasury clerk & manager one day before.
- **#36** non-financial edits only (date / employee name).
- **#38** + flexible payment-plan builder printable before posting (per the Excel sample).
- **#39 REPLACED** → **KPI targets vs actual** by date range (monthly/quarterly) + project taxes/expenses on project report.
- **#40** + commission taxes due, e-invoice link, commission entries per earlier doc.
- **#41 REPLACED** → payment plan total = **sale price + maintenance deposit**; extra down-payments/maintenance installments; cheque details; **one multi-cheque receipt per unit**.
- **#42** convert reservation-deposit (جدية حجز) payments into down-payment parts.

## Extra scope raised outside the list
Contracts module (Emad, 07-29) · multi-company · attachments everywhere · fingerprint DB · KPI module · Approval-Q + notifications ·
bank reconciliation (proposal PDF, Ahmad wants it in) · budget module (proposal PDF) · CRM↔Facebook · WhatsApp reminders ·
rentals · AI assistant via MCP (`AI-Assistant-MCP-Guide-ar`, sent 10-07).

## ChatGPT "phase two + PBI for certificates" (shared 08-05, re-sent 10-07)
- Certificate approval → **supplier invoice** for net value; payments linked one-to-many (partial/multiple); certificate shows net / paid / remaining / status (unpaid, partial, paid) / last payment / count.
- **Budget limits** at project → building → (unit) → cost-code; mandatory on certificate: project, phase, building, budget item, contractor.
- Control: <90% allow, 90–100% warn + PM/FM approval, >100% block until budget increase approved.
- Hierarchy Company → Project → Phase → Building → Floor/Unit → Cost Code (concrete, steel, masonry, electrical, plumbing, finishing, site).
- Budget Control screen: budget / actual / remaining / **commitments** (POs, approved-unpaid certificates, contracts) / available.
- PBI: Executive KPIs; drill Project→Phase→Building→Item→Contractor→Certificate; item matrix; contractor analysis (contract, certificates, paid, retention, remaining, % done); variance; cash flow.
- New tables: phases, buildings, cost codes, budgets, contractor contracts + contract items.

## ChatGPT "budget for next year" (08-15)
Operating / Cash-Flow / Project budgets; contracted future collections (installments) vs new-sales budget; Fact_Budget; Budget vs Actual vs Forecast; PBI page set.
Emad's reply PDF: forecast is derivable from data, **budget is a management decision that must be entered** → needs a module.

## Docs Emad sent to ERP GOLDEN on 2026-10-07 (read in full)

### `مقترح ميزة التسوية البنكية.pdf` — Bank Reconciliation proposal (8 pp, dated Aug 2026, addressed to management for approval)
- **Problem:** no in-system way to know if book balance = bank statement; verifying one account (CIB 110100) took days of manual,
  transaction-by-transaction tech work. Risks: errors undetected for months, wasted tech time, decisions on unsure cash, no audit-ready
  reconciliations, no fraud tripwire.
- **Concept:** match every book movement to a statement line; legit differences are only (a) outgoing cheques/transfers not yet cleared,
  (b) deposits in transit. Standard module in SAP/Oracle/D365.
- **Screens (mockups, not real):**
  1. `/bankReconciliation/110100` — book balance 206,518.45 vs statement 210,971.36, unexplained diff 4,452.91; list of book items not
     cleared by bank; list of statement items missing from the system.
  2. `/…/import` — upload statement (Excel/PDF), auto-match by amount + date + reference (e.g. 587/705 matched), manual match for the rest,
     approve & save reconciliation.
  3. `/bankReconciliation/outstandingItems` — open items across all banks with age in days; >30 days flagged "needs review".
- **Benefits:** trust in balances, minutes not days, early error detection, fraud protection, audit readiness, better cash decisions.
- **Phased plan:** (1) manual reconciliation — book balance + mark each line matched/outstanding; (2) statement upload + auto-match;
  (3) proactive alerts (aged items, duplicates, diffs over threshold). **Asks approval for Phase 1 only** as low-risk first step.
  Ends with a sign-off block (name / signature / date).
- Codebase: OFBiz `Domain/GlReconciliation.cs` + `GlReconciliationEntry.cs` exist; no Application/UI code uses them yet.

### `موديول-الموازنة-لماذا-نحتاج-إدخال-بيانات.pdf` — Budget module memo (4 pp, reply to Ahmad's 08-15 ChatGPT budget chat)
- **Thesis:** forecast can be built now from existing data (sales, installments, collections, expenses); **budget cannot** — it is a
  management decision that exists nowhere in the system, and no report/DAX can invent it.
- Quotes the agreed summary: Budget = management-approved plan; Actual = amounts recorded in ERP; Contracted Future = installment schedules.
  Fact_Budget = "management-approved values by date/project/account/cost item/budget type".
- Forecast vs Budget table: forecast = computed from existing data, start now (Power BI/DAX); budget = human-decided target
  (e.g. "5M EGP sales for project X in Q1"), needs **new screens + module**.
- **System already has the design but it is empty:** Budget header, Budget Items, Budget Item Types, link to GL account + project,
  approval status (draft → under review → approved → locked), revisions/reviews, budget roles — all "empty".
- If PBI is built first, Fact_Budget is empty → Budget vs Actual always shows 0, variances impossible, looks like a report bug.
- **To activate:** (1) entry screens by project → building → unit → cost item → month; (2) assign who enters / reviews / approves;
  (3) formal approval workflow, locked once approved; (4) link budget items to GL accounts & projects; (5) only then Budget vs Actual vs
  Forecast is meaningful.
- **Recommendation:** two parallel tracks — (a) forecast reports now from existing data, (b) build budget entry/approval module so
  approved budgets exist for comparison by start of next year.
- Codebase: OFBiz Budget* entities in `Domain/` (Budget, BudgetItem, BudgetItemType, BudgetStatus, BudgetRevision, BudgetReview,
  BudgetRole, BudgetScenario…), no feature code yet.

## Payroll, attendance (fingerprint) and KPI plans — gathered 2026-10-07
**Payroll asks (chats):** absence days/value + overtime totals (03-31) · per-employee accrued-salary accounts, cash vs bank "Load from Salary" (Ashraf 05-03, done as run payments) ·
hide payroll invoices from non-HR users (05-19) · Excel absence report filterable by employee (05-19) · leave-balance box (05-19) · keep last payroll draft if invoices cancelled (06-02) ·
payroll payment amount not editable (08-23) · edit payroll invoice/payment, change advance type, repay advance by receipt or JE (09-01) · staff cost incl. commissions monthly (09-07).
Ahmad's 6 Oct sheet #27–32: search/filter, edit one invoice w/o deleting run, leave used, edit advance deduction + cash-instalment checkbox, reassign/increase advance, advance statement.
**Emad's plan:** `Payroll-Status-and-Way-Forward` (6 Sep): Next = recompute run payments on invoice edit while unpaid, read-only amount, block revert after sent; then reversal/soft-delete/period close;
After = single deduction engine, valid schedule statuses, seed PAYROL_* item types, keep absence days in editor; Later = move attendance derivation server-side.
**Attendance today:** `FingerprintUpload.tsx` parses the device Excel in the browser → PAYROL_DD_ABSENCE lines. Per-employee `Party.FingerPrintAttendanceId` + `AttendanceStartsAt`.
Rules coded: late <60 min = ¼ day, ≥60 = ½ day, no punch = 1 day, Fri+Sat off for everyone.
**Agreed but not built (Ahmad/Ashraf 05-18/19):** department master with its own weekend (engineers work Saturday), monthly manual public holidays, manually entered leave that cancels a matching absence,
staff without fingerprint (external sites) entered manually, leave balance. Emad 07-29: integrate directly with the fingerprint program's database (asked for vendor contact 05-17). Not on Ahmad's sheet.
**KPIs:** Ahmad 07-29 PBI KPI page — sales vs target, construction % vs plan, collected vs due, spend vs budget. Emad: targets must be stored in ERP first. Sheet #39 (6 Oct) = targets vs actual monthly/quarterly.
No KPI/target tables in Domain; OFBiz HR entities present but unused: EmplLeave*, PerfReview*, Timesheet.
