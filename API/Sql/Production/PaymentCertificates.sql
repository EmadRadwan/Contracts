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