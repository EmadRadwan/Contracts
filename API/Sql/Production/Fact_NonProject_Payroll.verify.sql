-- =============================================================================
-- VERIFICATION — Fact_NonProject_Payroll
-- =============================================================================
-- Run this whole file after creating the view. Every statement is live SQL, so
-- it executes top to bottom in DataGrip / Workbench / the mysql CLI unedited.
--
-- Checks 2 and 3 compute PASS/FAIL from the data rather than comparing against
-- hardcoded numbers, so they are correct on production as well as on a dev
-- copy. Reference figures from the 2026-09-04 dev copy are context only.
-- These are read-only SELECTs. Nothing here modifies data.
-- =============================================================================


-- ── 1. Headline ──────────────────────────────────────────────────────────────
-- Dev copy 2026-09-04: 78 rows / 1,396,856  —  67 accrual / 1,190,648
--                                            —  11 ad-hoc  /   206,208
SELECT 'Headline'                    AS check_name,
       COUNT(*)                      AS rows_total,
       ROUND(SUM(AMOUNT))            AS amount_total,
       SUM(IsAccrual)                AS accrual_rows,
       ROUND(SUM(CASE WHEN IsAccrual = 1 THEN AMOUNT ELSE 0 END)) AS accrual_amount,
       ROUND(SUM(CASE WHEN IsAccrual = 0 THEN AMOUNT ELSE 0 END)) AS adhoc_amount,
       MIN(EFFECTIVE_DATE)           AS earliest,
       MAX(EFFECTIVE_DATE)           AS latest
FROM Fact_NonProject_Payroll;


-- ── 2. Completeness ──────────────────────────────────────────────────────────
-- This view + Fact_Project_Payroll must together account for every posted
-- payroll debit that the '276' rule does not exclude. gap must be 0.
SELECT 'Completeness' AS check_name,
       all_payroll,
       via_project,
       via_this_view,
       all_payroll - via_project - via_this_view AS gap,
       CASE WHEN all_payroll - via_project - via_this_view = 0
            THEN 'PASS' ELSE 'FAIL — labour cost is unaccounted for' END AS verdict
FROM (
    SELECT
      (SELECT COALESCE(ROUND(SUM(ate.AMOUNT)), 0)
         FROM ACCTG_TRANS_ENTRY ate
         JOIN ACCTG_TRANS at   ON at.ACCTG_TRANS_ID = ate.ACCTG_TRANS_ID
         LEFT JOIN PAYMENT pmt ON pmt.PAYMENT_ID    = at.PAYMENT_ID
        WHERE ate.DEBIT_CREDIT_FLAG = 'D'
          AND ate.GL_ACCOUNT_ID IS NOT NULL
          AND at.IS_POSTED = 'Y'
          AND (at.ACCTG_TRANS_TYPE_ID = 'PAYROL_INVOICE'
            OR (at.ACCTG_TRANS_TYPE_ID = 'OUTGOING_PAYMENT'
                AND pmt.PAYMENT_TYPE_ID = 'PAYROL_PAYMENT'
                AND pmt.PARTY_ID_TO <> '276')))            AS all_payroll,
      (SELECT COALESCE(ROUND(SUM(AMOUNT)), 0) FROM Fact_Project_Payroll)    AS via_project,
      (SELECT COALESCE(ROUND(SUM(AMOUNT)), 0) FROM Fact_NonProject_Payroll) AS via_this_view
) t;


-- ── 3. Disjointness ──────────────────────────────────────────────────────────
-- No entry may appear in both payroll views. overlap_rows must be 0.
--
-- NOTE: written as an explicit JOIN, not `WHERE PayrollId IN (SELECT ...)`.
-- The IN form returns the CROSS PRODUCT of the two views on this MySQL build
-- (78 x 85 = 6,630) instead of the 0 it should return, because neither side is
-- indexable and the semi-join rewrite misfires on the CONCAT-derived key. The
-- join form below is correct — verified against the same data.
SELECT 'Disjointness' AS check_name,
       COUNT(*)       AS overlap_rows,
       CASE WHEN COUNT(*) = 0 THEN 'PASS' ELSE 'FAIL — double counting' END AS verdict
