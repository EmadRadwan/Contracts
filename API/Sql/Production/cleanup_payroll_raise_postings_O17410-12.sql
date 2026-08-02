-- =============================================================================
-- Cleanup: reset the 3 manual payroll-raise payments and delete their WRONG
--          accounting transactions, so they can be re-approved with the fixed code.
--
-- Context
--   Payments O17410 / O17411 / O17412 are manual salary raises (3,000 / 4,000 /
--   5,000, PAYMENT_TYPE_ID = PAYROL_PAYMENT, OVERRIDE_GL_ACCOUNT_ID = 601000).
--   When they were sent, the OLD CreateAcctgTransAndEntriesForPayrollPayment
--   re-posted the ENTIRE July cash payroll (408,416.01) as the debit block of
--   EACH one -- triple-booking payroll across every charge account, including
--   14,034 x3 = 42,102 onto project 124426.
--
--   Fixed method: an ad-hoc PAYROL_PAYMENT (has an override account, or amount
--   != the whole month/method run) now posts a simple, balanced entry:
--       Dr <OVERRIDE_GL_ACCOUNT_ID = 601000 Salaries>  /  Cr <cash>
--   for its OWN amount only, never fanning out to the whole run.
--
-- What this script does
--   1. Deletes the erroneous OUTGOING_PAYMENT ACCTG_TRANS + entries for the 3 payments
--      (currently ACCTG_TRANS 19372 / 19373 / 19374 -- resolved by PAYMENT_ID, not
--      hard-coded, in case ids differ on another environment).
--   2. Resets the 3 payments PMNT_SENT -> PMNT_NOT_PAID.
--
-- After running: re-approve O17410 / O17411 / O17412 in the app. Each will then
--   post Dr 601000 / Cr cash for 3,000 / 4,000 / 5,000 and touch NO project account.
--
-- Pre-checked on this DB: these payments have NO FIN_ACCOUNT_TRANS and NO
--   PAYMENT_APPLICATION rows, so nothing else needs cleaning up.
--
-- SAFETY: runs in one transaction. Review the BEFORE / AFTER output.
--   Dry run -> change the final COMMIT to ROLLBACK.
-- =============================================================================

START TRANSACTION;

-- ---- BEFORE: what will be deleted / reset ----------------------------------
SELECT 'BEFORE: transactions to delete' AS step,
       T.ACCTG_TRANS_ID, T.PAYMENT_ID, T.ACCTG_TRANS_TYPE_ID, T.IS_POSTED,
       (SELECT COUNT(*) FROM ACCTG_TRANS_ENTRY E WHERE E.ACCTG_TRANS_ID = T.ACCTG_TRANS_ID) AS entry_lines
FROM ACCTG_TRANS T
WHERE T.PAYMENT_ID IN ('O17410','O17411','O17412')
  AND T.ACCTG_TRANS_TYPE_ID = 'OUTGOING_PAYMENT';

SELECT 'BEFORE: payment status' AS step,
       PAYMENT_ID, STATUS_ID, AMOUNT, OVERRIDE_GL_ACCOUNT_ID
FROM PAYMENT
WHERE PAYMENT_ID IN ('O17410','O17411','O17412');

-- ---- 1) Delete the entry lines (children first, for FK safety) --------------
DELETE E
FROM ACCTG_TRANS_ENTRY E
         JOIN ACCTG_TRANS T ON E.ACCTG_TRANS_ID = T.ACCTG_TRANS_ID
WHERE T.PAYMENT_ID IN ('O17410','O17411','O17412')
  AND T.ACCTG_TRANS_TYPE_ID = 'OUTGOING_PAYMENT';

-- ---- 2) Delete the transaction headers -------------------------------------
DELETE FROM ACCTG_TRANS
WHERE PAYMENT_ID IN ('O17410','O17411','O17412')
  AND ACCTG_TRANS_TYPE_ID = 'OUTGOING_PAYMENT';

-- ---- 3) Reset the payments so they can be re-approved -----------------------
UPDATE PAYMENT
SET STATUS_ID = 'PMNT_NOT_PAID',
    LAST_UPDATED_STAMP = NOW()
WHERE PAYMENT_ID IN ('O17410','O17411','O17412')
  AND STATUS_ID = 'PMNT_SENT';

-- ---- AFTER: confirm ---------------------------------------------------------
SELECT 'AFTER: remaining transactions (expect 0 rows)' AS step,
       ACCTG_TRANS_ID, PAYMENT_ID
FROM ACCTG_TRANS
WHERE PAYMENT_ID IN ('O17410','O17411','O17412')
  AND ACCTG_TRANS_TYPE_ID = 'OUTGOING_PAYMENT';

SELECT 'AFTER: payment status (expect PMNT_NOT_PAID)' AS step,
       PAYMENT_ID, STATUS_ID
FROM PAYMENT
WHERE PAYMENT_ID IN ('O17410','O17411','O17412');

-- ---- Sanity: 124426 should no longer carry the payment-side payroll ---------
-- Expect only the 17,734 PAYROL_INVOICE accrual to remain (payment triplication gone).
SELECT 'AFTER: 124426 payroll footprint' AS step,
       T.ACCTG_TRANS_TYPE_ID,
       COUNT(*) AS line_count,
       SUM(E.AMOUNT) AS total_amount
FROM ACCTG_TRANS_ENTRY E
         JOIN ACCTG_TRANS T ON E.ACCTG_TRANS_ID = T.ACCTG_TRANS_ID
WHERE E.GL_ACCOUNT_ID = '124426'
  AND E.DEBIT_CREDIT_FLAG = 'D'
  AND (E.PARTY_ID IN ('621','622','624','625') OR E.DESCRIPTION = 'Payroll for July 2026')
GROUP BY T.ACCTG_TRANS_TYPE_ID;

-- Review all output above. If correct:
COMMIT;
-- If anything looks off instead:
-- ROLLBACK;
