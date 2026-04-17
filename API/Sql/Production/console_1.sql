-- =============================================================
-- Refactored Views (V2) - Enhanced versions based on analysis
-- Changes include: Standardized naming (PascalCase aliases), dynamic status mappings, added flags/hierarchies, performance optimizations (e.g., CTEs instead of subqueries), more NULL handling, timestamps, and real estate-specific enhancements.
-- Original views remain unchanged; these are new with _V2 suffix.
-- =============================================================

-- =============================================================
-- 1. DIM PRODUCT CATEGORIES V2 (Enhanced hierarchy, active flag, timestamps)
-- =============================================================
DROP VIEW IF EXISTS DimProductCategories_V2;
CREATE OR REPLACE VIEW DimProductCategories_V2 AS
WITH RecursiveCategories AS (SELECT pc.PRODUCT_CATEGORY_ID                          AS CategoryId,
                                    COALESCE(pc.DESCRIPTION_ARABIC, pc.DESCRIPTION) AS CategoryName,
                                    pc.DESCRIPTION                                  AS CategoryNameEnglish,
                                    pc.PRIMARY_PARENT_CATEGORY_ID                   AS ParentCategoryId,
                                    parent.DESCRIPTION_ARABIC                       AS ParentCategoryName,
                                    1                                               AS CategoryLevel,
                                    CAST(pc.PRODUCT_CATEGORY_ID AS VARCHAR(255))    AS CategoryPath
                             FROM PRODUCT_CATEGORY pc
                                      LEFT JOIN PRODUCT_CATEGORY parent
                                                ON pc.PRIMARY_PARENT_CATEGORY_ID = parent.PRODUCT_CATEGORY_ID
                             WHERE pc.PRIMARY_PARENT_CATEGORY_ID IS NULL

                             UNION ALL

                             SELECT pc.PRODUCT_CATEGORY_ID                                 AS CategoryId,
                                    COALESCE(pc.DESCRIPTION_ARABIC, pc.DESCRIPTION)        AS CategoryName,
                                    pc.DESCRIPTION                                         AS CategoryNameEnglish,
                                    pc.PRIMARY_PARENT_CATEGORY_ID                          AS ParentCategoryId,
                                    parent.DESCRIPTION_ARABIC                              AS ParentCategoryName,
                                    rc.CategoryLevel + 1                                   AS CategoryLevel,
                                    CONCAT(rc.CategoryPath, ' > ', pc.PRODUCT_CATEGORY_ID) AS CategoryPath
                             FROM PRODUCT_CATEGORY pc
                                      INNER JOIN RecursiveCategories rc ON pc.PRIMARY_PARENT_CATEGORY_ID = rc.CategoryId
                                      LEFT JOIN PRODUCT_CATEGORY parent
                                                ON pc.PRIMARY_PARENT_CATEGORY_ID = parent.PRODUCT_CATEGORY_ID)
SELECT CategoryId,
       CategoryName,
       CategoryNameEnglish,
       ParentCategoryId,
       ParentCategoryName,
       CategoryLevel,
       (ParentCategoryId IS NULL) AS IsTopLevelCategory,
       CategoryPath
FROM RecursiveCategories
WHERE (THRU_DATE IS NULL OR THRU_DATE > CURDATE());
-- Active only

-- =============================================================
-- DIM COST CENTERS V2 – Added Arabic name, project join, boolean flag
-- =============================================================
DROP VIEW IF EXISTS DimCostCenters_V2;
CREATE OR REPLACE VIEW DimCostCenters_V2 AS
SELECT cc.COST_CENTER_ID                                                  AS CostCenterId,
       COALESCE(cc.DESCRIPTION_ARABIC, cc.DESCRIPTION, cc.COST_CENTER_ID) AS CostCenterNameArabic,
       cc.DESCRIPTION                                                     AS CostCenterNameEnglish,
       CASE WHEN cc.IS_OUT_PAYMENT = 'Y' THEN 1 ELSE 0 END                AS IsExpenseFlag,
       we.WORK_EFFORT_ID                                                  AS ProjectId,
       we.PROJECT_NAME                                                    AS ProjectName,
       cc.LAST_UPDATED_STAMP                                              AS LastUpdated
FROM COST_CENTER cc
         LEFT JOIN WORK_EFFORT we ON cc.PROJECT_ID = we.WORK_EFFORT_ID -- Assuming PROJECT_ID exists in COST_CENTER
WHERE (cc.THRU_DATE IS NULL OR cc.THRU_DATE >= CURDATE());

-- =============================================================
-- DIM APARTMENTS V2 – Dynamic status, added calcs/flags, strict filter
-- =============================================================
DROP VIEW IF EXISTS DimApartments_V2;
CREATE OR REPLACE VIEW DimApartments_V2 AS
SELECT p.PRODUCT_ID                                                                  AS ApartmentId,
       p.PRODUCT_NAME                                                                AS ApartmentCode,
       COALESCE(p.PRODUCT_NAME_ARABIC, p.PRODUCT_NAME)                               AS ApartmentNameArabic,
       p.BUILDING_NUMBER                                                             AS BuildingNumber,
       p.FLOOR_NUMBER                                                                AS FloorNumber,
       p.APARTMENT_SPACE_M2                                                          AS ApartmentAreaM2,
       p.GARDEN_SPACE_M2                                                             AS GardenAreaM2,
       p.APARTMENT_PRICE_PER_M2                                                      AS ApartmentPricePerM2,
       p.GARDEN_PRICE_PER_M2                                                         AS GardenPricePerM2,
       ROUND(p.APARTMENT_SPACE_M2 * p.APARTMENT_PRICE_PER_M2, 2)                     AS ApartmentBasePrice,
       ROUND(COALESCE(p.GARDEN_SPACE_M2, 0) * COALESCE(p.GARDEN_PRICE_PER_M2, 0), 2) AS GardenTotalPrice,
       ROUND(p.APARTMENT_SPACE_M2 * p.APARTMENT_PRICE_PER_M2 +
             COALESCE(p.GARDEN_SPACE_M2, 0) * COALESCE(p.GARDEN_PRICE_PER_M2, 0), 2) AS TotalListPrice,
       p.APARTMENT_STATUS_ID                                                         AS StatusId,
       si.DESCRIPTION_ARABIC                                                         AS StatusNameArabic,
       si.DESCRIPTION                                                                AS StatusNameEnglish,
       (p.APARTMENT_STATUS_ID = 'APARTMENT_AVAILABLE')                               AS IsAvailable,
       p.PROJECT_ID                                                                  AS ProjectId,
       w.PROJECT_NAME                                                                AS ProjectName,
       p.LAST_UPDATED_STAMP                                                          AS LastUpdated
FROM PRODUCT p
         LEFT JOIN WORK_EFFORT w ON p.PROJECT_ID = w.WORK_EFFORT_ID
         LEFT JOIN STATUS_ITEM si ON p.APARTMENT_STATUS_ID = si.STATUS_ID AND si.STATUS_TYPE_ID = 'APARTMENT_STATUS'
WHERE p.PRODUCT_TYPE_ID = 'APARTMENT';

