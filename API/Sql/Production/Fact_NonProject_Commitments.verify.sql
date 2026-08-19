-- =============================================================================
-- VERIFICATION — Fact_NonProject_Commitments
-- =============================================================================
-- Run this whole file after creating the view. Every statement below is live
-- SQL (nothing commented out), so running the file top to bottom works in
-- DataGrip / MySQL Workbench / the mysql CLI without editing anything.
--
-- All three checks are self-validating: they compute PASS/FAIL from the data
-- rather than comparing against hardcoded numbers, so they are correct on
-- production as well as on a dev copy. Reference figures from the 17 August
-- dev copy are given for context only — production will differ.
--
-- These are read-only SELECTs. Nothing here modifies data.
-- =============================================================================


-- ── 1. Headline ──────────────────────────────────────────────────────────────
-- What the view holds, split by timing.
-- Dev copy 17 Aug: 69 rows total / 64,501,124  —  10 future / 55,896,380
--                                              —  59 overdue / 8,579,144
SELECT 'Headline'                                        AS check_name,
       COUNT(*)                                          AS rows_total,
       ROUND(SUM(AMOUNT))                                AS amount_total,
       SUM(IsFuture)                                     AS rows_future,
       ROUND(SUM(FutureAmount))                          AS amount_future,
       SUM(IsOverdue)                                    AS rows_overdue,
       ROUND(SUM(OverdueAmount))                         AS amount_overdue,
       MIN(EFFECTIVE_DATE)                               AS earliest_maturity,
       MAX(EFFECTIVE_DATE)                               AS latest_maturity
FROM Fact_NonProject_Commitments;


-- ── 2. Completeness ──────────────────────────────────────────────────────────
-- This view + the two project views must together account for every unpaid
-- disbursement whose due date is still ahead. gap must be 0.
-- Dev copy 17 Aug: 142,839,026 = 82,472,646 + 4,470,000 + 55,896,380, gap 0.
SELECT 'Completeness' AS check_name,
       company_wide,
       via_direct,
       via_operating,
       via_this_view,
       company_wide - via_direct - via_operating - via_this_view AS gap,
       CASE WHEN company_wide - via_direct - via_operating - via_this_view = 0
            THEN 'PASS' ELSE 'FAIL — money is unaccounted for' END AS verdict
FROM (
    SELECT
      (SELECT COALESCE(ROUND(SUM(pyt.AMOUNT)), 0)
         FROM PAYMENT pyt
         JOIN PAYMENT_TYPE ptt ON pyt.PAYMENT_TYPE_ID = ptt.PAYMENT_TYPE_ID
        WHERE (ptt.PARENT_TYPE_ID = 'DISBURSEMENT' OR ptt.PAYMENT_TYPE_ID = 'DISBURSEMENT')
          AND pyt.STATUS_ID = 'PMNT_NOT_PAID'
          AND pyt.EFFECTIVE_DATE > CURDATE())                       AS company_wide,
      (SELECT COALESCE(ROUND(SUM(AMOUNT)), 0) FROM Fact_Project_DirectPayments_2
        WHERE STATUS_ID = 'PMNT_NOT_PAID' AND EFFECTIVE_DATE > CURDATE())  AS via_direct,
      (SELECT COALESCE(ROUND(SUM(AMOUNT)), 0) FROM Fact_Project_OperatingExpenses_2
        WHERE STATUS_ID = 'PMNT_NOT_PAID' AND EFFECTIVE_DATE > CURDATE())  AS via_operating,
      (SELECT COALESCE(ROUND(SUM(FutureAmount)), 0) FROM Fact_NonProject_Commitments)
                                                                    AS via_this_view
) t;


-- ── 3. Disjointness ──────────────────────────────────────────────────────────
-- No commitment may also appear in a project fact view. overlap_rows must be 0.
SELECT 'Disjointness' AS check_name,
       COUNT(*)       AS overlap_rows,
       CASE WHEN COUNT(*) = 0 THEN 'PASS' ELSE 'FAIL — double counting' END AS verdict
FROM Fact_NonProject_Commitments c
WHERE c.CommitmentId IN (SELECT PaymentId FROM Fact_Project_DirectPayments_2)
   OR c.CommitmentId IN (SELECT PaymentId FROM Fact_Project_OperatingExpenses_2)
   OR c.CommitmentId IN (SELECT PaymentId FROM Fact_Project_Expenses);


-- ── 4. What is actually in there ─────────────────────────────────────────────
-- The commitments themselves, largest first. Useful for eyeballing whether the
-- population matches what the accountant expects.
SELECT CommitmentId,
       PAYMENT_TYPE_ID,
       GlAccountId,
       GlAccountName,
       ROUND(AMOUNT)        AS amount,
       DATE(EFFECTIVE_DATE) AS due,
       DueBucketArabic
FROM Fact_NonProject_Commitments
ORDER BY AMOUNT DESC;


-- ── 5. Rollup by account ─────────────────────────────────────────────────────
-- Dev copy 17 Aug (future only): 250530 = 48,000,000 · 124810 = 6,396,380
--                                250444 =  1,500,000
SELECT GlAccountId,
       GlAccountName,
       COUNT(*)                 AS n,
       ROUND(SUM(AMOUNT))       AS total,
       ROUND(SUM(FutureAmount)) AS future,
       ROUND(SUM(OverdueAmount)) AS overdue
FROM Fact_NonProject_Commitments
GROUP BY GlAccountId, GlAccountName
ORDER BY total DESC;
