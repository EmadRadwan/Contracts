-- =============================================================================
-- Delete leftover garment-manufacturing DEMO data (Apache OFBiz seed data),
-- never used by this business -- including the full manufacturing routing/BOM
-- tree built on top of it.
--
-- Identified by the "MFGDEMO-" product id prefix -- OFBiz's stock tracksuit/
-- garment manufacturing demo:
--   MFGDEMO-1    ميلتون (melton fabric)              RAW_MATERIAL
--   MFGDEMO-11   ترنج اولادى محير كابيشو (tracksuit)  FINISHED_GOOD
--   MFGDEMO-121  كارت (card)                          RAW_MATERIAL
--   MFGDEMO-199  كردون حبل (drawstring cord)          RAW_MATERIAL
--   MFGDEMO-215  كيس للسيرى (bag)                     RAW_MATERIAL
--   MFGDEMO-234  كيس للقطعة (bag)                     RAW_MATERIAL
--   MFGDEMO-898  استك (elastic)                       RAW_MATERIAL
--   MFGDEMO-980  تيكت (ticket/label)                  RAW_MATERIAL
--
-- FULL DEPENDENCY SCAN (see chat -- this superseded an earlier, buggy scan
-- whose shell loop silently skipped multi-column tables; every table below
-- was re-verified with a fixed scan across ALL PRODUCT_ID-, INVENTORY_ITEM_ID-
-- and WORK_EFFORT_ID-like columns, not just the single-column ones):
--
--   PRODUCT_ASSOC            7 rows  BOM: MFGDEMO-11 (PRODUCT_ID) -> each raw
--                                    material (PRODUCT_ID_TO), type MANUF_COMPONENT
--   WORK_EFFORT               8 rows  routing tree: MFGDEMO-10021 (ROUTING,
--                                    "انتاج ترنج اولادى محير كابيشو") + 7 ROU_TASK
--                                    stages MFGDEMO-10022..10028 (فرز/قص/طباعة/
--                                    خياطة/تشطيب/مكواه/تعبئة)
--   WORK_EFFORT_ASSOC         7 rows  MFGDEMO-10021 -> each of the 7 tasks,
--                                    type ROUTING_COMPONENT (both WORK_EFFORT_ID_FROM/TO)
--   WORK_EFFORT_COST_CALC    14 rows  2 cost-calc links per task (FOH_GENERAL +
--                                    LABOR_COST); only the LINK rows are demo-
--                                    specific -- COST_COMPONENT_CALC methods
--                                    themselves (FOH_GENERAL_HOUR etc.) are
--                                    shared definitions and are NOT touched
--   WORK_EFFORT_GOOD_STANDARD 1 row   links MFGDEMO-10021 (routing) to
--                                    MFGDEMO-11 (product), type ROU_PROD_TEMPLATE
--   INVENTORY_ITEM            7 rows  one per RAW_MATERIAL demo product
--                                    (MFGDEMO-INV-1..7, facility MFGDEMO-FACILITY,
--                                    500 qty each); MFGDEMO-11 has none
--   INVENTORY_ITEM_DETAIL     7 rows  one per inventory item above
--   PRODUCT                   8 rows  the demo products themselves
--   PRODUCT_CATEGORY          3 rows  ACCESSORIES / MILTON_PLAIN /
--                                    BOYS_MEDIUM_TRACKSUIT -- reconfirmed unused
--                                    by any other product and absent from
--                                    PRODUCT_CATEGORY_MEMBER / _ROLLUP
--
-- Also checked and confirmed EMPTY for all 8 product ids / 7 inventory item ids /
-- 8 work effort ids: WORK_EFFORT_ASSOC_ATTRIBUTE, WORK_EFFORT_ATTRIBUTE,
-- WORK_EFFORT_PARTY_ASSIGNMENT, WORK_EFFORT_NOTE, WORK_EFFORT_FIXED_ASSET_STD,
-- WORK_EFFORT_INVENTORY_RES/ASSIGN/PRODUCED, PRODUCT_MANUFACTURING_RULE,
-- MRP_EVENT, COST_COMPONENT, PRODUCT_COST_COMPONENT_CALC, ORDER_ITEM,
-- INVOICE_ITEM, ACCTG_TRANS_ENTRY, SALES_REQUEST, and every other table with a
-- PRODUCT_ID/INVENTORY_ITEM_ID/WORK_EFFORT_ID-shaped column in the schema.
-- (dimproducts / inventoryitemsdetails / acctgtransactions / acctgtransentries
-- are Power BI reporting VIEWS, not base tables -- they auto-reflect once the
-- underlying tables are cleaned, no action needed.)
--
-- Delete order respects every FK above (children before parents):
--   WORK_EFFORT_COST_CALC -> WORK_EFFORT_ASSOC -> WORK_EFFORT_GOOD_STANDARD ->
--   WORK_EFFORT -> PRODUCT_ASSOC -> INVENTORY_ITEM_DETAIL -> INVENTORY_ITEM ->
--   PRODUCT -> PRODUCT_CATEGORY
--
-- SAFETY: runs in one transaction. Review the BEFORE / AFTER output.
--   Dry run -> change the final COMMIT to ROLLBACK.
-- =============================================================================

