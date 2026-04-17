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
-- 3. FLAT VIEW – THE ONE YOUR CLIENT WILL LOVE IN POWER BI
-- =============================================================
DROP VIEW IF EXISTS ProjectCertificatesWithItems;
CREATE OR REPLACE VIEW ProjectCertificatesWithItems AS
SELECT
    c.CertificateNumber,
    c.CertificateId,
    c.StatusName,
    c.CertificateType,
    c.CertificateDescription,
    c.PeriodStartDate,
    c.PeriodEndDate,
    c.ProjectName,
    c.PartyName,
    c.FacilityName,
    c.RelatedPurchaseOrderId,
    c.IsInternalIssueToSite,

    i.ItemId,
    i.ProductId,
    i.ProductName,
    i.Quantity,
    i.UomName,
    i.UnitPrice,
    i.MaterialPrice,
    i.LaborPrice,
    i.GrossAmount,
    i.DiscountAmount,
    i.TransportationExpenses,
    i.Gratuities,
    i.AchievementPercentage,
    i.Deductions,
    i.InsuranceAmount,
    i.AdditionalInsuranceAmount,
    i.NetAmount,
    i.ItemDescription,
    i.ProcurementDate AS ItemProcurementDate

FROM ProjectCertificates c
         LEFT JOIN ProjectCertificateItems i
                   ON c.CertificateId = i.CertificateId;
