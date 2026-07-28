-- ============================================================================
-- Corrects three supply receipts whose GL posting was lost, leaving GL 140000
-- (مخزون) with an impossible credit balance.
--
-- WHAT HAPPENED
-- On 2026-05-12, shipments 10566/10567/10568 (PO10889/10888/10887, supplier 110)
-- were received. CreateAcctgTransForShipmentReceiptForProject threw, and
-- ShipmentService.CreateShipmentReceipt's catch-all swallowed the exception --
-- so the ShipmentReceipt rows and InventoryItems persisted while the debit to
-- GL 140000 was never written. The tell is the shipment status: all three are
-- still PURCH_SHIP_SHIPPED, because the status update runs after the GL call.
--
-- Knock-on: with no SHIPMENT_RECEIPT posting to find, the purchase invoices
-- raised later (2026-07-11, INV1086/1087/1088) took the fallback branch in
-- GeneralLedgerService (~line 2330) and debited 124420 PROJECTS UNDER
-- CONSTRUCTION -- the generic parent -- instead of inventory. Their real
-- project accounts had already been debited correctly by the site issuance, so
-- 124420 is carrying 324,623.31 that belongs to no project at all.
--
--   cert       WE      project  project GL  invoice debited  issuance debited
--   110-0001   11333   100      124424      124420 (wrong)   124424 (right)
--   110-0002   11335   105      124426      124420 (wrong)   124426 (right)
--   110-0003   11337   111      140702      124420 (wrong)   140702 (right)
--
-- THE CORRECTION
-- Reclassify the three purchase-invoice debits from 124420 to 140000. AP
-- (210011) is already credited exactly once by those invoices and does not
-- move. This takes misfiled cost out of 124420 and puts it where the receipt
-- should have put it, closing the receipt side of GL 140000.
--
-- Code fixes that stop this recurring:
--   ShipmentService.CreateShipmentReceipt -- rethrows instead of swallowing, so
--     the receipt and its posting are atomic (the caller owns a transaction).
--   CreateAcctgTransForShipmentReceipt / ...ForProject -- resolve the receipt
--     and inventory item from the change tracker OR the database, and throw a
--     named error instead of dereferencing null.
--   ...ForProject -- refuses to post a receipt it cannot value, instead of
--     silently booking origAmount = 0.
--
-- STATUS
--   localhost   APPLIED 2026-07-28. Verified after commit: GL 140000 = +80.00
--               (the discount residual, cleared separately by
--               fix_certificate_100_0005_discount.sql), GL 124420 down exactly
--               324,623.31 to 3,458,376.00, AP 210011 untouched.
--   production  NOT APPLIED. Run the BEFORE queries there first: the affected
--               receipts may differ. The detector is the last BEFORE query --
--               purchase shipments holding receipts with no SHIPMENT_RECEIPT
--               accounting transaction.
--
-- NOT covered: a residual 79.99 on GL 140000 after this runs. Landed costs are
-- sourced twice -- receipts cost from ORDER_ADJUSTMENT (8,146.82 in total),
-- issuances from the certificate item's TransportationExpenses/Gratuities/
-- Discount columns (8,066.83). The two drift. Fixing that means picking one
-- source of truth and is a code change, not a data correction.
--
-- REVIEW BEFORE RUNNING. Take a backup first:
--   mysqldump erp_contracts ACCTG_TRANS ACCTG_TRANS_ENTRY \
--     > backup_before_receipt_fix.sql
-- ============================================================================

START TRANSACTION;

-- ---------------------------------------------------------------------------
-- BEFORE 1: the three misfiled debits -- expect 124420 x 3, total 324,623.31
-- ---------------------------------------------------------------------------
SELECT 'BEFORE' phase, act.ACCTG_TRANS_ID, act.INVOICE_ID, act.WORK_EFFORT_ID,
       ate.ACCTG_TRANS_ENTRY_SEQ_ID seq, ate.DEBIT_CREDIT_FLAG dc,
       ate.GL_ACCOUNT_ID, ate.AMOUNT
FROM ACCTG_TRANS act
JOIN ACCTG_TRANS_ENTRY ate ON ate.ACCTG_TRANS_ID = act.ACCTG_TRANS_ID
WHERE act.ACCTG_TRANS_ID IN ('17982', '17983', '17984')
ORDER BY act.ACCTG_TRANS_ID, ate.ACCTG_TRANS_ENTRY_SEQ_ID;

-- ---------------------------------------------------------------------------
-- BEFORE 2: GL 140000 -- expect -324,543.31, a credit balance on an asset
-- ---------------------------------------------------------------------------
SELECT 'BEFORE' phase, 'GL 140000' label,
       ROUND(SUM(CASE WHEN ate.DEBIT_CREDIT_FLAG = 'D' THEN ate.AMOUNT ELSE -ate.AMOUNT END), 2) balance
FROM ACCTG_TRANS act
JOIN ACCTG_TRANS_ENTRY ate ON ate.ACCTG_TRANS_ID = act.ACCTG_TRANS_ID
                          AND ate.GL_ACCOUNT_ID = '140000'
WHERE act.IS_POSTED = 'Y';

