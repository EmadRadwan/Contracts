-- Removes the duplicate GL posting for Payment I50121.
-- AcctgTrans 19071 (posted 2026-07-28 10:43:45) is the original, kept as-is.
-- AcctgTrans 19078 (posted 2026-07-28 12:17:14) is the duplicate created when the
-- payment was re-approved after drifting back to PMNT_NOT_PAID without its first
-- posting being cleaned up. It nets to zero on its own (D 111010 / C 124800, same
-- 499,875.000 both legs), so a straight delete is safe here — no reversing entry
-- needed since nothing outside this pair depends on it.
--
-- Verified before writing this: no rows in ACCTG_TRANS_ATTRIBUTE or
-- GL_RECONCILIATION_ENTRY reference either 19071 or 19078.

START TRANSACTION;

-- Sanity check — confirm we're deleting exactly the expected duplicate before committing.
SELECT ACCTG_TRANS_ID, ACCTG_TRANS_TYPE_ID, PAYMENT_ID, IS_POSTED, POSTED_DATE
FROM ACCTG_TRANS
WHERE ACCTG_TRANS_ID = '19078' AND PAYMENT_ID = 'I50121';

DELETE FROM ACCTG_TRANS_ENTRY
WHERE ACCTG_TRANS_ID = '19078';

DELETE FROM ACCTG_TRANS
WHERE ACCTG_TRANS_ID = '19078' AND PAYMENT_ID = 'I50121';

-- Expect: GL 124800's ending balance rises back by 499,875.00 (less credited),
-- GL 111010's ending balance falls back by 499,875.00 (less debited).
COMMIT;
