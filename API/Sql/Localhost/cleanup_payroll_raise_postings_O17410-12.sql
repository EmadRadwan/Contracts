-- =============================================================================
-- Cleanup: reset the 3 manual payroll-raise payments and delete their WRONG
--          accounting transactions, so they can be re-approved with the fixed code.
--
-- Context
--   Payments O17410 / O17411 / O17412 are manual salary raises (3,000 / 4,000 /
--   5,000, PaymentTypeId = PAYROL_PAYMENT, override_gl_account_id = 601000).
--   When they were sent, the OLD CreateAcctgTransAndEntriesForPayrollPayment
--   re-posted the ENTIRE July cash payroll (408,416.01) as the debit block of
--   EACH one -- triple-booking payroll across every charge account, including
--   14,034 x3 = 42,102 onto project 124426.
--
--   Fixed method: an ad-hoc PAYROL_PAYMENT (has an override account, or amount
--   != the whole month/method run) now posts a simple, balanced entry:
--       Dr <override_gl_account_id = 601000 Salaries>  /  Cr <cash>
--   for its OWN amount only, never fanning out to the whole run.
--
-- What this script does
--   1. Deletes the erroneous OUTGOING_PAYMENT acctg_trans + entries for the 3 payments
--      (currently acctg_trans 19372 / 19373 / 19374 -- resolved by payment_id, not
--      hard-coded, in case ids differ on another environment).
--   2. Resets the 3 payments PMNT_SENT -> PMNT_NOT_PAID.
--
-- After running: re-approve O17410 / O17411 / O17412 in the app. Each will then
--   post Dr 601000 / Cr cash for 3,000 / 4,000 / 5,000 and touch NO project account.
--
-- Pre-checked on this DB: these payments have NO fin_account_trans and NO
--   payment_application rows, so nothing else needs cleaning up.
--
-- SAFETY: runs in one transaction. Review the BEFORE / AFTER output.
--   Dry run -> change the final COMMIT to ROLLBACK.
-- =============================================================================

START TRANSACTION;

-- ---- BEFORE: what will be deleted / reset ----------------------------------
SELECT 'BEFORE: transactions to delete' AS step,
       t.acctg_trans_id, t.payment_id, t.acctg_trans_type_id, t.is_posted,
       (SELECT COUNT(*) FROM acctg_trans_entry e WHERE e.acctg_trans_id = t.acctg_trans_id) AS entry_lines
FROM acctg_trans t
WHERE t.payment_id IN ('O17410','O17411','O17412')
  AND t.acctg_trans_type_id = 'OUTGOING_PAYMENT';

SELECT 'BEFORE: payment status' AS step, payment_id, status_id, amount, override_gl_account_id
FROM payment
WHERE payment_id IN ('O17410','O17411','O17412');

-- ---- 1) Delete the entry lines (children first, for FK safety) --------------
DELETE e
FROM acctg_trans_entry e
JOIN acctg_trans t ON e.acctg_trans_id = t.acctg_trans_id
WHERE t.payment_id IN ('O17410','O17411','O17412')
  AND t.acctg_trans_type_id = 'OUTGOING_PAYMENT';

-- ---- 2) Delete the transaction headers -------------------------------------
DELETE FROM acctg_trans
WHERE payment_id IN ('O17410','O17411','O17412')
  AND acctg_trans_type_id = 'OUTGOING_PAYMENT';

-- ---- 3) Reset the payments so they can be re-approved -----------------------
UPDATE payment
SET status_id = 'PMNT_NOT_PAID',
    last_updated_stamp = NOW()
WHERE payment_id IN ('O17410','O17411','O17412')
  AND status_id = 'PMNT_SENT';

-- ---- AFTER: confirm ---------------------------------------------------------
SELECT 'AFTER: remaining transactions (expect 0 rows)' AS step, acctg_trans_id, payment_id
FROM acctg_trans
WHERE payment_id IN ('O17410','O17411','O17412')
  AND acctg_trans_type_id = 'OUTGOING_PAYMENT';

SELECT 'AFTER: payment status (expect PMNT_NOT_PAID)' AS step, payment_id, status_id
FROM payment
WHERE payment_id IN ('O17410','O17411','O17412');

-- ---- Sanity: 124426 should no longer carry the payment-side payroll ---------
-- Expect only the 17,734 PAYROL_INVOICE accrual to remain (payment triplication gone).
SELECT 'AFTER: 124426 payroll footprint' AS step, t.acctg_trans_type_id,
       COUNT(*) AS line_count, SUM(e.amount) AS total_amount
FROM acctg_trans_entry e
JOIN acctg_trans t ON e.acctg_trans_id = t.acctg_trans_id
WHERE e.gl_account_id = '124426'
  AND e.debit_credit_flag = 'D'
  AND (e.party_id IN ('621','622','624','625') OR e.description = 'Payroll for July 2026')
GROUP BY t.acctg_trans_type_id;

-- Review all output above. If correct:
COMMIT;
-- If anything looks off instead:
-- ROLLBACK;
