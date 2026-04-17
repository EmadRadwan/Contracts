-- =============================================================
-- 3. DIM APARTMENTS (for Sales & Real Estate dashboards)
-- =============================================================
DROP VIEW IF EXISTS DimApartments;

CREATE OR REPLACE VIEW DimApartments AS
SELECT
    p.PRODUCT_ID                                 AS ApartmentId,         -- KEY
    p.PRODUCT_NAME                               AS ApartmentCode,       -- e.g. A1-02
    COALESCE(p.PRODUCT_NAME_ARABIC, p.PRODUCT_NAME) AS ApartmentNameArabic, -- Full Arabic description

    p.BUILDING_NUMBER                            AS BuildingNumber,
    p.FLOOR_NUMBER                               AS FloorNumber,

    p.APARTMENT_SPACE_M2                         AS ApartmentAreaM2,
    p.GARDEN_SPACE_M2                            AS GardenAreaM2,

    p.APARTMENT_PRICE_PER_M2                     AS ApartmentPricePerM2,
    p.GARDEN_PRICE_PER_M2                        AS GardenPricePerM2,

    -- Calculated totals
    ROUND(p.APARTMENT_SPACE_M2 * p.APARTMENT_PRICE_PER_M2, 2) AS ApartmentBasePrice,
    ROUND(COALESCE(p.GARDEN_SPACE_M2,0) * COALESCE(p.GARDEN_PRICE_PER_M2,0), 2) AS GardenTotalPrice,
    ROUND(
                    p.APARTMENT_SPACE_M2 * p.APARTMENT_PRICE_PER_M2 +
                    COALESCE(p.GARDEN_SPACE_M2,0) * COALESCE(p.GARDEN_PRICE_PER_M2,0), 2
        ) AS TotalListPrice,

    -- Status
    p.APARTMENT_STATUS_ID                        AS StatusId,
    CASE p.APARTMENT_STATUS_ID
        WHEN 'APARTMENT_AVAILABLE' THEN 'متاحة للبيع'
        WHEN 'APARTMENT_RESERVED'  THEN 'محجوزة'
        WHEN 'APARTMENT_SOLD'      THEN 'مباعة'
        WHEN 'APARTMENT_BLOCKED'   THEN 'محظورة'
        ELSE 'غير معروف'
        END                                          AS StatusName,

    p.PROJECT_ID                                 AS ProjectId,

    -- REFACTOR: Added ProjectName via LEFT JOIN to the projects/work efforts table.
    --           Uses LEFT JOIN so apartments without a project still appear (ProjectName will be NULL).
    --           Improves dashboard usability by showing readable project name alongside ProjectId.
    COALESCE(w.PROJECT_NAME, NULL)               AS ProjectName

FROM PRODUCT p
-- REFACTOR: LEFT JOIN to retrieve the project name. Adjust table/column names if different in your schema.
         LEFT JOIN WORK_EFFORT w ON p.PROJECT_ID = w.WORK_EFFORT_ID
WHERE p.PRODUCT_TYPE_ID = 'APARTMENT';