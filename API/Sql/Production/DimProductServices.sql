-- =============================================================
-- 1. DIM SERVICES (for Workmanship & External Works certificates)
-- =============================================================
DROP VIEW IF EXISTS DimProductServices;
CREATE OR REPLACE VIEW DimProductServices AS
SELECT
    p.PRODUCT_ID                                 AS ProductId,           -- KEY
    p.PRODUCT_NAME                               AS ProductName,         -- Arabic name used in certificates
    p.PRIMARY_PRODUCT_CATEGORY_ID                  AS PrimaryProductCategoryId,
    cat.CategoryName                             AS CategoryName        -- e.g. أعمال الألمونيوم

FROM PRODUCT p
         LEFT JOIN DimProductCategories cat
                   ON p.PRIMARY_PRODUCT_CATEGORY_ID = cat.CategoryId
WHERE p.PRODUCT_TYPE_ID = 'SERVICE';