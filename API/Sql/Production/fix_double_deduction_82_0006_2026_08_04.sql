-- ============================================================================
-- Reverses a double-applied deduction for certificate 82-0006 (WE 17712,
-- order PO11071), discovered 2026-08-04 via INV1376.
--
-- ROOT CAUSE (code already fixed in CreateProjectCertificate.cs and
-- UpdateProjectCertificate.cs, WORKMANSHIP_CONTRACTING_CERTIFICATE branch):
-- the certificate item's Deductions value (95,020) was subtracted twice —
-- once correctly inside the `deserved` formula that priced the main
-- PROJECT_CERTIFICATE_ITEM order item (400 * 450 * 63.9% achievement =
-- 115,020 gross, minus 95,020 deduction = 20,000, exactly matching order
-- item 0001's price), and a second time via a redundant separate
-- PROJECT_DEDUCTION order item (0002, also 95,020) that the code
-- additionally created and billed into the same invoice. Net effect: INV1376
-- showed 20,000 - 95,020 = -75,020 instead of the certificate's correct net
-- of 20,000.
--
-- VERIFIED (2026-08-04, against a production data mirror): INV1376 is still
-- INVOICE_IN_PROCESS with zero ACCTG_TRANS and zero PAYMENT_APPLICATION --
-- not posted or paid, safe to correct outright. Order item 0002 is
-- ITEM_COMPLETED with one ORDER_ITEM_BILLING row (to INV1376), 3 ORDER_STATUS
-- history rows, one ORDER_ITEM_SHIP_GROUP_ASSOC row, no ORDER_ADJUSTMENT rows.
-- Already run and verified clean against the mirrored copy of this data
-- (invoice_total_after came out to exactly 20,000.00).
--
-- FIX: remove the redundant deduction order item (0002) and its billing/
-- invoice-item/status/ship-group rows entirely, leaving PO11071 with just its
-- correct order item 0001 and INV1376 with just its correct 20,000 invoice
-- item.
--
-- REVIEW BEFORE RUNNING. Take a backup first:
--   mysqldump erp_contracts INVOICE INVOICE_ITEM INVOICE_STATUS INVOICE_ROLE \
--     ORDER_ITEM ORDER_ITEM_BILLING ORDER_STATUS ORDER_ITEM_SHIP_GROUP_ASSOC \
--     > backup_before_82_0006_dedup.sql
-- ============================================================================

START TRANSACTION;

SELECT 'BEFORE' phase,
       (SELECT STATUS_ID FROM INVOICE WHERE INVOICE_ID='INV1376') invoice_status,
       (SELECT COUNT(*) FROM ACCTG_TRANS WHERE INVOICE_ID='INV1376') acctg_trans_MUST_BE_ZERO,
       (SELECT COUNT(*) FROM PAYMENT_APPLICATION WHERE INVOICE_ID='INV1376') payment_app_MUST_BE_ZERO,
       (SELECT ROUND(SUM(QUANTITY*AMOUNT),2) FROM INVOICE_ITEM WHERE INVOICE_ID='INV1376') invoice_total_before;

-- Remove the redundant deduction's billing link, invoice item, order item
-- status history, ship-group assoc, and the order item itself. Does not
-- touch order item 0001, INVOICE_ITEM 00001, or the WORK_EFFORT certificate
-- item -- those are correct as-is.
DELETE FROM ORDER_ITEM_BILLING WHERE ORDER_ID='PO11071' AND ORDER_ITEM_SEQ_ID='0002' AND INVOICE_ID='INV1376';
DELETE FROM INVOICE_ITEM WHERE INVOICE_ID='INV1376' AND INVOICE_ITEM_SEQ_ID='00002';
DELETE FROM ORDER_STATUS WHERE ORDER_ID='PO11071' AND ORDER_ITEM_SEQ_ID='0002';
DELETE FROM ORDER_ITEM_SHIP_GROUP_ASSOC WHERE ORDER_ID='PO11071' AND ORDER_ITEM_SEQ_ID='0002';
DELETE FROM ORDER_ITEM WHERE ORDER_ID='PO11071' AND ORDER_ITEM_SEQ_ID='0002';

SELECT 'AFTER' phase,
       (SELECT COUNT(*) FROM ORDER_ITEM WHERE ORDER_ID='PO11071' AND ORDER_ITEM_SEQ_ID='0002') item_0002_still_exists_MUST_BE_ZERO,
       (SELECT COUNT(*) FROM INVOICE_ITEM WHERE INVOICE_ID='INV1376') invoice_item_count_MUST_BE_1,
       (SELECT ROUND(SUM(QUANTITY*AMOUNT),2) FROM INVOICE_ITEM WHERE INVOICE_ID='INV1376') invoice_total_after_MUST_BE_20000;

-- ============================================================================
-- Review the output above, then run ONE of:
COMMIT;
--   ROLLBACK;
-- ============================================================================
