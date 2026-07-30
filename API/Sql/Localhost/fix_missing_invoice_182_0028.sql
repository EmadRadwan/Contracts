-- ============================================================================
-- Generates the missing purchase invoice for supply certificate 182-0028
-- (WE 17722, order PO11055), created 2026-07-30.
--
-- WHY IT WAS MISSING
-- The invoice is created only on a genuine transition INTO ORDER_COMPLETED
-- (OrderHelperService.cs:368  ->  request.StatusId != response.OldStatusId).
-- On this order the four receipts pre-completed it: UpdateOrderStatusFromReceipt
-- (ShipmentService.cs:518) sets ORDER_COMPLETED by direct assignment and never
-- calls CreateInvoiceFromOrder. By the time ProcessWorkmanCertificatePurchaseOrder
-- called OrderStatusChanges(ORDER_COMPLETED), the order was already completed, so
-- the guard was false and no invoice was raised. The receipts had already credited
-- 214000 (UNINVOICED_SHIP_RCPT); with no invoice to clear it, 214000 sits at a
-- 59,000 credit and the supplier's AP (210100) sits at a 59,000 debit from the
-- payment, with nothing to credit it.
--
-- WHAT THIS BUILDS  -- exactly what CreateInvoiceFromOrder +
-- CreateAcctgTransForPurchaseInvoice produce for every other current-path supply
-- certificate (template: INV1130 / WE 17679 / PO11044):
--   INVOICE               INV1359  (PURCHASE_INVOICE, INVOICE_READY)
--   INVOICE_ITEM   x4     PINV_CERTIFICATE_SUPPLY_ITEM, one per order line
--   ORDER_ITEM_BILLING x4 linking each line + its receipt to the invoice
--   INVOICE_STATUS x2     IN_PROCESS -> READY
--   ACCTG_TRANS 19372     PURCHASE_INVOICE, posted:
--       D 214000  46,000 / 5,670 / 5,800 / 1,530   (clears UNINVOICED_SHIP_RCPT)
--       C 210100  59,000                            (books real AP for supplier 182)
--
-- END STATE (for WE 17722): 214000 -> 0.00, AP 210100 -> 0.00, inventory
-- 140000 unchanged. The payment already made now has its matching liability.
--
-- >>> PRODUCTION WARNING -- HARD-CODED IDS <<<
-- This mints INV1359 and ACCTG_TRANS 19372 from the CURRENT dev sequence values
-- (PARTY_ACCTG_PREFERENCE.LAST_INVOICE_NUMBER=1358, SEQUENCE_VALUE_ITEM
-- .AcctgTrans=19371). On production those counters differ, so INV1359 / 19372
-- may already be taken. DO NOT run this verbatim there. Preferred production
-- route: deploy the code fix and re-trigger invoicing so the app allocates ids
-- itself. If a manual script is unavoidable, recompute both ids from that DB's
-- live sequences first.
--
-- Code fix that stops this recurring: make invoice creation idempotent rather
-- than transition-gated (call CreateInvoiceFromOrder guarded by "no invoice yet",
-- or route UpdateOrderStatusFromReceipt through OrderStatusChanges). NOT yet done.
--
-- STATUS
--   localhost   NOT YET APPLIED
--   production  NOT APPLIED. See the hard-coded-ids warning above.
--
-- REVIEW BEFORE RUNNING. Take a backup first:
--   mysqldump erp_contracts INVOICE INVOICE_ITEM INVOICE_STATUS ORDER_ITEM_BILLING \
--     ACCTG_TRANS ACCTG_TRANS_ENTRY PARTY_ACCTG_PREFERENCE SEQUENCE_VALUE_ITEM \
--     > backup_before_invoice_182_0028.sql
-- ============================================================================

SET NAMES utf8mb4;
SET @inv := 'INV1359';
SET @atx := '19372';
SET @sup := '182';
SET @org := 'Company';
SET @dt  := '2026-07-30';
SET @now := NOW();

START TRANSACTION;

