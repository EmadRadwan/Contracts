-- ============================================================================
-- Removes duplicated certificate items for certificate 110-0007 (WE 17828,
-- order PO11075), discovered 2026-08-04. Reported symptom: certificate went
-- from 19 items / 79,340 to 40 items / >160,000 after a couple of updates.
--
-- ROOT CAUSE (frontend, already fixed in useProjectCertificate.ts): after a
-- successful create/update, the hook dispatched setProcessedCertificateItems
-- (an upsert-only reducer, correct for single-row grid edits elsewhere in the
-- app) instead of setUiCertificateItemsFromApi (removeAll+setAll, the
-- reducer already used elsewhere for loading the authoritative list from the
-- API). Upsert never prunes stale entries, so newly-added items -- which
-- have no workEffortId locally until the server assigns one -- were never
-- reconciled with their real id after save. Each subsequent save resent
-- those same stale, still-id-less entries as "new" again.
--
-- CONFIRMED IN DATA: WORK_EFFORT items for WE 17828 show 7 originals
-- (created 11:31/11:45) plus the SAME 11 new items created three times,
-- 12:38:18 / 12:40:19 / 12:41:18, each batch with distinct WorkEffortIds.
-- PO11075's order items (0001-0040) mirror this exactly: 0001-0018 are the
-- 18 real, unique items; 0019-0029 and 0030-0040 are two extra full copies
-- of the same 11-item batch. Zero ORDER_ITEM_BILLING, zero payments, zero
-- ACCTG_TRANS against PO11075 -- nothing posted or paid, safe to remove
-- outright.
--
-- FIX: remove order items 0019-0040 (and their status/ship-group rows) and
-- the corresponding duplicate WORK_EFFORT rows (17855-17876), keeping the
-- first-created copy of each of the 11 items (17844-17854 / order items
-- 0008-0018) plus the 7 originals (17829-17835 / order items 0001-0007).
-- Note: this does not by itself reconcile against the "19 items / 79,340"
-- figure the user reported as the pre-duplication baseline -- current data
-- only supports 7 original + 11 unique new = 18 items. If a 19th item is
-- genuinely missing, that's a separate, not-yet-diagnosed issue.
--
-- REVIEW BEFORE RUNNING. Take a backup first:
--   mysqldump erp_contracts WORK_EFFORT ORDER_ITEM ORDER_STATUS \
--     ORDER_ITEM_SHIP_GROUP_ASSOC > backup_before_110_0007_dedup.sql
-- ============================================================================

START TRANSACTION;

SELECT 'BEFORE' phase,
       (SELECT COUNT(*) FROM WORK_EFFORT WHERE WORK_EFFORT_PARENT_ID='17828') certificate_item_count,
       (SELECT COUNT(*) FROM ORDER_ITEM WHERE ORDER_ID='PO11075') order_item_count,
       (SELECT ROUND(SUM(QUANTITY*UNIT_PRICE),2) FROM ORDER_ITEM WHERE ORDER_ID='PO11075') order_total;

DELETE FROM ORDER_STATUS WHERE ORDER_ID='PO11075' AND ORDER_ITEM_SEQ_ID >= '0019';
DELETE FROM ORDER_ITEM_SHIP_GROUP_ASSOC WHERE ORDER_ID='PO11075' AND ORDER_ITEM_SEQ_ID >= '0019';
DELETE FROM ORDER_ITEM WHERE ORDER_ID='PO11075' AND ORDER_ITEM_SEQ_ID >= '0019';
DELETE FROM WORK_EFFORT WHERE WORK_EFFORT_ID BETWEEN '17855' AND '17876';

SELECT 'AFTER' phase,
       (SELECT COUNT(*) FROM WORK_EFFORT WHERE WORK_EFFORT_PARENT_ID='17828') certificate_item_count_MUST_BE_18,
       (SELECT COUNT(*) FROM ORDER_ITEM WHERE ORDER_ID='PO11075') order_item_count_MUST_BE_18,
       (SELECT ROUND(SUM(QUANTITY*UNIT_PRICE),2) FROM ORDER_ITEM WHERE ORDER_ID='PO11075') order_total_after;

-- ============================================================================
-- Review the output above, then run ONE of:
COMMIT;
--   ROLLBACK;
-- ============================================================================
