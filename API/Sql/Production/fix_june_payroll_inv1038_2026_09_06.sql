-- ============================================================================================
-- Restores June payroll to what the batch actually produced: the 2-day absence deduction (667)
-- on INV1038, and each invoice's payment-method grouping AS IT STOOD WHEN THE RUN EXECUTED.
--
-- CONTEXT — two out-of-process changes broke a settled run
--   1. 2026-08-05  Party 155 (احمد عبدالرحمن, INV1032, 60,000) had his payroll method changed
--                  from BANK_TRANSFER to CASH. Group membership is derived live, so this
--                  retroactively removed him from a June bank run that had already executed.
--   2. 2026-08-25  INV1038 (احمد عبد العال, party 169) was reverted to INVOICE_IN_PROCESS, its
--                  PAYROL_DD_ABSENCE item deleted, and set back to INVOICE_READY — regenerating
--                  its accounting transaction (20788) at 10,000 instead of 9,333.
--   Together these moved the bank run from 322,078 to 262,745 while payment 16855 stayed at
--   322,078, and it has been unpostable since.
--
-- APPROACH — restore, do not re-plan
--   An earlier draft of this script split 16855 into two payments. That is no longer necessary
--   and is no longer the right answer. Restoring BOTH facts the run was built on makes every
--   June figure tie out with no payment changed and no new payment created:
--
--       BANK  20 invoices  322,078.00   =  payment 16855  322,078   (unsent, will now post)
--       CASH  30 invoices  330,684.00   =  payment 16849  330,684   (already sent and posted)
--
--   It also repairs the cash side as a by-product. Payment 16849 posted correctly on 19 July
--   against 30 employees, but the 5 August preference change left the live cash run reading
--   390,684 — 60,000 more than the payment it settled. After this script the two agree again.
--
-- THE HISTORICAL GROUPING IS EVIDENCE, NOT INFERENCE
--   It is taken from the posted cash transaction 18786, which debited exactly 30 employees'
--   accrued accounts. Those 30 were the CASH group when the run settled; the other 20 June
--   invoices were the BANK group. Party 155 is absent from those 30, which is what confirms he
--   was BANK_TRANSFER at the time. No judgement is applied and no value is typed in.
--
--   This is the opposite of backfilling from CURRENT preferences, which would write CASH for
--   party 155 and bake in the very defect being fixed. Do not do that.
--
-- !! HARD PREREQUISITE !!
--   The snapshot-reading code must be DEPLOYED before payment 16855 is sent — commit 130429d1,
--   in master. Without it GeneralLedgerService still reads the live Party preference, computes a
--   262,078 bank run, and refuses 16855 with "payment 322078.00 vs run total 262078.00".
--   The SQL below is safe to run either way; only the sending depends on the deploy.
--
-- SCOPE
--   SQL:  INVOICE_ITEM (1 insert), ACCTG_TRANS_ENTRY (2 updates), INVOICE_ATTRIBUTE (50 inserts).
--   UI:   sending payment 16855 — step 6.
--   NOT changed: payment 16855's amount, payment 16849, any other invoice, any party record.
--   It does NOT reset or re-run June payroll.
--
-- DO NOT run this unreviewed. Run each step, read the output, then decide.
-- ============================================================================================


-- --------------------------------------------------------------------------------------------
-- Step 1. PRE-FLIGHT. Read all four results before touching anything.
--   1a  exactly 1 row — the salary item only, no absence item present.
--   1b  2 rows, both 10000.000.
--   1c  AMOUNT 322078.000, STATUS_ID PMNT_NOT_PAID, OVERRIDE_GL_ACCOUNT_ID NULL.
--   1d  BANK_TRANSFER 19 / 262745.00 and CASH 31 / 390684.00 — the BROKEN live grouping this
--       script replaces. Neither matches its payment; that is the problem, not a surprise.
--   Any disagreement: STOP, the database is not in the state this script expects.
-- --------------------------------------------------------------------------------------------

