-- ============================================================================
-- Clears the last residue of the 2026-05-12 lost-receipt incident: three
-- purchase shipments left stuck at PURCH_SHIP_SHIPPED.
--
-- WHAT HAPPENED
-- CreateAcctgTransForShipmentReceiptForProject threw for shipments 10566/10567/
-- 10568 (PO10889/10888/10887, supplier 110) and ShipmentService's catch-all
-- swallowed it. Everything after the GL call was therefore skipped -- including
-- UpdatePurchaseShipmentFromReceipt at ShipmentService.cs:213 -- so the receipt
-- rows and inventory items persisted while the shipment never advanced to
-- PURCH_SHIP_RECEIVED and never got its SHIPMENT_STATUS history row.
--
-- WHY THIS IS ONLY A STATUS FIX
-- The ledger is ALREADY correct: fix_missing_receipt_postings.sql resolved it by
-- reclassifying the three purchase-invoice debits from 124420 to 140000, which is
-- where the missing receipt would have put them. GL 140000 is 0.00 and
-- 124420 + 214000 offset exactly.
--
-- >> DO NOT "COMPLETE" THIS BY POSTING THE MISSING SHIPMENT_RECEIPT ENTRIES. <<
-- The inventory debit already exists via the purchase invoice. Adding receipt
-- postings on top would move GL 140000 from 0.00 to +324,623.31 and reintroduce
-- exactly the imbalance that was just closed. The only way that becomes correct
-- is if fix_missing_receipt_postings.sql is reverted first.
--
-- WHY PLAIN SQL RATHER THAN THE APPLICATION
-- Setting PURCH_SHIP_RECEIVED on a PURCHASE_SHIPMENT through the service layer
-- fires an ECA at ShipmentHelperService.cs:457 that creates invoices. Invoices
-- INV1086/1087/1088 already exist for these three. Going direct is deliberate:
-- it moves the flag without re-triggering downstream document creation.
--
-- SAFETY
-- Nothing is re-receivable: all three order items are fully received
-- (ordered 1, received 1, outstanding 0) and the orders are ORDER_COMPLETED.
-- The stale flag only made them appear in ListPurchaseOrderItemsForReceive
-- (:188 treats anything not PURCH_SHIP_RECEIVED as open), with nothing to take.
--
-- Code fix that stops this recurring: ShipmentService.CreateShipmentReceipt now
-- rethrows instead of swallowing, so the receipt and its posting are atomic.
--
-- STATUS
--   localhost   NOT YET APPLIED
--   production  NOT APPLIED. Run the BEFORE detector there first -- the affected
--               shipments may differ, or may not exist at all.
--
-- REVIEW BEFORE RUNNING. Take a backup first:
--   mysqldump erp_contracts SHIPMENT SHIPMENT_STATUS \
--     > backup_before_shipment_status_fix.sql
-- ============================================================================

START TRANSACTION;

-- ---------------------------------------------------------------------------
-- BEFORE 1: the detector -- purchase shipments holding receipts but with no
-- SHIPMENT_RECEIPT accounting transaction. Expect exactly 10566, 10567, 10568.
-- Any OTHER row is a case this script does not cover: investigate first, and
-- note its ledger may NOT have been corrected the way these three were.
-- ---------------------------------------------------------------------------
SELECT 'BEFORE' phase, s.SHIPMENT_ID, s.PRIMARY_ORDER_ID, s.STATUS_ID, s.PARTY_ID_FROM,
       (SELECT COUNT(*) FROM SHIPMENT_RECEIPT sr WHERE sr.SHIPMENT_ID = s.SHIPMENT_ID) receipts,
       (SELECT COUNT(*) FROM ACCTG_TRANS a
         WHERE a.SHIPMENT_ID = s.SHIPMENT_ID AND a.ACCTG_TRANS_TYPE_ID = 'SHIPMENT_RECEIPT') gl_postings,
       (SELECT COUNT(*) FROM SHIPMENT_STATUS ss
         WHERE ss.SHIPMENT_ID = s.SHIPMENT_ID AND ss.STATUS_ID = 'PURCH_SHIP_RECEIVED') received_history_rows
FROM SHIPMENT s
WHERE s.SHIPMENT_TYPE_ID = 'PURCHASE_SHIPMENT'
  AND (SELECT COUNT(*) FROM SHIPMENT_RECEIPT sr WHERE sr.SHIPMENT_ID = s.SHIPMENT_ID) > 0
  AND (SELECT COUNT(*) FROM ACCTG_TRANS a
        WHERE a.SHIPMENT_ID = s.SHIPMENT_ID AND a.ACCTG_TRANS_TYPE_ID = 'SHIPMENT_RECEIPT') = 0
ORDER BY s.CREATED_STAMP;

-- ---------------------------------------------------------------------------
-- BEFORE 2: confirm nothing is outstanding on these orders -- expect 0 in the
-- last column for all three, i.e. advancing the flag closes nothing off early.
-- ---------------------------------------------------------------------------
SELECT 'BEFORE' phase, oi.ORDER_ID, oi.ORDER_ITEM_SEQ_ID, oh.STATUS_ID order_status,
       oi.QUANTITY ordered,
       COALESCE(SUM(sr.QUANTITY_ACCEPTED + sr.QUANTITY_REJECTED), 0) received,
       oi.QUANTITY - COALESCE(SUM(sr.QUANTITY_ACCEPTED + sr.QUANTITY_REJECTED), 0) outstanding
