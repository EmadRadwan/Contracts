-- ============================================================================
-- Fact_Project_Payroll — the missing Power BI fact view for project labour cost
-- ============================================================================
-- WHY THIS EXISTS
--   GetProjectReport.cs reports seven cost/revenue streams per project. Power BI
--   has views for four. This adds the fifth:
--
--       C# GetProjectPayroll  ->  Fact_Project_Payroll
--
--   Measured: 85 entries, 1,098,954 of project labour cost that Power BI cannot
--   see at all today, across 6 projects.
--
-- WHY THERE IS NO COMPANION JOURNAL-ENTRIES VIEW
--   An earlier draft of this script also created Fact_Project_JournalEntries to
--   mirror GetAccountingTransactions. It was withdrawn on review, for two reasons:
--
--     1. It would have duplicated rows the model already holds. Fact_GL_Transactions
--        already contains every GENERAL_JOURNAL and OPENING_BALANCE entry, and the
--        model already has a working path from a project to them:
--            DimProjects -> DimProject_Accounts -><- Dim_gl_account <- Fact_GL_Transactions
--        (the DimProject_Accounts <-> Dim_gl_account relationship is bi-directional).
--        That chain was run against the data and returns correct per-project journal
--        figures, so a MEASURE gets the same answer with no new view:
--            Project Journal Entries =
--                CALCULATE([Total_FTP_Raw],
--                          Fact_GL_Transactions[ACCTG_TRANS_TYPE_ID]
--                              IN {"GENERAL_JOURNAL", "OPENING_BALANCE"})
--        sliced by a DimProject_Accounts[ProjectName] slicer. Signed, so credits
--        correctly reduce project cost.
--
--     2. It would have silently disagreed with the ledger. The draft read
--        acctg_trans_entry directly, which INCLUDES unposted transactions, whereas
--        Fact_GL_Transactions ends with WHERE t.IS_POSTED = 'Y'. On project accounts
--        that is 4 unposted journal entries worth 600,370 — the view would have
--        reported 8,302,255 where the ledger reports 7,701,885, with nothing on any
--        page to explain the gap.
--
--   Payroll is the case where a view IS justified, because it carries a rule the
--   ledger alone does not express — see the party '276' note in the view below.
--
-- PROJECT ATTRIBUTION
--   The C# builds its GL account set per project in memory: WorkEffort.GlAccountId
--   plus OPERATING_EXPENSE_GL_ACCOUNT_ID and every descendant, walked recursively.
--   This view joins `dimproject`, the recursive-CTE bridge the two existing payment
--   views already use, which resolves the same set and tags each account
--   PROJECT_MAIN / OPERATING_PARENT / OPERATING_CHILD.
--   Verified safe to join: 0 GL accounts map to more than one project, and no
--   duplicate (ProjectId, GlAccountId) pairs — so no row fan-out.
--
-- COLUMN NAMING
--   Matches the core column names of Fact_Project_DirectPayments_2 /
--   Fact_Project_OperatingExpenses_2 so the semantic model can treat all three the
--   same way. Payment-specific columns with no meaning for a GL entry (payment
--   method, cheque number, cost centre, IsBankTransfer) are omitted.
--
-- HOW TO RUN
--   Review, then execute against the database Power BI reads
--   (erp_contracts on 129.146.22.240:3308 — NOT the local copy).
--   Creating a view changes no data and is reversible with DROP VIEW.
--   !! Deploy this together with the two payment-view exclusions in section 4 !!
--      Shipping either half alone produces a wrong number — see section 4.
-- ============================================================================


