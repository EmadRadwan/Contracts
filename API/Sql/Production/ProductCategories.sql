-- =============================================================
-- 1. DIM PRODUCT CATEGORIES (Hierarchy + Arabic Names)
-- =============================================================
DROP VIEW IF EXISTS DimProductCategories;
CREATE OR REPLACE VIEW DimProductCategories AS
SELECT
    pc.PRODUCT_CATEGORY_ID                                  AS CategoryId,                -- KEY

    -- Main names
    COALESCE(pc.DESCRIPTION_ARABIC, pc.DESCRIPTION)         AS CategoryName,               -- Arabic first
    pc.DESCRIPTION                                          AS CategoryNameEnglish,

    -- Parent hierarchy (for drill-down in Power BI)
    pc.PRIMARY_PARENT_CATEGORY_ID                           AS ParentCategoryId,
    parent.DESCRIPTION_ARABIC                               AS ParentCategoryName,

    -- Level calculation (optional – helps with sorting)
    CASE
        WHEN pc.PRIMARY_PARENT_CATEGORY_ID IS NULL THEN 1                     -- Top level
        ELSE 2                                                                -- Child level
        END                                                             AS CategoryLevel,

    -- Helpful flags
    (pc.PRIMARY_PARENT_CATEGORY_ID IS NULL)                         AS IsTopLevelCategory

FROM PRODUCT_CATEGORY pc
         LEFT JOIN PRODUCT_CATEGORY parent
                   ON pc.PRIMARY_PARENT_CATEGORY_ID = parent.PRODUCT_CATEGORY_ID;