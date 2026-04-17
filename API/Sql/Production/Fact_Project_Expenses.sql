CREATE OR REPLACE VIEW Fact_Project_Expenses AS
SELECT
    item.WORK_EFFORT_ID AS ExpenseItemKey,
    header.WORK_EFFORT_ID AS CertificateKey,
    CASE WHEN header.WORK_EFFORT_TYPE_ID = 'PROJECT_CERTIFICATE'
             THEN header.CERTIFICATE_NUMBER ELSE NULL END AS CertificateNumber,

    -- PaymentId linkage (new from C#)
    CASE WHEN header.WORK_EFFORT_TYPE_ID = 'PAYMENT_CERTIFICATE'
             THEN header.WORK_EFFORT_ID
         ELSE pyt.PAYMENT_ID END AS PaymentId,

    COALESCE(header.PROJECT_ID, item.PROJECT_ID) AS ProjectId,

    COALESCE(header.PARTY_ID_SUPPLIER, header.PARTY_ID_CONTRACTOR, header.PartyIdEmployee) AS PartyId,
    p.DESCRIPTION AS PartyName,

    COALESCE(item.PRODUCT_ID, item.SERVICE_ID) AS ProductId,
    prod.PRODUCT_NAME AS ProductName,   -- assuming column name

    -- Date (coalesced like in C#)
    DATE(COALESCE(item.ProcurementDate, item.ESTIMATED_START_DATE,
                  header.ESTIMATED_START_DATE, header.CREATED_DATE)) AS ExpenseDate,

    -- Record & Type (exact match to C#)
    CASE
        WHEN header.WORK_EFFORT_TYPE_ID = 'PROJECT_CERTIFICATE'
            AND header.CERTIFICATE_CATEGORY IN ('SUPPLY_PROCUREMENT_CERTIFICATE', 'WORKMANSHIP_CONTRACTING_CERTIFICATE')
            THEN 'ProjectCertificate'
        WHEN header.WORK_EFFORT_TYPE_ID = 'PAYMENT_CERTIFICATE' THEN 'MultiPaymentCertificate'
        ELSE 'Other'
        END AS RecordType,

    -- CertificateType and Arabic version (exact from C#)
    CASE
        WHEN header.WORK_EFFORT_TYPE_ID = 'PROJECT_CERTIFICATE' THEN
            CASE header.CERTIFICATE_CATEGORY
                WHEN 'SUPPLY_PROCUREMENT_CERTIFICATE' THEN 'Supply Procurement'
                WHEN 'WORKMANSHIP_CONTRACTING_CERTIFICATE' THEN 'Workmanship Contracting'
                ELSE 'Project Certificate'
                END
        WHEN header.WORK_EFFORT_TYPE_ID = 'PAYMENT_CERTIFICATE' THEN 'Multi-Payment / Direct Expense'
        ELSE 'Unknown'
        END AS CertificateType,

    CASE
        WHEN header.WORK_EFFORT_TYPE_ID = 'PROJECT_CERTIFICATE' THEN
            CASE header.CERTIFICATE_CATEGORY
                WHEN 'SUPPLY_PROCUREMENT_CERTIFICATE' THEN 'توريد مواد'
                WHEN 'WORKMANSHIP_CONTRACTING_CERTIFICATE' THEN 'مقاولات مصنعيات'
                WHEN 'COMPANY_SUPPLY_SALE_CERTIFICATE' THEN 'بيع توريدات الشركة'
                ELSE 'مستخلص مشروع'
                END
        WHEN header.WORK_EFFORT_TYPE_ID = 'PAYMENT_CERTIFICATE'
            THEN 'مستخلص دفعات متعددة / مصاريف مباشرة'
        ELSE 'غير معروف'
        END AS CertificateTypeArabic,

    header.CERTIFICATE_CATEGORY AS CertificateCategoryCode,
    header.DESCRIPTION AS CertificateDescription,
    item.DESCRIPTION AS ItemDescription,
    header.RELATED_ORDER_ID AS RelatedPurchaseOrderId,

    (header.CERTIFICATE_CATEGORY = 'SUPPLY_PROCUREMENT_CERTIFICATE') AS IsSupplyProcurement,
    (header.CERTIFICATE_CATEGORY = 'WORKMANSHIP_CONTRACTING_CERTIFICATE') AS IsWorkmanship,
    (header.WORK_EFFORT_TYPE_ID = 'PAYMENT_CERTIFICATE') AS IsMultiPaymentCertificate,

    COALESCE(item.QUANTITY, 1) AS Quantity,
    item.RATE AS UnitRate,
    COALESCE(item.TOTAL_AMOUNT, item.AMOUNT, 0) AS GrossAmount,
    COALESCE(item.DISCOUNT, 0) AS DiscountAmount,
    COALESCE(item.Deductions, 0) AS DeductionsAmount,
    COALESCE(item.Insurance, 0) AS InsuranceAmount,
    COALESCE(item.TransportationExpenses, 0) AS TransportationExpensesAmount,
    COALESCE(item.Gratuities, 0) AS GratuitiesAmount,

    -- Net amount in SQL (remove C# loop)
    COALESCE(item.TOTAL_AMOUNT, item.AMOUNT, 0)
        - COALESCE(item.DISCOUNT, 0)
        - COALESCE(item.Deductions, 0)
        - COALESCE(item.Insurance, 0)
        + COALESCE(item.TransportationExpenses, 0)
        + COALESCE(item.Gratuities, 0) AS NetCertifiedAmount,

    CASE
        WHEN header.CERTIFICATE_CATEGORY = 'WORKMANSHIP_CONTRACTING_CERTIFICATE'
            THEN COALESCE(item.AchievementPercent, 0)
        WHEN header.CERTIFICATE_CATEGORY = 'SUPPLY_PROCUREMENT_CERTIFICATE'
            OR header.WORK_EFFORT_TYPE_ID = 'PAYMENT_CERTIFICATE'
            THEN 100.0
        ELSE COALESCE(item.AchievementPercent, 0)
        END AS AchievementPercentage,

    item.AchievementPercent AS OriginalAchievementPercent,

    GREATEST(COALESCE(header.LAST_UPDATED_STAMP, '1900-01-01'),
             COALESCE(item.LAST_UPDATED_STAMP, '1900-01-01')) AS LastUpdatedStamp

FROM WORK_EFFORT header
         INNER JOIN WORK_EFFORT item
                    ON item.WORK_EFFORT_PARENT_ID = header.WORK_EFFORT_ID
                        AND item.WORK_EFFORT_TYPE_ID IN ('CERTIFICATE_ITEM', 'PAYMENT_CERTIFICATE_ITEM')

         LEFT JOIN PARTY p
                   ON COALESCE(header.PARTY_ID_SUPPLIER, header.PARTY_ID_CONTRACTOR, header.PartyIdEmployee) = p.PARTY_ID

         LEFT JOIN PRODUCT prod
                   ON COALESCE(item.PRODUCT_ID, item.SERVICE_ID) = prod.PRODUCT_ID

         LEFT JOIN ORDER_PAYMENT_PREFERENCE opp
                   ON header.RELATED_ORDER_ID = opp.ORDER_ID
         LEFT JOIN PAYMENT pyt
                   ON opp.ORDER_PAYMENT_PREFERENCE_ID = pyt.PAYMENT_PREFERENCE_ID

WHERE header.CURRENT_STATUS_ID = 'WEPR_APPROVED'
  AND COALESCE(header.CERTIFICATE_CATEGORY, '') <> 'COMPANY_SUPPLY_SALE_CERTIFICATE'
  AND NOT (header.WORK_EFFORT_TYPE_ID = 'PAYMENT_CERTIFICATE'
    AND COALESCE(header.PROJECT_ID, item.PROJECT_ID) IS NULL);