-- ---------------------------------------------------------------------------
-- BEFORE 3: the detector -- purchase shipments whose receipts never posted.
-- Expect exactly shipments 10566, 10567, 10568. Any OTHER row here is a case
-- this script does not cover: investigate before continuing.
-- ---------------------------------------------------------------------------
SELECT 'UNPOSTED RECEIPT' flag, s.SHIPMENT_ID, s.PRIMARY_ORDER_ID, s.STATUS_ID, s.PARTY_ID_FROM,
       (SELECT COUNT(*) FROM SHIPMENT_RECEIPT sr WHERE sr.SHIPMENT_ID = s.SHIPMENT_ID) receipts,
       (SELECT COUNT(*) FROM ACCTG_TRANS a
         WHERE a.SHIPMENT_ID = s.SHIPMENT_ID AND a.ACCTG_TRANS_TYPE_ID = 'SHIPMENT_RECEIPT') gl_postings
FROM SHIPMENT s
WHERE s.SHIPMENT_TYPE_ID = 'PURCHASE_SHIPMENT'
  AND (SELECT COUNT(*) FROM SHIPMENT_RECEIPT sr WHERE sr.SHIPMENT_ID = s.SHIPMENT_ID) > 0
  AND (SELECT COUNT(*) FROM ACCTG_TRANS a
        WHERE a.SHIPMENT_ID = s.SHIPMENT_ID AND a.ACCTG_TRANS_TYPE_ID = 'SHIPMENT_RECEIPT') = 0
ORDER BY s.CREATED_STAMP;

-- ---------------------------------------------------------------------------
-- STEP 1 -- reclassify the debit leg from 124420 to 140000.
--
-- Pinned on transaction id, sequence, direction AND the account it is expected
-- to hold, so the statement is idempotent and cannot touch the credit leg or a
-- row that has already been corrected.
-- ---------------------------------------------------------------------------

UPDATE ACCTG_TRANS_ENTRY
   SET GL_ACCOUNT_ID      = '140000',
       GL_ACCOUNT_TYPE_ID = 'INVENTORY_ACCOUNT',
       LAST_UPDATED_STAMP = NOW()
 WHERE ACCTG_TRANS_ID IN ('17982', '17983', '17984')
   AND ACCTG_TRANS_ENTRY_SEQ_ID = '01'
   AND DEBIT_CREDIT_FLAG = 'D'
   AND GL_ACCOUNT_ID = '124420';

-- ---------------------------------------------------------------------------
-- AFTER: checks 1-3 must return ZERO rows.
-- ---------------------------------------------------------------------------

-- 1. No debit of these three transactions still points at 124420.
SELECT 'STILL MISFILED' flag, ACCTG_TRANS_ID, GL_ACCOUNT_ID, AMOUNT
FROM ACCTG_TRANS_ENTRY
WHERE ACCTG_TRANS_ID IN ('17982', '17983', '17984')
  AND DEBIT_CREDIT_FLAG = 'D'
  AND GL_ACCOUNT_ID <> '140000';

-- 2. The credit leg is untouched -- AP still carries all three, once each.
SELECT 'AP MOVED' flag, ACCTG_TRANS_ID, GL_ACCOUNT_ID, AMOUNT
FROM ACCTG_TRANS_ENTRY
WHERE ACCTG_TRANS_ID IN ('17982', '17983', '17984')
  AND DEBIT_CREDIT_FLAG = 'C'
  AND GL_ACCOUNT_ID <> '210011';

-- 3. All three transactions still balance.
SELECT 'UNBALANCED' flag, ACCTG_TRANS_ID,
       SUM(CASE WHEN DEBIT_CREDIT_FLAG = 'D' THEN AMOUNT ELSE -AMOUNT END) diff
FROM ACCTG_TRANS_ENTRY
WHERE ACCTG_TRANS_ID IN ('17982', '17983', '17984')
GROUP BY ACCTG_TRANS_ID
HAVING ABS(diff) > 0.001;

-- 4. GL 140000 -- expect +80.00 (the 79.99 landed-cost residual noted above).
SELECT 'AFTER' phase, 'GL 140000' label,
       ROUND(SUM(CASE WHEN ate.DEBIT_CREDIT_FLAG = 'D' THEN ate.AMOUNT ELSE -ate.AMOUNT END), 2) balance
FROM ACCTG_TRANS act
JOIN ACCTG_TRANS_ENTRY ate ON ate.ACCTG_TRANS_ID = act.ACCTG_TRANS_ID
                          AND ate.GL_ACCOUNT_ID = '140000'
WHERE act.IS_POSTED = 'Y';

-- 5. GL 124420 -- expect it to drop by exactly 324,623.31.
SELECT 'AFTER' phase, 'GL 124420' label,
       ROUND(SUM(CASE WHEN ate.DEBIT_CREDIT_FLAG = 'D' THEN ate.AMOUNT ELSE -ate.AMOUNT END), 2) balance
FROM ACCTG_TRANS act
JOIN ACCTG_TRANS_ENTRY ate ON ate.ACCTG_TRANS_ID = act.ACCTG_TRANS_ID
                          AND ate.GL_ACCOUNT_ID = '124420'
WHERE act.IS_POSTED = 'Y';

-- ============================================================================
-- Review the output above, then run ONE of:
--   COMMIT;
--   ROLLBACK;
-- ============================================================================
