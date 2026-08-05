-- ============================================================================
-- Completes an incomplete Reset for certificate 110-0008 (WE 17904, order
-- PO11078), discovered 2026-08-05.
--
-- ROOT CAUSE (code already fixed in ResetProjectCertificate.cs): for
-- SUPPLY_PROCUREMENT_CERTIFICATE, Reset only cleared ORDER_ITEM_BILLING rows
-- tied to a ShipmentReceipt (via SHIPMENT_RECEIPT_ID). PO11078's 8 items were
-- billed into INV1379 through a path with no ShipmentReceipt row, so that
-- cleanup loop found nothing and never ran. Reset correctly reverted
-- ORDER_HEADER.STATUS_ID to ORDER_CREATED and WORK_EFFORT.CURRENT_STATUS_ID
-- to WEPR_CREATED, but left PO11078 fully billed and INV1379 fully intact --
-- the certificate looked reset while its order/invoice weren't. This then
-- blocked the next Update's "delete old PO" cascade with an
-- ORDER_ADJUSTMENT_BILLING FK error (a -0.31 CERTIFICATE_DISCOUNT_ADJUSTMENT
-- billed into INV1379 item 00009, which nothing had cleared).
--
-- VERIFIED (2026-08-05): INV1379 is INVOICE_IN_PROCESS, zero ACCTG_TRANS,
-- zero PAYMENT_APPLICATION -- not posted or paid, safe to remove outright.
-- 8 ORDER_ITEM_BILLING rows, 1 ORDER_ADJUSTMENT_BILLING row, 9 INVOICE_ITEM,
-- 4 INVOICE_ROLE, 1 INVOICE_STATUS.
--
-- FIX: unlink and remove INV1379 entirely (mirrors what CleanupInvoices does
-- in code), matching what Reset should have done. Does NOT touch ORDER_ITEM,
-- ORDER_ADJUSTMENT, or ORDER_HEADER -- those are correctly left as-is by the
-- original reset (order structure stays, ready to be re-billed on the next
-- successful Update/approval).
--
-- REVIEW BEFORE RUNNING. Take a backup first:
--   mysqldump erp_contracts INVOICE INVOICE_ITEM INVOICE_STATUS INVOICE_ROLE \
--     ORDER_ITEM_BILLING ORDER_ADJUSTMENT_BILLING > backup_before_110_0008_reset_fix.sql
-- ============================================================================

START TRANSACTION;

SELECT 'BEFORE' phase,
       (SELECT STATUS_ID FROM INVOICE WHERE INVOICE_ID='INV1379') invoice_status,
       (SELECT COUNT(*) FROM ACCTG_TRANS WHERE INVOICE_ID='INV1379') acctg_trans_MUST_BE_ZERO,
       (SELECT COUNT(*) FROM PAYMENT_APPLICATION WHERE INVOICE_ID='INV1379') payment_app_MUST_BE_ZERO,
       (SELECT COUNT(*) FROM ORDER_ITEM_BILLING WHERE ORDER_ID='PO11078') order_item_billing_count;

DELETE FROM ORDER_ADJUSTMENT_BILLING WHERE INVOICE_ID='INV1379';
DELETE FROM ORDER_ITEM_BILLING WHERE ORDER_ID='PO11078' AND INVOICE_ID='INV1379';
-- Item 00009 (discount) has PARENT_INVOICE_ITEM_SEQ_ID=00008 -- self-referencing FK
-- within the invoice, so the child must go before the single-statement bulk delete below.
DELETE FROM INVOICE_ITEM WHERE INVOICE_ID='INV1379' AND INVOICE_ITEM_SEQ_ID='00009';
DELETE FROM INVOICE_ITEM WHERE INVOICE_ID='INV1379';
DELETE FROM INVOICE_ROLE WHERE INVOICE_ID='INV1379';
DELETE FROM INVOICE_STATUS WHERE INVOICE_ID='INV1379';
DELETE FROM INVOICE WHERE INVOICE_ID='INV1379';

SELECT 'AFTER' phase,
       (SELECT COUNT(*) FROM INVOICE WHERE INVOICE_ID='INV1379') invoice_still_exists_MUST_BE_ZERO,
       (SELECT COUNT(*) FROM ORDER_ITEM_BILLING WHERE ORDER_ID='PO11078') order_item_billing_MUST_BE_ZERO,
       (SELECT COUNT(*) FROM ORDER_ITEM WHERE ORDER_ID='PO11078') order_items_still_present_MUST_BE_8,
       (SELECT COUNT(*) FROM ORDER_ADJUSTMENT WHERE ORDER_ID='PO11078') order_adjustment_still_present_MUST_BE_1;

-- ============================================================================
-- Review the output above, then run ONE of:
COMMIT;
--   ROLLBACK;
-- ============================================================================