-- 1a. INV1038's items
SELECT ii.INVOICE_ITEM_SEQ_ID AS SEQ, ii.INVOICE_ITEM_TYPE_ID AS ITEM_TYPE,
       ii.QUANTITY, ii.AMOUNT, ii.QUANTITY * ii.AMOUNT AS LINE_TOTAL, ii.DESCRIPTION
FROM INVOICE_ITEM ii WHERE ii.INVOICE_ID = 'INV1038' ORDER BY ii.INVOICE_ITEM_SEQ_ID;

-- 1b. The accrual behind INV1038
SELECT ate.ACCTG_TRANS_ID, ate.ACCTG_TRANS_ENTRY_SEQ_ID AS SEQ, ate.GL_ACCOUNT_ID,
       ate.DEBIT_CREDIT_FLAG AS DR_CR, ate.AMOUNT, ate.DESCRIPTION
FROM ACCTG_TRANS_ENTRY ate
JOIN ACCTG_TRANS at ON at.ACCTG_TRANS_ID = ate.ACCTG_TRANS_ID
WHERE at.INVOICE_ID = 'INV1038' ORDER BY ate.ACCTG_TRANS_ENTRY_SEQ_ID;

-- 1c. The blocked payment — its amount is NOT changed by this script
SELECT p.PAYMENT_ID, p.PAYMENT_TYPE_ID, p.PARTY_ID_TO, p.PAYMENT_METHOD_ID,
       p.AMOUNT, p.STATUS_ID, p.EFFECTIVE_DATE, p.OVERRIDE_GL_ACCOUNT_ID
FROM PAYMENT p WHERE p.PAYMENT_ID = '16855';

-- 1d. The broken live grouping
SELECT pty.PREFERRED_PAYROLL_PMT_METHOD AS LIVE_METHOD, COUNT(*) AS INVOICES,
       ROUND(SUM(t.total), 2) AS RUN_TOTAL
FROM INVOICE i
JOIN PARTY pty ON pty.PARTY_ID = i.PARTY_ID_FROM
JOIN (SELECT ii.INVOICE_ID,
             ROUND(SUM(ii.QUANTITY * ii.AMOUNT
                       * CASE WHEN iit.IS_POSITIVE_AMOUNT = 0 THEN -1 ELSE 1 END), 2) AS total
      FROM INVOICE_ITEM ii JOIN INVOICE_ITEM_TYPE iit USING (INVOICE_ITEM_TYPE_ID)
      GROUP BY ii.INVOICE_ID) t ON t.INVOICE_ID = i.INVOICE_ID
WHERE i.INVOICE_TYPE_ID = 'PAYROL_INVOICE' AND i.PARTY_ID = 'Company'
  AND i.INVOICE_DATE BETWEEN '2026-06-01' AND '2026-06-30'
GROUP BY 1;


-- --------------------------------------------------------------------------------------------
-- Step 2. THE CORRECTION. Every statement is guarded, so the script is idempotent.
--   Expect 1, 2 and 50 rows affected.
-- --------------------------------------------------------------------------------------------
START TRANSACTION;

-- 2a. Restore the absence item. quantity = days, amount = per-day rate — the batch's convention,
--     which ListAbsenceData and ListPayrollData2 read as the day count. 2 x 333.500 = 667.000.
INSERT INTO INVOICE_ITEM
      (INVOICE_ID, INVOICE_ITEM_SEQ_ID, INVOICE_ITEM_TYPE_ID,
       QUANTITY, AMOUNT, DESCRIPTION, CREATED_STAMP, LAST_UPDATED_STAMP)
SELECT 'INV1038', '00002', 'PAYROL_DD_ABSENCE',
       2.000000, 333.500, 'Absence: 2 days', UTC_TIMESTAMP(), UTC_TIMESTAMP()
WHERE NOT EXISTS (SELECT 1 FROM INVOICE_ITEM x
                  WHERE x.INVOICE_ID = 'INV1038'
                    AND x.INVOICE_ITEM_TYPE_ID = 'PAYROL_DD_ABSENCE');

