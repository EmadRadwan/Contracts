-- =============================================================
-- 2. DIM RAW MATERIALS (for Supply Procurement certificates)
-- =============================================================
DROP VIEW IF EXISTS DimProductRawMaterials;
CREATE OR REPLACE VIEW DimProductRawMaterials AS
SELECT
    p.PRODUCT_ID                                 AS MaterialId,          -- KEY
    p.PRODUCT_NAME                               AS MaterialName,        -- e.g. جركن اداموند لدمج الخرسانة

    cat.CategoryName                             AS CategoryName,        -- e.g. مواد خام
    cat.ParentCategoryName                       AS MainCategoryName     -- usually null or top-level

FROM PRODUCT p
         LEFT JOIN DimProductCategories cat
                   ON p.PRIMARY_PRODUCT_CATEGORY_ID = cat.CategoryId
WHERE p.PRODUCT_TYPE_ID = 'RAW_MATERIAL';