-- =============================================================
-- DIM PAYMENT METHOD TYPES V2 – Added GL join, hierarchy
-- =============================================================
DROP VIEW IF EXISTS DimPaymentMethodTypes_V2;
CREATE OR REPLACE VIEW DimPaymentMethodTypes_V2 AS
SELECT pmt.PAYMENT_METHOD_TYPE_ID                            AS PaymentMethodTypeId,
       COALESCE(pmt.DESCRIPTION, pmt.PAYMENT_METHOD_TYPE_ID) AS PaymentMethodTypeName,
       pmt.DESCRIPTION_ARABIC                                AS PaymentMethodTypeNameArabic,
       pmt.DEFAULT_GL_ACCOUNT_ID                             AS DefaultGlAccountId,
       gla.ACCOUNT_NAME                                      AS DefaultGlAccountName,
       CASE pmt.PAYMENT_METHOD_TYPE_ID
           WHEN 'CASH' THEN 'Cash'
           WHEN 'CERTIFIED_CHECK' THEN 'Check'
           WHEN 'COMPANY_CHECK' THEN 'Check'
           WHEN 'PERSONAL_CHECK' THEN 'Check'
           WHEN 'COMPANY_ACCOUNT' THEN 'Bank Transfer'
           WHEN 'ELECTRONIC' THEN 'Electronic'
           WHEN 'CREDIT_CARD' THEN 'Credit Card'
           ELSE 'Other'
           END                                               AS PaymentCategory,
       parent_pmt.PAYMENT_METHOD_TYPE_ID                     AS ParentTypeId,
       parent_pmt.DESCRIPTION                                AS ParentTypeName,
       pmt.LAST_UPDATED_STAMP                                AS LastUpdatedStamp,
       pmt.CREATED_STAMP                                     AS CreatedStamp
FROM PAYMENT_METHOD_TYPE pmt
         LEFT JOIN GL_ACCOUNT gla ON pmt.DEFAULT_GL_ACCOUNT_ID = gla.GL_ACCOUNT_ID
         LEFT JOIN PAYMENT_METHOD_TYPE parent_pmt ON pmt.PARENT_TYPE_ID = parent_pmt.PAYMENT_METHOD_TYPE_ID;
-- Assuming hierarchy exists

-- =============================================================
-- DIM PAYMENT TYPES V2 – Recursive hierarchy with path
-- =============================================================
DROP VIEW IF EXISTS DimPaymentTypes_V2;
CREATE OR REPLACE VIEW DimPaymentTypes_V2 AS
WITH RecursiveTypes AS (SELECT pt.PAYMENT_TYPE_ID                           AS PaymentTypeId,
                               COALESCE(pt.DESCRIPTION, pt.PAYMENT_TYPE_ID) AS PaymentTypeName,
                               pt.DESCRIPTION_ARABIC                        AS PaymentTypeNameArabic,
                               pt.PARENT_TYPE_ID                            AS ParentTypeId,
                               parent_pt.DESCRIPTION                        AS ParentTypeName,
                               parent_pt.DESCRIPTION_ARABIC                 AS ParentTypeNameArabic,
                               pt.HAS_TABLE                                 AS HasTableFlag,
                               1                                            AS HierarchyLevel,
                               CAST(pt.PAYMENT_TYPE_ID AS VARCHAR(255))     AS HierarchyPath
                        FROM PAYMENT_TYPE pt
                                 LEFT JOIN PAYMENT_TYPE parent_pt ON pt.PARENT_TYPE_ID = parent_pt.PAYMENT_TYPE_ID
                        WHERE pt.PARENT_TYPE_ID IS NULL

                        UNION ALL

                        SELECT pt.PAYMENT_TYPE_ID                                  AS PaymentTypeId,
                               COALESCE(pt.DESCRIPTION, pt.PAYMENT_TYPE_ID)        AS PaymentTypeName,
                               pt.DESCRIPTION_ARABIC                               AS PaymentTypeNameArabic,
                               pt.PARENT_TYPE_ID                                   AS ParentTypeId,
                               parent_pt.DESCRIPTION                               AS ParentTypeName,
                               parent_pt.DESCRIPTION_ARABIC                        AS ParentTypeNameArabic,
                               pt.HAS_TABLE                                        AS HasTableFlag,
                               rt.HierarchyLevel + 1                               AS HierarchyLevel,
                               CONCAT(rt.HierarchyPath, ' > ', pt.PAYMENT_TYPE_ID) AS HierarchyPath
                        FROM PAYMENT_TYPE pt
                                 INNER JOIN RecursiveTypes rt ON pt.PARENT_TYPE_ID = rt.PaymentTypeId
                                 LEFT JOIN PAYMENT_TYPE parent_pt ON pt.PARENT_TYPE_ID = parent_pt.PAYMENT_TYPE_ID)
SELECT PaymentTypeId,
       PaymentTypeName,
       PaymentTypeNameArabic,
       ParentTypeId,
       ParentTypeName,
       ParentTypeNameArabic,
       HasTableFlag,
       HierarchyLevel,
       HierarchyPath,
       (HasTableFlag = 'Y') AS IsLeafNode
FROM RecursiveTypes;

-- =============================================================
-- DIM PRODUCT RAW MATERIALS V2 – Merged with Services, added units
-- =============================================================
DROP VIEW IF EXISTS DimProductMaterialsAndServices_V2;
CREATE OR REPLACE VIEW DimProductMaterialsAndServices_V2 AS
SELECT p.PRODUCT_ID           AS ProductId,
       p.PRODUCT_NAME         AS ProductName,
       cat.CategoryName       AS CategoryName,
       cat.ParentCategoryName AS MainCategoryName,
       p.PRODUCT_TYPE_ID      AS ProductTypeId,
       uom.DESCRIPTION        AS DefaultUomName,
       p.LAST_UPDATED_STAMP   AS LastUpdated
FROM PRODUCT p
         LEFT JOIN DimProductCategories_V2 cat ON p.PRIMARY_PRODUCT_CATEGORY_ID = cat.CategoryId -- Use V2 dim
         LEFT JOIN UOM uom ON p.DEFAULT_UOM_ID = uom.UOM_ID -- Assuming DEFAULT_UOM_ID exists
WHERE p.PRODUCT_TYPE_ID IN ('RAW_MATERIAL', 'SERVICE');

-- =============================================================
-- DIM PROJECTS V2 – Dynamic status, added duration
-- =============================================================
DROP VIEW IF EXISTS DimProjects_V2;
CREATE OR REPLACE VIEW DimProjects_V2 AS
SELECT we.WORK_EFFORT_ID                                               AS ProjectId,
       we.PROJECT_NAME                                                 AS ProjectName,
       si.STATUS_ID                                                    AS StatusId,
       si.DESCRIPTION                                                  AS StatusNameEnglish,
       si.DESCRIPTION_ARABIC                                           AS StatusNameArabic,
       we.ESTIMATED_START_DATE                                         AS PlannedStartDate,
       we.ESTIMATED_COMPLETION_DATE                                    AS PlannedEndDate,
       DATEDIFF(we.ESTIMATED_COMPLETION_DATE, we.ESTIMATED_START_DATE) AS PlannedDurationDays,
       we.FACILITY_ID                                                  AS FacilityId,
       fac.FACILITY_NAME                                               AS FacilityName,
       we.LAST_UPDATED_STAMP                                           AS LastUpdated
FROM WORK_EFFORT we
         LEFT JOIN FACILITY fac ON we.FACILITY_ID = fac.FACILITY_ID
         LEFT JOIN STATUS_ITEM si ON we.CURRENT_STATUS_ID = si.STATUS_ID
WHERE we.WORK_EFFORT_TYPE_ID = 'PROJECT';

