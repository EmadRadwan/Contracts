-- =============================================================
-- DIM PAYMENT TYPES – Clean dimension table (Power BI DimPaymentTypes)
-- =============================================================
DROP VIEW IF EXISTS DimPaymentTypes;
CREATE OR REPLACE VIEW DimPaymentTypes AS
SELECT
    -- Surrogate key – unique identifier for relationships
    pt.PAYMENT_TYPE_ID                                      AS PaymentTypeId,           -- KEY

    -- English description (primary display name in reports)
    COALESCE(pt.DESCRIPTION, pt.PAYMENT_TYPE_ID)            AS PaymentTypeName,         -- e.g. "Equipment Expenses"

    -- Arabic description (for bilingual reports)
    pt.DESCRIPTION_ARABIC                                   AS PaymentTypeNameArabic,   -- e.g. "مصاريف معدات"

    -- Parent category (for hierarchy / grouping)
    pt.PARENT_TYPE_ID                                       AS ParentTypeId,

    -- Parent category name – human readable (optional, derived from the same table)
    parent_pt.DESCRIPTION                                   AS ParentTypeName,
    parent_pt.DESCRIPTION_ARABIC                            AS ParentTypeNameArabic,

    -- Flag to indicate if this is a leaf/node that has its own table in OFBiz
    pt.HAS_TABLE                                            AS HasTableFlag,            -- 'Y' or 'N'

    -- Hierarchy level (simple 2-level for now: root vs child)
    CASE
        WHEN pt.PARENT_TYPE_ID IS NULL THEN 'Root'
        ELSE 'Child'
        END                                                     AS HierarchyLevel,

    -- Audit fields (optional – useful for debugging / data lineage)
    pt.LAST_UPDATED_STAMP                                   AS LastUpdatedStamp,
    pt.CREATED_STAMP                                        AS CreatedStamp

FROM PAYMENT_TYPE pt
         LEFT JOIN PAYMENT_TYPE parent_pt
                   ON pt.PARENT_TYPE_ID = parent_pt.PAYMENT_TYPE_ID;

-- =============================================================
-- USAGE NOTES
-- =============================================================
-- • PaymentTypeId is the primary key used in fact tables (e.g., Payment, AcctgTrans)
-- • Use PaymentTypeName for English reports, PaymentTypeNameArabic for Arabic
-- • ParentTypeName allows easy grouping (e.g., all rows with ParentTypeName = 'Disbursement')
-- • Add this view to your Power BI model as "DimPaymentTypes"