-- 2b. Bring the accrual into line: 10,000 -> 9,333 on both legs.
--     CreateAcctgTransForPayrollInvoice debits additions (10,000) minus expense-reducing
--     deductions (667), and credits the same figure as Net Salary Payable, so both carry 9,333.
UPDATE ACCTG_TRANS_ENTRY ate
JOIN ACCTG_TRANS at ON at.ACCTG_TRANS_ID = ate.ACCTG_TRANS_ID
SET ate.AMOUNT = 9333.000, ate.LAST_UPDATED_STAMP = UTC_TIMESTAMP()
WHERE at.INVOICE_ID = 'INV1038'
  AND at.ACCTG_TRANS_TYPE_ID = 'PAYROL_INVOICE'
  AND ate.AMOUNT = 10000.000
  AND ate.GL_ACCOUNT_ID IN ('601000', '220013');

-- 2c. Freeze each June invoice's run-time payment method as a PAYROLL_PMT_METHOD attribute —
--     the same snapshot BatchCreatePayrollInvoices now writes for every new run.
--     CASH  = the 30 employees the posted cash transaction 18786 actually debited.
--     BANK_TRANSFER = the remaining 20, party 155 among them.
--     Reading is by the ROW's presence, so writing these makes June immune to any future
--     preference change — including another one on party 155.
INSERT INTO INVOICE_ATTRIBUTE
      (INVOICE_ID, ATTR_NAME, ATTR_VALUE, ATTR_DESCRIPTION,
       CREATED_STAMP, LAST_UPDATED_STAMP)
SELECT i.INVOICE_ID,
       'PAYROLL_PMT_METHOD',
       CASE WHEN EXISTS (SELECT 1 FROM ACCTG_TRANS_ENTRY ate
                         WHERE ate.ACCTG_TRANS_ID = '18786'
                           AND ate.DEBIT_CREDIT_FLAG = 'D'
                           AND ate.PARTY_ID = i.PARTY_ID_FROM)
            THEN 'CASH' ELSE 'BANK_TRANSFER' END,
       'Payroll payment method at run time (restored 2026-09-06 from posted cash trans 18786)',
       UTC_TIMESTAMP(), UTC_TIMESTAMP()
FROM INVOICE i
WHERE i.INVOICE_TYPE_ID = 'PAYROL_INVOICE'
  AND i.PARTY_ID        = 'Company'
  AND i.INVOICE_DATE BETWEEN '2026-06-01' AND '2026-06-30'
  AND NOT EXISTS (SELECT 1 FROM INVOICE_ATTRIBUTE ia
                  WHERE ia.INVOICE_ID = i.INVOICE_ID
                    AND ia.ATTR_NAME  = 'PAYROLL_PMT_METHOD');

UPDATE INVOICE SET LAST_UPDATED_STAMP = UTC_TIMESTAMP() WHERE INVOICE_ID = 'INV1038';


-- --------------------------------------------------------------------------------------------
-- Step 3. VERIFY BEFORE COMMITTING. Run both while the transaction is still open.
--   3a — every OK_ column must read 1.
--   3b — the decisive check. Both rows must show OK_MATCHES = 1:
--          BANK_TRANSFER  20  322078.00  vs payment 16855  322078.000
--          CASH           30  330684.00  vs payment 16849  330684.000
-- --------------------------------------------------------------------------------------------

