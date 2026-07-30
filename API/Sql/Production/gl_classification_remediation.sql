-- =============================================================================
-- gl_classification_remediation.sql
-- Database: erp_contracts  (dev now mirrors production @ 129.146.22.240:3308)
-- Prepared 2026-07-29
--
-- The PENDING GL-classification fixes, consolidated into one ordered script.
-- Everything here was verified against the current (production-mirror) DB: 833
-- active rows in Dim_gl_account.
--
-- ALREADY APPLIED — intentionally NOT repeated here:
--   * dim-gl-account-classification-fixes.sql (view v2 with IS_LEAF/HAS_CHILDREN/
--     SUBACCOUNT_KEY; 124410 -> 'Cheques Under Collection'; 5 bank/cash backfills)
--   * console_9.sql ('Partner Investment Participations' label, 14 accounts)
--   * fix_subaccount_labels.sql (48 accounts -> 43 SUBACC_* labels + WIP Third Party)
--
-- This script supersedes the stand-alone API/Sql/Localhost/fix_partner_current_accounts.sql
-- (Section 1 below is that same fix).
--
-- RUN ORDER:  Section 1  ->  Section 2  ->  (Section 3 only after accountant sign-off)
-- Sections 1 and 2 are each transaction-wrapped and idempotent (safe to re-run).
-- =============================================================================


-- =============================================================================
-- SECTION 1  (ACTIVE)  — Partner current accounts wrongly on the Dividends-Paid line
-- -----------------------------------------------------------------------------
-- Accounts 321000 / 321100 / 321200 ("جارى الشركاء") are owners'-equity partner
-- current accounts, but are classified RETURN_OF_CAPITAL -> RETAINED_EARNINGS ->
-- DIVIDENDS_PAID (الأرباح الموزعة المدفوعة). Their identically-named twins
-- 302000 / 303000 / 304000 are correctly EQUITY -> SHARE_CAPITAL. This aligns the
-- 321xxx group to that correct pattern.
--
-- Changing GL_ACCOUNT_CLASS_ID (root nature) as well as the presentation columns
-- makes the fix durable: otherwise a re-run of gl_account_classifier_update.sql
-- would push them back to DIVIDENDS_PAID.
--
-- SAFETY: all three have zero balance and zero posted transactions — no restatement.
-- The sign flips +1 (debit) -> -1 (credit) automatically (SHARE_CAPITAL is credit-normal).
--
-- NOT TOUCHED (correctly stay on the Dividends-Paid line):
--   321300 السحوبات - الشريك 3  (a genuine drawing)
--   311000 / 343xxx السحوبات    (drawings)
--   334000 / 335000 / 342xxx الأرباح الموزعة  (real dividends)
-- =============================================================================

START TRANSACTION;

UPDATE GL_ACCOUNT
SET GL_ACCOUNT_CLASS_ID        = 'EQUITY',          -- was RETURN_OF_CAPITAL
    GL_SUB_CLASS_2_ID          = 'SHARE_CAPITAL',   -- was RETAINED_EARNINGS
    GL_ACCOUNT_COURSE_LABEL_ID = 'SHARE_CAPITAL'    -- was DIVIDENDS_PAID
WHERE GL_ACCOUNT_ID IN ('321000','321100','321200');
-- expect: 3 rows affected
-- (unchanged, already correct: GL_REPORT_ID=BALANCE_SHEET,
--  GL_CLASS_COURSE_ID=LIABILITIES_AND_OWNERS_EQUITY, GL_SUB_CLASS_ID=OWNERS_EQUITY,
--  GL_ACCOUNT_TYPE_ID=OWNERS_EQUITY, GL_SUB_ACCOUNT_COURSE_LABEL_ID='Partner Current Accounts')

COMMIT;