-- =============================================================
-- DIM STATUS ITEMS V2 – Added type description
-- =============================================================
DROP VIEW IF EXISTS DimStatusItems_V2;
CREATE OR REPLACE VIEW DimStatusItems_V2 AS
SELECT si.STATUS_ID                                                  AS StatusId,
       COALESCE(si.DESCRIPTION_ARABIC, si.DESCRIPTION, si.STATUS_ID) AS StatusName,
       COALESCE(si.DESCRIPTION, si.STATUS_ID)                        AS StatusNameEnglish,
       si.STATUS_TYPE_ID                                             AS StatusTypeId,
       st.DESCRIPTION                                                AS StatusTypeDescription,
       si.LAST_UPDATED_STAMP                                         AS LastUpdatedStamp,
       si.CREATED_STAMP                                              AS CreatedStamp
FROM STATUS_ITEM si
         LEFT JOIN STATUS_TYPE st ON si.STATUS_TYPE_ID = st.STATUS_TYPE_ID;

-- =============================================================
-- DIM PARTIES V2 – Combined Suppliers/Contractors/Employees with role
-- =============================================================
DROP VIEW IF EXISTS DimParties_V2;
CREATE OR REPLACE VIEW DimParties_V2 AS
SELECT p.PARTY_ID            AS PartyId,
       p.DESCRIPTION         AS PartyName,
       p.PARTY_TYPE_ID       AS PartyTypeId,
       CASE p.PARTY_TYPE_ID
           WHEN 'PERSON' THEN 'Individual'
           WHEN 'PARTY_GROUP' THEN 'Company'
           ELSE p.PARTY_TYPE_ID
           END               AS PartyType,
       p.STATUS_ID           AS StatusId,
       si.DESCRIPTION        AS StatusNameEnglish,
       si.DESCRIPTION_ARABIC AS StatusNameArabic,
       p.MAIN_ROLE           AS MainRole,      -- e.g., 'SUPPLIER', 'CONTRACTOR', 'EMPLOYEE'
       p.CREATED_DATE        AS CreatedDate,
       p.LAST_UPDATED_STAMP  AS LastUpdatedDate,
       pcm.CONTACT_MECH_ID   AS ContactMechId, -- Example for phone/email
       pcm.INFO_STRING       AS ContactInfo
FROM PARTY p
         LEFT JOIN STATUS_ITEM si ON p.STATUS_ID = si.STATUS_ID
         LEFT JOIN PARTY_CONTACT_MECH pcm ON p.PARTY_ID = pcm.PARTY_ID -- Add contacts
WHERE p.MAIN_ROLE IN ('SUPPLIER', 'CONTRACTOR', 'EMPLOYEE');

-- =============================================================
-- DIM PAYMENT METHODS V2 – Derived flags from type, added electronic flag
-- =============================================================
DROP VIEW IF EXISTS DimPaymentMethods_V2;
CREATE OR REPLACE VIEW DimPaymentMethods_V2 AS
SELECT pm.PAYMENT_METHOD_ID                                                                       AS PaymentMethodId,
       COALESCE(pm.DESCRIPTION, pm.PAYMENT_METHOD_ID)                                             AS PaymentMethodName,
       COALESCE(pmt.DESCRIPTION, pm.PAYMENT_METHOD_ID)                                            AS PaymentMethodNameEnglish,
       pm.PAYMENT_METHOD_TYPE_ID                                                                  AS PaymentMethodTypeId,
       pmt.DESCRIPTION                                                                            AS PaymentMethodTypeName,
       pmt.DESCRIPTION_ARABIC                                                                     AS PaymentMethodTypeNameArabic,
       pm.FIN_ACCOUNT_ID                                                                          AS FinAccountId,
       pm.GL_ACCOUNT_ID                                                                           AS GlAccountId,
       pm.PARTY_ID                                                                                AS PartyId,
       pm.FROM_DATE                                                                               AS ValidFrom,
       pm.THRU_DATE                                                                               AS ValidThru,
       CASE WHEN pm.THRU_DATE IS NULL THEN 'Y' ELSE 'N' END                                       AS IsActive,
       CASE WHEN pm.PAYMENT_METHOD_TYPE_ID = 'CASH' THEN 'Y' ELSE 'N' END                         AS IsCash,
       CASE
           WHEN pm.PAYMENT_METHOD_TYPE_ID IN ('COMPANY_CHECK', 'CERTIFIED_CHECK') OR pm.FIN_ACCOUNT_ID IS NOT NULL
               THEN 'Y'
           ELSE 'N' END                                                                           AS IsBankAccount,
       CASE WHEN pm.PAYMENT_METHOD_TYPE_ID = 'FIN_ACCOUNT' THEN 'Y' ELSE 'N' END                  AS IsCustody,
       CASE WHEN pm.PAYMENT_METHOD_TYPE_ID IN ('ELECTRONIC', 'CREDIT_CARD') THEN 'Y' ELSE 'N' END AS IsElectronic,
       pm.LAST_UPDATED_STAMP                                                                      AS LastUpdatedStamp,
       pm.CREATED_STAMP                                                                           AS CreatedStamp
FROM PAYMENT_METHOD pm
         LEFT JOIN PAYMENT_METHOD_TYPE pmt ON pm.PAYMENT_METHOD_TYPE_ID = pmt.PAYMENT_METHOD_TYPE_ID
WHERE (pm.THRU_DATE IS NULL OR pm.THRU_DATE >= CURDATE());

-- =============================================================
-- INVENTORY ITEMS DETAILS V2 – Added order names, simplified WHERE
-- =============================================================
DROP VIEW IF EXISTS InventoryItemsDetails_V2;
CREATE OR REPLACE VIEW InventoryItemsDetails_V2 AS
SELECT invi.PRODUCT_ID                   AS ProductId,
       prd.PRODUCT_NAME                  AS ProductName,
       invi.QUANTITY_ON_HAND_TOTAL       AS QuantityOnHandTotal,
       invi.AVAILABLE_TO_PROMISE_TOTAL   AS AvailableToPromiseTotal,
       invi.INVENTORY_ITEM_ID            AS InventoryItemId,
       invi.FACILITY_ID                  AS FacilityId,
       fac.FACILITY_NAME                 AS FacilityName,
       invd.INVENTORY_ITEM_DETAIL_SEQ_ID AS InventoryItemDetailSeqId,
       invd.EFFECTIVE_DATE               AS EffectiveDate,
       invd.QUANTITY_ON_HAND_DIFF        AS QuantityOnHandDiff,
       invd.AVAILABLE_TO_PROMISE_DIFF    AS AvailableToPromiseDiff,
       invd.ACCOUNTING_QUANTITY_DIFF     AS AccountingQuantityDiff,
       invd.ORDER_ID                     AS OrderId,
       oh.ORDER_NAME                     AS OrderName,     -- Added
       invd.WORK_EFFORT_ID               AS WorkEffortId,
       we.PROJECT_NAME                   AS WorkEffortName -- Assuming project name
FROM INVENTORY_ITEM invi
         JOIN INVENTORY_ITEM_DETAIL invd ON invi.INVENTORY_ITEM_ID = invd.INVENTORY_ITEM_ID
         JOIN PRODUCT prd ON invi.PRODUCT_ID = prd.PRODUCT_ID
         JOIN FACILITY fac ON invi.FACILITY_ID = fac.FACILITY_ID
         LEFT JOIN WORK_EFFORT we ON invd.WORK_EFFORT_ID = we.WORK_EFFORT_ID
         LEFT JOIN ORDER_HEADER oh ON invd.ORDER_ID = oh.ORDER_ID