START TRANSACTION;

-- ---- BEFORE: what will be deleted ------------------------------------------
SELECT 'BEFORE: demo products' AS step,
       PRODUCT_ID, PRODUCT_TYPE_ID, PRODUCT_NAME, PRIMARY_PRODUCT_CATEGORY_ID
FROM PRODUCT
WHERE PRODUCT_ID LIKE 'MFGDEMO-%';

SELECT 'BEFORE: demo BOM (PRODUCT_ASSOC)' AS step,
       PRODUCT_ID, PRODUCT_ID_TO, PRODUCT_ASSOC_TYPE_ID, QUANTITY
FROM PRODUCT_ASSOC
WHERE PRODUCT_ID LIKE 'MFGDEMO-%' OR PRODUCT_ID_TO LIKE 'MFGDEMO-%';

SELECT 'BEFORE: demo routing work efforts' AS step,
       WORK_EFFORT_ID, WORK_EFFORT_TYPE_ID, WORK_EFFORT_NAME, CURRENT_STATUS_ID
FROM WORK_EFFORT
WHERE WORK_EFFORT_ID LIKE 'MFGDEMO-1002%';

SELECT 'BEFORE: demo routing assoc' AS step,
       WORK_EFFORT_ID_FROM, WORK_EFFORT_ID_TO, WORK_EFFORT_ASSOC_TYPE_ID, SEQUENCE_NUM
FROM WORK_EFFORT_ASSOC
WHERE WORK_EFFORT_ID_FROM LIKE 'MFGDEMO-1002%' OR WORK_EFFORT_ID_TO LIKE 'MFGDEMO-1002%';

SELECT 'BEFORE: demo routing cost calc links' AS step,
       WORK_EFFORT_ID, COST_COMPONENT_TYPE_ID, COST_COMPONENT_CALC_ID
FROM WORK_EFFORT_COST_CALC
WHERE WORK_EFFORT_ID LIKE 'MFGDEMO-1002%';

SELECT 'BEFORE: demo routing<->product template link' AS step,
       WORK_EFFORT_ID, PRODUCT_ID, WORK_EFFORT_GOOD_STD_TYPE_ID
FROM WORK_EFFORT_GOOD_STANDARD
WHERE WORK_EFFORT_ID LIKE 'MFGDEMO-1002%' OR PRODUCT_ID LIKE 'MFGDEMO-%';

SELECT 'BEFORE: demo inventory items' AS step,
       INVENTORY_ITEM_ID, PRODUCT_ID, FACILITY_ID, QUANTITY_ON_HAND_TOTAL