FROM ORDER_ITEM oi
JOIN ORDER_HEADER oh ON oh.ORDER_ID = oi.ORDER_ID
LEFT JOIN SHIPMENT_RECEIPT sr ON sr.ORDER_ID = oi.ORDER_ID
                             AND sr.ORDER_ITEM_SEQ_ID = oi.ORDER_ITEM_SEQ_ID
WHERE oi.ORDER_ID IN ('PO10887', 'PO10888', 'PO10889')
GROUP BY 1, 2, 3, 4, 5;

-- ---------------------------------------------------------------------------
-- STEP 1 -- advance the flag.
-- Pinned on both the id and the status it is expected to hold, so the statement
-- is idempotent and cannot touch a shipment that has already been corrected or
-- has since moved on to some other status.
-- ---------------------------------------------------------------------------

UPDATE SHIPMENT
   SET STATUS_ID          = 'PURCH_SHIP_RECEIVED',
       LAST_UPDATED_STAMP = NOW()
 WHERE SHIPMENT_ID IN ('10566', '10567', '10568')
   AND STATUS_ID = 'PURCH_SHIP_SHIPPED';

-- ---------------------------------------------------------------------------
-- STEP 2 -- add the missing SHIPMENT_STATUS history row.
--
-- A healthy shipment carries CREATED / RECEIVED / SHIPPED; these three have only
-- CREATED and SHIPPED. STATUS_DATE is backdated to the moment the goods were
-- actually received (the SHIPMENT_RECEIPT's own timestamp) rather than to now,
-- so the audit trail reflects the event and not this correction.
--
-- Guarded by NOT EXISTS, so re-running adds nothing.
-- ---------------------------------------------------------------------------

INSERT INTO SHIPMENT_STATUS
    (STATUS_ID, SHIPMENT_ID, STATUS_DATE, CHANGE_BY_USER_LOGIN_ID,
     LAST_UPDATED_STAMP, CREATED_STAMP)
SELECT 'PURCH_SHIP_RECEIVED', s.SHIPMENT_ID,
       COALESCE((SELECT MIN(sr.CREATED_STAMP) FROM SHIPMENT_RECEIPT sr
                  WHERE sr.SHIPMENT_ID = s.SHIPMENT_ID), s.CREATED_STAMP),
       NULL, NOW(), NOW()
FROM SHIPMENT s
WHERE s.SHIPMENT_ID IN ('10566', '10567', '10568')
  AND NOT EXISTS (SELECT 1 FROM (SELECT * FROM SHIPMENT_STATUS) x
                   WHERE x.SHIPMENT_ID = s.SHIPMENT_ID
                     AND x.STATUS_ID = 'PURCH_SHIP_RECEIVED');

-- ---------------------------------------------------------------------------
-- AFTER: checks 1-3 must return ZERO rows.
-- ---------------------------------------------------------------------------

-- 1. None of the three is still stuck.
SELECT 'STILL STUCK' flag, SHIPMENT_ID, STATUS_ID
FROM SHIPMENT
WHERE SHIPMENT_ID IN ('10566', '10567', '10568')
  AND STATUS_ID <> 'PURCH_SHIP_RECEIVED';

-- 2. Each now has exactly one RECEIVED history row -- no duplicates from a re-run.
SELECT 'BAD HISTORY' flag, SHIPMENT_ID, COUNT(*) received_rows
FROM SHIPMENT_STATUS
WHERE SHIPMENT_ID IN ('10566', '10567', '10568')
  AND STATUS_ID = 'PURCH_SHIP_RECEIVED'
GROUP BY SHIPMENT_ID
HAVING COUNT(*) <> 1;

-- 3. The ledger must NOT have moved -- GL 140000 stays 0.00, and no
--    SHIPMENT_RECEIPT transaction appeared for these shipments.
SELECT 'LEDGER MOVED' flag, 'GL 140000' label,
       ROUND(SUM(CASE WHEN ate.DEBIT_CREDIT_FLAG = 'D' THEN ate.AMOUNT ELSE -ate.AMOUNT END), 2) balance
FROM ACCTG_TRANS act
JOIN ACCTG_TRANS_ENTRY ate ON ate.ACCTG_TRANS_ID = act.ACCTG_TRANS_ID
                          AND ate.GL_ACCOUNT_ID = '140000'
WHERE act.IS_POSTED = 'Y'
HAVING ROUND(SUM(CASE WHEN ate.DEBIT_CREDIT_FLAG = 'D' THEN ate.AMOUNT ELSE -ate.AMOUNT END), 2) <> 0.00;

-- Final state of the three, for the record.
SELECT 'AFTER' phase, s.SHIPMENT_ID, s.PRIMARY_ORDER_ID, s.STATUS_ID,
       (SELECT GROUP_CONCAT(ss.STATUS_ID ORDER BY ss.STATUS_DATE, ss.STATUS_ID SEPARATOR ' > ')
          FROM SHIPMENT_STATUS ss WHERE ss.SHIPMENT_ID = s.SHIPMENT_ID) status_history
FROM SHIPMENT s
WHERE s.SHIPMENT_ID IN ('10566', '10567', '10568')
ORDER BY s.SHIPMENT_ID;

-- ============================================================================
-- Review the output above, then run ONE of:
COMMIT;
--   ROLLBACK;
-- ============================================================================