WHERE ABS(COALESCE(invd.ACCOUNTING_QUANTITY_DIFF, 0)) > 0
   OR NOT (ABS(COALESCE(invd.QUANTITY_ON_HAND_DIFF, 0)) <= 0 AND ABS(COALESCE(invd.AVAILABLE_TO_PROMISE_DIFF, 0)) <= 0)
   OR (ABS(COALESCE(invd.QUANTITY_ON_HAND_DIFF, 0) - COALESCE(invd.AVAILABLE_TO_PROMISE_DIFF, 0)) <= 0
    AND ABS(COALESCE(invd.QUANTITY_ON_HAND_DIFF, 0) - COALESCE(invd.ACCOUNTING_QUANTITY_DIFF, 0)) <= 0
    AND ABS(COALESCE(invd.QUANTITY_ON_HAND_DIFF, 0)) > 0);

-- =============================================================
-- PAYMENT APPLICATIONS V2 – CTE for unapplied, added fully applied flag
-- =============================================================
DROP VIEW IF EXISTS PaymentApplications_V2;
CREATE OR REPLACE VIEW PaymentApplications_V2 AS
WITH AppliedSums AS (SELECT PAYMENT_ID, SUM(COALESCE(AMOUNT_APPLIED, 0)) AS TotalApplied
                     FROM PAYMENT_APPLICATION
                     GROUP BY PAYMENT_ID)
SELECT p.PAYMENT_ID                                                                 AS PaymentId,
       COALESCE(pa.PAYMENT_APPLICATION_ID, CONCAT('UNAPPLIED_', p.PAYMENT_ID))      AS PaymentApplicationKey,
       pa.PAYMENT_APPLICATION_ID                                                    AS PaymentApplicationId,
       pa.INVOICE_ID                                                                AS InvoiceId,
       pa.INVOICE_ITEM_SEQ_ID                                                       AS InvoiceItemSeqId,
       pa.TO_PAYMENT_ID                                                             AS ToPaymentId,
       pa.BILLING_ACCOUNT_ID                                                        AS BillingAccountId,
       COALESCE(pa.AMOUNT_APPLIED, 0)                                               AS AmountApplied,
       p.AMOUNT                                                                     AS PaymentAmount,
       p.AMOUNT - COALESCE(asp.TotalApplied, 0)                                     AS AmountUnapplied,
       CASE WHEN pa.PAYMENT_APPLICATION_ID IS NOT NULL THEN 'Y' ELSE 'N' END        AS IsApplied,
       CASE WHEN pa.INVOICE_ID IS NOT NULL THEN 'Y' ELSE 'N' END                    AS IsAppliedToInvoice,
       CASE WHEN pa.TO_PAYMENT_ID IS NOT NULL THEN 'Y' ELSE 'N' END                 AS IsAppliedToPayment,
       (p.AMOUNT = COALESCE(asp.TotalApplied, 0))                                   AS IsFullyApplied,
       p.PAYMENT_TYPE_ID,
       p.PAYMENT_METHOD_TYPE_ID,
       p.PAYMENT_METHOD_ID,
       p.PAYMENT_REF_NUM                                                            AS PaymentRefNum,
       p.EFFECTIVE_DATE                                                             AS EffectiveDate,
       p.STATUS_ID                                                                  AS PaymentStatusId,
       p.PARTY_ID_FROM,
       p.PARTY_ID_TO,
       p.CURRENCY_UOM_ID,
       p.WORK_EFFORT_ID                                                             AS ProjectId,
       p.COST_CENTER_ID                                                             AS CostCenterId,
       p.COMMENTS,
       GREATEST(COALESCE(p.LAST_UPDATED_STAMP, p.CREATED_STAMP),
                COALESCE(pa.LAST_UPDATED_STAMP, pa.CREATED_STAMP, p.CREATED_STAMP)) AS RowLastUpdated
FROM PAYMENT p
         LEFT JOIN PAYMENT_APPLICATION pa ON pa.PAYMENT_ID = p.PAYMENT_ID
         LEFT JOIN AppliedSums asp ON p.PAYMENT_ID = asp.PAYMENT_ID
WHERE COALESCE(p.STATUS_ID, '') NOT IN ('PMNT_CANCELLED', 'PMNT_VOID')
ORDER BY p.EFFECTIVE_DATE DESC, p.PAYMENT_ID;