-- 3a
SELECT
    (SELECT ROUND(SUM(ii.QUANTITY * ii.AMOUNT
                      * CASE WHEN iit.IS_POSITIVE_AMOUNT = 0 THEN -1 ELSE 1 END), 2)
     FROM INVOICE_ITEM ii JOIN INVOICE_ITEM_TYPE iit USING (INVOICE_ITEM_TYPE_ID)
     WHERE ii.INVOICE_ID = 'INV1038')                                    AS INV1038_NET,
    (SELECT ROUND(SUM(ii.QUANTITY * ii.AMOUNT
                      * CASE WHEN iit.IS_POSITIVE_AMOUNT = 0 THEN -1 ELSE 1 END), 2) = 9333.00
     FROM INVOICE_ITEM ii JOIN INVOICE_ITEM_TYPE iit USING (INVOICE_ITEM_TYPE_ID)
     WHERE ii.INVOICE_ID = 'INV1038')                                    AS OK_INVOICE_9333,
    (SELECT COUNT(*) = 2 FROM ACCTG_TRANS_ENTRY ate
     JOIN ACCTG_TRANS at ON at.ACCTG_TRANS_ID = ate.ACCTG_TRANS_ID
     WHERE at.INVOICE_ID = 'INV1038' AND ate.AMOUNT = 9333.000)          AS OK_ACCRUAL_9333,
    (SELECT SUM(CASE WHEN ate.DEBIT_CREDIT_FLAG = 'D' THEN ate.AMOUNT ELSE -ate.AMOUNT END) = 0
     FROM ACCTG_TRANS_ENTRY ate
     JOIN ACCTG_TRANS at ON at.ACCTG_TRANS_ID = ate.ACCTG_TRANS_ID
     WHERE at.INVOICE_ID = 'INV1038')                                    AS OK_ACCRUAL_BALANCED,
    (SELECT COUNT(*) FROM INVOICE_ATTRIBUTE ia JOIN INVOICE i USING (INVOICE_ID)
     WHERE ia.ATTR_NAME = 'PAYROLL_PMT_METHOD'
       AND i.INVOICE_TYPE_ID = 'PAYROL_INVOICE'
       AND i.INVOICE_DATE BETWEEN '2026-06-01' AND '2026-06-30') = 50    AS OK_50_SNAPSHOTS,
    (SELECT ia.ATTR_VALUE FROM INVOICE_ATTRIBUTE ia
     WHERE ia.INVOICE_ID = 'INV1032' AND ia.ATTR_NAME = 'PAYROLL_PMT_METHOD')
                                                                         AS PARTY155_SNAPSHOT;

-- 3b
SELECT ia.ATTR_VALUE                                   AS SNAPSHOT_METHOD,
       COUNT(*)                                        AS INVOICES,
       ROUND(SUM(t.total), 2)                          AS RUN_TOTAL,
       pay.AMOUNT                                      AS PAYMENT_AMOUNT,
       pay.PAYMENT_ID,
       (ROUND(SUM(t.total), 2) = pay.AMOUNT)           AS OK_MATCHES
FROM INVOICE i
JOIN INVOICE_ATTRIBUTE ia ON ia.INVOICE_ID = i.INVOICE_ID
                         AND ia.ATTR_NAME  = 'PAYROLL_PMT_METHOD'
JOIN (SELECT ii.INVOICE_ID,
             ROUND(SUM(ii.QUANTITY * ii.AMOUNT
                       * CASE WHEN iit.IS_POSITIVE_AMOUNT = 0 THEN -1 ELSE 1 END), 2) AS total
      FROM INVOICE_ITEM ii JOIN INVOICE_ITEM_TYPE iit USING (INVOICE_ITEM_TYPE_ID)
      GROUP BY ii.INVOICE_ID) t ON t.INVOICE_ID = i.INVOICE_ID
JOIN PAYMENT pay ON pay.PAYMENT_ID = CASE ia.ATTR_VALUE WHEN 'CASH' THEN '16849'
                                                        ELSE '16855' END
WHERE i.INVOICE_TYPE_ID = 'PAYROL_INVOICE' AND i.PARTY_ID = 'Company'
  AND i.INVOICE_DATE BETWEEN '2026-06-01' AND '2026-06-30'
GROUP BY ia.ATTR_VALUE, pay.AMOUNT, pay.PAYMENT_ID;


-- --------------------------------------------------------------------------------------------
-- Step 4. If step 3 is all 1s:
-- --------------------------------------------------------------------------------------------
COMMIT;
-- If anything is off:
-- ROLLBACK;