-- ---------------------------------------------------------------------------
-- BEFORE: expect 214000 credit 59,000 and AP 210100 debit 59,000, no invoice.
-- If the "invoices" count is not 0, this order already has one -- STOP.
-- ---------------------------------------------------------------------------
SELECT 'BEFORE' phase,
       (SELECT COUNT(DISTINCT INVOICE_ID) FROM ORDER_ITEM_BILLING WHERE ORDER_ID='PO11055') invoices,
       ROUND((SELECT SUM(CASE WHEN ate.DEBIT_CREDIT_FLAG='D' THEN ate.AMOUNT ELSE -ate.AMOUNT END)
              FROM ACCTG_TRANS act JOIN ACCTG_TRANS_ENTRY ate ON ate.ACCTG_TRANS_ID=act.ACCTG_TRANS_ID
              WHERE act.WORK_EFFORT_ID='17722' AND ate.GL_ACCOUNT_ID='214000'),2) we_214000,
       ROUND((SELECT SUM(CASE WHEN ate.DEBIT_CREDIT_FLAG='D' THEN ate.AMOUNT ELSE -ate.AMOUNT END)
              FROM ACCTG_TRANS act JOIN ACCTG_TRANS_ENTRY ate ON ate.ACCTG_TRANS_ID=act.ACCTG_TRANS_ID
              WHERE act.WORK_EFFORT_ID='17722' AND ate.GL_ACCOUNT_ID='210100'),2) we_ap_210100;

-- Guard: refuse to proceed if this order already has an invoice, or if the
-- ids we are about to mint are already taken (idempotent / collision-safe).
SET @already_invoiced := (SELECT COUNT(DISTINCT INVOICE_ID) FROM ORDER_ITEM_BILLING WHERE ORDER_ID='PO11055');
SET @inv_taken := (SELECT COUNT(*) FROM INVOICE WHERE INVOICE_ID=@inv);
SET @atx_taken := (SELECT COUNT(*) FROM ACCTG_TRANS WHERE ACCTG_TRANS_ID=@atx);
SET @go := IF(@already_invoiced=0 AND @inv_taken=0 AND @atx_taken=0, 1, 0);
SELECT @go AS proceed, @already_invoiced already_invoiced, @inv_taken inv_id_taken, @atx_taken atx_id_taken;

-- ---------------------------------------------------------------------------
-- STEP 1 -- INVOICE header
-- ---------------------------------------------------------------------------
INSERT INTO INVOICE (INVOICE_ID, INVOICE_TYPE_ID, PARTY_ID_FROM, PARTY_ID, STATUS_ID,
                     INVOICE_DATE, CURRENCY_UOM_ID, LAST_UPDATED_STAMP, CREATED_STAMP)
SELECT @inv, 'PURCHASE_INVOICE', @sup, @org, 'INVOICE_READY', @dt, 'EGP', @now, @now
WHERE @go=1;

-- ---------------------------------------------------------------------------
-- STEP 2 -- INVOICE_ITEM (one per order line; AMOUNT = unit price, qty = 1)
-- ---------------------------------------------------------------------------
INSERT INTO INVOICE_ITEM (INVOICE_ID, INVOICE_ITEM_SEQ_ID, INVOICE_ITEM_TYPE_ID,
                          PRODUCT_ID, QUANTITY, AMOUNT, DESCRIPTION,
                          LAST_UPDATED_STAMP, CREATED_STAMP)
SELECT @inv, oi.ORDER_ITEM_SEQ_ID, 'PINV_CERTIFICATE_SUPPLY_ITEM',
       oi.PRODUCT_ID, oi.QUANTITY, oi.UNIT_PRICE, oi.ITEM_DESCRIPTION, @now, @now
FROM ORDER_ITEM oi WHERE oi.ORDER_ID='PO11055' AND @go=1;

-- ---------------------------------------------------------------------------
-- STEP 3 -- ORDER_ITEM_BILLING (line + its receipt -> invoice item)
-- ---------------------------------------------------------------------------
INSERT INTO ORDER_ITEM_BILLING (ORDER_ID, ORDER_ITEM_SEQ_ID, INVOICE_ID, INVOICE_ITEM_SEQ_ID,
                                SHIPMENT_RECEIPT_ID, QUANTITY, AMOUNT,
                                LAST_UPDATED_STAMP, CREATED_STAMP)