-- =============================================================
-- PAYMENTS V2 – Split inbound/outbound logic, more COALESCE
-- =============================================================
DROP VIEW IF EXISTS Payments_V2;
CREATE OR REPLACE VIEW Payments_V2 AS
SELECT p.PAYMENT_ID                                                                                    AS PaymentId,
       p.AMOUNT                                                                                        AS Amount,
       p.ACTUAL_CURRENCY_AMOUNT                                                                        AS ActualAmount,
       COALESCE(p.CURRENCY_UOM_ID, 'EGP')                                                              AS CurrencyUomId,
       p.PARTY_ID_FROM                                                                                 AS PartyIdFrom,
       COALESCE(pf.DESCRIPTION, 'Unknown')                                                             AS PartyNameFrom,
       p.PARTY_ID_TO                                                                                   AS PartyIdTo,
       COALESCE(pt.DESCRIPTION, CASE WHEN p.PARTY_ID_TO = 'Company' THEN 'Company' ELSE 'Unknown' END) AS PartyNameTo,
       p.PAYMENT_TYPE_ID                                                                               AS PaymentTypeId,
       p.SALES_REQUEST_ID                                                                              AS SalesRequestId,
       pt_type.DESCRIPTION                                                                             AS PaymentTypeDescription,
       pt_type.DESCRIPTION_ARABIC                                                                      AS PaymentTypeDescriptionArabic,
       p.PAYMENT_METHOD_TYPE_ID                                                                        AS PaymentMethodTypeId,
       pmt_type.DESCRIPTION                                                                            AS PaymentMethodTypeName,
       pmt_type.DESCRIPTION_ARABIC                                                                     AS PaymentMethodTypeNameArabic,
       p.PAYMENT_REF_NUM                                                                               AS PaymentRefNum,
       p.PAYMENT_METHOD_ID                                                                             AS PaymentMethodId,
       COALESCE(pm.DESCRIPTION, 'Unknown')                                                             AS PaymentMethodName,
       p.WORK_EFFORT_ID                                                                                AS ProjectId,
       COALESCE(we.PROJECT_NAME, 'Unknown')                                                            AS ProjectName,
       p.COST_CENTER_ID                                                                                AS CostCenterId,
       COALESCE(cc.DESCRIPTION, 'Unknown')                                                             AS CostCenterName,
       opp.ORDER_ID                                                                                    AS OrderId,
       p.STATUS_ID                                                                                     AS StatusId,
       COALESCE(si.DESCRIPTION, 'Unknown')                                                             AS StatusNameEnglish,
       COALESCE(si.DESCRIPTION_ARABIC, 'غير معروف')                                                    AS StatusNameArabic,
       p.EFFECTIVE_DATE                                                                                AS EffectiveDate,
       DATEDIFF(p.EFFECTIVE_DATE, CURDATE())                                                           AS DaysUntilDue,
       CASE
           WHEN p.STATUS_ID <> 'PMNT_NOT_PAID' THEN si.DESCRIPTION_ARABIC
           ELSE
               CASE
                   WHEN DATEDIFF(p.EFFECTIVE_DATE, CURDATE()) < 0 THEN
                       CASE
                           WHEN ABS(DATEDIFF(p.EFFECTIVE_DATE, CURDATE())) <= 30 THEN
                               CONCAT(CASE WHEN pt_type.PARENT_TYPE_ID = 'DISBURSEMENT' THEN 'دفعة' ELSE 'مستحق' END,
                                      ' متأخرة منذ ', ABS(DATEDIFF(p.EFFECTIVE_DATE, CURDATE())), ' يوم')
                           ELSE CONCAT(CASE WHEN pt_type.PARENT_TYPE_ID = 'DISBURSEMENT' THEN 'دفعة' ELSE 'مستحق' END,
                                       ' متأخرة جداً')
                           END
                   WHEN DATEDIFF(p.EFFECTIVE_DATE, CURDATE()) = 0 THEN CONCAT(
                           CASE WHEN pt_type.PARENT_TYPE_ID = 'DISBURSEMENT' THEN 'دفعة مستحقة' ELSE 'مستحق' END,
                           ' اليوم')
                   WHEN DATEDIFF(p.EFFECTIVE_DATE, CURDATE()) = 1 THEN CONCAT(
                           CASE WHEN pt_type.PARENT_TYPE_ID = 'DISBURSEMENT' THEN 'دفعة مستحقة' ELSE 'مستحق' END,
                           ' غداً')
                   WHEN DATEDIFF(p.EFFECTIVE_DATE, CURDATE()) <= 3 THEN CONCAT(
                           CASE WHEN pt_type.PARENT_TYPE_ID = 'DISBURSEMENT' THEN 'دفعة مستحقة' ELSE 'مستحق' END,
                           ' بعد ', DATEDIFF(p.EFFECTIVE_DATE, CURDATE()), ' أيام')
                   WHEN DATEDIFF(p.EFFECTIVE_DATE, CURDATE()) <= 7 THEN CONCAT(
                           CASE WHEN pt_type.PARENT_TYPE_ID = 'DISBURSEMENT' THEN 'دفعة مستحقة' ELSE 'مستحق' END,
                           ' هذا الأسبوع')
                   WHEN DATEDIFF(p.EFFECTIVE_DATE, CURDATE()) <= 30 THEN CONCAT(
                           CASE WHEN pt_type.PARENT_TYPE_ID = 'DISBURSEMENT' THEN 'دفعة مستحقة' ELSE 'مستحق' END,
                           ' خلال الشهر')
                   WHEN DATEDIFF(p.EFFECTIVE_DATE, CURDATE()) <= 90 THEN CONCAT(
                           CASE WHEN pt_type.PARENT_TYPE_ID = 'DISBURSEMENT' THEN 'دفعة مستحقة' ELSE 'مستحق' END,
                           ' خلال 3 أشهر')
                   ELSE CONCAT(CASE WHEN pt_type.PARENT_TYPE_ID = 'DISBURSEMENT' THEN 'دفعة مستحقة' ELSE 'مستحق' END,
                               ' لاحقاً')
                   END
           END                                                                                         AS DueStatusArabic,
       CASE WHEN pt_type.PARENT_TYPE_ID = 'DISBURSEMENT' THEN 1 ELSE 0 END                             AS IsDisbursement,
       CASE
           WHEN pt_type.PARENT_TYPE_ID = 'DISBURSEMENT' THEN p.PARTY_ID_FROM
           ELSE p.PARTY_ID_TO END                                                                      AS OrganizationPartyId,
       CASE
           WHEN pt_type.PARENT_TYPE_ID = 'DISBURSEMENT' THEN 'Outbound'
           WHEN p.PARTY_ID_TO = 'Company' THEN 'Inbound'
           ELSE 'Unknown' END                                                                          AS PaymentDirection,
       p.COMMENTS                                                                                      AS Comments,
       p.ChequeNumber                                                                                  AS ChequeNumber,
       p.ChequeDate                                                                                    AS ChequeDate,
       p.OVERRIDE_GL_ACCOUNT_ID                                                                        AS OverrideGlAccountId,
       p.CREATED_STAMP                                                                                 AS CreatedDate,
       p.PAYMENT_PREFERENCE_ID                                                                         AS PaymentPreferenceId
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
         LEFT JOIN ORDER_HEADER ord ON opp.ORDER_ID = ord.ORDER_ID
ORDER BY p.EFFECTIVE_DATE DESC, p.PAYMENT_ID DESC;

-- =============================================================
-- PROJECT CERTIFICATES V2 – Added aggregated totals from items
-- =============================================================
DROP VIEW IF EXISTS ProjectCertificates_V2;
CREATE OR REPLACE VIEW ProjectCertificates_V2 AS
SELECT we.CERTIFICATE_NUMBER                                                                           AS CertificateNumber,
       we.WORK_EFFORT_ID                                                                               AS CertificateId,
       we.CURRENT_STATUS_ID                                                                            AS CurrentStatusId,
       si.DESCRIPTION                                                                                  AS StatusNameEnglish,
       si.DESCRIPTION_ARABIC                                                                           AS StatusNameArabic,
       we.CERTIFICATE_CATEGORY                                                                         AS CertificateTypeCode,
       CASE we.CERTIFICATE_CATEGORY
           WHEN 'SUPPLY_PROCUREMENT_CERTIFICATE' THEN 'Supply Procurement'
           WHEN 'WORKMANSHIP_CONTRACTING_CERTIFICATE' THEN 'Workmanship Contracting'
           WHEN 'COMPANY_SUPPLY_SALE_CERTIFICATE' THEN 'Company Supply Sale'
           ELSE we.CERTIFICATE_CATEGORY
           END                                                                                         AS CertificateType,
       we.DESCRIPTION                                                                                  AS CertificateDescription,
       we.ESTIMATED_START_DATE                                                                         AS PeriodStartDate,
       we.ESTIMATED_COMPLETION_DATE                                                                    AS PeriodEndDate,
       we.CREATED_DATE                                                                                 AS CreatedDate,
       we.LAST_UPDATED_STAMP                                                                           AS LastUpdatedDate,
       proj_we.WORK_EFFORT_ID                                                                          AS ProjectId,
       proj_we.PROJECT_NAME                                                                            AS ProjectName,
       proj_fac.FACILITY_NAME                                                                          AS ProjectFacilityName,
       COALESCE(we.PARTY_ID_SUPPLIER, we.PARTY_ID_CONTRACTOR)                                          AS PartyId,
       COALESCE(supp.DESCRIPTION, contr.DESCRIPTION)                                                   AS PartyName,
       we.PARTY_ID_SUPPLIER                                                                            AS SupplierPartyId,
       supp.DESCRIPTION                                                                                AS SupplierName,
       we.PARTY_ID_CONTRACTOR                                                                          AS ContractorPartyId,
       contr.DESCRIPTION                                                                               AS ContractorName,
       we.FACILITY_ID                                                                                  AS FacilityId,
       fac.FACILITY_NAME                                                                               AS FacilityName,
       we.RELATED_ORDER_ID                                                                             AS RelatedPurchaseOrderId,
       (we.PARTY_ID_CONTRACTOR = 'SITE')                                                               AS IsInternalIssueToSite,
       (we.CERTIFICATE_CATEGORY = 'SUPPLY_PROCUREMENT_CERTIFICATE')                                    AS IsSupplyProcurement,
       (we.CERTIFICATE_CATEGORY = 'WORKMANSHIP_CONTRACTING_CERTIFICATE')                               AS IsWorkmanship,
       (we.CERTIFICATE_CATEGORY = 'COMPANY_SUPPLY_SALE_CERTIFICATE')                                   AS IsCompanySale,
       (SELECT SUM(NetAmount)
        FROM ProjectCertificateItems_V2
        WHERE CertificateId = we.WORK_EFFORT_ID)                                                       AS TotalNetAmount -- Aggregated
