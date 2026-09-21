-- Retire APARTMENT_RESERVED (Sep 2026).
-- A unit is now APARTMENT_AVAILABLE or APARTMENT_SOLD only. A pending sales request keeps
-- the unit AVAILABLE and holds it through PRODUCT.RESERVED_BY_SALES_REQUEST_ID
-- (see Application/Order/SalesRequests/ApartmentLock.cs).
--
-- Run the checks first; the UPDATE only when every check comes back empty.
-- The STATUS_ITEM row APARTMENT_RESERVED is deliberately kept (FK-safe, history readable).

-- 1. Units that will flip, with their holder. Expected: every row has a CREATED request
--    whose id equals RESERVED_BY_SALES_REQUEST_ID (4 rows on the 2026-09-21 prod copy).
SELECT p.PRODUCT_ID,
       p.RESERVED_BY_SALES_REQUEST_ID,
       (SELECT GROUP_CONCAT(CONCAT(s.SALES_REQUEST_ID, ':', s.STATUS_ID))
          FROM SALES_REQUEST s
         WHERE s.PRODUCT_ID = p.PRODUCT_ID
           AND s.STATUS_ID <> 'SALES_REQUEST_CANCELLED') AS live_requests
  FROM PRODUCT p
 WHERE p.PRODUCT_TYPE_ID = 'APARTMENT'
   AND p.APARTMENT_STATUS_ID = 'APARTMENT_RESERVED'
 ORDER BY p.PRODUCT_ID;

-- 2. RESERVED units with NO live sales request. These become genuinely available once
--    flipped — review before running the UPDATE. Expected: empty.
SELECT p.PRODUCT_ID, p.RESERVED_BY_SALES_REQUEST_ID
  FROM PRODUCT p
 WHERE p.PRODUCT_TYPE_ID = 'APARTMENT'
   AND p.APARTMENT_STATUS_ID = 'APARTMENT_RESERVED'
   AND NOT EXISTS (SELECT 1 FROM SALES_REQUEST s
                    WHERE s.PRODUCT_ID = p.PRODUCT_ID
                      AND s.STATUS_ID <> 'SALES_REQUEST_CANCELLED');

-- 3. Live pending requests whose unit does not point back at them. The new guard relies on
--    this pointer, so fix these first (set RESERVED_BY_SALES_REQUEST_ID = SALES_REQUEST_ID).
--    Expected: empty.
SELECT s.SALES_REQUEST_ID, s.PRODUCT_ID, p.RESERVED_BY_SALES_REQUEST_ID
  FROM SALES_REQUEST s
  JOIN PRODUCT p ON p.PRODUCT_ID = s.PRODUCT_ID
 WHERE s.STATUS_ID = 'SALES_REQUEST_CREATED'
   AND (p.RESERVED_BY_SALES_REQUEST_ID IS NULL
        OR p.RESERVED_BY_SALES_REQUEST_ID <> s.SALES_REQUEST_ID);

-- 4. Dangling pointers (holder cancelled or deleted). Harmless to the guard — it re-checks
--    the holder is live — but clean them so the picker does not show them as held.
UPDATE PRODUCT p
  LEFT JOIN SALES_REQUEST s ON s.SALES_REQUEST_ID = p.RESERVED_BY_SALES_REQUEST_ID
   SET p.RESERVED_BY_SALES_REQUEST_ID = NULL
 WHERE p.RESERVED_BY_SALES_REQUEST_ID IS NOT NULL
   AND (s.SALES_REQUEST_ID IS NULL OR s.STATUS_ID = 'SALES_REQUEST_CANCELLED');

-- 5. The flip.
UPDATE PRODUCT
   SET APARTMENT_STATUS_ID  = 'APARTMENT_AVAILABLE',
       LAST_UPDATED_STAMP   = UTC_TIMESTAMP()
 WHERE PRODUCT_TYPE_ID      = 'APARTMENT'
   AND APARTMENT_STATUS_ID  = 'APARTMENT_RESERVED';

-- 6. Verify. Expected: no APARTMENT_RESERVED row.
SELECT APARTMENT_STATUS_ID, COUNT(*) FROM PRODUCT
 WHERE PRODUCT_TYPE_ID = 'APARTMENT' GROUP BY APARTMENT_STATUS_ID;
