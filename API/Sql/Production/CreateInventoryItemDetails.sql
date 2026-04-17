-- Drop if it already exists (safe to run multiple times)
DROP VIEW IF EXISTS InventoryItemsDetails;

-- Power BI-friendly view with exact same logic as your C# handler
CREATE OR REPLACE VIEW InventoryItemsDetails AS
SELECT
    invi.PRODUCT_ID                  AS ProductId,
    prd.PRODUCT_NAME                 AS ProductName,
    invi.QUANTITY_ON_HAND_TOTAL      AS QuantityOnHandTotal,
    invi.AVAILABLE_TO_PROMISE_TOTAL  AS AvailableToPromiseTotal,
    invi.INVENTORY_ITEM_ID           AS InventoryItemId,
    invi.FACILITY_ID                 AS FacilityId,
    fac.FACILITY_NAME                AS FacilityName,
    invd.INVENTORY_ITEM_DETAIL_SEQ_ID AS InventoryItemDetailSeqId,
    invd.EFFECTIVE_DATE              AS EffectiveDate,
    invd.QUANTITY_ON_HAND_DIFF       AS QuantityOnHandDiff,
    invd.AVAILABLE_TO_PROMISE_DIFF   AS AvailableToPromiseDiff,
    invd.ACCOUNTING_QUANTITY_DIFF    AS AccountingQuantityDiff,
    invd.ORDER_ID                    AS OrderId,
    invd.WORK_EFFORT_ID              AS WorkEffortId,
    we.CERTIFICATE_NUMBER            AS CertificateNumber
FROM INVENTORY_ITEM invi
         JOIN INVENTORY_ITEM_DETAIL invd
              ON invi.INVENTORY_ITEM_ID = invd.INVENTORY_ITEM_ID
         JOIN PRODUCT prd
              ON invi.PRODUCT_ID = prd.PRODUCT_ID
         JOIN FACILITY fac
              ON invi.FACILITY_ID = fac.FACILITY_ID
         LEFT JOIN WORK_EFFORT we
                   ON invd.WORK_EFFORT_ID = we.WORK_EFFORT_ID
WHERE
   -- Case 1: Accounting diff is meaningfully different from zero
        ABS(IFNULL(invd.ACCOUNTING_QUANTITY_DIFF, 0)) > 0.000001

   -- Case 2: Exclude rows where both QOH and ATP diffs are (near) zero
   OR NOT (
            ABS(IFNULL(invd.QUANTITY_ON_HAND_DIFF, 0)) <= 0.000001
        AND ABS(IFNULL(invd.AVAILABLE_TO_PROMISE_DIFF, 0)) <= 0.000001
    )

   -- Case 3: "Starting records" – all three diffs are identical and non-zero
   OR (
            ABS(IFNULL(invd.QUANTITY_ON_HAND_DIFF, 0) - IFNULL(invd.AVAILABLE_TO_PROMISE_DIFF, 0)) <= 0.000001
        AND ABS(IFNULL(invd.QUANTITY_ON_HAND_DIFF, 0) - IFNULL(invd.ACCOUNTING_QUANTITY_DIFF, 0)) <= 0.000001
        AND ABS(IFNULL(invd.QUANTITY_ON_HAND_DIFF, 0)) > 0.000001
    );