FROM WORK_EFFORT we
         LEFT JOIN WORK_EFFORT proj_we
                   ON we.PROJECT_ID = proj_we.WORK_EFFORT_ID AND proj_we.WORK_EFFORT_TYPE_ID = 'PROJECT'
         LEFT JOIN FACILITY proj_fac ON proj_we.FACILITY_ID = proj_fac.FACILITY_ID
         LEFT JOIN FACILITY fac ON we.FACILITY_ID = fac.FACILITY_ID
         LEFT JOIN PARTY supp ON we.PARTY_ID_SUPPLIER = supp.PARTY_ID
         LEFT JOIN PARTY contr ON we.PARTY_ID_CONTRACTOR = contr.PARTY_ID
         LEFT JOIN STATUS_ITEM si ON we.CURRENT_STATUS_ID = si.STATUS_ID
WHERE we.WORK_EFFORT_TYPE_ID = 'PROJECT_CERTIFICATE'
  AND we.CERTIFICATE_NUMBER IS NOT NULL;

-- =============================================================
-- PROJECT CERTIFICATE ITEMS V2 – No changes needed, but reference V2 header
-- =============================================================
DROP VIEW IF EXISTS ProjectCertificateItems_V2;
CREATE OR REPLACE VIEW ProjectCertificateItems_V2 AS
SELECT cert.CertificateNumber,
       item.WORK_EFFORT_ID                                                AS ItemId,
       item.WORK_EFFORT_PARENT_ID                                         AS CertificateId,
       item.PRODUCT_ID                                                    AS ProductId,
       prd.PRODUCT_NAME                                                   AS ProductName,
       item.QUANTITY                                                      AS Quantity,
       item.QuantityUomId                                                 AS UomId,
       uom.DESCRIPTION                                                    AS UomName,
       item.RATE                                                          AS UnitPrice,
       item.MATERIAL_PRICE                                                AS MaterialPrice,
       item.LABOR_PRICE                                                   AS LaborPrice,
       (COALESCE(item.MATERIAL_PRICE, 0) + COALESCE(item.LABOR_PRICE, 0)) AS UnitPriceWorkmanship,
       item.TOTAL_AMOUNT                                                  AS GrossAmount,
       item.DISCOUNT                                                      AS DiscountAmount,
       item.TransportationExpenses                                        AS TransportationExpenses,
       item.Gratuities                                                    AS Gratuities,
       item.AchievementPercent                                            AS AchievementPercentage,
       item.DEDUCTIONS                                                    AS Deductions,
       item.INSURANCE                                                     AS InsuranceAmount,
       item.ADDITIONAL_INSURANCE                                          AS AdditionalInsuranceAmount,
       item.TOTAL_AMOUNT - COALESCE(item.DISCOUNT, 0) - COALESCE(item.DEDUCTIONS, 0) - COALESCE(item.INSURANCE, 0) -
       COALESCE(item.ADDITIONAL_INSURANCE, 0) + COALESCE(item.TransportationExpenses, 0) +
       COALESCE(item.Gratuities, 0)                                       AS NetAmount,
       item.DESCRIPTION                                                   AS ItemDescription,
       item.DEDUCTION_DESCRIPTION                                         AS DeductionDescription,
       item.ProcurementDate                                               AS ProcurementDate
FROM WORK_EFFORT item
         JOIN ProjectCertificates_V2 cert ON item.WORK_EFFORT_PARENT_ID = cert.CertificateId
         LEFT JOIN PRODUCT prd ON item.PRODUCT_ID = prd.PRODUCT_ID
         LEFT JOIN UOM uom ON item.QuantityUomId = uom.UOM_ID
WHERE item.WORK_EFFORT_TYPE_ID = 'CERTIFICATE_ITEM';

-- =============================================================
-- PAYMENT CERTIFICATES V2 – Added type dim description
-- =============================================================
DROP VIEW IF EXISTS PaymentCertificates_V2;
CREATE OR REPLACE VIEW PaymentCertificates_V2 AS
SELECT we.WORK_EFFORT_ID       AS CertificateId,
       we.CURRENT_STATUS_ID    AS CurrentStatusId,
       si.DESCRIPTION          AS StatusNameEnglish,
       si.DESCRIPTION_ARABIC   AS StatusNameArabic,
       we.DESCRIPTION          AS Description,
       we.ESTIMATED_START_DATE AS CertificateDate
FROM WORK_EFFORT we
         LEFT JOIN STATUS_ITEM si ON we.CURRENT_STATUS_ID = si.STATUS_ID
WHERE we.WORK_EFFORT_TYPE_ID = 'PAYMENT_CERTIFICATE';

-- =============================================================
-- PAYMENT CERTIFICATE ITEMS V2 – Added cost type description
-- =============================================================
DROP VIEW IF EXISTS PaymentCertificateItems_V2;
CREATE OR REPLACE VIEW PaymentCertificateItems_V2 AS
SELECT cert.CertificateId,
       item.WORK_EFFORT_ID                        AS ItemId,
       item.WORK_EFFORT_PARENT_ID                 AS ParentCertificateId,
       item.COST_TYPE                             AS CostTypeCode,
       ct.DESCRIPTION                             AS CostTypeDescription, -- Assuming COST_TYPE table exists; adjust if needed
       COALESCE(item.PRODUCT_ID, item.SERVICE_ID) AS ProductId,
       prod.PRODUCT_NAME                          AS ProductName,
       item.DESCRIPTION                           AS ItemDescription,
       item.TOTAL_AMOUNT                          AS GrossAmount,
       item.AMOUNT                                AS Amount,
       item.DISCOUNT                              AS DiscountAmount,
       item.TransportationExpenses                AS TransportationExpenses,
       item.Gratuities                            AS Gratuities,
       (COALESCE(item.TOTAL_AMOUNT, 0) - COALESCE(item.DISCOUNT, 0) + COALESCE(item.TransportationExpenses, 0) +
        COALESCE(item.Gratuities, 0))             AS NetAmount,
       item.PROJECT_ID                            AS ItemProjectId,
       proj_we.PROJECT_NAME                       AS ProjectName,
       item.PARTY_ID_CONTRACTOR                   AS ItemContractorPartyId,
       contractor_party.DESCRIPTION               AS ContractorName,
       item.PARTY_ID_SUPPLIER                     AS ItemSupplierPartyId,
       supplier_party.DESCRIPTION                 AS SupplierName,
       item.CURRENT_STATUS_ID                     AS StatusId,
       status_item.DESCRIPTION                    AS StatusDescription
FROM WORK_EFFORT item
         JOIN PaymentCertificates_V2 cert ON item.WORK_EFFORT_PARENT_ID = cert.CertificateId
         LEFT JOIN PRODUCT prod ON COALESCE(item.PRODUCT_ID, item.SERVICE_ID) = prod.PRODUCT_ID
         LEFT JOIN WORK_EFFORT proj_we ON item.PROJECT_ID = proj_we.WORK_EFFORT_ID
         LEFT JOIN PARTY contractor_party ON item.PARTY_ID_CONTRACTOR = contractor_party.PARTY_ID
         LEFT JOIN PARTY supplier_party ON item.PARTY_ID_SUPPLIER = supplier_party.PARTY_ID
         LEFT JOIN STATUS_ITEM status_item ON item.CURRENT_STATUS_ID = status_item.STATUS_ID
         LEFT JOIN COST_TYPE ct ON item.COST_TYPE = ct.COST_TYPE_ID -- Add if COST_TYPE table exists
WHERE item.WORK_EFFORT_TYPE_ID = 'PAYMENT_CERTIFICATE_ITEM';

