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

-- =============================================================

-- =============================================================
-- DIM COST CENTERS – Clean dimension table (Power BI DimCostCenters)
-- =============================================================
DROP VIEW IF EXISTS DimCostCenters;
CREATE OR REPLACE VIEW DimCostCenters AS
SELECT
    -- Surrogate key (string to preserve leading zeros if needed)
    cc.COST_CENTER_ID                                       AS CostCenterId,             -- KEY

    -- Primary display name – Arabic (your real business language)
    COALESCE(cc.DESCRIPTION, cc.COST_CENTER_ID)             AS CostCenterName,           -- e.g. "الصحراوى 10.5 فدان"

    -- Payment direction flag – critical for separating revenue vs expense reporting
    cc.IS_OUT_PAYMENT                                       AS IsOutPaymentFlag         -- 'Y' = Expense Cost Center, 'N' = Revenue/Project Cost Center

FROM COST_CENTER cc
-- Optional: filter only active cost centers in the future
-- WHERE (cc.THRU_DATE IS NULL OR cc.THRU_DATE >= CURDATE())
;
-- =============================================================

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
    invd.WORK_EFFORT_ID              AS WorkEffortId
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

-- =============================================================

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
-- =============================================================

-- =============================================================
-- DIM PAYMENT METHOD TYPES – Clean dimension table (Power BI DimPaymentMethodTypes)
-- =============================================================
DROP VIEW IF EXISTS DimPaymentMethodTypes;
CREATE OR REPLACE VIEW DimPaymentMethodTypes AS
SELECT
    -- Surrogate key – unique identifier used in Payment and PaymentMethod tables
    pmt.PAYMENT_METHOD_TYPE_ID                              AS PaymentMethodTypeId,     -- KEY

    -- English name (primary display name)
    COALESCE(pmt.DESCRIPTION, pmt.PAYMENT_METHOD_TYPE_ID)   AS PaymentMethodTypeName,   -- e.g. "Cash"

    -- Arabic name (for bilingual reports)
    pmt.DESCRIPTION_ARABIC                                  AS PaymentMethodTypeNameArabic, -- e.g. "نق

    -- Default GL Account (useful for finance users & reconciliation reports)
    pmt.DEFAULT_GL_ACCOUNT_ID                               AS DefaultGlAccountId,

    -- Optional: Bring GL account description if you have a chart of accounts view/table
    -- coa.ACCOUNT_NAME                                     AS DefaultGlAccountName,   -- uncomment if you join later

    -- Payment category / grouping (common business grouping)
    CASE pmt.PAYMENT_METHOD_TYPE_ID
        WHEN 'CASH'              THEN 'Cash'
        WHEN 'CERTIFIED_CHECK'   THEN 'Check'
        WHEN 'COMPANY_CHECK'     THEN 'Check'
        WHEN 'PERSONAL_CHECK'    THEN 'Check'
        WHEN 'COMPANY_ACCOUNT'   THEN 'Bank Transfer'
        WHEN 'ELECTRONIC'        THEN 'Electronic'
        WHEN 'CREDIT_CARD'       THEN 'Credit Card'
        ELSE 'Other'
        END                                                     AS PaymentCategory,

    -- Audit fields (optional but helpful)
    pmt.LAST_UPDATED_STAMP                                  AS LastUpdatedStamp,
    pmt.CREATED_STAMP                                      AS CreatedStamp

FROM PAYMENT_METHOD_TYPE pmt;

-- =============================================================

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

-- =============================================================

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

DROP VIEW IF EXISTS DimProducts;

CREATE OR REPLACE VIEW DimProducts AS
SELECT
    p.PRODUCT_ID                      AS ProductId,        -- KEY
    p.PRODUCT_NAME                    AS ProductName,

    p.PRODUCT_TYPE_ID                 AS ProductType,      -- RAW_MATERIAL / SERVICE

    p.PRIMARY_PRODUCT_CATEGORY_ID     AS PrimaryCategoryId,
    cat.CategoryName                  AS CategoryName,
    cat.ParentCategoryName            AS MainCategoryName,

    p.CREATED_DATE                    AS CreatedDate,
    p.LAST_UPDATED_STAMP              AS LastUpdatedDate

FROM PRODUCT p
         LEFT JOIN DimProductCategories cat
                   ON p.PRIMARY_PRODUCT_CATEGORY_ID = cat.CategoryId

WHERE p.PRODUCT_TYPE_ID IN ('RAW_MATERIAL', 'SERVICE');
-- =============================================================

-- =============================================================
-- DIM PROJECTS – Clean dimension table (Power BI DimProjects)
-- =============================================================
DROP VIEW IF EXISTS DimProjects;
CREATE OR REPLACE VIEW DimProjects AS
SELECT
    -- Surrogate key – unique identifier for relationships
    we.WORK_EFFORT_ID                                   AS ProjectId,              -- KEY

    -- Main display name – this is what users see
    we.PROJECT_NAME                                     AS ProjectName,            -- e.g. "الصحراوى 3 فدان"

    -- Status
    CASE we.CURRENT_STATUS_ID
        WHEN 'WEPR_CREATED'     THEN 'Created'
        WHEN 'WEPR_IN_PROGRESS' THEN 'In Progress'
        WHEN 'WEPR_COMPLETE'    THEN 'Completed'
        WHEN 'WEPR_CANCELLED'   THEN 'Cancelled'
        WHEN 'WEPR_ON_HOLD'     THEN 'On Hold'
        ELSE 'Unknown'
        END                                                 AS StatusName,

    -- Dates
    we.ESTIMATED_START_DATE                             AS PlannedStartDate,
    we.ESTIMATED_COMPLETION_DATE                        AS PlannedEndDate,

    -- Facility / Site
    we.FACILITY_ID                                      AS FacilityId,
    fac.FACILITY_NAME                                   AS FacilityName,

    -- -----------------------------------------------------------------
    -- Ownership: is this the company's own project, or work the company
    -- is doing for someone else? Source: WORK_EFFORT.IS_COMPANY_PROJECT
    -- (tinyint(1), nullable — added 2026-08-17, backfilled by
    --  API/Sql/Localhost/set_is_company_project_flags.sql).
    --
    -- NULL is treated as 0 ("work for others"): that matches the default
    -- of the ProjectForm checkbox and keeps legacy rows that predate the
    -- column out of the company-project figures rather than silently
    -- inflating them.
    --
    -- NOTE for Power BI: COALESCE strips the tinyint(1) display width, so
    -- the MySQL connector surfaces this as a Whole Number (0/1), NOT as
    -- True/False. Write DAX as [IsCompanyProject] = 1, not = TRUE().
    -- -----------------------------------------------------------------
    COALESCE(we.IS_COMPANY_PROJECT, 0)                  AS IsCompanyProject,       -- 1 = company's own, 0 = for others

    -- Ready-made slicer / legend labels so the report doesn't have to
    -- build them in DAX (same pattern as DimApartments status labels)
    CASE WHEN COALESCE(we.IS_COMPANY_PROJECT, 0) = 1
             THEN 'مشاريع الشركة'
         ELSE 'أعمال للغير'
        END                                             AS ProjectOwnership,

    CASE WHEN COALESCE(we.IS_COMPANY_PROJECT, 0) = 1
             THEN 'Company Project'
         ELSE 'Work for Others'
        END                                             AS ProjectOwnershipEnglish

FROM WORK_EFFORT we
         LEFT JOIN FACILITY fac
                   ON we.FACILITY_ID = fac.FACILITY_ID
         LEFT JOIN WORK_EFFORT parent_we
                   ON we.WORK_EFFORT_PARENT_ID = parent_we.WORK_EFFORT_ID
                       AND parent_we.WORK_EFFORT_TYPE_ID = 'PROJECT'

WHERE we.WORK_EFFORT_TYPE_ID = 'PROJECT';

-- =============================================================

-- =============================================================
-- DIM STATUS ITEMS – Clean dimension for all status types (Power BI DimStatusItems)
-- =============================================================
DROP VIEW IF EXISTS DimStatusItems;
CREATE OR REPLACE VIEW DimStatusItems AS
SELECT
    -- Surrogate key – used in almost every OFBiz entity that has status
    si.STATUS_ID                                            AS StatusId,                 -- KEY

    -- Primary display name (Arabic – matches your real usage)
    COALESCE(si.DESCRIPTION_ARABIC, si.DESCRIPTION, si.STATUS_ID)
                                                            AS StatusName,               -- e.g. "متصالح"

    -- English name (fallback or for English dashboards)
    COALESCE(si.DESCRIPTION, si.STATUS_ID)                  AS StatusNameEnglish,        -- e.g. "Reconciled"

    -- Status type (very useful for filtering specific workflows)
    si.STATUS_TYPE_ID                                       AS StatusTypeId,


    -- Audit
    si.LAST_UPDATED_STAMP                                   AS LastUpdatedStamp,
    si.CREATED_STAMP                                        AS CreatedStamp

FROM STATUS_ITEM si
-- Optional: filter only active/relevant status types if needed
-- WHERE si.STATUS_TYPE_ID IN ('ACCTG_ENREC_STATUS', 'ORDER_STATUS', 'INVOICE_STATUS', ...)
;

-- =============================================================

-- =============================================================
-- 1. DIM SUPPLIERS
-- =============================================================
DROP VIEW IF EXISTS DimSuppliers;
CREATE OR REPLACE VIEW DimSuppliers AS
SELECT
    p.PARTY_ID                     AS SupplierId,          -- KEY
    p.DESCRIPTION                  AS SupplierName,
    p.PARTY_TYPE_ID                AS PartyTypeId,
    CASE p.PARTY_TYPE_ID
        WHEN 'PERSON'      THEN 'Individual'
        WHEN 'PARTY_GROUP' THEN 'Company'
        ELSE p.PARTY_TYPE_ID
        END                            AS PartyType,
    p.STATUS_ID                    AS StatusId,
    CASE p.STATUS_ID
        WHEN 'PARTY_ENABLED'  THEN 'Active'
        WHEN 'PARTY_DISABLED' THEN 'Inactive'
        ELSE 'Unknown'
        END                            AS StatusName,
    p.CREATED_DATE                 AS CreatedDate,
    p.LAST_UPDATED_STAMP           AS LastUpdatedDate
FROM PARTY p
WHERE p.MAIN_ROLE = 'SUPPLIER';


-- =============================================================
-- 2. DIM CONTRACTORS
-- =============================================================
DROP VIEW IF EXISTS DimContractors;
CREATE OR REPLACE VIEW DimContractors AS
SELECT
    p.PARTY_ID                     AS ContractorId,        -- KEY
    p.DESCRIPTION                  AS ContractorName,
    p.PARTY_TYPE_ID                AS PartyTypeId,
    CASE p.PARTY_TYPE_ID
        WHEN 'PERSON'      THEN 'Individual'
        WHEN 'PARTY_GROUP' THEN 'Company'
        ELSE p.PARTY_TYPE_ID
        END                            AS PartyType,
    p.STATUS_ID                    AS StatusId,
    CASE p.STATUS_ID
        WHEN 'PARTY_ENABLED'  THEN 'Active'
        WHEN 'PARTY_DISABLED' THEN 'Inactive'
        ELSE 'Unknown'
        END                            AS StatusName,
    p.CREATED_DATE                 AS CreatedDate,
    p.LAST_UPDATED_STAMP           AS LastUpdatedDate
FROM PARTY p
WHERE p.MAIN_ROLE = 'CONTRACTOR';


-- =============================================================
-- 3. DIM EMPLOYEES (Internal Team / Project Owners)
-- =============================================================
DROP VIEW IF EXISTS DimEmployees;
CREATE OR REPLACE VIEW DimEmployees AS
SELECT
    p.PARTY_ID                     AS EmployeeId,          -- KEY
    p.DESCRIPTION                  AS EmployeeName,
    p.STATUS_ID                    AS StatusId,
    CASE p.STATUS_ID
        WHEN 'PARTY_ENABLED'  THEN 'Active'
        WHEN 'PARTY_DISABLED' THEN 'Inactive'
        ELSE 'Unknown'
        END                            AS StatusName,
    p.CREATED_DATE                 AS HireDate,
    p.LAST_UPDATED_STAMP           AS LastUpdatedDate
FROM PARTY p
WHERE p.MAIN_ROLE = 'EMPLOYEE';

DROP VIEW IF EXISTS DimParties;

CREATE OR REPLACE VIEW DimParties AS
SELECT
    p.PARTY_ID              AS PartyId,        -- KEY
    p.DESCRIPTION           AS PartyName,
    p.MAIN_ROLE             AS PartyType     -- CONTRACTOR / CUSTOMER / SUPPLIER