SELECT 'PO11055', oi.ORDER_ITEM_SEQ_ID, @inv, oi.ORDER_ITEM_SEQ_ID,
       (SELECT sr.RECEIPT_ID FROM SHIPMENT_RECEIPT sr
         WHERE sr.ORDER_ID='PO11055' AND sr.ORDER_ITEM_SEQ_ID=oi.ORDER_ITEM_SEQ_ID LIMIT 1),
       oi.QUANTITY, oi.UNIT_PRICE, @now, @now
FROM ORDER_ITEM oi WHERE oi.ORDER_ID='PO11055' AND @go=1;

-- ---------------------------------------------------------------------------
-- STEP 4 -- INVOICE_STATUS history (IN_PROCESS -> READY)
-- ---------------------------------------------------------------------------
INSERT INTO INVOICE_STATUS (STATUS_ID, INVOICE_ID, STATUS_DATE, LAST_UPDATED_STAMP, CREATED_STAMP)
SELECT 'INVOICE_IN_PROCESS', @inv, @now, @now, @now WHERE @go=1
UNION ALL
SELECT 'INVOICE_READY', @inv, @now, @now, @now WHERE @go=1;

-- ---------------------------------------------------------------------------
-- STEP 5 -- ACCTG_TRANS header (PURCHASE_INVOICE, posted)
-- ---------------------------------------------------------------------------
INSERT INTO ACCTG_TRANS (ACCTG_TRANS_ID, ACCTG_TRANS_TYPE_ID, TRANSACTION_DATE, IS_POSTED,
                         POSTED_DATE, GL_FISCAL_TYPE_ID, PARTY_ID, ROLE_TYPE_ID, INVOICE_ID,
                         WORK_EFFORT_ID, CREATED_DATE, LAST_MODIFIED_DATE,
                         LAST_UPDATED_STAMP, CREATED_STAMP)
SELECT @atx, 'PURCHASE_INVOICE', @dt, 'Y', @now, 'ACTUAL', @sup, 'BILL_FROM_VENDOR', @inv,
       '17722', @now, @now, @now, @now
WHERE @go=1;

-- ---------------------------------------------------------------------------
-- STEP 6 -- ACCTG_TRANS_ENTRY: one 214000 debit per line, one AP credit total
-- ---------------------------------------------------------------------------
-- Debits: clear UNINVOICED_SHIP_RCPT, line by line. Entry seq = order item seq.
INSERT INTO ACCTG_TRANS_ENTRY (ACCTG_TRANS_ID, ACCTG_TRANS_ENTRY_SEQ_ID, ACCTG_TRANS_ENTRY_TYPE_ID,
                               DESCRIPTION, PARTY_ID, ROLE_TYPE_ID, PRODUCT_ID,
                               GL_ACCOUNT_TYPE_ID, GL_ACCOUNT_ID, ORGANIZATION_PARTY_ID,
                               AMOUNT, CURRENCY_UOM_ID, ORIG_AMOUNT, ORIG_CURRENCY_UOM_ID,
                               DEBIT_CREDIT_FLAG, RECONCILE_STATUS_ID)
SELECT @atx, oi.ORDER_ITEM_SEQ_ID, '_NA_', oi.ITEM_DESCRIPTION, @sup, 'BILL_FROM_VENDOR',
       oi.PRODUCT_ID, 'UNINVOICED_SHIP_RCPT', '214000', @org,
       ROUND(oi.QUANTITY*oi.UNIT_PRICE,2), 'EGP', ROUND(oi.QUANTITY*oi.UNIT_PRICE,2), 'EGP',
       'D', 'AES_NOT_RECONCILED'
FROM ORDER_ITEM oi WHERE oi.ORDER_ID='PO11055' AND @go=1;

-- Credit: real AP for supplier 182, grand total. Seq '99' to sit after the lines.
INSERT INTO ACCTG_TRANS_ENTRY (ACCTG_TRANS_ID, ACCTG_TRANS_ENTRY_SEQ_ID, ACCTG_TRANS_ENTRY_TYPE_ID,
                               DESCRIPTION, PARTY_ID, ROLE_TYPE_ID,
                               GL_ACCOUNT_TYPE_ID, GL_ACCOUNT_ID, ORGANIZATION_PARTY_ID,
                               AMOUNT, CURRENCY_UOM_ID, ORIG_AMOUNT, ORIG_CURRENCY_UOM_ID,
                               DEBIT_CREDIT_FLAG, RECONCILE_STATUS_ID)