-- ────────────────────────────────────────────────────────────────────────────
-- 1. Fact_Project_Payroll   (mirrors GetProjectPayroll)
-- ────────────────────────────────────────────────────────────────────────────
-- Project labour cost taken straight from the GL: every DEBIT to a project GL
-- account that originates in a payroll transaction. Two cases are included and one
-- is deliberately excluded:
--
--   INCLUDED  PAYROL_INVOICE — the monthly accrual. The real per-employee project
--                              labour cost. 84 entries, 1,088,954.
--   INCLUDED  OUTGOING_PAYMENT where the payment is PAYROL_PAYMENT and is NOT
--                              addressed to party '276' — an ad-hoc salary paid
--                              straight to a project account, bypassing the accrual.
--                              1 entry, 10,000 (payment 14757, an advance on
--                              February salary).
--   EXCLUDED  the batch run's aggregate PAYROL_PAYMENT (PARTY_ID_TO = '276').
--
-- ***  DO NOT REMOVE THE '276' CONDITION — it is guarding a live data problem.  ***
--   A batch payroll payment is meant to DEBIT each employee's accrued payable
--   (220xxx) and clear it, not re-debit the expense account the accrual already hit.
--   Three historical batch payments got this wrong and debited project expense
--   accounts directly:
--
--       payment 16849  (2026-06-29)   payment 16855  (2026-06-30)
--       payment O17409 (2026-07-30)
--
--       600029  رواتب نسيم           19 entries   309,800
--       124424  قرية السدة           10 entries    58,500
--       124430  نسيم                  2 entries    11,000
--       124425  الصحراوى 10.5         1 entry       9,667
--                                   ------------------------
--                                     31 entries   388,967
--
--   That 388,967 is a DOUBLE-BOOK: on 600029 the accrual charged 813,853 and these
--   settlements charged a further 309,800 to the same account. It is exactly the
--   fault the current code warns against in CreateAcctgTransAndEntriesForPayrollPayment
--   ("re-debiting it here would double-book the salary cost and leave the payable
--   uncleared"). The code is fixed; these three are residue.
--
--   Consequence: without the '276' condition this view would overstate project
--   labour cost by ~35%. The underlying entries still need reversing in the ledger —
--   that is an accounting task, tracked separately, and NOT solved by this view.
CREATE OR REPLACE VIEW Fact_Project_Payroll AS
SELECT
    CONCAT(at.ACCTG_TRANS_ID, ':', ate.ACCTG_TRANS_ENTRY_SEQ_ID)         AS PaymentId,
    at.ACCTG_TRANS_TYPE_ID                                               AS PAYMENT_TYPE_ID,
    CASE WHEN at.ACCTG_TRANS_TYPE_ID = 'PAYROL_INVOICE'
         THEN 'رواتب المشروع (استحقاق)'
         ELSE 'رواتب المشروع (دفع)' END                                  AS PaymentTypeDescription,
    ate.PARTY_ID                                                         AS PartyIdFrom,
    COALESCE(emp.DESCRIPTION, ate.PARTY_ID, '')                          AS PartyIdFromName,
    'Company'                                                            AS PartyIdTo,
    dp.ProjectName                                                       AS PartyIdToName,
    'POSTED'                                                             AS STATUS_ID,
    'تم الترحيل'                                                          AS StatusDescription,
    'Posted'                                                             AS StatusDescriptionEnglish,
    CAST(at.TRANSACTION_DATE AS DATE)                                    AS EFFECTIVE_DATE,
    at.CREATED_STAMP                                                     AS CreatedStamp,
    COALESCE(ate.DESCRIPTION, at.DESCRIPTION)                            AS COMMENTS,
    COALESCE(ate.VOUCHER_REF, at.VOUCHER_REF)                            AS PaymentRefNum,
    COALESCE(ate.AMOUNT, 0)                                              AS AMOUNT,
    COALESCE(ate.AMOUNT, 0)                                              AS ActualCurrencyAmount,
    COALESCE(ate.CURRENCY_UOM_ID, 'EGP')                                 AS CurrencyUomId,
    1                                                                    AS IsDisbursement,
    -- 1 for the accrual, 0 for an ad-hoc payment: lets a measure separate
    -- "labour cost incurred" from "labour cost paid outside the accrual".
    CASE WHEN at.ACCTG_TRANS_TYPE_ID = 'PAYROL_INVOICE' THEN 1 ELSE 0 END AS IsAccrual,
    ate.ORGANIZATION_PARTY_ID                                            AS OrganizationPartyId,
    dp.ProjectId                                                         AS ProjectId,
    dp.ProjectName                                                       AS ProjectName,
    ate.GL_ACCOUNT_ID                                                    AS OverrideGlAccountId,
    dp.GlAccountType                                                     AS ProjectGlAccountType,
    'تم الترحيل'                                                          AS DueStatusArabic
FROM ACCTG_TRANS_ENTRY ate
JOIN ACCTG_TRANS at
       ON at.ACCTG_TRANS_ID = ate.ACCTG_TRANS_ID
JOIN DimProject dp
       ON dp.GlAccountId = ate.GL_ACCOUNT_ID
LEFT JOIN PAYMENT pmt
       ON pmt.PAYMENT_ID = at.PAYMENT_ID
LEFT JOIN PARTY emp
       ON emp.PARTY_ID = ate.PARTY_ID
WHERE ate.DEBIT_CREDIT_FLAG = 'D'
  AND ate.GL_ACCOUNT_ID IS NOT NULL
  -- Match Fact_GL_Transactions, which ends with WHERE t.IS_POSTED = 'Y'. No effect
  -- today (every project payroll entry is posted) but it stops this view drifting
  -- away from the ledger the first time an unposted payroll entry appears. The
  -- status columns above are therefore constants rather than CASE expressions.
  AND at.IS_POSTED = 'Y'
  AND (
        at.ACCTG_TRANS_TYPE_ID = 'PAYROL_INVOICE'
     OR (    at.ACCTG_TRANS_TYPE_ID = 'OUTGOING_PAYMENT'
         AND pmt.PAYMENT_TYPE_ID    = 'PAYROL_PAYMENT'
         AND pmt.PARTY_ID_TO       <> '276')   -- see the block comment above
      );


-- ────────────────────────────────────────────────────────────────────────────
-- 2. Verification — run after creating
-- ────────────────────────────────────────────────────────────────────────────
-- Expected, measured read-only before this script was finalised:
--     85 rows · AMOUNT 1,098,954 · 6 projects · IsAccrual=1 on 84 of the 85 rows
--     (84 accrual rows 1,088,954  +  1 ad-hoc row 10,000)
SELECT COUNT(*)                  AS rows_,
       ROUND(SUM(AMOUNT), 0)     AS total,
       COUNT(DISTINCT ProjectId) AS projects,
       SUM(IsAccrual)            AS accrual_rows
FROM Fact_Project_Payroll;

-- Must return 0: the batch double-book must stay out.
SELECT COUNT(*) AS batch_rows_must_be_zero
FROM Fact_Project_Payroll
WHERE PaymentId LIKE '16849:%' OR PaymentId LIKE '16855:%' OR PaymentId LIKE 'O17409:%';

-- Must return 0 on all three: payroll must not overlap the existing expense views.
SELECT 'payroll ∩ expenses'  AS check_name, COUNT(*) AS must_be_zero
FROM Fact_Project_Payroll p
JOIN Fact_Project_Expenses e            ON e.PaymentId = p.PaymentId
UNION ALL
SELECT 'payroll ∩ direct',   COUNT(*)
FROM Fact_Project_Payroll p
JOIN Fact_Project_DirectPayments_2 d    ON d.PaymentId = p.PaymentId
UNION ALL
SELECT 'payroll ∩ operating', COUNT(*)
FROM Fact_Project_Payroll p
JOIN Fact_Project_OperatingExpenses_2 o ON o.PaymentId = p.PaymentId;


-- ────────────────────────────────────────────────────────────────────────────
-- 3. Power BI model changes
-- ────────────────────────────────────────────────────────────────────────────
-- RELATIONSHIPS (one-directional, many-to-one)
--     Fact_Project_Payroll[ProjectId]      -> DimProjects[ProjectId]
--     Fact_Project_Payroll[EFFECTIVE_DATE] -> DateTbl[Date]
--
-- MEASURES (add to MeasureTbl, beside Total Certificate/Direct/Operating Expenses)
--     Total Project Payroll         = SUM(Fact_Project_Payroll[AMOUNT])
--     Total Project Payroll Accrued =
--         CALCULATE(SUM(Fact_Project_Payroll[AMOUNT]), Fact_Project_Payroll[IsAccrual] = 1)
--
-- AND THE ONE THAT MATTERS — Total Spent understates project cost. Today:
--     [Total Certificate Expenses] + [Total Direct Expenses] + [Total Operating Expenses]
-- Becomes:
--     [Total Certificate Expenses] + [Total Direct Expenses] + [Total Operating Expenses]
--   + [Total Project Payroll]
--
-- Effect: project cost rises by 1,098,954. Note this also moves [Net Profit]
-- (cash basis) = [Total Collected Revenue] - [Total Spent], and both per-project
-- breakdown tables on the Revenue & Expenses page.
--
-- OPTIONAL, to fully match GetProjectReport.cs, add project journal entries too —
-- as a MEASURE over the ledger, not a view (see the header):
--     Project Journal Entries =
--         CALCULATE([Total_FTP_Raw],
--                   Fact_GL_Transactions[ACCTG_TRANS_TYPE_ID]
--                       IN {"GENERAL_JOURNAL", "OPENING_BALANCE"})
-- Adding that to Total Spent contributes a further 7,701,885 (posted only, signed —
-- 8,944,128 opening balance less 1,242,243 net journal credits). Decide it
-- consciously: an 8.9M opening balance landing in "project cost" is defensible for a
-- true cost-to-date figure and misleading for a period figure.


-- ────────────────────────────────────────────────────────────────────────────
-- 4. REQUIRED COMPANION CHANGE — WRITTEN. Deploy in the SAME release.
-- ────────────────────────────────────────────────────────────────────────────
-- Neither payment view excluded PAYROL_PAYMENT, which GetProjectReport.cs does at
-- lines 462 and 696. Exposure is one row — payment 14757, 10,000, an advance on
-- February salary charged to 600029 رواتب نسيم — but it is the SAME row this payroll
-- view now claims, so the two would double count it.
--
--     payroll view | exclusion | the 10,000
--     -------------|-----------|---------------------------------------
--          no      |    no     | counted once  (the old state — correct by luck)
--         YES      |    no     | counted TWICE  <-- do not ship this state
--          no      |   YES     | lost entirely
--         YES      |   YES     | counted once, in payroll  <-- correct
--
-- DONE — this line is now in the WHERE clause of both:
--   API/Sql/Production/Fact_Project_DirectPayments_2.sql
--   API/Sql/Production/Fact_Project_OperatingExpenses_2.sql
--
--       AND pyt.PAYMENT_TYPE_ID <> 'PAYROL_PAYMENT'
--
-- (The batch double-book from section 1 never reaches these two views — those three
--  payments carry no OVERRIDE_GL_ACCOUNT_ID — so this companion change concerns the
--  single ad-hoc payment only.)
--
-- ────────────────────────────────────────────────────────────────────────────
-- 5. THE RELEASE — three files, one deployment
-- ────────────────────────────────────────────────────────────────────────────
-- Apply all three to erp_contracts on 129.146.22.240:3308, then refresh the model.
-- Deploying a subset produces a wrong number, as section 4 shows.
--
--   1. Fact_Project_DirectPayments_2.sql   PMNT_SENT filter removed + payroll excl.
--   2. Fact_Project_OperatingExpenses_2.sql                          payroll excl.
--   3. this file                           creates Fact_Project_Payroll
--
-- Measured effect (local erp_contracts, read-only, before deployment):
--
--   Fact_Project_DirectPayments_2     164 rows  44,785,699  ->  178 rows  127,258,345
--                                                                (+14, +82,472,646)
--   Fact_Project_OperatingExpenses_2  192 rows  33,768,230  ->  191 rows   33,758,230
--                                                                 (-1, -10,000)
--   Fact_Project_Payroll                        (new)       ->   85 rows    1,098,954
--                                                          ------------------------
--   Net change to project cost                                        +83,561,600
--
-- The two payment views cannot interact: DirectPayments_2 takes PROJECT_MAIN accounts
-- only and OperatingExpenses_2 takes everything except PROJECT_MAIN, so the 14 rows
-- returning to the first can never have been in the second.
--
-- Almost all of that +83.5M is the PMNT_SENT removal — post-dated government cheques
-- the ledger had already recognised. It is a correction, not new spending, and it is
-- what finally makes a project reconcile against its GL trial balance. Expect
-- [Total Spent] and [Net Profit] to move sharply on the Revenue & Expenses page, and
-- tell anyone reading those figures why before they see them.
-- ============================================================================
