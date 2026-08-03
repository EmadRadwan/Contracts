-- ============================================================================
-- Removes duplicate purchase invoices found by a full audit of all project
-- certificates (WORKMANSHIP_CONTRACTING_CERTIFICATE, SUPPLY_PROCUREMENT_
-- CERTIFICATE, COMPANY_SUPPLY_SALE_CERTIFICATE) run 2026-08-02.
--
-- AUDIT METHOD
-- For every PROJECT_CERTIFICATE work effort with a RELATED_ORDER_ID, grouped
-- ORDER_ITEM_BILLING by (ORDER_ID, ORDER_ITEM_SEQ_ID) and flagged any group
-- billed into more than one INVOICE_ID. COMPANY_SUPPLY_SALE_CERTIFICATE (97
-- certificates) never has a RELATED_ORDER_ID -- it issues materials directly
-- and never invoices, so it is not exposed to this bug at all. Also checked
-- every certificate-linked invoice for PAYMENT_APPLICATION total exceeding
-- the invoice total (duplicate/overpayment) -- none found; the only payment
-- oddity (certificate 100-0008 / INV87, PAYMENT 11451 split into two
-- PAYMENT_APPLICATION rows: 11,575 + 40,000) sums to exactly the invoice
-- total, so it is a redundant application row, not a duplicate/overpayment,
-- and is NOT touched by this script.
--
-- RESULT: exactly two duplicate-invoice pairs found, both
-- WORKMANSHIP_CONTRACTING_CERTIFICATE, both created today via the same
-- root cause -- see Application/Accounting/Services/InvoiceHelperService.cs
-- CreateInvoiceFromOrder(), which checked "any ORDER_ITEM_BILLING rows yet?"
-- with no row lock, combined with the frontend Actions menu
-- (ProjectCertificateForm.tsx) not being disabled while the approval
-- mutation was in flight. A double-click fired two concurrent approval
-- requests that both saw "nothing billed yet" and each created a full
-- invoice for the same order item:
--
--   Certificate  Order    Item  Kept     Removed  Amount(each)  Created (kept / removed)
--   57-0008      PO11061  0001  INV1366  INV1367  15,000        2026-08-02 14:19:23 / 14:19:24
--   233-0026     PO11052  0001  INV1364  INV1365   7,000        2026-08-02 14:13:22 / 14:13:23
--
-- Both removed invoices are still INVOICE_IN_PROCESS with zero ACCTG_TRANS
-- and zero PAYMENT_APPLICATION rows (verified below) -- neither was ever
-- approved/posted or paid, so there is no GL or payment entry to unwind.
--
-- Code fix applied alongside this script (already in place, not part of
-- this script):
--   - InvoiceHelperService.CreateInvoiceFromOrder now takes a
--     `SELECT ... FOR UPDATE` lock on the order header before the
--     ORDER_ITEM_BILLING check, closing the race for callers that hold the
--     transaction open (e.g. ProcessWorkmanCertificatePurchaseOrder).
--   - client-app ProjectCertificateForm.tsx: the Actions menu is now also
--     disabled while `isLoading` (covers the whole Approve/Reset/Receive/
--     Issue-Materials flow) is true, closing the double-click window.
--
-- REVIEW BEFORE RUNNING. Take a backup first:
--   mysqldump erp_contracts INVOICE INVOICE_ITEM INVOICE_STATUS INVOICE_ROLE \
--     ORDER_ITEM_BILLING > backup_before_dup_certificate_invoices_2026_08_02.sql
-- ============================================================================

-- NOTE: CREATE/DROP TEMPORARY TABLE do not cause an implicit commit in MySQL,
-- but ALTER TABLE always does (even on a temp table) -- so the `go` guard
-- column is declared up front here rather than added later, to keep the
-- whole guard-check-then-delete sequence below inside one real transaction.
DROP TEMPORARY TABLE IF EXISTS tmp_dup_invoices;
CREATE TEMPORARY TABLE tmp_dup_invoices (
  dup_inv VARCHAR(36) PRIMARY KEY,
  keep_inv VARCHAR(36),
  go TINYINT DEFAULT 0
);
INSERT INTO tmp_dup_invoices (dup_inv, keep_inv) VALUES
  ('INV1367', 'INV1366'),   -- certificate 57-0008 / PO11061
  ('INV1365', 'INV1364');   -- certificate 233-0026 / PO11052