-- =============================================================
-- PAYMENT CERTIFICATES WITH ITEMS V2 – Reference V2 items
-- =============================================================
DROP VIEW IF EXISTS PaymentCertificatesWithItems_V2;
CREATE OR REPLACE VIEW PaymentCertificatesWithItems_V2 AS
SELECT c.CertificateId,
       c.StatusNameEnglish, -- Updated to use English for variety; adjust
       c.CertificateDate,
       c.Description,
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
       i.ProjectName    AS ItemProjectName,
       i.ItemContractorPartyId,
       i.ContractorName AS ItemContractorName,
       i.ItemSupplierPartyId,
       i.SupplierName   AS ItemSupplierName,
       i.StatusId,
       i.StatusDescription
FROM PaymentCertificates_V2 c
         LEFT JOIN PaymentCertificateItems_V2 i ON c.CertificateId = i.ParentCertificateId;

-- =============================================================
-- BILLING ACCOUNTS V2 – CTE for balances, added flag
-- =============================================================
DROP VIEW IF EXISTS BillingAccounts_V2;
CREATE OR REPLACE VIEW BillingAccounts_V2 AS
WITH UsedBalances AS (SELECT ba.BILLING_ACCOUNT_ID,
                             SUM(COALESCE(p.AMOUNT, 0)) AS UsedBalance
                      FROM BILLING_ACCOUNT ba
                               LEFT JOIN BILLING_ACCOUNT_ROLE bar ON ba.BILLING_ACCOUNT_ID = bar.BILLING_ACCOUNT_ID
                               LEFT JOIN PAYMENT p
                                         ON p.WORK_EFFORT_ID = ba.WORK_EFFORT_ID AND p.PARTY_ID_TO = bar.PARTY_ID
                      WHERE p.STATUS_ID = 'PMNT_SENT'
                        AND p.PAYMENT_TYPE_ID = 'ADVANCE_TO_VENDOR_CONTRACTOR'
                      GROUP BY ba.BILLING_ACCOUNT_ID)
SELECT ba.BILLING_ACCOUNT_ID                                         AS BillingAccountId,
       ba.ACCOUNT_LIMIT                                              AS AccountLimit,
       COALESCE(ub.UsedBalance, 0)                                   AS UsedBalance,
       COALESCE(ba.ACCOUNT_LIMIT, 0) - COALESCE(ub.UsedBalance, 0)   AS RemainingBalance,
       (COALESCE(ba.ACCOUNT_LIMIT, 0) < COALESCE(ub.UsedBalance, 0)) AS IsOverLimit,
       ba.WORK_EFFORT_ID                                             AS ProjectId,
       we.PROJECT_NAME                                               AS ProjectName,
       ba.ACCOUNT_CURRENCY_UOM_ID                                    AS AccountCurrencyUomId,
       uom.DESCRIPTION                                               AS AccountCurrencyUomDescription,
       bar.PARTY_ID                                                  AS PartyId,
       pty.DESCRIPTION                                               AS PartyName,
       ba.FROM_DATE                                                  AS FromDate,
       ba.THRU_DATE                                                  AS ThruDate
FROM BILLING_ACCOUNT ba
         INNER JOIN BILLING_ACCOUNT_ROLE bar ON ba.BILLING_ACCOUNT_ID = bar.BILLING_ACCOUNT_ID
         INNER JOIN PARTY pty ON bar.PARTY_ID = pty.PARTY_ID
         INNER JOIN UOM uom ON ba.ACCOUNT_CURRENCY_UOM_ID = uom.UOM_ID
         INNER JOIN WORK_EFFORT we ON ba.WORK_EFFORT_ID = we.WORK_EFFORT_ID
         LEFT JOIN UsedBalances ub ON ba.BILLING_ACCOUNT_ID = ub.BILLING_ACCOUNT_ID
WHERE ba.THRU_DATE IS NULL
ORDER BY ba.BILLING_ACCOUNT_ID;

-- =============================================================
-- ACCTG TRANSACTIONS V2 – Added posted filter default
-- =============================================================
DROP VIEW IF EXISTS AcctgTransactions_V2;
CREATE OR REPLACE VIEW AcctgTransactions_V2 AS
SELECT t.ACCTG_TRANS_ID,
       t.ACCTG_TRANS_TYPE_ID,
       t.INVOICE_ID,
       t.PAYMENT_ID,
       t.WORK_EFFORT_ID,
       t.FIN_ACCOUNT_TRANS_ID,
       t.SHIPMENT_ID,
       t.RECEIPT_ID,
       t.PARTY_ID,
       t.TRANSACTION_DATE,
       t.IS_POSTED,
       t.POSTED_DATE,
       t.SCHEDULED_POSTING_DATE,
       t.GL_FISCAL_TYPE_ID,
       t.GL_JOURNAL_ID,
       t.DESCRIPTION,
       t.VOUCHER_REF,
       t.VOUCHER_DATE,
       t.CREATED_DATE,
       t.CREATED_BY_USER_LOGIN,
       t.LAST_MODIFIED_DATE,
       t.LAST_MODIFIED_BY_USER_LOGIN,
       t.THEIR_ACCTG_TRANS_ID,
       t.GROUP_STATUS_ID,
       t.FIXED_ASSET_ID,
       t.INVENTORY_ITEM_ID,
       t.PHYSICAL_INVENTORY_ID
FROM ACCTG_TRANS t
WHERE t.IS_POSTED = 'Y';
-- Default to posted

-- =============================================================
-- ACCTG TRANS ENTRIES V2 – No major changes
-- =============================================================
DROP VIEW IF EXISTS AcctgTransEntries_V2;
CREATE OR REPLACE VIEW AcctgTransEntries_V2 AS
SELECT te.ACCTG_TRANS_ID,
       te.ACCTG_TRANS_ENTRY_SEQ_ID,
       te.GL_ACCOUNT_ID,
       gla.ACCOUNT_NAME        AS GlAccountNameEnglish,
       gla.ACCOUNT_NAME_ARABIC AS GlAccountNameArabic,
       te.AMOUNT,
       te.DEBIT_CREDIT_FLAG,
       te.CURRENCY_UOM_ID,
       te.ORIG_AMOUNT,
       te.ORIG_CURRENCY_UOM_ID,
       te.PARTY_ID,
       p.DESCRIPTION           AS PartyName,
       te.PRODUCT_ID,
       te.DESCRIPTION          AS EntryDescription,
       te.RECONCILE_STATUS_ID,
       te.DUE_DATE,
       te.CREATED_STAMP,
       te.LAST_UPDATED_STAMP
FROM ACCTG_TRANS_ENTRY te
         LEFT JOIN GL_ACCOUNT gla ON te.GL_ACCOUNT_ID = gla.GL_ACCOUNT_ID
         LEFT JOIN PARTY p ON te.PARTY_ID = p.PARTY_ID;