FROM INVENTORY_ITEM
WHERE PRODUCT_ID LIKE 'MFGDEMO-%';

-- ---- 1) Cost-calc links on the routing tasks --------------------------------
DELETE FROM WORK_EFFORT_COST_CALC
WHERE WORK_EFFORT_ID LIKE 'MFGDEMO-1002%';

-- ---- 2) Routing -> task associations ---------------------------------------
DELETE FROM WORK_EFFORT_ASSOC
WHERE WORK_EFFORT_ID_FROM LIKE 'MFGDEMO-1002%' OR WORK_EFFORT_ID_TO LIKE 'MFGDEMO-1002%';

-- ---- 3) Routing <-> product template link -----------------------------------
DELETE FROM WORK_EFFORT_GOOD_STANDARD
WHERE WORK_EFFORT_ID LIKE 'MFGDEMO-1002%' OR PRODUCT_ID LIKE 'MFGDEMO-%';

-- ---- 4) The routing + its 7 task work efforts -------------------------------
DELETE FROM WORK_EFFORT
WHERE WORK_EFFORT_ID LIKE 'MFGDEMO-1002%';

-- ---- 5) BOM associations (both directions) ----------------------------------
DELETE FROM PRODUCT_ASSOC
WHERE PRODUCT_ID LIKE 'MFGDEMO-%' OR PRODUCT_ID_TO LIKE 'MFGDEMO-%';

-- ---- 6) Inventory item detail (child of INVENTORY_ITEM) ---------------------
DELETE d
FROM INVENTORY_ITEM_DETAIL d
JOIN INVENTORY_ITEM i ON i.INVENTORY_ITEM_ID = d.INVENTORY_ITEM_ID
WHERE i.PRODUCT_ID LIKE 'MFGDEMO-%';

-- ---- 7) Inventory items ------------------------------------------------------
DELETE FROM INVENTORY_ITEM
WHERE PRODUCT_ID LIKE 'MFGDEMO-%';

-- ---- 8) The 8 demo products --------------------------------------------------
DELETE FROM PRODUCT
WHERE PRODUCT_ID LIKE 'MFGDEMO-%';

-- ---- 9) Their now-unused demo categories -------------------------------------
DELETE FROM PRODUCT_CATEGORY
WHERE PRODUCT_CATEGORY_ID IN ('ACCESSORIES', 'MILTON_PLAIN', 'BOYS_MEDIUM_TRACKSUIT');

-- ---- AFTER: confirm ----------------------------------------------------------
SELECT 'AFTER: remaining MFGDEMO products (expect 0)' AS step, COUNT(*) AS n
FROM PRODUCT WHERE PRODUCT_ID LIKE 'MFGDEMO-%';

SELECT 'AFTER: remaining demo routing work efforts (expect 0)' AS step, COUNT(*) AS n
FROM WORK_EFFORT WHERE WORK_EFFORT_ID LIKE 'MFGDEMO-1002%';

SELECT 'AFTER: remaining demo BOM rows (expect 0)' AS step, COUNT(*) AS n
FROM PRODUCT_ASSOC WHERE PRODUCT_ID LIKE 'MFGDEMO-%' OR PRODUCT_ID_TO LIKE 'MFGDEMO-%';

SELECT 'AFTER: remaining demo inventory items (expect 0)' AS step, COUNT(*) AS n
FROM INVENTORY_ITEM WHERE PRODUCT_ID LIKE 'MFGDEMO-%';

SELECT 'AFTER: remaining demo categories (expect 0)' AS step, COUNT(*) AS n
FROM PRODUCT_CATEGORY
WHERE PRODUCT_CATEGORY_ID IN ('ACCESSORIES', 'MILTON_PLAIN', 'BOYS_MEDIUM_TRACKSUIT');

-- Review all output above. If correct:
COMMIT;
-- If anything looks off instead:
-- ROLLBACK;
