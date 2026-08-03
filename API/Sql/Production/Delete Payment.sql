-- =============================================================================
-- Delete Payment.sql
-- Deletes a single PAYMENT and all of its related artifacts, starting from the
-- payment id (not from a sales request).
--
-- Removes:  accounting transactions + entries, fin-account transactions,
--           payment applications, attributes, contents, budget allocations,
--           and payment-group memberships tied to this payment.
-- Nulls:    soft references from context rows (deduction, employee advance,
--           perf review, return item response) so their FKs don't block the
--           delete and the surrounding business rows survive.
--
-- Set @pmtId below, review, then run. Wrapped in a transaction — inspect the
-- row counts, then COMMIT (or ROLLBACK to abort).
-- =============================================================================

SET @pmtId = '16758';

START TRANSACTION;

-- 1. Accounting transaction entries created from this payment
DELETE ate
FROM ACCTG_TRANS_ENTRY ate
         INNER JOIN ACCTG_TRANS at
                    ON ate.ACCTG_TRANS_ID = at.ACCTG_TRANS_ID
WHERE at.PAYMENT_ID = @pmtId;

-- 2. Accounting transaction header(s)
DELETE FROM ACCTG_TRANS
WHERE PAYMENT_ID = @pmtId;

-- 3. Break the circular FK before deleting the fin-account transaction
# UPDATE PAYMENT
# SET FIN_ACCOUNT_TRANS_ID = NULL
# WHERE PAYMENT_ID = @pmtId
#   AND FIN_ACCOUNT_TRANS_ID IS NOT NULL;

-- 4. Fin-account transaction(s) for this payment
DELETE FROM FIN_ACCOUNT_TRANS
WHERE PAYMENT_ID = @pmtId;

-- 5. Payment applications (this payment as the paying or the to-payment side)
DELETE FROM PAYMENT_APPLICATION
WHERE PAYMENT_ID = @pmtId
   OR TO_PAYMENT_ID = @pmtId;

-- 6. Other direct children of PAYMENT
DELETE FROM PAYMENT_ATTRIBUTE        WHERE PAYMENT_ID = @pmtId;
DELETE FROM PAYMENT_CONTENT          WHERE PAYMENT_ID = @pmtId;
DELETE FROM PAYMENT_BUDGET_ALLOCATION WHERE PAYMENT_ID = @pmtId;
DELETE FROM PAYMENT_GROUP_MEMBER     WHERE PAYMENT_ID = @pmtId;

-- 7. Soft references — keep the business rows, just detach them from the payment
UPDATE DEDUCTION            SET PAYMENT_ID = NULL WHERE PAYMENT_ID = @pmtId;
UPDATE EMPLOYEE_ADVANCE     SET PAYMENT_ID = NULL WHERE PAYMENT_ID = @pmtId;
UPDATE PERF_REVIEW          SET PAYMENT_ID = NULL WHERE PAYMENT_ID = @pmtId;
UPDATE RETURN_ITEM_RESPONSE SET PAYMENT_ID = NULL WHERE PAYMENT_ID = @pmtId;

-- 8. Finally, the payment itself
DELETE FROM PAYMENT
WHERE PAYMENT_ID = @pmtId;

-- Review the affected rows above, then:
COMMIT;
-- ROLLBACK;