-- =============================================================
-- SALES REQUESTS V2 – JOIN for project name instead of subquery
-- =============================================================
DROP VIEW IF EXISTS SalesRequests_V2;
CREATE OR REPLACE VIEW SalesRequests_V2 AS
SELECT sr.SALES_REQUEST_ID                                             AS SalesRequestId,
       sr.PRODUCT_ID                                                   AS ApartmentId,
       p.PRODUCT_NAME                                                  AS ApartmentName,
       pt.DESCRIPTION                                                  AS ProductTypeDescriptionEnglish,
       pt.DESCRIPTION_ARABIC                                           AS ProductTypeDescriptionArabic,
       COALESCE(pt.DESCRIPTION_ARABIC, pt.DESCRIPTION)                 AS ProductTypeDescription,
       we.PROJECT_NAME                                                 AS ProjectName, -- JOIN instead
       CASE p.FLOOR_NUMBER
           WHEN '0' THEN 'الطابق الأرضي'
           WHEN '1' THEN 'الطابق الأول'
           WHEN '2' THEN 'الطابق الثاني'
           WHEN '3' THEN 'الطابق الثالث'
           WHEN '4' THEN 'الطابق الرابع'
           WHEN '5' THEN 'الطابق الخامس'
           WHEN '6' THEN 'الطابق السادس'
           ELSE CONCAT('الطابق ', COALESCE(p.FLOOR_NUMBER, 'غير محدد'))
           END                                                         AS FloorNameArabic,
       p.APARTMENT_SPACE_M2                                            AS ApartmentSpaceM2,
       p.GARDEN_SPACE_M2                                               AS GardenSpaceM2,
       p.APARTMENT_STATUS_ID                                           AS ApartmentStatusId,
       ast.DESCRIPTION                                                 AS ApartmentStatusDescription,
       COALESCE(ast.DESCRIPTION_ARABIC, ast.DESCRIPTION)               AS ApartmentStatusDescriptionArabic,
       sr.FROM_PARTY_ID                                                AS FromPartyId,
       c.DESCRIPTION                                                   AS FromPartyName,
       sr.EMPLOYEE_PARTY_ID                                            AS EmployeePartyId,
       e.DESCRIPTION                                                   AS EmployeeName,
       sr.APARTMENT_PRICE_PER_M2                                       AS ApartmentPricePerM2,
       sr.GARDEN_PRICE_PER_M2                                          AS GardenPricePerM2,
       sr.DISCOUNT                                                     AS Discount,
       sr.TOTAL_PRICE                                                  AS TotalPrice,
       sr.ADVANCE_PAYMENT                                              AS AdvancePayment,
       sr.NUMBER_OF_INSTALLMENTS                                       AS NumberOfInstallments,
       sr.DATE_OF_FIRST_INSTALLMENT                                    AS DateOfFirstInstallment,
       sr.MONTHS_BETWEEN_INSTALLMENTS                                  AS MonthsBetweenInstallments,
       sr.MAINTENANCE_DEPOSIT                                          AS MaintenanceDeposit,
       sr.STATUS_ID                                                    AS StatusId,
       srs.DESCRIPTION                                                 AS SalesRequestStatusDescriptionEnglish,
       COALESCE(srs.DESCRIPTION_ARABIC, srs.DESCRIPTION)               AS SalesRequestStatusDescriptionArabic,
       COALESCE(srs.DESCRIPTION_ARABIC, srs.DESCRIPTION, sr.STATUS_ID) AS StatusDescription,
       sr.SALE_DATE                                                    AS SaleDate,
       sr.COMMENTS                                                     AS Comments,
       sr.CREATED_STAMP                                                AS CreatedStamp,
       sr.LAST_UPDATED_STAMP                                           AS LastUpdatedStamp
FROM SALES_REQUEST sr
         INNER JOIN PRODUCT p ON sr.PRODUCT_ID = p.PRODUCT_ID
         INNER JOIN PRODUCT_TYPE pt ON p.PRODUCT_TYPE_ID = pt.PRODUCT_TYPE_ID
         LEFT JOIN STATUS_ITEM ast ON p.APARTMENT_STATUS_ID = ast.STATUS_ID AND ast.STATUS_TYPE_ID = 'APARTMENT_STATUS'
         LEFT JOIN STATUS_ITEM srs ON sr.STATUS_ID = srs.STATUS_ID AND srs.STATUS_TYPE_ID = 'SALES_REQUEST_STATUS'
         LEFT JOIN PARTY c ON sr.FROM_PARTY_ID = c.PARTY_ID
         LEFT JOIN PARTY e ON sr.EMPLOYEE_PARTY_ID = e.PARTY_ID
         LEFT JOIN WORK_EFFORT we ON p.PROJECT_ID = we.WORK_EFFORT_ID AND we.WORK_EFFORT_TYPE_ID = 'PROJECT'
ORDER BY sr.CREATED_STAMP DESC, sr.SALES_REQUEST_ID DESC;

-- =============================================================
-- RESERVE REQUESTS V2 – JOIN for project, added floor dim reference
-- =============================================================
DROP VIEW IF EXISTS ReserveRequests_V2;
CREATE OR REPLACE VIEW ReserveRequests_V2 AS
SELECT rr.RESERVE_REQUEST_ID                                         AS ReserveRequestId,
       rr.PRODUCT_ID                                                 AS ApartmentId,
       p.PRODUCT_NAME                                                AS ApartmentName,
       COALESCE(pt.DESCRIPTION_ARABIC, pt.DESCRIPTION)               AS ProductTypeDescriptionAr,
       pt.DESCRIPTION                                                AS ProductTypeDescriptionEn,
       we.PROJECT_NAME                                               AS ProjectName, -- JOIN instead of subquery
       CASE p.FLOOR_NUMBER
           WHEN '0' THEN 'الطابق الأرضي'
           WHEN '1' THEN 'الطابق الأول'
           WHEN '2' THEN 'الطابق الثاني'
           WHEN '3' THEN 'الطابق الثالث'
           WHEN '4' THEN 'الطابق الرابع'
           WHEN '5' THEN 'الطابق الخامس'
           WHEN '6' THEN 'الطابق السادس'
           ELSE COALESCE(p.FLOOR_NUMBER, '')
           END                                                       AS FloorNumber,
       COALESCE(p.APARTMENT_SPACE_M2, 0)                             AS ApartmentSpaceM2,
       rr.FROM_PARTY_ID                                              AS FromPartyId,
       COALESCE(c.DESCRIPTION, '')                                   AS FromPartyName,
       rr.EMPLOYEE_PARTY_ID                                          AS EmployeePartyId,
       COALESCE(e.DESCRIPTION, '')                                   AS EmployeeName,
       rr.RESERVE_DATE                                               AS ReserveDate,
       rr.RESERVE_AMOUNT                                             AS ReserveAmount,
       rr.PAY_METHOD                                                 AS PayMethod,
       rr.COMMENTS                                                   AS Comments,
       COALESCE(rr.STATUS_ID, '')                                    AS StatusId,
       si.DESCRIPTION                                                AS StatusDescriptionEn,
       COALESCE(si.DESCRIPTION_ARABIC, si.DESCRIPTION, rr.STATUS_ID) AS StatusDescriptionAr,
       rr.CREATED_STAMP                                              AS CreatedStamp,
       rr.LAST_UPDATED_STAMP                                         AS LastUpdatedStamp
FROM RESERVE_REQUEST rr
         INNER JOIN PRODUCT p ON rr.PRODUCT_ID = p.PRODUCT_ID
         INNER JOIN PRODUCT_TYPE pt ON p.PRODUCT_TYPE_ID = pt.PRODUCT_TYPE_ID
         LEFT JOIN PARTY c ON rr.FROM_PARTY_ID = c.PARTY_ID
         LEFT JOIN PARTY e ON rr.EMPLOYEE_PARTY_ID = e.PARTY_ID
         LEFT JOIN STATUS_ITEM si ON rr.STATUS_ID = si.STATUS_ID AND si.STATUS_TYPE_ID = 'RESERVE_REQUEST_STATUS'
         LEFT JOIN WORK_EFFORT we ON p.PROJECT_ID = we.WORK_EFFORT_ID AND we.WORK_EFFORT_TYPE_ID = 'PROJECT'
ORDER BY rr.RESERVE_REQUEST_ID;