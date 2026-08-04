-- ============================================================================
-- Removes a duplicate purchase invoice created on DEV 2026-08-04 while testing
-- certificate 546-0003 (WE 17749, order PO11062) after the order_header
-- casing fix (InvoiceHelperService.cs:1480).
--
-- ROOT CAUSE: ProcessWorkmanCertificatePurchaseOrder.Handler.Handle calls
-- OrderStatusChanges (which can itself invoice via ChangeOrderStatus ->
-- CreateInvoiceFromOrder) and then explicitly calls CreateInvoiceFromOrder
-- again as an "ensure invoice exists" safety net -- but nothing flushed the
-- first invoice to the database in between, so the second call's "already
-- billed?" check queried an empty ORDER_ITEM_BILLING and created a second
-- invoice for the same order item. Both created 2026-08-04 09:08, 11s apart,
-- in the same request/transaction. This was previously masked entirely by
-- the order_header bug (every CreateInvoiceFromOrder call threw before
-- reaching this path), so fixing that bug is what exposed this one.
--
-- CODE FIX applied alongside this script (already in place, not part of this
-- script): ProcessWorkmanCertificatePurchaseOrder.cs now calls
-- _context.SaveChangesAsync() immediately after OrderStatusChanges, before
-- the explicit "ensure invoice" call.
--
-- INV1368 kept (created first, 09:08:30), INV1369 removed (09:08:41). Both
-- verified INVOICE_IN_PROCESS with zero ACCTG_TRANS and zero
-- PAYMENT_APPLICATION -- neither posted or paid, dev-only, no GL/payment
-- entry to unwind.
-- ============================================================================

DROP TEMPORARY TABLE IF EXISTS tmp_dup_invoices;
CREATE TEMPORARY TABLE tmp_dup_invoices (
  dup_inv VARCHAR(36) PRIMARY KEY,
  keep_inv VARCHAR(36),
  go TINYINT DEFAULT 0
);
INSERT INTO tmp_dup_invoices (dup_inv, keep_inv) VALUES
  ('INV1369', 'INV1368');   -- certificate 546-0003 / PO11062

START TRANSACTION;

SELECT 'BEFORE' phase, t.dup_inv, t.keep_inv,
       i.STATUS_ID dup_status,
       (SELECT COUNT(*) FROM ACCTG_TRANS WHERE INVOICE_ID=t.dup_inv) dup_acctg_trans_MUST_BE_ZERO,
       (SELECT COUNT(*) FROM PAYMENT_APPLICATION WHERE INVOICE_ID=t.dup_inv) dup_payment_app_MUST_BE_ZERO
FROM tmp_dup_invoices t
JOIN INVOICE i ON i.INVOICE_ID = t.dup_inv;

UPDATE tmp_dup_invoices t
JOIN INVOICE i ON i.INVOICE_ID = t.dup_inv
SET t.go = CASE
    WHEN i.STATUS_ID = 'INVOICE_IN_PROCESS'
     AND (SELECT COUNT(*) FROM ACCTG_TRANS WHERE INVOICE_ID=t.dup_inv) = 0
     AND (SELECT COUNT(*) FROM PAYMENT_APPLICATION WHERE INVOICE_ID=t.dup_inv) = 0
    THEN 1 ELSE 0
END;
SELECT * FROM tmp_dup_invoices;

DELETE oib FROM ORDER_ITEM_BILLING oib
JOIN tmp_dup_invoices t ON t.dup_inv = oib.INVOICE_ID AND t.go = 1;

DELETE ii FROM INVOICE_ITEM ii   JOIN tmp_dup_invoices t ON t.dup_inv = ii.INVOICE_ID   AND t.go = 1;
DELETE ir FROM INVOICE_ROLE ir   JOIN tmp_dup_invoices t ON t.dup_inv = ir.INVOICE_ID   AND t.go = 1;
DELETE ist FROM INVOICE_STATUS ist JOIN tmp_dup_invoices t ON t.dup_inv = ist.INVOICE_ID AND t.go = 1;
DELETE i FROM INVOICE i          JOIN tmp_dup_invoices t ON t.dup_inv = i.INVOICE_ID    AND t.go = 1;

SELECT 'AFTER' phase, t.dup_inv,
       (SELECT COUNT(*) FROM INVOICE WHERE INVOICE_ID=t.dup_inv) dup_still_exists_MUST_BE_ZERO,
       t.keep_inv,
       (SELECT COUNT(*) FROM INVOICE_ITEM WHERE INVOICE_ID=t.keep_inv) kept_items,
       (SELECT ROUND(SUM(QUANTITY*AMOUNT),2) FROM INVOICE_ITEM WHERE INVOICE_ID=t.keep_inv) kept_total
FROM tmp_dup_invoices t;

DROP TEMPORARY TABLE tmp_dup_invoices;

COMMIT;
