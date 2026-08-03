-- ============================================================================
-- Removes two orphaned duplicate purchase invoices for workmanship certificate
-- 233-0026 (WE 17718, order PO11052), discovered 2026-08-03.
--
-- WHAT THESE ARE
-- INV1362 (created 2026-08-02 14:06:24) and INV1363 (created 14:06:25) are an
-- EARLIER duplicate pair for the same certificate than the one already fixed
-- (INV1364/INV1365, see fix_duplicate_certificate_invoices_2026_08_02.sql).
-- They were missed by that audit because they have NO ORDER_ITEM_BILLING row
-- at all -- that audit only found duplicates by grouping ORDER_ITEM_BILLING,
-- so an invoice with zero billing rows is invisible to it. Both still carry a
-- full INVOICE header, one INVOICE_ITEM (PINV_CERTIFICATE_ITEM, 7,000, party
-- 233), and 4 INVOICE_ROLE rows -- i.e. fully-formed but unlinked invoices.
--
-- LIKELY ROOT CAUSE (not fully provable from data alone -- would need app
-- logs to confirm): ResetProjectCertificate.cs is the only code path in the
-- app that deletes ORDER_ITEM_BILLING rows (WORKMANSHIP_CONTRACTING_CERTIFICATE
-- branch, lines 259-273). It queries the order's current billing rows, deletes
-- them, then calls CleanupInvoices() on the invoice ids it just found --
-- which is supposed to cascade-delete the invoice header/items/roles/status
-- too. For INV1362/INV1363 the billing removal clearly happened (they have
-- none now) but the invoice cleanup did not fully take effect. No FK-blocking
-- rows were found (INVOICE_ITEM_ASSOC, SHIPMENT_ITEM_BILLING,
-- WORK_EFFORT_BILLING, TIME_ENTRY, RETURN_ITEM_BILLING all checked, all zero
-- for these ids), so the exact failure mode is unconfirmed.
--
-- VERIFIED (2026-08-03): both invoices are still INVOICE_IN_PROCESS with zero
-- ACCTG_TRANS, zero PAYMENT_APPLICATION, and zero ORDER_ITEM_BILLING rows --
-- pure orphans, safe to delete outright (unlike the earlier pair, there is no
-- billing row to unlink first). A full audit of all 411 certificates for this
-- same "PINV_CERTIFICATE% item with zero ORDER_ITEM_BILLING rows" pattern
-- found no other occurrences -- isolated to this one certificate.
--
-- REVIEW BEFORE RUNNING. Take a backup first:
--   mysqldump erp_contracts INVOICE INVOICE_ITEM INVOICE_STATUS INVOICE_ROLE \
--     ORDER_ITEM_BILLING > backup_before_orphan_invoices_233_0026.sql
-- ============================================================================

DROP TEMPORARY TABLE IF EXISTS tmp_orphan_invoices;
CREATE TEMPORARY TABLE tmp_orphan_invoices (
  orphan_inv VARCHAR(36) PRIMARY KEY,
  go TINYINT DEFAULT 0
);
INSERT INTO tmp_orphan_invoices (orphan_inv) VALUES ('INV1362'), ('INV1363');

START TRANSACTION;

-- ---------------------------------------------------------------------------
-- BEFORE: confirm both are still unposted/unpaid/unlinked. If any MUST_BE_ZERO
-- column is non-zero, STOP -- something has happened to this invoice since
-- 2026-08-03 and it needs manual review before deleting.
-- ---------------------------------------------------------------------------
SELECT 'BEFORE' phase, t.orphan_inv,
       i.STATUS_ID status,
       (SELECT COUNT(*) FROM ORDER_ITEM_BILLING WHERE INVOICE_ID=t.orphan_inv) billing_rows_MUST_BE_ZERO,
       (SELECT COUNT(*) FROM ACCTG_TRANS WHERE INVOICE_ID=t.orphan_inv) acctg_trans_MUST_BE_ZERO,
       (SELECT COUNT(*) FROM PAYMENT_APPLICATION WHERE INVOICE_ID=t.orphan_inv) payment_app_MUST_BE_ZERO
FROM tmp_orphan_invoices t
JOIN INVOICE i ON i.INVOICE_ID = t.orphan_inv;

UPDATE tmp_orphan_invoices t
JOIN INVOICE i ON i.INVOICE_ID = t.orphan_inv
SET t.go = CASE
    WHEN i.STATUS_ID = 'INVOICE_IN_PROCESS'
     AND (SELECT COUNT(*) FROM ORDER_ITEM_BILLING WHERE INVOICE_ID=t.orphan_inv) = 0
     AND (SELECT COUNT(*) FROM ACCTG_TRANS WHERE INVOICE_ID=t.orphan_inv) = 0
     AND (SELECT COUNT(*) FROM PAYMENT_APPLICATION WHERE INVOICE_ID=t.orphan_inv) = 0
    THEN 1 ELSE 0
END;
SELECT * FROM tmp_orphan_invoices;

-- ---------------------------------------------------------------------------
-- DELETE: full removal -- header, items, roles, status. No ORDER_ITEM_BILLING
-- to unlink (already gone).
-- ---------------------------------------------------------------------------
DELETE ii FROM INVOICE_ITEM ii     JOIN tmp_orphan_invoices t ON t.orphan_inv = ii.INVOICE_ID  AND t.go = 1;
DELETE ir FROM INVOICE_ROLE ir     JOIN tmp_orphan_invoices t ON t.orphan_inv = ir.INVOICE_ID  AND t.go = 1;
DELETE ist FROM INVOICE_STATUS ist JOIN tmp_orphan_invoices t ON t.orphan_inv = ist.INVOICE_ID AND t.go = 1;
DELETE i FROM INVOICE i            JOIN tmp_orphan_invoices t ON t.orphan_inv = i.INVOICE_ID   AND t.go = 1;

-- ---------------------------------------------------------------------------
-- AFTER: both must be gone.
-- ---------------------------------------------------------------------------
SELECT 'AFTER' phase, t.orphan_inv,
       (SELECT COUNT(*) FROM INVOICE WHERE INVOICE_ID=t.orphan_inv) still_exists_MUST_BE_ZERO
FROM tmp_orphan_invoices t;

DROP TEMPORARY TABLE tmp_orphan_invoices;

-- ============================================================================
-- Review the output above, then run ONE of:
--   COMMIT;
--   ROLLBACK;
-- ============================================================================