-- Verify: the three 321xxx should now read identically to the 304000 family,
--         and 321300 (drawing) should still be on DIVIDENDS_PAID.
SELECT d.GL_ACCOUNT_ID, d.ACCOUNT_NAME_ARABIC, d.ACCOUNT AS matrix_line,
       d.ACCOUNT_AR, d.SIGN_MULTIPLIER, d.SUBACCOUNT_AR
FROM Dim_gl_account d
WHERE d.GL_ACCOUNT_ID IN ('302000','303000','304000','321000','321100','321200','321300')
ORDER BY d.ACCOUNT_SORT, d.GL_ACCOUNT_ID;


-- =============================================================================
-- SECTION 2  (ACTIVE)  — SORT_ORDER collision at 200 (Travel vs Trade Payables)
-- -----------------------------------------------------------------------------
-- 'Travel' (السفر) and 'Trade Payables' (ذمم الدائنون التجاريون) both have
-- SORT_ORDER 200, so any matrix ordered by SUBACCOUNT_SORT shows them in an
-- unstable order. Travel belongs with the expense labels (390-460); moving it to
-- 470 (just after 'Interest Expense' = 460; 470 is currently free) resolves the
-- clash without disturbing any other label.
-- =============================================================================

START TRANSACTION;

UPDATE GL_SUB_ACCOUNT_COURSE_LABEL
SET SORT_ORDER = '470'
WHERE GL_SUB_ACCOUNT_COURSE_LABEL_ID = 'Travel'
  AND SORT_ORDER = '200';
-- expect: 1 row affected

COMMIT;

-- Verify: no SORT_ORDER shared by more than one label (expect 0 rows)
SELECT SORT_ORDER, COUNT(*) AS labels,
       GROUP_CONCAT(DESCRIPTION_ARABIC SEPARATOR ' | ') AS names
FROM GL_SUB_ACCOUNT_COURSE_LABEL
GROUP BY SORT_ORDER HAVING COUNT(*) > 1;


-- =============================================================================
-- SECTION 3  (OPTIONAL — needs accountant sign-off; leave commented until then)
-- -----------------------------------------------------------------------------
-- 124410 (شيكات تحت التحصيل) sits on the ACCOUNT-level line CASH_AND_CASH_EQUIVALENTS,
-- so uncollected cheques are reported inside "cash & equivalents". Under IFRS an
-- uncollected cheque is normally a receivable / cash-in-transit, not cash. Its
-- parent is 100010 (current assets), not 110000 (banks) — the hierarchy already
-- agrees it is not a bank. Precedent for in-transit items under RECEIVABLES:
-- 'Credit Card Transit' (مدفوعات بطاقات ائتمان في الطريق).
--
-- !! THIS CHANGES REPORTED CASH ON THE BALANCE SHEET. Confirm before applying.
-- (Its SUBACCOUNT stays 'Cheques Under Collection', which currently sorts at 15 —
--  after this move you may also want to renumber that label into the receivables
--  range so it orders sensibly under the Receivables line.)
-- =============================================================================
-- START TRANSACTION;
-- UPDATE GL_ACCOUNT
-- SET GL_ACCOUNT_COURSE_LABEL_ID = 'RECEIVABLES'
-- WHERE GL_ACCOUNT_ID = '124410';
-- COMMIT;


-- =============================================================================
-- NOT INCLUDED (needs per-account business decisions, cannot be a blind script):
--   * 37 accounts still have NULL sub-account labels (biggest groups: INVENTORY 8,
--     OTHER_PAYABLES 8, RECEIVABLES 4, OTHER_EXPENSES 3). Each needs the correct
--     GL_SUB_ACCOUNT_COURSE_LABEL_ID decided, or a new label. List them with:
--       SELECT ACCOUNT, GL_ACCOUNT_ID, ACCOUNT_NAME_ARABIC
--       FROM Dim_gl_account WHERE SUBACCOUNT IS NULL ORDER BY ACCOUNT;
--
-- AFTER RUNNING: refresh the Power BI model.
-- =============================================================================