SELECT @atx, '99', '_NA_',
       'طلب شراء: PO11055 - 1918 فاتورة توريدات كهربا رقم ', @sup, 'BILL_FROM_VENDOR',
       'ACCOUNTS_PAYABLE', '210100', @org,
       (SELECT ROUND(SUM(oi.QUANTITY*oi.UNIT_PRICE),2) FROM ORDER_ITEM oi WHERE oi.ORDER_ID='PO11055'),
       'EGP',
       (SELECT ROUND(SUM(oi.QUANTITY*oi.UNIT_PRICE),2) FROM ORDER_ITEM oi WHERE oi.ORDER_ID='PO11055'),
       'EGP', 'C', 'AES_NOT_RECONCILED'
WHERE @go=1;

-- ---------------------------------------------------------------------------
-- STEP 7 -- advance the id sequences so the app never reissues these ids.
-- ---------------------------------------------------------------------------
UPDATE PARTY_ACCTG_PREFERENCE
   SET LAST_INVOICE_NUMBER = GREATEST(LAST_INVOICE_NUMBER, 1359)
 WHERE PARTY_ID='Company' AND @go=1;

UPDATE SEQUENCE_VALUE_ITEM
   SET SEQ_ID = GREATEST(SEQ_ID, 19372)
 WHERE SEQ_NAME='AcctgTrans' AND @go=1;

-- ---------------------------------------------------------------------------
-- AFTER: checks 1-2 must return ZERO rows; balances must both be 0.
-- ---------------------------------------------------------------------------

-- 1. The new PURCHASE_INVOICE transaction must balance.
SELECT 'UNBALANCED' flag, ACCTG_TRANS_ID,
       SUM(CASE WHEN DEBIT_CREDIT_FLAG='D' THEN AMOUNT ELSE -AMOUNT END) diff
FROM ACCTG_TRANS_ENTRY WHERE ACCTG_TRANS_ID=@atx
GROUP BY ACCTG_TRANS_ID HAVING ABS(diff)>0.001;

-- 2. Invoice line total must equal the order total (59,000).
SELECT 'INVOICE TOTAL MISMATCH' flag,
       (SELECT ROUND(SUM(QUANTITY*AMOUNT),2) FROM INVOICE_ITEM WHERE INVOICE_ID=@inv) invoice_total,
       (SELECT ROUND(SUM(QUANTITY*UNIT_PRICE),2) FROM ORDER_ITEM WHERE ORDER_ID='PO11055') order_total
HAVING invoice_total <> order_total;

-- 3. Per-WE balances -- expect 214000 = 0.00 and AP 210100 = 0.00.
SELECT 'AFTER' phase, ate.GL_ACCOUNT_ID acct,
       ROUND(SUM(CASE WHEN ate.DEBIT_CREDIT_FLAG='D' THEN ate.AMOUNT ELSE -ate.AMOUNT END),2) we_balance
FROM ACCTG_TRANS act JOIN ACCTG_TRANS_ENTRY ate ON ate.ACCTG_TRANS_ID=act.ACCTG_TRANS_ID
WHERE act.WORK_EFFORT_ID='17722' AND ate.GL_ACCOUNT_ID IN ('214000','210100')
GROUP BY ate.GL_ACCOUNT_ID;

-- 4. Full invoice, for the record.
SELECT 'AFTER' phase, i.INVOICE_ID, i.STATUS_ID, i.PARTY_ID_FROM supplier,
       (SELECT COUNT(*) FROM INVOICE_ITEM WHERE INVOICE_ID=i.INVOICE_ID) items,
       (SELECT ROUND(SUM(QUANTITY*AMOUNT),2) FROM INVOICE_ITEM WHERE INVOICE_ID=i.INVOICE_ID) total
FROM INVOICE i WHERE i.INVOICE_ID=@inv;

-- ============================================================================
-- Review the output above, then run ONE of:
--   COMMIT;
--   ROLLBACK;
-- ============================================================================