-- --------------------------------------------------------------------------------------------
-- Step 5. OPTIONAL, RECOMMENDED — protect July the same way.
--   July needs no correction: its two payments already tie out exactly against the live
--   preferences (BANK 19 / 266,727.00 = O17409; CASH 37 / 408,416.01 = O17408). Snapshotting
--   that verified-correct grouping simply freezes it, so the next preference change cannot do
--   to July what it did to June. Run the 3b-style check afterwards to confirm both still match.
--   Safe here ONLY because July's live grouping is provably correct — never assume that.
-- --------------------------------------------------------------------------------------------
-- START TRANSACTION;
-- INSERT INTO INVOICE_ATTRIBUTE
--       (INVOICE_ID, ATTR_NAME, ATTR_VALUE, ATTR_DESCRIPTION, CREATED_STAMP, LAST_UPDATED_STAMP)
-- SELECT i.INVOICE_ID, 'PAYROLL_PMT_METHOD', pty.PREFERRED_PAYROLL_PMT_METHOD,
--        'Payroll payment method at run time (frozen 2026-09-06; live grouping verified against O17408/O17409)',
--        UTC_TIMESTAMP(), UTC_TIMESTAMP()
-- FROM INVOICE i
-- JOIN PARTY pty ON pty.PARTY_ID = i.PARTY_ID_FROM
-- WHERE i.INVOICE_TYPE_ID = 'PAYROL_INVOICE' AND i.PARTY_ID = 'Company'
--   AND i.INVOICE_DATE BETWEEN '2026-07-01' AND '2026-07-31'
--   AND pty.PREFERRED_PAYROLL_PMT_METHOD IS NOT NULL
--   AND NOT EXISTS (SELECT 1 FROM INVOICE_ATTRIBUTE ia
--                   WHERE ia.INVOICE_ID = i.INVOICE_ID AND ia.ATTR_NAME = 'PAYROLL_PMT_METHOD');
-- COMMIT;


-- --------------------------------------------------------------------------------------------
-- Step 6. SENDING. In the UI set payment 16855 (322,078, unchanged) to PMNT_SENT.
--   It posts as the run settlement: a 322,078 bank credit against the accrued salary accounts of
--   all 20 bank employees, party 155's 220008 among them — clearing his June 60,000, which has
--   been outstanding since the run. No second payment is needed.
--
--   !! Requires commit 130429d1 deployed. Without it the posting reads the live preference,
--      computes 262,078, and refuses. See the HARD PREREQUISITE at the top.
--
--   !! DO NOT RE-RUN JUNE PAYROLL. BatchCreatePayrollInvoices deletes and rebuilds every invoice
--      and every payment addressed to party 276 for the month, destroying this correction.
--
--   !! DO NOT RESET PAYMENT 16849. It is correctly posted (transaction 18786). After this script
--      the cash run matches it again, so a reset would be re-postable — but a reset still deletes
--      a posted transaction and its audit trail, which is the practice under review.
--
--   !! DO NOT change party 155's payment method back to BANK_TRANSFER. CASH is correct going
--      forward — July already paid him through the cash aggregate O17408 (2026-07-30, debit
--      220008, 60,000). The snapshot written in 2c is what makes June correct; the live
--      preference should keep describing how he is paid from now on.
-- --------------------------------------------------------------------------------------------


-- ============================================================================================
-- ROLLBACK AFTER COMMIT, if ever needed. Restores the pre-fix values verbatim.
--
--   START TRANSACTION;
--   DELETE FROM INVOICE_ITEM
--    WHERE INVOICE_ID = 'INV1038' AND INVOICE_ITEM_SEQ_ID = '00002'
--      AND INVOICE_ITEM_TYPE_ID = 'PAYROL_DD_ABSENCE';
--   UPDATE ACCTG_TRANS_ENTRY ate
--     JOIN ACCTG_TRANS at ON at.ACCTG_TRANS_ID = ate.ACCTG_TRANS_ID
--      SET ate.AMOUNT = 10000.000, ate.LAST_UPDATED_STAMP = UTC_TIMESTAMP()
--    WHERE at.INVOICE_ID = 'INV1038' AND ate.AMOUNT = 9333.000;
--   DELETE ia FROM INVOICE_ATTRIBUTE ia JOIN INVOICE i USING (INVOICE_ID)
--    WHERE ia.ATTR_NAME = 'PAYROLL_PMT_METHOD'
--      AND i.INVOICE_TYPE_ID = 'PAYROL_INVOICE'
--      AND i.INVOICE_DATE BETWEEN '2026-06-01' AND '2026-06-30';
--   COMMIT;
-- ============================================================================================
