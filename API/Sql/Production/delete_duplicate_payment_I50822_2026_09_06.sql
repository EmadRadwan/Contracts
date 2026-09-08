-- ============================================================================================
-- Deletes payment I50822 — a stray duplicate of payment 16824 on sales request 10881 that the
-- application refuses to delete.
--
-- CONTEXT
--   On 2026-09-01 payment 16824 (Advance payment #2, Apt A7-15, SR 10881) was duplicated twice
--   through DuplicatePayment.cs, producing I50821 (102,000) and I50822 (170,000). That handler
--   copies SALES_REQUEST_ID from the original (DuplicatePayment.cs:64), so each copy carries the
--   sales-request link — and DeletePayment.cs:40 refuses outright to delete ANY payment holding
--   a SALES_REQUEST_ID:
--
--       "لا يمكن حذف الدفعة لأنها مرتبطة بطلب مبيعات رقم: 10881"
--
--   The app therefore creates these copies and offers no way to remove them. The only in-app
--   escape is ResetSalesRequest, which purges all 20 payments on SR 10881 — including the three
--   already received (200,000 + 165,000 + 100,000 = 465,000) — to remove one unused duplicate.
--   That is disproportionate, hence this script.
--
-- WHY A RAW DELETE IS SAFE HERE
--   I50822 is PMNT_NOT_PAID and has NO artifacts whatsoever. Every table that references
--   PAYMENT was checked and each returns 0 rows for it: ACCTG_TRANS, ACCTG_TRANS_ENTRY (via
--   ACCTG_TRANS), DEDUCTION, EMPLOYEE_ADVANCE, PAYMENT_APPLICATION (PAYMENT_ID and
--   TO_PAYMENT_ID), PAYMENT_ATTRIBUTE, PAYMENT_BUDGET_ALLOCATION, PAYMENT_CONTENT,
--   PAYMENT_GROUP_MEMBER, PERF_REVIEW, RETURN_ITEM_RESPONSE, FIN_ACCOUNT_TRANS. Its
--   PAYMENT_PREFERENCE_ID is NULL, so there is no ORDER_PAYMENT_PREFERENCE to unwind either.
--   There is no ledger footprint to reverse: deleting the row removes the payment and nothing
--   else. Step 2's DELETE re-asserts all of that in its WHERE clause and will affect 0 rows if
--   any of it has changed since.
--
-- SCOPE
--   PAYMENT: 1 delete (I50822).
--   NOT touched: I50821 (the other duplicate, 102,000 — deliberately left in place, delete it
--   separately if wanted), payment 16824 (the original), any other payment on SR 10881, the
--   sales request itself, its installments, and every accounting record in the database.
--
-- DO NOT run this unreviewed. Run each step, read the output, then decide.
-- ============================================================================================


-- --------------------------------------------------------------------------------------------
-- Step 1. PRE-FLIGHT. Read both results before touching anything.
--   1a  exactly 1 row: I50822 / RECEIPT_ADVANCE_PAYMENT / PMNT_NOT_PAID / 170000.000 /
--       2026-09-01 / SR 10881, PAYMENT_PREFERENCE_ID NULL.
--   1b  exactly 12 rows, every N = 0. Any non-zero means the payment has grown an artifact
--       since this script was written — STOP and re-assess; it is no longer a free delete.
--   Any disagreement: STOP, the database is not in the state this script expects.
-- --------------------------------------------------------------------------------------------

-- 1a — the row itself
SELECT PAYMENT_ID, PAYMENT_TYPE_ID, STATUS_ID, AMOUNT, EFFECTIVE_DATE,
       PARTY_ID_FROM, PARTY_ID_TO, SALES_REQUEST_ID, PAYMENT_PREFERENCE_ID, COMMENTS
FROM   PAYMENT
WHERE  PAYMENT_ID = 'I50822';

-- 1b — every dependent table must report 0
SELECT 'ACCTG_TRANS'               AS DEPENDENT, COUNT(*) AS N FROM ACCTG_TRANS               WHERE PAYMENT_ID    = 'I50822'
UNION ALL SELECT 'DEDUCTION',                    COUNT(*)      FROM DEDUCTION                 WHERE PAYMENT_ID    = 'I50822'
UNION ALL SELECT 'EMPLOYEE_ADVANCE',             COUNT(*)      FROM EMPLOYEE_ADVANCE          WHERE PAYMENT_ID    = 'I50822'
UNION ALL SELECT 'PAYMENT_APPLICATION',          COUNT(*)      FROM PAYMENT_APPLICATION       WHERE PAYMENT_ID    = 'I50822'
UNION ALL SELECT 'PAYMENT_APPLICATION (to)',     COUNT(*)      FROM PAYMENT_APPLICATION       WHERE TO_PAYMENT_ID = 'I50822'
UNION ALL SELECT 'PAYMENT_ATTRIBUTE',            COUNT(*)      FROM PAYMENT_ATTRIBUTE         WHERE PAYMENT_ID    = 'I50822'
UNION ALL SELECT 'PAYMENT_BUDGET_ALLOCATION',    COUNT(*)      FROM PAYMENT_BUDGET_ALLOCATION WHERE PAYMENT_ID    = 'I50822'
UNION ALL SELECT 'PAYMENT_CONTENT',              COUNT(*)      FROM PAYMENT_CONTENT           WHERE PAYMENT_ID    = 'I50822'
UNION ALL SELECT 'PAYMENT_GROUP_MEMBER',         COUNT(*)      FROM PAYMENT_GROUP_MEMBER      WHERE PAYMENT_ID    = 'I50822'
UNION ALL SELECT 'PERF_REVIEW',                  COUNT(*)      FROM PERF_REVIEW               WHERE PAYMENT_ID    = 'I50822'
UNION ALL SELECT 'RETURN_ITEM_RESPONSE',         COUNT(*)      FROM RETURN_ITEM_RESPONSE      WHERE PAYMENT_ID    = 'I50822'
UNION ALL SELECT 'FIN_ACCOUNT_TRANS',            COUNT(*)      FROM FIN_ACCOUNT_TRANS         WHERE PAYMENT_ID    = 'I50822';


-- --------------------------------------------------------------------------------------------
-- Step 2. THE DELETE. Only if step 1 matched.
--
--   The WHERE clause re-states every precondition rather than trusting step 1 was read: the
--   identity of the row (id, amount, status, sales request) and the absence of all twelve
--   dependents. If anything drifted between the check and the run this deletes 0 rows and
--   nothing is lost. Expected: "Query OK, 1 row affected".
--
--   Run the DELETE, confirm 1 row, then COMMIT. On 0 rows or anything unexpected, ROLLBACK.
-- --------------------------------------------------------------------------------------------

START TRANSACTION;

DELETE FROM PAYMENT
WHERE  PAYMENT_ID       = 'I50822'
  AND  STATUS_ID        = 'PMNT_NOT_PAID'
  AND  AMOUNT           = 170000.000
  AND  SALES_REQUEST_ID = '10881'
  AND  NOT EXISTS (SELECT 1 FROM ACCTG_TRANS               t WHERE t.PAYMENT_ID    = 'I50822')
  AND  NOT EXISTS (SELECT 1 FROM DEDUCTION                 d WHERE d.PAYMENT_ID    = 'I50822')
  AND  NOT EXISTS (SELECT 1 FROM EMPLOYEE_ADVANCE          e WHERE e.PAYMENT_ID    = 'I50822')
  AND  NOT EXISTS (SELECT 1 FROM PAYMENT_APPLICATION      pa WHERE pa.PAYMENT_ID   = 'I50822')
  AND  NOT EXISTS (SELECT 1 FROM PAYMENT_APPLICATION      pt WHERE pt.TO_PAYMENT_ID= 'I50822')
  AND  NOT EXISTS (SELECT 1 FROM PAYMENT_ATTRIBUTE        at WHERE at.PAYMENT_ID   = 'I50822')
  AND  NOT EXISTS (SELECT 1 FROM PAYMENT_BUDGET_ALLOCATION ba WHERE ba.PAYMENT_ID  = 'I50822')
  AND  NOT EXISTS (SELECT 1 FROM PAYMENT_CONTENT          pc WHERE pc.PAYMENT_ID   = 'I50822')
  AND  NOT EXISTS (SELECT 1 FROM PAYMENT_GROUP_MEMBER     gm WHERE gm.PAYMENT_ID   = 'I50822')
  AND  NOT EXISTS (SELECT 1 FROM PERF_REVIEW              pr WHERE pr.PAYMENT_ID   = 'I50822')
  AND  NOT EXISTS (SELECT 1 FROM RETURN_ITEM_RESPONSE     ri WHERE ri.PAYMENT_ID   = 'I50822')
  AND  NOT EXISTS (SELECT 1 FROM FIN_ACCOUNT_TRANS        ft WHERE ft.PAYMENT_ID   = 'I50822');

-- Read the affected-rows count above. 1 → COMMIT. Anything else → ROLLBACK.
COMMIT;


-- --------------------------------------------------------------------------------------------
-- Step 3. POST-FLIGHT. Confirm the outcome.
--   3a  0 rows — I50822 is gone.
--   3b  exactly three lines for SR 10881 — the three already-received payments untouched and
--       I50821 still present:
--           PMNT_NOT_PAID   16   5,658,600.000
--           PMNT_RECEIVED    3     465,000.000   (200,000 / 165,000 / 100,000)
--           NULL (rollup)   19   6,123,600.000
-- --------------------------------------------------------------------------------------------

-- 3a
SELECT COUNT(*) AS SHOULD_BE_ZERO FROM PAYMENT WHERE PAYMENT_ID = 'I50822';

-- 3b
SELECT STATUS_ID, COUNT(*) AS PAYMENTS, SUM(AMOUNT) AS TOTAL
FROM   PAYMENT
WHERE  SALES_REQUEST_ID = '10881'
GROUP  BY STATUS_ID WITH ROLLUP;
