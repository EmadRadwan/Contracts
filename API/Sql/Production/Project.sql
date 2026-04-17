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