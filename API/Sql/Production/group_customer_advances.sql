-- =============================================================================
-- group_customer_advances.sql
-- Database: erp_contracts
-- Prepared 2026-07-29
--
-- Groups all "money collected from customers that is NOT yet recognized as sales"
-- under ONE shared sub-account label, so a single measure can total it and any
-- FUTURE deposit account is picked up automatically (no hardcoded account list).
--
-- These are all already correctly classified as LIABILITIES (OTHER_PAYABLES) — this
-- only changes their SUB-ACCOUNT drill label so they aggregate as one group. It is
-- NOT a reclassification and does not touch any amounts or the balance sheet totals.
--
-- Accounts grouped (all ACCOUNT = OTHER_PAYABLES, SUBCLASS = LIABILITIES):
--   250120  دفعات مقدمة من العملاء           (unit installment advances)  ~650.7M
--   250453  جدية حجز مشروع سوا               (booking deposit)              ~9.7M
--   250459  جدية حجز مشروع لادريس اكتوبر      (booking deposit)              ~2.0M
--   250437  جدية حجز مشروع نسيم              (booking deposit)              ~0.5M
--   250000  الإيرادات المقدمة                (deferred/unearned revenue)     0 (future)
--   Total ≈ 663M held as advances/deposits, not sales.
--
-- Idempotent: INSERT ... ON DUPLICATE KEY UPDATE + deterministic UPDATE. Safe to re-run.
-- After running: refresh the Power BI model. The 'Customer Advances' measure filters
--   SUBACCOUNT = 'Customer Advances & Deposits', so the summary card then shows ~663M.
-- =============================================================================

START TRANSACTION;

-- 1. New shared sub-account label (sort 235 = between Customer Credits 230 and Subcontractor Payables 240)
INSERT INTO GL_SUB_ACCOUNT_COURSE_LABEL
  (GL_SUB_ACCOUNT_COURSE_LABEL_ID, DESCRIPTION, DESCRIPTION_ARABIC, SORT_ORDER)
  VALUES ('Customer Advances & Deposits', 'Customer Advances & Deposits',
          'دفعات وعرابين العملاء المقدمة', '235')
  ON DUPLICATE KEY UPDATE DESCRIPTION=VALUES(DESCRIPTION),
    DESCRIPTION_ARABIC=VALUES(DESCRIPTION_ARABIC), SORT_ORDER=VALUES(SORT_ORDER);

-- 2. Repoint the five accounts onto it
UPDATE GL_ACCOUNT
SET GL_SUB_ACCOUNT_COURSE_LABEL_ID = 'Customer Advances & Deposits'
WHERE GL_ACCOUNT_ID IN ('250120','250453','250459','250437','250000');
-- expect: 5 rows affected

COMMIT;

-- Verification: all five should now share the group, and the total should be ~663M
SELECT d.GL_ACCOUNT_ID, d.ACCOUNT_NAME_ARABIC, d.SUBACCOUNT, d.SUBACCOUNT_AR
FROM Dim_gl_account d
WHERE d.GL_ACCOUNT_ID IN ('250120','250453','250459','250437','250000')
ORDER BY d.GL_ACCOUNT_ID;

SELECT ROUND(SUM(e.AMOUNT * IF(e.DEBIT_CREDIT_FLAG='C',1,-1))) AS total_advances_663m
FROM ACCTG_TRANS_ENTRY e
JOIN ACCTG_TRANS t ON t.ACCTG_TRANS_ID = e.ACCTG_TRANS_ID AND t.IS_POSTED='Y'
JOIN Dim_gl_account d ON d.GL_ACCOUNT_ID = e.GL_ACCOUNT_ID
WHERE d.SUBACCOUNT = 'Customer Advances & Deposits';

-- Optional (not included): 213300 'ودائع مقدمة للطلبيات' (custom-order deposits) is a
-- borderline member — add its id to the UPDATE above if the business counts it here.
-- =============================================================================