FROM      (SELECT PayrollId AS k FROM Fact_NonProject_Payroll) n
JOIN      (SELECT PaymentId AS k FROM Fact_Project_Payroll)    p ON p.k = n.k;


-- ── 4. The '276' double-book still sitting in the ledger ─────────────────────
-- NOT part of the view. This is the accounting problem the exclusion guards.
-- Every row below is a batch payroll settlement that debited an EXPENSE account
-- instead of clearing the 220xxx payable the accrual created, so the cost is
-- booked twice in the ledger. Dev copy 2026-09-04: 10 entries, 3,097,024.
-- These need reversing entries; the views neither include nor fix them.
SELECT at.PAYMENT_ID,
       DATE(at.TRANSACTION_DATE)                          AS trans_date,
       ate.GL_ACCOUNT_ID                                  AS gl_account,
       COALESCE(ga.ACCOUNT_NAME_ARABIC, ga.ACCOUNT_NAME)  AS account_name,
       ROUND(ate.AMOUNT)                                  AS amount,
       CASE WHEN dp.GlAccountId IS NULL THEN 'non-project' ELSE 'project' END AS side
FROM ACCTG_TRANS_ENTRY ate
JOIN ACCTG_TRANS at     ON at.ACCTG_TRANS_ID = ate.ACCTG_TRANS_ID
JOIN PAYMENT pmt        ON pmt.PAYMENT_ID    = at.PAYMENT_ID
LEFT JOIN GL_ACCOUNT ga ON ga.GL_ACCOUNT_ID  = ate.GL_ACCOUNT_ID
LEFT JOIN DimProject dp ON dp.GlAccountId    = ate.GL_ACCOUNT_ID
WHERE ate.DEBIT_CREDIT_FLAG = 'D'
  AND at.IS_POSTED = 'Y'
  AND pmt.PAYMENT_TYPE_ID = 'PAYROL_PAYMENT'
  AND pmt.PARTY_ID_TO = '276'
  AND COALESCE(ga.GL_ACCOUNT_TYPE_ID, '') <> 'ACCOUNTS_PAYABLE'
ORDER BY ate.AMOUNT DESC;


-- ── 5. Rollup by account ─────────────────────────────────────────────────────
-- Dev copy 2026-09-04: 601000 = 1,330,759 · 600021 = 29,030 · 601060 = 24,000
SELECT GlAccountId,
       GlAccountName,
       COUNT(*)                                                  AS n,
       ROUND(SUM(AMOUNT))                                        AS total,
       ROUND(SUM(CASE WHEN IsAccrual = 1 THEN AMOUNT ELSE 0 END)) AS accrual,
       ROUND(SUM(CASE WHEN IsAccrual = 0 THEN AMOUNT ELSE 0 END)) AS adhoc
FROM Fact_NonProject_Payroll
GROUP BY GlAccountId, GlAccountName
ORDER BY total DESC;


-- ── 6. Monthly profile ───────────────────────────────────────────────────────
-- Company-wide labour cost by month, both views combined. Use this to answer
-- "has payroll stopped being posted?" — the project view alone shows nothing
-- after 2026-07-30, which is a recording gap, not a business fact.
SELECT DATE_FORMAT(EFFECTIVE_DATE, '%Y-%m') AS month_,
       ROUND(SUM(project_amt))              AS project_payroll,
       ROUND(SUM(nonproject_amt))           AS nonproject_payroll,
       ROUND(SUM(project_amt + nonproject_amt)) AS total_payroll
FROM (
    SELECT EFFECTIVE_DATE, AMOUNT AS project_amt, 0 AS nonproject_amt FROM Fact_Project_Payroll
    UNION ALL
    SELECT EFFECTIVE_DATE, 0, AMOUNT FROM Fact_NonProject_Payroll
) u
GROUP BY DATE_FORMAT(EFFECTIVE_DATE, '%Y-%m')
ORDER BY month_;