FROM PARTY p
WHERE p.MAIN_ROLE IN ('CONTRACTOR', 'CUSTOMER', 'SUPPLIER');

-- =============================================================


-- =============================================================
-- DIM PAYMENT METHODS – Clean dimension table (Power BI DimPaymentMethods)
-- =============================================================
DROP VIEW IF EXISTS DimPaymentMethods;
CREATE OR REPLACE VIEW DimPaymentMethods AS
SELECT
    -- Surrogate key – used in Payment fact table
    pm.PAYMENT_METHOD_ID                                    AS PaymentMethodId,          -- KEY

    -- Main display name (Arabic description is usually richer in your system)
    COALESCE(pm.DESCRIPTION, pm.PAYMENT_METHOD_ID)          AS PaymentMethodName,        -- e.g. "بنك أبو ظبي الإسلامي" or "كاش"

    -- English fallback (derived from PaymentMethodType description)
    COALESCE(pmt.DESCRIPTION, pm.PAYMENT_METHOD_ID)         AS PaymentMethodNameEnglish,

    -- Payment Method Type (link to DimPaymentMethodTypes)
    pm.PAYMENT_METHOD_TYPE_ID                               AS PaymentMethodTypeId,
    pmt.DESCRIPTION                                         AS PaymentMethodTypeName,
    pmt.DESCRIPTION_ARABIC                                  AS PaymentMethodTypeNameArabic,

    -- Bank / Financial Account details
    pm.FIN_ACCOUNT_ID                                       AS FinAccountId,
    pm.GL_ACCOUNT_ID                                        AS GlAccountId,

    -- Party (almost always "Company" in your case, but kept for completeness)
    pm.PARTY_ID                                             AS PartyId,

    -- Active status (slowly changing dimension type 2 support)
    pm.FROM_DATE                                            AS ValidFrom,
    pm.THRU_DATE                                            AS ValidThru,
    CASE WHEN pm.THRU_DATE IS NULL THEN 'Y' ELSE 'N' END    AS IsActive,

    -- Helpful categorization flags
    CASE
        WHEN pm.PAYMENT_METHOD_TYPE_ID = 'CASH'                  THEN 'Y' ELSE 'N'
        END                                                     AS IsCash,
    CASE
        WHEN pm.PAYMENT_METHOD_TYPE_ID IN ('COMPANY_CHECK', 'CERTIFIED_CHECK')
            OR pm.FIN_ACCOUNT_ID IS NOT NULL                    THEN 'Y' ELSE 'N'
        END                                                     AS IsBankAccount,
    CASE
        WHEN pm.PAYMENT_METHOD_TYPE_ID = 'FIN_ACCOUNT'           THEN 'Y' ELSE 'N'
        END                                                     AS IsCustody,  -- عهدة مستديمة

    -- Audit
    pm.LAST_UPDATED_STAMP                                   AS LastUpdatedStamp,
    pm.CREATED_STAMP                                        AS CreatedStamp

FROM PAYMENT_METHOD pm
         LEFT JOIN PAYMENT_METHOD_TYPE pmt
                   ON pm.PAYMENT_METHOD_TYPE_ID = pmt.PAYMENT_METHOD_TYPE_ID

WHERE (pm.THRU_DATE IS NULL OR pm.THRU_DATE >= CURDATE());
-- =============================================================