START TRANSACTION;

-- ---------------------------------------------------------------------------
-- BEFORE: for each duplicate, confirm it is still unposted/unpaid.
-- If any dup_status is not INVOICE_IN_PROCESS, or either MUST_BE_ZERO column
-- is non-zero for that row, STOP and investigate that certificate manually
-- before proceeding -- do not run this script for a row that fails the guard.
-- ---------------------------------------------------------------------------
SELECT 'BEFORE' phase, t.dup_inv, t.keep_inv,
       i.STATUS_ID dup_status,
       (SELECT COUNT(*) FROM ACCTG_TRANS WHERE INVOICE_ID=t.dup_inv) dup_acctg_trans_MUST_BE_ZERO,
       (SELECT COUNT(*) FROM PAYMENT_APPLICATION WHERE INVOICE_ID=t.dup_inv) dup_payment_app_MUST_BE_ZERO
FROM tmp_dup_invoices t
JOIN INVOICE i ON i.INVOICE_ID = t.dup_inv;

-- Guard: mark each duplicate go/no-go individually.
UPDATE tmp_dup_invoices t
JOIN INVOICE i ON i.INVOICE_ID = t.dup_inv
SET t.go = CASE
    WHEN i.STATUS_ID = 'INVOICE_IN_PROCESS'
     AND (SELECT COUNT(*) FROM ACCTG_TRANS WHERE INVOICE_ID=t.dup_inv) = 0
     AND (SELECT COUNT(*) FROM PAYMENT_APPLICATION WHERE INVOICE_ID=t.dup_inv) = 0
    THEN 1 ELSE 0
END;
SELECT * FROM tmp_dup_invoices;

-- ---------------------------------------------------------------------------
-- STEP 1 -- unlink each duplicate's billing row so the order item goes back
-- to being billed only by the kept invoice.
-- ---------------------------------------------------------------------------
DELETE oib FROM ORDER_ITEM_BILLING oib
JOIN tmp_dup_invoices t ON t.dup_inv = oib.INVOICE_ID AND t.go = 1;

-- ---------------------------------------------------------------------------
-- STEP 2 -- remove each duplicate invoice's child rows, then its header.
-- ---------------------------------------------------------------------------
DELETE ii FROM INVOICE_ITEM ii   JOIN tmp_dup_invoices t ON t.dup_inv = ii.INVOICE_ID   AND t.go = 1;
DELETE ir FROM INVOICE_ROLE ir   JOIN tmp_dup_invoices t ON t.dup_inv = ir.INVOICE_ID   AND t.go = 1;
DELETE ist FROM INVOICE_STATUS ist JOIN tmp_dup_invoices t ON t.dup_inv = ist.INVOICE_ID AND t.go = 1;
DELETE i FROM INVOICE i          JOIN tmp_dup_invoices t ON t.dup_inv = i.INVOICE_ID    AND t.go = 1;

-- ---------------------------------------------------------------------------
-- AFTER: each duplicate must be gone; each kept invoice must still bill its
-- order item exactly once.
-- ---------------------------------------------------------------------------
SELECT 'AFTER' phase, t.dup_inv,
       (SELECT COUNT(*) FROM INVOICE WHERE INVOICE_ID=t.dup_inv) dup_still_exists_MUST_BE_ZERO,
       t.keep_inv,
       (SELECT COUNT(*) FROM INVOICE_ITEM WHERE INVOICE_ID=t.keep_inv) kept_items,
       (SELECT ROUND(SUM(QUANTITY*AMOUNT),2) FROM INVOICE_ITEM WHERE INVOICE_ID=t.keep_inv) kept_total
FROM tmp_dup_invoices t;

DROP TEMPORARY TABLE tmp_dup_invoices;

-- ============================================================================
-- Review the output above, then run ONE of:
   COMMIT;
--   ROLLBACK;
-- ============================================================================