-- =============================================================
-- FACT PAYMENTS – Complete, clean, universal fact table
-- Includes ALL payment types (Cash, Cheque, Bank Transfer, Credit Card, etc.)
-- NO joins – all dimensions resolved in Power BI
-- =============================================================
DROP VIEW IF EXISTS Payments;
CREATE OR REPLACE VIEW Payments AS
SELECT
    -- =================================================================
    -- Core Keys (keep for relationships in Power BI / reporting tools)
    -- =================================================================
    p.PAYMENT_ID AS PaymentId,

    -- =================================================================
    -- Amounts & Currency
    -- =================================================================
    p.AMOUNT AS Amount,
    p.ACTUAL_CURRENCY_AMOUNT AS ActualAmount,
    COALESCE(p.CURRENCY_UOM_ID, 'EGP') AS CurrencyUomId,

    -- =================================================================
    -- Parties – Raw IDs + Names
    -- =================================================================
    p.PARTY_ID_FROM AS PartyIdFrom,
    pf.DESCRIPTION AS PartyNameFrom,                  -- e.g. "الشركة", "محمد أحمد"

    p.PARTY_ID_TO AS PartyIdTo,
    COALESCE(pt.DESCRIPTION,
             CASE WHEN p.PARTY_ID_TO = 'Company' THEN 'Company' ELSE p.PARTY_ID_TO END,
             'Unknown') AS PartyNameTo,               -- Handles "Company" literal and missing parties

    -- =================================================================
    -- Payment Classification + Names
    -- =================================================================
    p.PAYMENT_TYPE_ID AS PaymentTypeId,
    p.SALES_REQUEST_ID AS SalesRequestId,
    pt_type.DESCRIPTION AS PaymentTypeDescription,    -- REFACTOR: Added join to PaymentType for readable type name
    pt_type.DESCRIPTION_ARABIC AS PaymentTypeDescriptionArabic,

    p.PAYMENT_METHOD_TYPE_ID AS PaymentMethodTypeId,
    pmt_type.DESCRIPTION AS PaymentMethodTypeName,
    pmt_type.DESCRIPTION_ARABIC AS PaymentMethodTypeNameArabic,
    p.PAYMENT_REF_NUM AS PaymentRefNum,
    p.PAYMENT_METHOD_ID AS PaymentMethodId,
    pm.DESCRIPTION AS PaymentMethodName,              -- e.g. "بنك أبوظبي الإسلامي", "كاش"
    prod.PRODUCT_ID AS ProductId,                 -- e.g. "A1-01"
    prod.BUILDING_NUMBER AS BuildingNumber,       -- e.g. "A1"

    -- =================================================================
    -- Project & Cost Center + Names
    -- =================================================================
    p.WORK_EFFORT_ID AS ProjectId,
    we.PROJECT_NAME AS ProjectName,

    p.COST_CENTER_ID AS CostCenterId,
    cc.DESCRIPTION AS CostCenterName,
    opp.ORDER_ID                        AS OrderId,                     -- ← the main field you requested

    -- =================================================================
    -- Status + Names (English & Arabic)
    -- =================================================================
    p.STATUS_ID AS StatusId,
    si.DESCRIPTION AS StatusNameEnglish,
    si.DESCRIPTION_ARABIC AS StatusNameArabic,

    -- =================================================================
    -- Dates & Core Due Calculation
    -- =================================================================
    p.EFFECTIVE_DATE AS EffectiveDate,
    DATEDIFF(p.EFFECTIVE_DATE, CURDATE()) AS DaysUntilDue,  -- Positive = future, 0 = today, negative = overdue

    -- =================================================================
    -- REFACTOR: Arabic Due Status – Matching C# Handler Logic Exactly
    -- Includes Quarter + Year for long-term and very overdue cases
    -- =================================================================
    CASE
        WHEN p.STATUS_ID <> 'PMNT_NOT_PAID' THEN si.DESCRIPTION_ARABIC

        ELSE
            -- Only for unpaid payments (PMNT_NOT_PAID)
            CASE
                WHEN DATEDIFF(p.EFFECTIVE_DATE, CURDATE()) < 0 THEN
                    -- Overdue
                    CASE
                        WHEN ABS(DATEDIFF(p.EFFECTIVE_DATE, CURDATE())) <= 30 THEN
                            CONCAT(
                                    CASE WHEN pt_type.PARENT_TYPE_ID = 'DISBURSEMENT' THEN 'دفعة' ELSE 'مستحق' END,
                                    ' متأخرة منذ ',
                                    ABS(DATEDIFF(p.EFFECTIVE_DATE, CURDATE())),
                                    ' يوم'
                            )
                        ELSE
                            -- > 30 days overdue → "متأخرة جداً" + quarter
                            CONCAT(
                                    CASE WHEN pt_type.PARENT_TYPE_ID = 'DISBURSEMENT' THEN 'دفعة' ELSE 'مستحق' END,
                                    ' متأخرة جداً ',
                                    '(الربع ',
                                    CASE
                                        WHEN MONTH(p.EFFECTIVE_DATE) BETWEEN 1 AND 3 THEN 'الأول'
                                        WHEN MONTH(p.EFFECTIVE_DATE) BETWEEN 4 AND 6 THEN 'الثاني'
                                        WHEN MONTH(p.EFFECTIVE_DATE) BETWEEN 7 AND 9 THEN 'الثالث'
                                        ELSE 'الرابع'
                                        END,
                                    ' ',
                                    YEAR(p.EFFECTIVE_DATE),
                                    ')'
                            )
                        END

                WHEN DATEDIFF(p.EFFECTIVE_DATE, CURDATE()) = 0 THEN
                    CONCAT(CASE WHEN pt_type.PARENT_TYPE_ID = 'DISBURSEMENT' THEN 'دفعة مستحقة' ELSE 'مستحق' END, ' اليوم')

                WHEN DATEDIFF(p.EFFECTIVE_DATE, CURDATE()) = 1 THEN
                    CONCAT(CASE WHEN pt_type.PARENT_TYPE_ID = 'DISBURSEMENT' THEN 'دفعة مستحقة' ELSE 'مستحق' END, ' غداً')

                WHEN DATEDIFF(p.EFFECTIVE_DATE, CURDATE()) <= 3 THEN
                    CONCAT(
                            CASE WHEN pt_type.PARENT_TYPE_ID = 'DISBURSEMENT' THEN 'دفعة مستحقة' ELSE 'مستحق' END,
                            ' بعد ',
                            DATEDIFF(p.EFFECTIVE_DATE, CURDATE()),
                            ' أيام'
                    )

                WHEN DATEDIFF(p.EFFECTIVE_DATE, CURDATE()) <= 7 THEN
                    CONCAT(CASE WHEN pt_type.PARENT_TYPE_ID = 'DISBURSEMENT' THEN 'دفعة مستحقة' ELSE 'مستحق' END, ' هذا الأسبوع')

                WHEN DATEDIFF(p.EFFECTIVE_DATE, CURDATE()) <= 30 THEN
                    CONCAT(CASE WHEN pt_type.PARENT_TYPE_ID = 'DISBURSEMENT' THEN 'دفعة مستحقة' ELSE 'مستحق' END, ' خلال الشهر')

                WHEN DATEDIFF(p.EFFECTIVE_DATE, CURDATE()) <= 90 THEN
                    -- "خلال 3 أشهر" + quarter (as in C#)
                    CONCAT(
                            CASE WHEN pt_type.PARENT_TYPE_ID = 'DISBURSEMENT' THEN 'دفعة مستحقة' ELSE 'مستحق' END,
                            ' خلال 3 أشهر ',
                            '(الربع ',
                            CASE
                                WHEN MONTH(p.EFFECTIVE_DATE) BETWEEN 1 AND 3 THEN 'الأول'
                                WHEN MONTH(p.EFFECTIVE_DATE) BETWEEN 4 AND 6 THEN 'الثاني'
                                WHEN MONTH(p.EFFECTIVE_DATE) BETWEEN 7 AND 9 THEN 'الثالث'
                                ELSE 'الرابع'
                                END,
                            ' ',
                            YEAR(p.EFFECTIVE_DATE),
                            ')'
                    )

                ELSE
                    -- Far future
                    CONCAT(
                            CASE WHEN pt_type.PARENT_TYPE_ID = 'DISBURSEMENT' THEN 'دفعة مستحقة' ELSE 'مستحق' END,
                            ' لاحقاً ',
                            '(الربع ',
                            CASE
                                WHEN MONTH(p.EFFECTIVE_DATE) BETWEEN 1 AND 3 THEN 'الأول'
                                WHEN MONTH(p.EFFECTIVE_DATE) BETWEEN 4 AND 6 THEN 'الثاني'
                                WHEN MONTH(p.EFFECTIVE_DATE) BETWEEN 7 AND 9 THEN 'الثالث'
                                ELSE 'الرابع'
                                END,
                            ' ',
                            YEAR(p.EFFECTIVE_DATE),
                            ')'
                    )
                END
        END AS DueStatusArabic,
    -- =================================================================
    -- Helpful Flags
    -- =================================================================
    CASE WHEN pt_type.PARENT_TYPE_ID = 'DISBURSEMENT' THEN 1 ELSE 0 END AS IsDisbursement,  -- REFACTOR: More reliable than PartyId check
    CASE
        WHEN pt_type.PARENT_TYPE_ID = 'DISBURSEMENT' THEN p.PARTY_ID_FROM   -- Outgoing: organization is From
        ELSE p.PARTY_ID_TO                                                 -- Incoming: organization is To
        END AS OrganizationPartyId,

    CASE
        WHEN pt_type.PARENT_TYPE_ID = 'DISBURSEMENT' THEN 'Outbound'
        WHEN p.PARTY_ID_TO = 'Company' THEN 'Inbound'
        ELSE 'Unknown'
        END AS PaymentDirection,

    -- =================================================================
    -- Additional fields (kept for completeness)
    -- =================================================================
    p.COMMENTS AS Comments,
    p.ChequeNumber AS ChequeNumber,
    p.ChequeDate AS ChequeDate,
    p.OVERRIDE_GL_ACCOUNT_ID AS OverrideGlAccountId,
    p.CREATED_STAMP AS CreatedDate,
    p.PAYMENT_PREFERENCE_ID             AS PaymentPreferenceId


FROM PAYMENT p

-- Required joins (inner – assumed always present)
         LEFT JOIN PARTY pf ON p.PARTY_ID_FROM = pf.PARTY_ID
         LEFT JOIN PARTY pt ON p.PARTY_ID_TO = pt.PARTY_ID
         LEFT JOIN PAYMENT_METHOD pm ON p.PAYMENT_METHOD_ID = pm.PAYMENT_METHOD_ID
         LEFT JOIN COST_CENTER cc ON p.COST_CENTER_ID = cc.COST_CENTER_ID
         LEFT JOIN STATUS_ITEM si ON p.STATUS_ID = si.STATUS_ID
         LEFT JOIN WORK_EFFORT we ON p.WORK_EFFORT_ID = we.WORK_EFFORT_ID

-- REFACTOR: Added essential joins for accurate direction and descriptions
         LEFT JOIN PAYMENT_TYPE pt_type ON p.PAYMENT_TYPE_ID = pt_type.PAYMENT_TYPE_ID
         LEFT JOIN PAYMENT_METHOD_TYPE pmt_type ON p.PAYMENT_METHOD_TYPE_ID = pmt_type.PAYMENT_METHOD_TYPE_ID

         LEFT JOIN ORDER_PAYMENT_PREFERENCE opp ON p.PAYMENT_PREFERENCE_ID = opp.ORDER_PAYMENT_PREFERENCE_ID
         LEFT JOIN ORDER_HEADER       ord       ON opp.ORDER_ID            = ord.ORDER_ID

-- NEW JOINS for ProductId & BuildingNumber
         LEFT JOIN SALES_REQUEST sr ON p.SALES_REQUEST_ID = sr.SALES_REQUEST_ID
         LEFT JOIN PRODUCT prod ON sr.PRODUCT_ID = prod.PRODUCT_ID

ORDER BY p.EFFECTIVE_DATE DESC, p.PAYMENT_ID DESC;
-- =============================================================

DROP VIEW IF EXISTS Payments_2;

CREATE OR REPLACE VIEW Payments_2 AS
SELECT
    -- =================================================================
    -- Core Keys
    -- =================================================================
    p.PAYMENT_ID AS PaymentId,

    -- =================================================================
    -- Amounts & Currency
    -- =================================================================
    p.AMOUNT AS Amount,
    p.ACTUAL_CURRENCY_AMOUNT AS ActualAmount,
    COALESCE(p.CURRENCY_UOM_ID, 'EGP') AS CurrencyUomId,

    -- =================================================================
    -- Parties
    -- =================================================================
    p.PARTY_ID_FROM AS PartyIdFrom,
    pf.DESCRIPTION AS PartyNameFrom,
    p.PARTY_ID_TO AS PartyIdTo,
    COALESCE(pt.DESCRIPTION,
             CASE WHEN p.PARTY_ID_TO = 'Company' THEN 'Company' ELSE p.PARTY_ID_TO END,
             'Unknown') AS PartyNameTo,

    -- =================================================================
    -- Payment Classification
    -- =================================================================
    p.PAYMENT_TYPE_ID AS PaymentTypeId,
    pt_type.DESCRIPTION AS PaymentTypeDescription,
    pt_type.DESCRIPTION_ARABIC AS PaymentTypeDescriptionArabic,
    p.PAYMENT_METHOD_TYPE_ID AS PaymentMethodTypeId,
    pmt_type.DESCRIPTION_ARABIC AS PaymentMethodTypeNameArabic,
    p.PAYMENT_REF_NUM AS PaymentRefNum,
    p.PAYMENT_METHOD_ID AS PaymentMethodId,
    pm.DESCRIPTION AS PaymentMethodName,

    prod.PRODUCT_ID AS ProductId,
    prod.BUILDING_NUMBER AS BuildingNumber,

    -- =================================================================
    -- Project & Cost Center
    -- =================================================================
    p.WORK_EFFORT_ID AS ProjectId,
    we.PROJECT_NAME AS ProjectName,
    p.COST_CENTER_ID AS CostCenterId,
    cc.DESCRIPTION AS CostCenterName,
    opp.ORDER_ID AS OrderId,

    -- =================================================================
    -- Status
    -- =================================================================
    p.STATUS_ID AS StatusId,
    si.DESCRIPTION_ARABIC AS StatusNameArabic,

    -- =================================================================
    -- Dates
    -- =================================================================
    p.EFFECTIVE_DATE AS EffectiveDate,
    YEAR(p.EFFECTIVE_DATE) AS DueYear,

    -- Quarter in Arabic (for clean tree display)
    CONCAT('الربع ',
           CASE
               WHEN MONTH(p.EFFECTIVE_DATE) BETWEEN 1 AND 3 THEN 'الأول'
               WHEN MONTH(p.EFFECTIVE_DATE) BETWEEN 4 AND 6 THEN 'الثاني'
               WHEN MONTH(p.EFFECTIVE_DATE) BETWEEN 7 AND 9 THEN 'الثالث'
               ELSE 'الرابع'
               END,
           ' ',
           YEAR(p.EFFECTIVE_DATE)) AS DueQuarterArabic,

    -- Sortable quarter number (hidden in visual, used for correct ordering)
    CONCAT(YEAR(p.EFFECTIVE_DATE),
           LPAD(CEILING(MONTH(p.EFFECTIVE_DATE)/3), 2, '0')) AS DueQuarterSort,

    DATEDIFF(p.EFFECTIVE_DATE, CURDATE()) AS DaysUntilDue,

    -- =================================================================
    -- DueStatusArabic (kept exactly as before)
    -- =================================================================
    CASE
        WHEN p.STATUS_ID <> 'PMNT_NOT_PAID' THEN si.DESCRIPTION_ARABIC
        ELSE
            CASE
                WHEN DATEDIFF(p.EFFECTIVE_DATE, CURDATE()) < 0 THEN
                    CASE
                        WHEN ABS(DATEDIFF(p.EFFECTIVE_DATE, CURDATE())) <= 30 THEN
                            CONCAT(CASE WHEN pt_type.PARENT_TYPE_ID = 'DISBURSEMENT' THEN 'دفعة' ELSE 'مستحق' END,
                                   ' متأخرة منذ ', ABS(DATEDIFF(p.EFFECTIVE_DATE, CURDATE())), ' يوم')
                        ELSE
                            CONCAT(CASE WHEN pt_type.PARENT_TYPE_ID = 'DISBURSEMENT' THEN 'دفعة' ELSE 'مستحق' END,
                                   ' متأخرة جداً ',
                                   '(الربع ',
                                   CASE WHEN MONTH(p.EFFECTIVE_DATE) BETWEEN 1 AND 3 THEN 'الأول'
                                        WHEN MONTH(p.EFFECTIVE_DATE) BETWEEN 4 AND 6 THEN 'الثاني'
                                        WHEN MONTH(p.EFFECTIVE_DATE) BETWEEN 7 AND 9 THEN 'الثالث'
                                        ELSE 'الرابع' END,
                                   ' ', YEAR(p.EFFECTIVE_DATE), ')')
                        END

                WHEN DATEDIFF(p.EFFECTIVE_DATE, CURDATE()) = 0 THEN
                    CONCAT(CASE WHEN pt_type.PARENT_TYPE_ID = 'DISBURSEMENT' THEN 'دفعة مستحقة' ELSE 'مستحق' END, ' اليوم')

                WHEN DATEDIFF(p.EFFECTIVE_DATE, CURDATE()) = 1 THEN
                    CONCAT(CASE WHEN pt_type.PARENT_TYPE_ID = 'DISBURSEMENT' THEN 'دفعة مستحقة' ELSE 'مستحق' END, ' غداً')

                WHEN DATEDIFF(p.EFFECTIVE_DATE, CURDATE()) <= 3 THEN
                    CONCAT(CASE WHEN pt_type.PARENT_TYPE_ID = 'DISBURSEMENT' THEN 'دفعة مستحقة' ELSE 'مستحق' END,
                           ' بعد ', DATEDIFF(p.EFFECTIVE_DATE, CURDATE()), ' أيام')

                WHEN DATEDIFF(p.EFFECTIVE_DATE, CURDATE()) <= 7 THEN
                    CONCAT(CASE WHEN pt_type.PARENT_TYPE_ID = 'DISBURSEMENT' THEN 'دفعة مستحقة' ELSE 'مستحق' END, ' هذا الأسبوع')

                WHEN DATEDIFF(p.EFFECTIVE_DATE, CURDATE()) <= 30 THEN
                    CONCAT(CASE WHEN pt_type.PARENT_TYPE_ID = 'DISBURSEMENT' THEN 'دفعة مستحقة' ELSE 'مستحق' END, ' خلال الشهر')

                WHEN DATEDIFF(p.EFFECTIVE_DATE, CURDATE()) <= 90 THEN
                    CONCAT(CASE WHEN pt_type.PARENT_TYPE_ID = 'DISBURSEMENT' THEN 'دفعة مستحقة' ELSE 'مستحق' END,
                           ' خلال 3 أشهر ',
                           '(الربع ',
                           CASE WHEN MONTH(p.EFFECTIVE_DATE) BETWEEN 1 AND 3 THEN 'الأول'
                                WHEN MONTH(p.EFFECTIVE_DATE) BETWEEN 4 AND 6 THEN 'الثاني'
                                WHEN MONTH(p.EFFECTIVE_DATE) BETWEEN 7 AND 9 THEN 'الثالث'
                                ELSE 'الرابع' END,
                           ' ', YEAR(p.EFFECTIVE_DATE), ')')

                ELSE
                    CONCAT(CASE WHEN pt_type.PARENT_TYPE_ID = 'DISBURSEMENT' THEN 'دفعة مستحقة' ELSE 'مستحق' END,
                           ' لاحقاً ',
                           '(الربع ',
                           CASE WHEN MONTH(p.EFFECTIVE_DATE) BETWEEN 1 AND 3 THEN 'الأول'
                                WHEN MONTH(p.EFFECTIVE_DATE) BETWEEN 4 AND 6 THEN 'الثاني'
                                WHEN MONTH(p.EFFECTIVE_DATE) BETWEEN 7 AND 9 THEN 'الثالث'
                                ELSE 'الرابع' END,
                           ' ', YEAR(p.EFFECTIVE_DATE), ')')
                END
        END AS DueStatusArabic,

    -- =================================================================
    -- Helpful Flags
    -- =================================================================
    CASE WHEN pt_type.PARENT_TYPE_ID = 'DISBURSEMENT' THEN 1 ELSE 0 END AS IsDisbursement,
    CASE WHEN pt_type.PARENT_TYPE_ID = 'DISBURSEMENT' THEN 'Outbound'
         WHEN p.PARTY_ID_TO = 'Company' THEN 'Inbound'
         ELSE 'Unknown' END AS PaymentDirection,

    -- Additional fields
    p.COMMENTS AS Comments,
    p.ChequeNumber AS ChequeNumber,
    p.CREATED_STAMP AS CreatedDate

FROM PAYMENT p
         LEFT JOIN PARTY pf ON p.PARTY_ID_FROM = pf.PARTY_ID
         LEFT JOIN PARTY pt ON p.PARTY_ID_TO = pt.PARTY_ID
         LEFT JOIN PAYMENT_METHOD pm ON p.PAYMENT_METHOD_ID = pm.PAYMENT_METHOD_ID
         LEFT JOIN COST_CENTER cc ON p.COST_CENTER_ID = cc.COST_CENTER_ID
         LEFT JOIN STATUS_ITEM si ON p.STATUS_ID = si.STATUS_ID
         LEFT JOIN WORK_EFFORT we ON p.WORK_EFFORT_ID = we.WORK_EFFORT_ID
         LEFT JOIN PAYMENT_TYPE pt_type ON p.PAYMENT_TYPE_ID = pt_type.PAYMENT_TYPE_ID
         LEFT JOIN PAYMENT_METHOD_TYPE pmt_type ON p.PAYMENT_METHOD_TYPE_ID = pmt_type.PAYMENT_METHOD_TYPE_ID
         LEFT JOIN ORDER_PAYMENT_PREFERENCE opp ON p.PAYMENT_PREFERENCE_ID = opp.ORDER_PAYMENT_PREFERENCE_ID
         LEFT JOIN SALES_REQUEST sr ON p.SALES_REQUEST_ID = sr.SALES_REQUEST_ID
         LEFT JOIN PRODUCT prod ON sr.PRODUCT_ID = prod.PRODUCT_ID

ORDER BY p.EFFECTIVE_DATE DESC, p.PAYMENT_ID DESC;

-- =============================================================
-- 1. PROJECT CERTIFICATES – CertificateNumber is now a KEY column
-- =============================================================
DROP VIEW IF EXISTS ProjectCertificates;
CREATE OR REPLACE VIEW ProjectCertificates AS
SELECT
    we.CERTIFICATE_NUMBER                            AS CertificateNumber,      -- This is what users know!
    we.WORK_EFFORT_ID                                AS CertificateId,

    we.CURRENT_STATUS_ID                             AS CurrentStatusId,
    CASE we.CURRENT_STATUS_ID
        WHEN 'WEPR_CREATED'   THEN 'Created'
        WHEN 'WEPR_APPROVED'  THEN 'Approved'
        WHEN 'WEPR_COMPLETE'  THEN 'Completed'
        ELSE 'Unknown'
        END                                              AS StatusName,

    we.CERTIFICATE_CATEGORY                          AS CertificateTypeCode,
    CASE we.CERTIFICATE_CATEGORY
        WHEN 'SUPPLY_PROCUREMENT_CERTIFICATE'      THEN 'Supply Procurement'
        WHEN 'WORKMANSHIP_CONTRACTING_CERTIFICATE' THEN 'Workmanship Contracting'
        WHEN 'COMPANY_SUPPLY_SALE_CERTIFICATE'     THEN 'Company Supply Sale'
        ELSE we.CERTIFICATE_CATEGORY
        END                                              AS CertificateType,

    we.DESCRIPTION                                   AS CertificateDescription,
    we.ESTIMATED_START_DATE                          AS PeriodStartDate,
    we.ESTIMATED_COMPLETION_DATE                     AS PeriodEndDate,
    we.CREATED_DATE                                  AS CreatedDate,
    we.LAST_UPDATED_STAMP                            AS LastUpdatedDate,

    -- Project
    proj_we.WORK_EFFORT_ID                           AS ProjectId,
    proj_we.PROJECT_NAME                             AS ProjectName,
    proj_fac.FACILITY_NAME                           AS ProjectFacilityName,

    -- Party (Supplier / Contractor)
    COALESCE(we.PARTY_ID_SUPPLIER, we.PARTY_ID_CONTRACTOR) AS PartyId,
    COALESCE(supp.DESCRIPTION, contr.DESCRIPTION)           AS PartyName,
    we.PARTY_ID_SUPPLIER                             AS SupplierPartyId,
    supp.DESCRIPTION                                  AS SupplierName,
    we.PARTY_ID_CONTRACTOR                           AS ContractorPartyId,
    contr.DESCRIPTION                                 AS ContractorName,

    -- Facility
    we.FACILITY_ID                                   AS FacilityId,
    fac.FACILITY_NAME                                AS FacilityName,

    -- Related PO
    we.RELATED_ORDER_ID                              AS RelatedPurchaseOrderId,

    -- Flags for easy filtering in Power BI
    (we.PARTY_ID_CONTRACTOR = 'SITE')                AS IsInternalIssueToSite,
    (we.CERTIFICATE_CATEGORY = 'SUPPLY_PROCUREMENT_CERTIFICATE')     AS IsSupplyProcurement,
    (we.CERTIFICATE_CATEGORY = 'WORKMANSHIP_CONTRACTING_CERTIFICATE') AS IsWorkmanship,
    (we.CERTIFICATE_CATEGORY = 'COMPANY_SUPPLY_SALE_CERTIFICATE')    AS IsCompanySale

FROM WORK_EFFORT we
         LEFT JOIN WORK_EFFORT proj_we
                   ON we.PROJECT_ID = proj_we.WORK_EFFORT_ID
                       AND proj_we.WORK_EFFORT_TYPE_ID = 'PROJECT'
         LEFT JOIN FACILITY proj_fac ON proj_we.FACILITY_ID = proj_fac.FACILITY_ID
         LEFT JOIN FACILITY fac ON we.FACILITY_ID = fac.FACILITY_ID
         LEFT JOIN PARTY supp ON we.PARTY_ID_SUPPLIER = supp.PARTY_ID
         LEFT JOIN PARTY contr ON we.PARTY_ID_CONTRACTOR = contr.PARTY_ID
WHERE we.WORK_EFFORT_TYPE_ID = 'PROJECT_CERTIFICATE'
  AND we.CERTIFICATE_NUMBER IS NOT NULL;  -- Safety: only real certificates


-- =============================================================
-- 2. CERTIFICATE ITEMS – Linked via CertificateNumber too
-- =============================================================
DROP VIEW IF EXISTS ProjectCertificateItems;
CREATE OR REPLACE VIEW ProjectCertificateItems AS
SELECT
    cert.CertificateNumber,
    item.WORK_EFFORT_ID                              AS ItemId,
    item.WORK_EFFORT_PARENT_ID                       AS CertificateId,

    item.PRODUCT_ID                                  AS ProductId,
    prd.PRODUCT_NAME                                 AS ProductName,

    item.QUANTITY                                    AS Quantity,
    item.QuantityUomId                               AS UomId,
    uom.DESCRIPTION                                  AS UomName,

    item.RATE                                        AS UnitPrice,
    item.MATERIAL_PRICE                              AS MaterialPrice,
    item.LABOR_PRICE                                 AS LaborPrice,
    (COALESCE(item.MATERIAL_PRICE,0) + COALESCE(item.LABOR_PRICE,0)) AS UnitPriceWorkmanship,

    item.TOTAL_AMOUNT                                AS GrossAmount,
    item.DISCOUNT                                    AS DiscountAmount,
    item.TransportationExpenses                      AS TransportationExpenses,
    item.Gratuities                                  AS Gratuities,
    item.AchievementPercent                          AS AchievementPercentage,
    item.DEDUCTIONS                                  AS Deductions,
    item.INSURANCE                                   AS InsuranceAmount,
    item.ADDITIONAL_INSURANCE                        AS AdditionalInsuranceAmount,

    -- Final Net (matches your frontend logic exactly)
    item.TOTAL_AMOUNT
        - COALESCE(item.DISCOUNT,0)
        - COALESCE(item.DEDUCTIONS,0)
        - COALESCE(item.INSURANCE,0)
        - COALESCE(item.ADDITIONAL_INSURANCE,0)
        + COALESCE(item.TransportationExpenses,0)
        + COALESCE(item.Gratuities,0)                 AS NetAmount,

    item.DESCRIPTION                                 AS ItemDescription,
    item.DEDUCTION_DESCRIPTION                       AS DeductionDescription,
    item.ProcurementDate                             AS ProcurementDate

FROM WORK_EFFORT item
         JOIN ProjectCertificates cert
              ON item.WORK_EFFORT_PARENT_ID = cert.CertificateId
         LEFT JOIN PRODUCT prd ON item.PRODUCT_ID = prd.PRODUCT_ID
         LEFT JOIN UOM uom ON item.QuantityUomId = uom.UOM_ID
WHERE item.WORK_EFFORT_TYPE_ID = 'CERTIFICATE_ITEM';


-- =============================================================
-- 1. PAYMENT CERTIFICATES – Main header view (updated with correct joins)
-- =============================================================
DROP VIEW IF EXISTS PaymentCertificates;
CREATE OR REPLACE VIEW PaymentCertificates AS

SELECT
    we.WORK_EFFORT_ID                                       AS CertificateId,

    we.CURRENT_STATUS_ID                                    AS CurrentStatusId,
    CASE we.CURRENT_STATUS_ID
        WHEN 'WEPR_CREATED'   THEN 'Created'
        WHEN 'WEPR_APPROVED'  THEN 'Approved'
        WHEN 'WEPR_COMPLETE'  THEN 'Completed'
        ELSE 'Unknown'
        END                                                     AS StatusName,

    we.DESCRIPTION                                        AS Description,

    -- Dates
    we.ESTIMATED_START_DATE                                 AS CertificateDate

FROM WORK_EFFORT we

WHERE we.WORK_EFFORT_TYPE_ID = 'PAYMENT_CERTIFICATE';


-- =============================================================
-- 2. PAYMENT CERTIFICATE ITEMS – Enriched with names/descriptions
-- =============================================================
DROP VIEW IF EXISTS PaymentCertificateItems;
CREATE OR REPLACE VIEW PaymentCertificateItems AS

SELECT
    cert.CertificateId,

    item.WORK_EFFORT_ID                                     AS ItemId,
    item.WORK_EFFORT_PARENT_ID                              AS ParentCertificateId,

    item.COST_TYPE                                          AS CostTypeCode,

    -- REFACTOR: Both PRODUCT_ID and SERVICE_ID point to the same PRODUCT table
    COALESCE(item.PRODUCT_ID, item.SERVICE_ID)              AS ProductId,
    prod.PRODUCT_NAME                                       AS ProductName,

    item.DESCRIPTION                                        AS ItemDescription,

    -- Amounts
    item.TOTAL_AMOUNT                                       AS GrossAmount,
    item.AMOUNT                                             AS Amount,
    item.DISCOUNT                                           AS DiscountAmount,
    item.TransportationExpenses                              AS TransportationExpenses,
    item.Gratuities                                         AS Gratuities,

    -- REFACTOR: Net amount calculation (extend if DEDUCTIONS/INSURANCE are used later)
    (COALESCE(item.TOTAL_AMOUNT, 0)
         - COALESCE(item.DISCOUNT, 0)
        + COALESCE(item.TransportationExpenses, 0)
        + COALESCE(item.Gratuities, 0))                      AS NetAmount,

    -- Project info
    item.PROJECT_ID                                         AS ItemProjectId,
    proj_we.PROJECT_NAME                                AS ProjectName,             -- Name of the project (WORK_EFFORT record)

    -- Contractor
    item.PARTY_ID_CONTRACTOR                                AS ItemContractorPartyId,
    contractor_party.DESCRIPTION                              AS ContractorName,          -- Company name

    -- Supplier
    item.PARTY_ID_SUPPLIER                                  AS ItemSupplierPartyId,
    supplier_party.DESCRIPTION                                AS SupplierName,            -- Company name

    -- Status (optional but useful)
    item.CURRENT_STATUS_ID                                  AS StatusId,
    status_item.DESCRIPTION                                 AS StatusDescription

FROM WORK_EFFORT item
         JOIN PaymentCertificates cert
              ON item.WORK_EFFORT_PARENT_ID = cert.CertificateId

    -- Product / Service (single table)
         LEFT JOIN PRODUCT prod
                   ON COALESCE(item.PRODUCT_ID, item.SERVICE_ID) = prod.PRODUCT_ID

    -- Project name (the project itself is also a WORK_EFFORT)
         LEFT JOIN WORK_EFFORT proj_we
                   ON item.PROJECT_ID = proj_we.WORK_EFFORT_ID

    -- Contractor name (Party → either a company or a person)
         LEFT JOIN PARTY contractor_party ON item.PARTY_ID_CONTRACTOR = contractor_party.PARTY_ID
         LEFT JOIN PARTY_GROUP contractor_group ON contractor_party.PARTY_ID = contractor_group.PARTY_ID
         LEFT JOIN PERSON contractor_person ON contractor_party.PARTY_ID = contractor_person.PARTY_ID

    -- Supplier name (same logic)
         LEFT JOIN PARTY supplier_party ON item.PARTY_ID_SUPPLIER = supplier_party.PARTY_ID

    -- Status description
         LEFT JOIN STATUS_ITEM status_item
                   ON item.CURRENT_STATUS_ID = status_item.STATUS_ID

WHERE item.WORK_EFFORT_TYPE_ID = 'PAYMENT_CERTIFICATE_ITEM';
-- =============================================================
-- PAYMENT CERTIFICATES WITH ITEMS – Header + flattened line items
-- =============================================================
DROP VIEW IF EXISTS PaymentCertificatesWithItems;
CREATE OR REPLACE VIEW PaymentCertificatesWithItems AS

SELECT
    c.CertificateId,
    c.StatusName,
    c.CertificateDate,
    c.Description,

    -- Item-level columns (NULL when no items / header row)
    i.ItemId,
    i.CostTypeCode,
    i.ProductId,
    i.ProductName,
    i.ItemDescription,
    i.GrossAmount,
    i.Amount,
    i.DiscountAmount,
    i.TransportationExpenses,
    i.Gratuities,
    i.NetAmount,
    i.ItemProjectId,
    i.ProjectName               AS ItemProjectName,     -- Project name at item level (may override header)
    i.ItemContractorPartyId,
    i.ContractorName            AS ItemContractorName,  -- Contractor name at item level
    i.ItemSupplierPartyId,
    i.SupplierName              AS ItemSupplierName,    -- Supplier name at item level
    i.StatusId,
    i.StatusDescription

FROM PaymentCertificates c
         LEFT JOIN PaymentCertificateItems i
                   ON c.CertificateId = i.ParentCertificateId;
-- =============================================================
-- BILLING_ACCOUNTS – Enriched active billing accounts view
-- Joins resolve foreign keys for project, party (role), and currency names
-- Computed used/remaining balance from related payments (active accounts only)
-- =============================================================
DROP VIEW IF EXISTS BillingAccounts;
CREATE OR REPLACE VIEW BillingAccounts AS

SELECT
    -- =================================================================
    -- Core Keys (keep for relationships)
    -- =================================================================
    ba.BILLING_ACCOUNT_ID                                   AS BillingAccountId,

    -- =================================================================
    -- Account Limits & Balances
    -- =================================================================
    ba.ACCOUNT_LIMIT                                        AS AccountLimit,
    COALESCE(
            (
                SELECT SUM(p.AMOUNT)
                FROM PAYMENT p
                WHERE p.WORK_EFFORT_ID = ba.WORK_EFFORT_ID
                  AND p.PARTY_ID_TO = bar.PARTY_ID
                  AND p.STATUS_ID = 'PMNT_SENT'
                  AND p.PAYMENT_TYPE_ID = 'ADVANCE_TO_VENDOR_CONTRACTOR'
            ), 0
        )                                                       AS UsedBalance,             -- Sum of qualifying advance payments

    -- REFACTOR: Computed as AccountLimit minus UsedBalance (handles NULL AccountLimit as 0)
    (COALESCE(ba.ACCOUNT_LIMIT, 0) - COALESCE(
            (
                SELECT SUM(p.AMOUNT)
                FROM PAYMENT p
                WHERE p.WORK_EFFORT_ID = ba.WORK_EFFORT_ID
                  AND p.PARTY_ID_TO = bar.PARTY_ID
                  AND p.STATUS_ID = 'PMNT_SENT'
                  AND p.PAYMENT_TYPE_ID = 'ADVANCE_TO_VENDOR_CONTRACTOR'
            ), 0
        ))                                                      AS RemainingBalance,

    -- =================================================================
    -- Project (Work Effort)
    -- =================================================================
    ba.WORK_EFFORT_ID                                       AS ProjectId,
    we.PROJECT_NAME                                         AS ProjectName,             -- e.g. "مشروع 1234"

    -- =================================================================
    -- Currency
    -- =================================================================
    ba.ACCOUNT_CURRENCY_UOM_ID                              AS AccountCurrencyUomId,
    uom.DESCRIPTION                                         AS AccountCurrencyUomDescription,

    -- =================================================================
    -- Party (via BillingAccountRole)
    -- =================================================================
    bar.PARTY_ID                                            AS PartyId,
    pty.DESCRIPTION                                         AS PartyName,               -- e.g. "المورد أحمد", "عميل 123"

    -- =================================================================
    -- Dates
    -- =================================================================
    ba.FROM_DATE                                            AS FromDate,
    ba.THRU_DATE                                            AS ThruDate               -- NULL for active accounts (filtered below)

    -- =================================================================
    -- Additional raw columns (for completeness, add more if needed)
    -- =================================================================
    -- ba.OTHER_COLUMN                                       AS OtherColumnExample

FROM BILLING_ACCOUNT ba

-- Join to roles (assuming one active role per account; adjust if multiple possible)
         INNER JOIN BILLING_ACCOUNT_ROLE bar
                    ON ba.BILLING_ACCOUNT_ID = bar.BILLING_ACCOUNT_ID

-- Party from role
         INNER JOIN PARTY pty
                    ON bar.PARTY_ID = pty.PARTY_ID

-- Currency UoM
         INNER JOIN UOM uom
                    ON ba.ACCOUNT_CURRENCY_UOM_ID = uom.UOM_ID

-- Project (Work Effort)
         INNER JOIN WORK_EFFORT we
                    ON ba.WORK_EFFORT_ID = we.WORK_EFFORT_ID

-- Filter for active billing accounts only (ThruDate IS NULL)
WHERE ba.THRU_DATE IS NULL

ORDER BY ba.BILLING_ACCOUNT_ID;

-- =============================================================

-- =============================================================
-- ACCOUNTING_TRANSACTION_ENTRIES – Enriched view with names and multilingual support
-- Resolves foreign keys for transaction, GL account (with Arabic/English names), party, project
-- Optional company filter handled in application layer (via OData), but view includes organization link
-- =============================================================
DROP VIEW IF EXISTS AcctgTransactions;

CREATE OR REPLACE VIEW AcctgTransactions AS
SELECT
    -- Core identifiers
    t.ACCTG_TRANS_ID,
    t.ACCTG_TRANS_TYPE_ID,

    -- Important references
    t.INVOICE_ID,
    t.PAYMENT_ID,
    t.WORK_EFFORT_ID,
    t.FIN_ACCOUNT_TRANS_ID,
    t.SHIPMENT_ID,
    t.RECEIPT_ID,

    -- Parties & organization
    t.PARTY_ID,

    -- Dates & status
    t.TRANSACTION_DATE,
    t.IS_POSTED,
    t.POSTED_DATE,
    t.SCHEDULED_POSTING_DATE,

    -- Fiscal & classification
    t.GL_FISCAL_TYPE_ID,
    t.GL_JOURNAL_ID,

    -- Description & references
    t.DESCRIPTION,
    t.VOUCHER_REF,
    t.VOUCHER_DATE,

    -- Audit fields (useful for debugging & incremental refresh)
    t.CREATED_DATE,
    t.CREATED_BY_USER_LOGIN,
    t.LAST_MODIFIED_DATE,
    t.LAST_MODIFIED_BY_USER_LOGIN,

    -- Less frequently used but sometimes needed
    t.THEIR_ACCTG_TRANS_ID,
    t.GROUP_STATUS_ID,
    t.FIXED_ASSET_ID,
    t.INVENTORY_ITEM_ID,
    t.PHYSICAL_INVENTORY_ID

FROM ACCTG_TRANS t;
-- =============================================================
DROP VIEW IF EXISTS AcctgTransEntries;

CREATE OR REPLACE VIEW AcctgTransEntries AS
SELECT
    -- Keys
    te.ACCTG_TRANS_ID,
    te.ACCTG_TRANS_ENTRY_SEQ_ID,

    -- GL Account
    te.GL_ACCOUNT_ID,
    gla.ACCOUNT_NAME            AS GlAccountNameEnglish,
    gla.ACCOUNT_NAME_ARABIC     AS GlAccountNameArabic,

    -- Amounts & direction
    te.AMOUNT,
    te.DEBIT_CREDIT_FLAG,
    te.CURRENCY_UOM_ID,
    te.ORIG_AMOUNT,
    te.ORIG_CURRENCY_UOM_ID,

    -- Party (usually customer / supplier / employee)
    te.PARTY_ID,
    p.DESCRIPTION               AS PartyName,

    -- Product dimension (if used)
    te.PRODUCT_ID,

    -- Description
    te.DESCRIPTION              AS EntryDescription,

    -- Reconciliation & status
    te.RECONCILE_STATUS_ID,
    te.DUE_DATE,

    -- Audit / system fields
    te.CREATED_STAMP,
    te.LAST_UPDATED_STAMP

FROM ACCTG_TRANS_ENTRY te

-- Optional light joins (only the most useful ones – others in Power BI)
         LEFT JOIN GL_ACCOUNT gla
                   ON te.GL_ACCOUNT_ID = gla.GL_ACCOUNT_ID

         LEFT JOIN PARTY p
                   ON te.PARTY_ID = p.PARTY_ID;
-- =============================================================

DROP VIEW IF EXISTS SalesRequests;
CREATE OR REPLACE VIEW SalesRequests AS
SELECT
    -- =================================================================
    -- Core Keys
    -- =================================================================
    sr.SALES_REQUEST_ID AS SalesRequestId,
    sr.PRODUCT_ID AS ApartmentId,                     -- Product represents the apartment/unit

    -- =================================================================
    -- Apartment / Product Details
    -- =================================================================
    p.PRODUCT_NAME AS ApartmentName,
    p.BUILDING_NUMBER AS BuildingNumber,
    pt.DESCRIPTION AS ProductTypeDescriptionEnglish,
    pt.DESCRIPTION_ARABIC AS ProductTypeDescriptionArabic,

    -- REFACTOR: Bilingual product type description for reporting flexibility
    COALESCE(pt.DESCRIPTION_ARABIC, pt.DESCRIPTION) AS ProductTypeDescription,  -- Arabic-first fallback

    p.PROJECT_ID AS ProjectId,
    we.PROJECT_NAME AS ProjectName,                   -- Latest project name (assumes one active per ID)

    p.FLOOR_NUMBER AS FloorNumberRaw,                 -- Original numeric/string code e.g. "0", "1"

    -- REFACTOR: Human-readable Arabic floor name
    CASE p.FLOOR_NUMBER
        WHEN '0' THEN 'الطابق الأرضي'
        WHEN '1' THEN 'الطابق الأول'
        WHEN '2' THEN 'الطابق الثاني'
        WHEN '3' THEN 'الطابق الثالث'
        WHEN '4' THEN 'الطابق الرابع'
        WHEN '5' THEN 'الطابق الخامس'
        WHEN '6' THEN 'الطابق السادس'
        ELSE CONCAT('الطابق ', COALESCE(p.FLOOR_NUMBER, 'غير محدد'))
        END AS FloorNameArabic,

    p.APARTMENT_SPACE_M2 AS ApartmentSpaceM2,
    p.GARDEN_SPACE_M2 AS GardenSpaceM2,

    -- Apartment current status (e.g. Available, Reserved, Sold)
    p.APARTMENT_STATUS_ID AS ApartmentStatusId,
    ast.DESCRIPTION AS ApartmentStatusDescription,
    COALESCE(ast.DESCRIPTION_ARABIC, ast.DESCRIPTION) AS ApartmentStatusDescriptionArabic,

    -- =================================================================
    -- Parties
    -- =================================================================
    sr.FROM_PARTY_ID AS FromPartyId,
    c.DESCRIPTION AS FromPartyName,


    sr.EMPLOYEE_PARTY_ID AS EmployeePartyId,
    e.DESCRIPTION AS EmployeeName,

    -- =================================================================
    -- Pricing & Payment Terms
    -- =================================================================
    sr.APARTMENT_PRICE_PER_M2 AS ApartmentPricePerM2,
    sr.GARDEN_PRICE_PER_M2 AS GardenPricePerM2,
    sr.DISCOUNT AS Discount,
    sr.TOTAL_PRICE AS TotalPrice,
    sr.ADVANCE_PAYMENT AS AdvancePayment,
    sr.NUMBER_OF_INSTALLMENTS AS NumberOfInstallments,
    sr.DATE_OF_FIRST_INSTALLMENT AS DateOfFirstInstallment,
    sr.MONTHS_BETWEEN_INSTALLMENTS AS MonthsBetweenInstallments,
    sr.MAINTENANCE_DEPOSIT AS MaintenanceDeposit,

    -- =================================================================
    -- Sales Request Status
    -- =================================================================
    sr.STATUS_ID AS StatusId,
    srs.DESCRIPTION AS SalesRequestStatusDescriptionEnglish,
    COALESCE(srs.DESCRIPTION_ARABIC, srs.DESCRIPTION) AS SalesRequestStatusDescriptionArabic,

    -- REFACTOR: Default to Arabic description for primary status column (matches app behavior)
    COALESCE(srs.DESCRIPTION_ARABIC, srs.DESCRIPTION, sr.STATUS_ID) AS StatusDescription,

    -- =================================================================
    -- Dates & Comments
    -- =================================================================
    sr.SALE_DATE AS SaleDate,
    sr.COMMENTS AS Comments,
    sr.CREATED_STAMP AS CreatedStamp,
    sr.LAST_UPDATED_STAMP AS LastUpdatedStamp

FROM SALES_REQUEST sr

-- Core required joins
         INNER JOIN PRODUCT p ON sr.PRODUCT_ID = p.PRODUCT_ID
         INNER JOIN PRODUCT_TYPE pt ON p.PRODUCT_TYPE_ID = pt.PRODUCT_TYPE_ID

-- Apartment status (from Product)
         LEFT JOIN STATUS_ITEM ast
                   ON p.APARTMENT_STATUS_ID = ast.STATUS_ID
                       AND ast.STATUS_TYPE_ID = 'APARTMENT_STATUS'

-- Sales request status
         LEFT JOIN STATUS_ITEM srs
                   ON sr.STATUS_ID = srs.STATUS_ID
                       AND srs.STATUS_TYPE_ID = 'SALES_REQUEST_STATUS'  -- Adjust if your type ID differs

-- Parties
         LEFT JOIN PARTY c ON sr.FROM_PARTY_ID = c.PARTY_ID
         LEFT JOIN PARTY e ON sr.EMPLOYEE_PARTY_ID = e.PARTY_ID



-- Project name from WorkEffort (latest/active project)
         LEFT JOIN WORK_EFFORT we
                   ON p.PROJECT_ID = we.WORK_EFFORT_ID
                       AND we.WORK_EFFORT_TYPE_ID = 'PROJECT'

ORDER BY sr.CREATED_STAMP DESC, sr.SALES_REQUEST_ID DESC;
-- =============================================================
-- =============================================================
-- ReserveRequests – Enriched view for reserve requests (apartments)
-- Joins resolve product details, party names, product type descriptions,
-- status descriptions (both English and Arabic), and project name
-- Floor number translated to Arabic description where applicable
-- =============================================================

DROP VIEW IF EXISTS ReserveRequests;
CREATE OR REPLACE VIEW ReserveRequests AS

SELECT
    -- =================================================================
    -- Core Keys
    -- =================================================================
    rr.RESERVE_REQUEST_ID                                   AS ReserveRequestId,
    rr.PRODUCT_ID                                           AS ApartmentId,

    -- =================================================================
    -- Apartment Details
    -- =================================================================
    p.PRODUCT_NAME                                          AS ApartmentName,
    COALESCE(pt.DESCRIPTION_ARABIC, pt.DESCRIPTION)         AS ProductTypeDescriptionAr,  -- Preferred Arabic fallback
    pt.DESCRIPTION                                          AS ProductTypeDescriptionEn,

    -- REFACTOR: Correlated subquery to get the project name (assumes the latest/most relevant PROJECT_NAME for the project; uses MAX as simple approximation if multiple rows exist)
    (SELECT MAX(we.PROJECT_NAME)
     FROM WORK_EFFORT we
     WHERE we.WORK_EFFORT_ID = p.PROJECT_ID
       AND we.WORK_EFFORT_TYPE_ID = 'PROJECT')              AS ProjectName,

    -- REFACTOR: CASE for Arabic floor descriptions (hardcoded map matching the original code; falls back to raw value if no match or NULL)
    CASE p.FLOOR_NUMBER
        WHEN '0' THEN 'الطابق الأرضي'
        WHEN '1' THEN 'الطابق الأول'
        WHEN '2' THEN 'الطابق الثاني'
        WHEN '3' THEN 'الطابق الثالث'
        WHEN '4' THEN 'الطابق الرابع'
        WHEN '5' THEN 'الطابق الخامس'
        WHEN '6' THEN 'الطابق السادس'
        ELSE COALESCE(p.FLOOR_NUMBER, '')
        END                                                     AS FloorNumber,

    COALESCE(p.APARTMENT_SPACE_M2, 0)                       AS ApartmentSpaceM2,

    -- =================================================================
    -- Parties (Customer & Employee)
    -- =================================================================
    rr.FROM_PARTY_ID                                        AS FromPartyId,
    COALESCE(c.DESCRIPTION, '')                             AS FromPartyName,

    rr.EMPLOYEE_PARTY_ID                                    AS EmployeePartyId,
    COALESCE(e.DESCRIPTION, '')                             AS EmployeeName,

    -- =================================================================
    -- Reserve Request Details
    -- =================================================================
    rr.RESERVE_DATE                                         AS ReserveDate,
    rr.RESERVE_AMOUNT                                       AS ReserveAmount,
    rr.PAY_METHOD                                           AS PayMethod,
    rr.COMMENTS                                             AS Comments,

    -- =================================================================
    -- Status
    -- =================================================================
    COALESCE(rr.STATUS_ID, '')                              AS StatusId,
    si.DESCRIPTION                                          AS StatusDescriptionEn,
    COALESCE(si.DESCRIPTION_ARABIC, si.DESCRIPTION, rr.STATUS_ID) AS StatusDescriptionAr,

    -- =================================================================
    -- Timestamps
    -- =================================================================
    rr.CREATED_STAMP                                        AS CreatedStamp,
    rr.LAST_UPDATED_STAMP                                   AS LastUpdatedStamp

FROM RESERVE_REQUEST rr

-- Product (apartment)
         INNER JOIN PRODUCT p
                    ON rr.PRODUCT_ID = p.PRODUCT_ID

-- Product Type (for descriptions)
         INNER JOIN PRODUCT_TYPE pt
                    ON p.PRODUCT_TYPE_ID = pt.PRODUCT_TYPE_ID

-- Customer Party (left join – may be null)
         LEFT JOIN PARTY c
                   ON rr.FROM_PARTY_ID = c.PARTY_ID

-- Employee Party (left join – may be null)
         LEFT JOIN PARTY e
                   ON rr.EMPLOYEE_PARTY_ID = e.PARTY_ID

-- Status Item (left join – fallback if no matching status)
         LEFT JOIN STATUS_ITEM si
                   ON rr.STATUS_ID = si.STATUS_ID
                       AND si.STATUS_TYPE_ID = 'RESERVE_REQUEST_STATUS'  -- Adjust if the type ID differs

ORDER BY rr.RESERVE_REQUEST_ID;
-- =============================================================
DROP VIEW IF EXISTS ProjectCertificates_2;

CREATE OR REPLACE VIEW ProjectCertificates_2 AS
SELECT
    cert.CertificateNumber,
    cert.CertificateId,
    cert.CertificateType,
    cert.ProjectId,
    cert.ProjectName,
    cert.PartyName AS SupplierOrContractorName,
    cert.RelatedPurchaseOrderId,

    -- Payment details (linked via Order → OrderPaymentPreference)
    p.PaymentId,
    p.Amount,
    p.ActualAmount,
    p.EffectiveDate,
    p.DueStatusArabic,
    p.PaymentTypeDescriptionArabic,
    p.PaymentMethodTypeNameArabic,
    p.PaymentMethodName,
    p.StatusNameArabic,
    p.IsDisbursement,
    p.PartyNameFrom,
    p.PartyNameTo,
    p.CostCenterName,
    p.Comments,

    -- Order-level info (for verification)
    ord.ORDER_ID AS OrderId,
    ord.ORDER_TYPE_ID,
    ord.STATUS_ID AS OrderStatusId,
    ord.ORDER_DATE,

    -- How the payment is linked
    CASE
        WHEN p.PaymentId IS NOT NULL THEN 'Via Order Payment Preference'
        ELSE 'No Payment Linked'
        END AS PaymentLinkType,

    -- Helpful flags
    CASE WHEN p.PaymentId IS NOT NULL THEN 'Has Payment'
         ELSE 'No Payment'
        END AS PaymentStatus

FROM ProjectCertificates cert

-- Link certificate to its related Purchase Order
         LEFT JOIN ORDER_HEADER ord
                   ON cert.RelatedPurchaseOrderId = ord.ORDER_ID

-- Link order to its payment preferences
         LEFT JOIN ORDER_PAYMENT_PREFERENCE opp
                   ON ord.ORDER_ID = opp.ORDER_ID

-- Link to actual payments via the payment preference
         LEFT JOIN Payments p
                   ON opp.ORDER_PAYMENT_PREFERENCE_ID = p.PaymentPreferenceId

ORDER BY cert.CertificateNumber, p.EffectiveDate DESC;

-- =============================================================
-- 1. PROJECT CERTIFICATES - Improved Version
-- =============================================================
DROP VIEW IF EXISTS ProjectCertificatesV2;

CREATE OR REPLACE VIEW ProjectCertificatesV2 AS
SELECT
    we.CERTIFICATE_NUMBER                              AS CertificateNumber,      -- Primary business key
    we.WORK_EFFORT_ID                                  AS CertificateId,

    we.CURRENT_STATUS_ID                               AS CurrentStatusId,
    CASE we.CURRENT_STATUS_ID
        WHEN 'WEPR_CREATED'   THEN 'Created'
        WHEN 'WEPR_APPROVED'  THEN 'Approved'
        WHEN 'WEPR_COMPLETE'  THEN 'Completed'
        ELSE 'Unknown'
        END                                                AS StatusName,

    we.CERTIFICATE_CATEGORY                            AS CertificateTypeCode,
    CASE we.CERTIFICATE_CATEGORY
        WHEN 'SUPPLY_PROCUREMENT_CERTIFICATE'      THEN 'Supply Procurement'
        WHEN 'WORKMANSHIP_CONTRACTING_CERTIFICATE' THEN 'Workmanship Contracting'
        WHEN 'COMPANY_SUPPLY_SALE_CERTIFICATE'     THEN 'Company Supply Sale'
        ELSE we.CERTIFICATE_CATEGORY
        END                                                AS CertificateType,

    we.DESCRIPTION                                     AS CertificateDescription,
    we.ESTIMATED_START_DATE                            AS PeriodStartDate,
    we.ESTIMATED_COMPLETION_DATE                       AS PeriodEndDate,

    we.CREATED_DATE                                    AS CreatedDate,
    we.LAST_UPDATED_STAMP                              AS LastUpdatedDate,

    -- Project
    proj_we.WORK_EFFORT_ID                             AS ProjectId,
    proj_we.PROJECT_NAME                               AS ProjectName,
    proj_fac.FACILITY_NAME                             AS ProjectFacilityName,

    -- Party (Supplier or Contractor)
    COALESCE(we.PARTY_ID_SUPPLIER, we.PARTY_ID_CONTRACTOR) AS PartyId,
    COALESCE(supp.DESCRIPTION, contr.DESCRIPTION)          AS PartyName,

    we.PARTY_ID_SUPPLIER                               AS SupplierPartyId,
    supp.DESCRIPTION                                   AS SupplierName,
    we.PARTY_ID_CONTRACTOR                             AS ContractorPartyId,
    contr.DESCRIPTION                                  AS ContractorName,

    -- Facility
    we.FACILITY_ID                                     AS FacilityId,
    fac.FACILITY_NAME                                  AS FacilityName,

    -- Related PO
    we.RELATED_ORDER_ID                                AS RelatedPurchaseOrderId,

    -- Helpful Flags
    (we.PARTY_ID_CONTRACTOR = 'SITE')                  AS IsInternalIssueToSite,
    (we.CERTIFICATE_CATEGORY = 'SUPPLY_PROCUREMENT_CERTIFICATE')      AS IsSupplyProcurement,
    (we.CERTIFICATE_CATEGORY = 'WORKMANSHIP_CONTRACTING_CERTIFICATE') AS IsWorkmanship,
    (we.CERTIFICATE_CATEGORY = 'COMPANY_SUPPLY_SALE_CERTIFICATE')    AS IsCompanySale,

    -- New: Certificate Type as clean English + Arabic (good for visuals)
    CASE
        WHEN we.CERTIFICATE_CATEGORY = 'SUPPLY_PROCUREMENT_CERTIFICATE'
            THEN 'توريد مواد'
        WHEN we.CERTIFICATE_CATEGORY = 'WORKMANSHIP_CONTRACTING_CERTIFICATE'
            THEN 'أعمال مقاولات'
        ELSE 'غير محدد'
        END                                                AS CertificateTypeArabic

FROM WORK_EFFORT we
         LEFT JOIN WORK_EFFORT proj_we
                   ON we.PROJECT_ID = proj_we.WORK_EFFORT_ID
                       AND proj_we.WORK_EFFORT_TYPE_ID = 'PROJECT'
         LEFT JOIN FACILITY proj_fac
                   ON proj_we.FACILITY_ID = proj_fac.FACILITY_ID
         LEFT JOIN FACILITY fac
                   ON we.FACILITY_ID = fac.FACILITY_ID
         LEFT JOIN PARTY supp
                   ON we.PARTY_ID_SUPPLIER = supp.PARTY_ID
         LEFT JOIN PARTY contr
                   ON we.PARTY_ID_CONTRACTOR = contr.PARTY_ID

WHERE we.WORK_EFFORT_TYPE_ID = 'PROJECT_CERTIFICATE'
  AND we.CERTIFICATE_NUMBER IS NOT NULL;


-- =============================================================
-- 2. PROJECT CERTIFICATE ITEMS - Improved Version
-- =============================================================
DROP VIEW IF EXISTS ProjectCertificateItemsV2;

CREATE OR REPLACE VIEW ProjectCertificateItemsV2 AS
SELECT
    cert.CertificateNumber,
    item.WORK_EFFORT_ID                                AS ItemId,
    item.WORK_EFFORT_PARENT_ID                         AS CertificateId,

    item.PRODUCT_ID                                    AS ProductId,
    prd.PRODUCT_NAME                                   AS ProductName,

    item.QUANTITY                                      AS Quantity,
    item.QuantityUomId                                 AS UomId,
    uom.DESCRIPTION                                    AS UomName,

    item.RATE                                          AS UnitPrice,
    item.MATERIAL_PRICE                                AS MaterialPrice,
    item.LABOR_PRICE                                   AS LaborPrice,

    -- Improved: Clear separation between Material & Labor
    COALESCE(item.MATERIAL_PRICE, 0)                   AS MaterialAmount,
    COALESCE(item.LABOR_PRICE, 0)                      AS LaborAmount,

    (COALESCE(item.MATERIAL_PRICE,0) + COALESCE(item.LABOR_PRICE,0)) AS UnitPriceWorkmanship,

    item.TOTAL_AMOUNT                                  AS GrossAmount,
    item.DISCOUNT                                      AS DiscountAmount,
    item.TransportationExpenses                        AS TransportationExpenses,
    item.Gratuities                                    AS Gratuities,

    item.AchievementPercent                            AS AchievementPercentage,

    item.DEDUCTIONS                                    AS Deductions,
    item.INSURANCE                                     AS InsuranceAmount,
    item.ADDITIONAL_INSURANCE                          AS AdditionalInsuranceAmount,

    -- Improved Net Amount Calculation (more robust)
    COALESCE(item.TOTAL_AMOUNT, 0)
        - COALESCE(item.DISCOUNT, 0)
        - COALESCE(item.DEDUCTIONS, 0)
        - COALESCE(item.INSURANCE, 0)
        - COALESCE(item.ADDITIONAL_INSURANCE, 0)
        + COALESCE(item.TransportationExpenses, 0)
        + COALESCE(item.Gratuities, 0)                 AS NetAmount,

    -- New: Paid vs Certified logic ready for completion %
    cert.CertificateType                               AS CertificateType,
    cert.CertificateTypeArabic                         AS CertificateTypeArabic,
    cert.ProjectName,
    cert.PartyName,
    cert.SupplierName,
    cert.ContractorName,

    item.DESCRIPTION                                   AS ItemDescription,
    item.DEDUCTION_DESCRIPTION                         AS DeductionDescription,
    item.ProcurementDate                               AS ProcurementDate,

    -- New: Simple Completion Flag
    CASE
        WHEN COALESCE(item.AchievementPercent, 0) >= 100 THEN 'Completed'
        WHEN COALESCE(item.AchievementPercent, 0) > 0    THEN 'In Progress'
        ELSE 'Not Started'
        END                                                AS ItemCompletionStatus

FROM WORK_EFFORT item
         INNER JOIN ProjectCertificatesV2 cert
                    ON item.WORK_EFFORT_PARENT_ID = cert.CertificateId

         LEFT JOIN PRODUCT prd
                   ON item.PRODUCT_ID = prd.PRODUCT_ID
         LEFT JOIN UOM uom
                   ON item.QuantityUomId = uom.UOM_ID

WHERE item.WORK_EFFORT_TYPE_ID = 'CERTIFICATE_ITEM';

DROP VIEW IF EXISTS Fact_Expenses;

CREATE OR REPLACE VIEW Fact_Expenses AS
SELECT
    -- ==================== KEYS ====================
    item.WORK_EFFORT_ID                          AS ExpenseItemKey,
    header.WORK_EFFORT_ID                        AS CertificateKey,
    cert.CertificateNumber                       AS CertificateNumber,

    -- Dimension Foreign Keys
    COALESCE(proj.ProjectId, header.PROJECT_ID)  AS ProjectId,                 -- → DimProjects
    COALESCE(header.PARTY_ID_SUPPLIER, header.PARTY_ID_CONTRACTOR) AS PartyId, -- → DimSuppliers or DimContractors

    CASE
        WHEN header.PARTY_ID_SUPPLIER IS NOT NULL THEN 'Supplier'
        WHEN header.PARTY_ID_CONTRACTOR IS NOT NULL THEN 'Contractor'
        ELSE 'Unknown'
        END                                          AS PartyRole,

    item.PRODUCT_ID                              AS ProductId,                  -- → DimProductRawMaterials / DimProductServices
    item.SERVICE_ID                              AS ServiceId,

    -- Date Keys for Dim_Date
    DATE(COALESCE(item.ProcurementDate, header.LAST_STATUS_UPDATE, header.CREATED_DATE)) AS ExpenseDateKey,
    DATE(header.LAST_STATUS_UPDATE)              AS ApprovalDateKey,
    DATE(header.CREATED_DATE)                    AS CertificateCreatedDateKey,

    -- ==================== ATTRIBUTES ====================
    -- Record Type (very useful for filtering/slicing)
    CASE
        WHEN header.CERTIFICATE_CATEGORY IN ('SUPPLY_PROCUREMENT_CERTIFICATE', 'WORKMANSHIP_CONTRACTING_CERTIFICATE', 'COMPANY_SUPPLY_SALE_CERTIFICATE')
            THEN 'ProjectCertificate'
        ELSE 'MultiPaymentCertificate'
        END                                          AS RecordType,

    cert.CertificateType                         AS CertificateType,
    cert.CertificateTypeArabic                   AS CertificateTypeArabic,
    header.CERTIFICATE_CATEGORY                  AS CertificateCategoryCode,

    header.DESCRIPTION                           AS CertificateDescription,
    item.DESCRIPTION                             AS ItemDescription,
    item.DEDUCTION_DESCRIPTION                   AS DeductionDescription,

    header.RELATED_ORDER_ID                      AS RelatedPurchaseOrderId,
    header.CURRENT_STATUS_ID                     AS StatusId,                   -- Always 'WEPR_APPROVED'
    cert.StatusName                              AS StatusName,                 -- 'Approved'

    -- Classification Flags
    cert.IsSupplyProcurement,
    cert.IsWorkmanship,
    cert.IsCompanySale,
    (CASE
         WHEN header.CERTIFICATE_CATEGORY IN ('SUPPLY_PROCUREMENT_CERTIFICATE', 'WORKMANSHIP_CONTRACTING_CERTIFICATE', 'COMPANY_SUPPLY_SALE_CERTIFICATE')
             THEN FALSE
         ELSE TRUE
        END)                                        AS IsMultiPaymentCertificate,

    -- ==================== MEASURES ====================
    item.QUANTITY,
    item.RATE                                    AS UnitRate,

    COALESCE(item.MATERIAL_PRICE, 0)             AS MaterialAmount,
    COALESCE(item.LABOR_PRICE, 0)                AS LaborAmount,

    item.TOTAL_AMOUNT                            AS GrossAmount,
    COALESCE(item.DISCOUNT, 0)                   AS DiscountAmount,
    COALESCE(item.Deductions, 0)                 AS DeductionsAmount,
    COALESCE(item.Insurance, 0)                  AS InsuranceAmount,
    COALESCE(item.ADDITIONAL_INSURANCE, 0)        AS AdditionalInsuranceAmount,
    COALESCE(item.TransportationExpenses, 0)     AS TransportationExpensesAmount,
    COALESCE(item.Gratuities, 0)                 AS GratuitiesAmount,

    -- Core Net Expense Measure
    COALESCE(item.TOTAL_AMOUNT, 0)
        - COALESCE(item.DISCOUNT, 0)
        - COALESCE(item.Deductions, 0)
        - COALESCE(item.Insurance, 0)
        - COALESCE(item.ADDITIONAL_INSURANCE, 0)
        + COALESCE(item.TransportationExpenses, 0)
        + COALESCE(item.Gratuities, 0)             AS NetCertifiedAmount,

    item.AchievementPercent                      AS AchievementPercentage,

    -- Useful Flags
    1                                            AS IsApproved,

    CASE
        WHEN item.AchievementPercent >= 100 THEN 'Completed'
        WHEN item.AchievementPercent > 0    THEN 'In Progress'
        ELSE 'Not Started'
        END                                          AS ItemCompletionStatus,

    -- For Power BI Incremental Refresh
    GREATEST(COALESCE(header.LAST_UPDATED_STAMP, '1900-01-01'),
             COALESCE(item.LAST_UPDATED_STAMP, '1900-01-01')) AS LastUpdatedStamp

FROM WORK_EFFORT header
         INNER JOIN WORK_EFFORT item
                    ON item.WORK_EFFORT_PARENT_ID = header.WORK_EFFORT_ID
                        AND item.WORK_EFFORT_TYPE_ID = 'CERTIFICATE_ITEM'

         LEFT JOIN ProjectCertificatesV2 cert
                   ON header.WORK_EFFORT_ID = cert.CertificateId

         LEFT JOIN DimProjects proj
                   ON COALESCE(header.PROJECT_ID, header.WORK_EFFORT_ID) = proj.ProjectId

WHERE header.WORK_EFFORT_TYPE_ID = 'PROJECT_CERTIFICATE'
  AND header.CURRENT_STATUS_ID = 'WEPR_APPROVED';          -- Only Approved records = Actual Expenses

DROP VIEW IF EXISTS Fact_Projects_Expenses;

CREATE OR REPLACE VIEW Fact_Projects_Expenses AS
SELECT
    -- Keys
    item.WORK_EFFORT_ID                                                                    AS ExpenseItemKey,
    header.WORK_EFFORT_ID                                                                  AS CertificateKey,

    CASE
        WHEN header.WORK_EFFORT_TYPE_ID = 'PROJECT_CERTIFICATE'
            THEN header.CERTIFICATE_NUMBER
        ELSE NULL
        END                                                                                AS CertificateNumber,

    -- ==================== PROJECT ID LOGIC (Fixed) ====================
    COALESCE(
            header.PROJECT_ID, -- First: from header (for normal Project Certificates)
            item.PROJECT_ID -- Second: from item (for Multi-payment certificates)
    )                                                                                      AS ProjectId,

    -- Dimensions
    COALESCE(header.PARTY_ID_SUPPLIER, header.PARTY_ID_CONTRACTOR, header.PartyIdEmployee) AS PartyId,

    CASE
        WHEN header.PARTY_ID_SUPPLIER IS NOT NULL THEN 'Supplier'
        WHEN header.PARTY_ID_CONTRACTOR IS NOT NULL THEN 'Contractor'
        WHEN header.PartyIdEmployee IS NOT NULL THEN 'Employee'
        ELSE 'Unknown'
        END                                                                                AS PartyRole,

    COALESCE(item.PRODUCT_ID, item.SERVICE_ID)                                             AS ProductId,

    -- Dates
    DATE(COALESCE(
            item.ProcurementDate, -- Best for normal certificate items
            item.ESTIMATED_START_DATE, -- Used in multi-payment items
            header.ESTIMATED_START_DATE, -- Certificate level date
            header.CREATED_DATE -- Final fallback
         ))                                                                                AS ExpenseDateKey,

    -- Record Type & Classification
    CASE
        WHEN header.WORK_EFFORT_TYPE_ID = 'PROJECT_CERTIFICATE'
            AND header.CERTIFICATE_CATEGORY IN ('SUPPLY_PROCUREMENT_CERTIFICATE', 'WORKMANSHIP_CONTRACTING_CERTIFICATE')
            THEN 'ProjectCertificate'
        WHEN header.WORK_EFFORT_TYPE_ID = 'PAYMENT_CERTIFICATE'
            THEN 'MultiPaymentCertificate'
        ELSE 'Other'
        END                                                                                AS RecordType,

    CASE
        WHEN header.WORK_EFFORT_TYPE_ID = 'PROJECT_CERTIFICATE' THEN
            CASE header.CERTIFICATE_CATEGORY
                WHEN 'SUPPLY_PROCUREMENT_CERTIFICATE' THEN 'Supply Procurement'
                WHEN 'WORKMANSHIP_CONTRACTING_CERTIFICATE' THEN 'Workmanship Contracting'
                ELSE 'Project Certificate'
                END
        WHEN header.WORK_EFFORT_TYPE_ID = 'PAYMENT_CERTIFICATE' THEN 'Multi-Payment / Direct Expense'
        ELSE 'Unknown'
        END                                                                                AS CertificateType,

    header.CERTIFICATE_CATEGORY                                                            AS CertificateCategoryCode,
    header.DESCRIPTION                                                                     AS CertificateDescription,
    item.DESCRIPTION                                                                       AS ItemDescription,

    header.RELATED_ORDER_ID                                                                AS RelatedPurchaseOrderId,

    -- Flags
    (header.CERTIFICATE_CATEGORY = 'SUPPLY_PROCUREMENT_CERTIFICATE')                       AS IsSupplyProcurement,
    (header.CERTIFICATE_CATEGORY = 'WORKMANSHIP_CONTRACTING_CERTIFICATE')                  AS IsWorkmanship,
    (header.WORK_EFFORT_TYPE_ID = 'PAYMENT_CERTIFICATE')                                   AS IsMultiPaymentCertificate,
    1                                                                                      AS IsApproved,

    -- ==================== MEASURES ====================
    COALESCE(item.QUANTITY, 1)                                                             AS Quantity,
    item.RATE                                                                              AS UnitRate,


    COALESCE(item.TOTAL_AMOUNT, item.AMOUNT, 0)                                            AS GrossAmount,

    COALESCE(item.DISCOUNT, 0)                                                             AS DiscountAmount,
    COALESCE(item.Deductions, 0)                                                           AS DeductionsAmount,
    COALESCE(item.Insurance, 0)                                                            AS InsuranceAmount,
    COALESCE(item.TransportationExpenses, 0)                                               AS TransportationExpensesAmount,
    COALESCE(item.Gratuities, 0)                                                           AS GratuitiesAmount,

    -- Main Expense Measure
    COALESCE(item.TOTAL_AMOUNT, item.AMOUNT, 0)
        - COALESCE(item.DISCOUNT, 0)
        - COALESCE(item.Deductions, 0)
        - COALESCE(item.Insurance, 0)
        + COALESCE(item.TransportationExpenses, 0)
        + COALESCE(item.Gratuities, 0)                                                     AS NetCertifiedAmount,

    CASE
        -- Workmanship keeps the actual percentage
        WHEN header.CERTIFICATE_CATEGORY = 'WORKMANSHIP_CONTRACTING_CERTIFICATE'
            THEN COALESCE(item.AchievementPercent, 0)

        -- Supply Procurement and Multi-Payment are treated as 100% completed
        WHEN header.CERTIFICATE_CATEGORY = 'SUPPLY_PROCUREMENT_CERTIFICATE'
            OR header.WORK_EFFORT_TYPE_ID = 'PAYMENT_CERTIFICATE'
            THEN 100.0

        ELSE COALESCE(item.AchievementPercent, 0)
        END                                                                                AS AchievementPercentage,
    -- Flag to know original value
    item.AchievementPercent                                                                AS OriginalAchievementPercent,

    -- Incremental Refresh
    GREATEST(COALESCE(header.LAST_UPDATED_STAMP, '1900-01-01'),
             COALESCE(item.LAST_UPDATED_STAMP, '1900-01-01'))                              AS LastUpdatedStamp

FROM WORK_EFFORT header
         INNER JOIN WORK_EFFORT item
                    ON item.WORK_EFFORT_PARENT_ID = header.WORK_EFFORT_ID
                        AND item.WORK_EFFORT_TYPE_ID IN ('CERTIFICATE_ITEM', 'PAYMENT_CERTIFICATE_ITEM')

         LEFT JOIN DimProjects proj
                   ON COALESCE(header.PROJECT_ID, item.PROJECT_ID) = proj.ProjectId -- Updated join logic

WHERE header.CURRENT_STATUS_ID = 'WEPR_APPROVED'
  AND COALESCE(header.CERTIFICATE_CATEGORY, '') <> 'COMPANY_SUPPLY_SALE_CERTIFICATE'
  AND NOT (
    header.WORK_EFFORT_TYPE_ID = 'PAYMENT_CERTIFICATE'
        AND COALESCE(header.PROJECT_ID, item.PROJECT_ID) IS NULL
    );


DROP VIEW IF EXISTS Fact_Apartment_Payments;

CREATE OR REPLACE VIEW Fact_Apartment_Payments AS
SELECT
    p.PAYMENT_ID                                 AS PaymentId,
    p.SALES_REQUEST_ID                           AS SalesRequestId,
    sr.PRODUCT_ID                                 AS ApartmentId,

    -- ==================== PROJECT INFO (Correct Way) ====================
    apt.PROJECT_ID    AS ProjectId,
    proj.ProjectName,                            -- From your DimProjects view

    -- Customer
    p.PARTY_ID_FROM                              AS CustomerPartyId,
    pf.DESCRIPTION                               AS CustomerName,

    -- Payment Details
    p.PAYMENT_TYPE_ID,
    pt_type.DESCRIPTION_ARABIC                   AS PaymentTypeArabic,

    CASE p.PAYMENT_TYPE_ID
        WHEN 'RECEIPT_ADVANCE_PAYMENT'    THEN 'Advance Payment'
        WHEN 'RECEIPT_DUE_INSTALLMENT'    THEN 'Installment'
        WHEN 'RECEIPT_MAINTENANCE_AMOUNT' THEN 'Maintenance Deposit'
        ELSE 'Other'
        END                                          AS RevenueCategory,

    -- Amounts
    COALESCE(p.AMOUNT, 0)                        AS ScheduledAmount,

    CASE WHEN p.STATUS_ID IN ('PMNT_RECEIVED')
             THEN COALESCE(p.AMOUNT, 0) ELSE 0 END   AS CollectedAmount,

    CASE WHEN p.STATUS_ID NOT IN ('PMNT_RECEIVED')
             THEN COALESCE(p.AMOUNT, 0) ELSE 0 END   AS OutstandingAmount,

    CASE WHEN p.STATUS_ID NOT IN ('PMNT_RECEIVED')
        AND DATE(p.EFFECTIVE_DATE) < CURDATE()
             THEN COALESCE(p.AMOUNT, 0) ELSE 0 END   AS LateAmount,

    CASE WHEN p.STATUS_ID NOT IN ('PMNT_RECEIVED')
        AND DATE(p.EFFECTIVE_DATE) >= CURDATE()
             THEN COALESCE(p.AMOUNT, 0) ELSE 0 END   AS FutureAmount,

    -- ==================== IMPROVED STATUS LOGIC ====================
    CASE
        WHEN p.STATUS_ID IN ('PMNT_RECEIVED') THEN 'Received'
        WHEN DATE(p.EFFECTIVE_DATE) < CURDATE()            THEN 'Late'
        ELSE 'Upcoming'                                      -- Better than "Due"
        END                                              AS PaymentStatus,

    -- Detailed Bucket (Clean & Business Friendly)
    CASE
        WHEN p.STATUS_ID IN ('PMNT_RECEIVED') THEN 'Received'
        WHEN DATE(p.EFFECTIVE_DATE) >= CURDATE()           THEN 'Upcoming'
        WHEN DATEDIFF(CURDATE(), DATE(p.EFFECTIVE_DATE)) BETWEEN 1 AND 30 THEN 'Late (1-30 Days)'
        WHEN DATEDIFF(CURDATE(), DATE(p.EFFECTIVE_DATE)) BETWEEN 31 AND 90 THEN 'Late (31-90 Days)'
        ELSE 'Late (Over 90 Days)'
        END                                              AS OverdueBucket,

    -- Days Overdue (Only positive for late payments, 0 otherwise)
    CASE
        WHEN p.STATUS_ID NOT IN ('PMNT_RECEIVED')
            AND DATE(p.EFFECTIVE_DATE) < CURDATE()
            THEN DATEDIFF(CURDATE(), DATE(p.EFFECTIVE_DATE))
        ELSE 0
        END                                              AS DaysOverdue,

    -- Dates
    DATE(p.EFFECTIVE_DATE)                       AS DueDateKey,
    DATE(p.CREATED_STAMP)                        AS CreatedDateKey,

    p.COMMENTS,
    p.ChequeNumber

FROM PAYMENT p
         LEFT JOIN PARTY pf              ON p.PARTY_ID_FROM = pf.PARTY_ID
         LEFT JOIN PAYMENT_TYPE pt_type  ON p.PAYMENT_TYPE_ID = pt_type.PAYMENT_TYPE_ID
         LEFT JOIN SALES_REQUEST sr      ON p.SALES_REQUEST_ID = sr.SALES_REQUEST_ID
         LEFT JOIN PRODUCT apt           ON sr.PRODUCT_ID = apt.PRODUCT_ID                    -- Get ProjectId from Apartment
         LEFT JOIN DimProjects proj      ON COALESCE(apt.PROJECT_ID, p.WORK_EFFORT_ID) = proj.ProjectId   -- Join your existing DimProjects

WHERE p.PARTY_ID_TO = 'Company' AND p.SALES_REQUEST_ID IS NOT NULL
  AND p.PAYMENT_TYPE_ID IN ('RECEIPT_ADVANCE_PAYMENT',
                            'RECEIPT_DUE_INSTALLMENT',
                            'RECEIPT_MAINTENANCE_AMOUNT')
  AND COALESCE(p.AMOUNT, 0) > 0;

DROP VIEW IF EXISTS Fact_Apartment_Installment_Payment_Link;

CREATE OR REPLACE VIEW Fact_Apartment_Installment_Payment_Link AS
SELECT
    -- Installment side (original plan)
    i.SALES_REQUEST_ID,
    i.INSTALLMENT_NUMBER,
    i.DUE_DATE                                      AS PlannedDueDate,
    DATE(i.DUE_DATE)                                AS PlannedDueDateKey,
    i.AMOUNT                                        AS PlannedAmount,
    i.IS_ADVANCE,

    -- Payment side (what actually exists)
    p.PAYMENT_ID,
    p.STATUS_ID                                     AS PaymentStatusId,
    p.EFFECTIVE_DATE                                AS ActualEffectiveDate,
    DATE(p.EFFECTIVE_DATE)                          AS ActualDueDateKey,
    p.AMOUNT                                        AS ActualAmount,

    -- Matching & Difference flags
    CASE
        WHEN p.PAYMENT_ID IS NULL THEN 'Missing Payment'
        WHEN ABS(COALESCE(p.AMOUNT, 0) - i.AMOUNT) > 0.01 THEN 'Amount Mismatch'
        WHEN DATE(p.EFFECTIVE_DATE) <> DATE(i.DUE_DATE) THEN 'Date Mismatch'
        WHEN p.STATUS_ID = 'PMNT_RECEIVED' THEN 'Paid'
        ELSE 'Matched - Not Paid'
        END                                             AS MatchStatus,

    -- Useful calculated fields
    COALESCE(p.AMOUNT, 0) - i.AMOUNT                AS AmountDifference,
    DATEDIFF(DATE(p.EFFECTIVE_DATE), DATE(i.DUE_DATE)) AS DaysDifference,

    -- Extra info from Payment
    p.COMMENTS                                      AS PaymentComments,
    p.PAYMENT_TYPE_ID,
    p.PARTY_ID_FROM                                 AS CustomerPartyId,
    pf.DESCRIPTION                                  AS CustomerName,
    p.ChequeNumber,
    p.ChequeDate,

    -- From Sales Request (for context)
    sr.PRODUCT_ID                                   AS ApartmentId,
    sr.STATUS_ID                                    AS SalesRequestStatus

FROM SALES_REQUEST_INSTALLMENT i
         LEFT JOIN PAYMENT p
                   ON p.SALES_REQUEST_ID = i.SALES_REQUEST_ID
                       AND DATE(p.EFFECTIVE_DATE) = DATE(i.DUE_DATE)          -- Main matching key
                       AND ABS(COALESCE(p.AMOUNT, 0) - i.AMOUNT) <= 0.01      -- Allow small rounding tolerance
                       AND p.PAYMENT_TYPE_ID IN ('RECEIPT_ADVANCE_PAYMENT', 'RECEIPT_DUE_INSTALLMENT', 'RECEIPT_MAINTENANCE_AMOUNT')
         LEFT JOIN PARTY pf
                   ON p.PARTY_ID_FROM = pf.PARTY_ID                     -- ← Added join for Customer Name

         LEFT JOIN SALES_REQUEST sr
                   ON i.SALES_REQUEST_ID = sr.SALES_REQUEST_ID

WHERE i.SALES_REQUEST_ID IS NOT NULL
ORDER BY i.SALES_REQUEST_ID, i.INSTALLMENT_NUMBER, p.PAYMENT_ID;