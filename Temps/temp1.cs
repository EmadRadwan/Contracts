DROP VIEW IF EXISTS Fact_Apartment_Payments;

CREATE OR REPLACE VIEW Fact_Apartment_Payments AS
SELECT 
    p.PAYMENT_ID                                 AS PaymentId,
    p.SALES_REQUEST_ID                           AS SalesRequestId,
    sr.PRODUCT_ID                                AS ApartmentId,

    apt.PROJECT_ID                               AS ProjectId,
    proj.ProjectName,

    p.PARTY_ID_FROM                              AS CustomerPartyId,
    pf.DESCRIPTION                               AS CustomerName,

    p.PAYMENT_TYPE_ID,
    pt_type.DESCRIPTION_ARABIC                   AS PaymentTypeArabic,

    CASE p.PAYMENT_TYPE_ID 
        WHEN 'RECEIPT_ADVANCE_PAYMENT'    THEN 'Advance Payment'
        WHEN 'RECEIPT_DUE_INSTALLMENT'    THEN 'Installment'
        WHEN 'RECEIPT_MAINTENANCE_AMOUNT' THEN 'Maintenance Deposit'
        ELSE 'Other' 
    END                                          AS RevenueCategory,

    COALESCE(p.AMOUNT, 0)                        AS ScheduledAmount,

    -- Amount Split (Best for Power BI)
    CASE WHEN p.STATUS_ID IN ('PMNT_RECEIVED', 'PMNT_PAID') 
         THEN COALESCE(p.AMOUNT, 0) ELSE 0 END   AS CollectedAmount,

    CASE WHEN p.STATUS_ID NOT IN ('PMNT_RECEIVED', 'PMNT_PAID') 
         THEN COALESCE(p.AMOUNT, 0) ELSE 0 END   AS OutstandingAmount,

    CASE WHEN p.STATUS_ID NOT IN ('PMNT_RECEIVED', 'PMNT_PAID') 
             AND DATE(p.EFFECTIVE_DATE) < CURDATE() 
         THEN COALESCE(p.AMOUNT, 0) ELSE 0 END   AS LateAmount,

    CASE WHEN p.STATUS_ID NOT IN ('PMNT_RECEIVED', 'PMNT_PAID') 
             AND DATE(p.EFFECTIVE_DATE) >= CURDATE() 
         THEN COALESCE(p.AMOUNT, 0) ELSE 0 END   AS FutureAmount,

    -- ==================== IMPROVED STATUS LOGIC ====================
    CASE 
        WHEN p.STATUS_ID IN ('PMNT_RECEIVED', 'PMNT_PAID') THEN 'Received'
        WHEN DATE(p.EFFECTIVE_DATE) < CURDATE()            THEN 'Late'
        ELSE 'Upcoming'                                      -- Better than "Due"
    END                                              AS PaymentStatus,

    -- Detailed Bucket (Clean & Business Friendly)
    CASE 
        WHEN p.STATUS_ID IN ('PMNT_RECEIVED', 'PMNT_PAID') THEN 'Received'
        WHEN DATE(p.EFFECTIVE_DATE) >= CURDATE()           THEN 'Upcoming'
        WHEN DATEDIFF(CURDATE(), DATE(p.EFFECTIVE_DATE)) BETWEEN 1 AND 30 THEN 'Late (1-30 Days)'
        WHEN DATEDIFF(CURDATE(), DATE(p.EFFECTIVE_DATE)) BETWEEN 31 AND 90 THEN 'Late (31-90 Days)'
        ELSE 'Late (Over 90 Days)'
    END                                              AS OverdueBucket,

    -- Days Overdue (Only positive for late payments, 0 otherwise)
    CASE 
        WHEN p.STATUS_ID NOT IN ('PMNT_RECEIVED', 'PMNT_PAID') 
             AND DATE(p.EFFECTIVE_DATE) < CURDATE() 
            THEN DATEDIFF(CURDATE(), DATE(p.EFFECTIVE_DATE))
        ELSE 0 
    END                                              AS DaysOverdue,

    -- Dates
    DATE(p.EFFECTIVE_DATE)                           AS DueDateKey,
    DATE(p.CREATED_STAMP)                            AS CreatedDateKey,

    p.COMMENTS,
    p.ChequeNumber

FROM PAYMENT p
LEFT JOIN PARTY pf              ON p.PARTY_ID_FROM = pf.PARTY_ID
LEFT JOIN PAYMENT_TYPE pt_type  ON p.PAYMENT_TYPE_ID = pt_type.PAYMENT_TYPE_ID
LEFT JOIN SALES_REQUEST sr      ON p.SALES_REQUEST_ID = sr.SALES_REQUEST_ID
LEFT JOIN PRODUCT apt           ON sr.PRODUCT_ID = apt.PRODUCT_ID
LEFT JOIN DimProjects proj      ON COALESCE(apt.PROJECT_ID, p.WORK_EFFORT_ID) = proj.ProjectId

WHERE p.PARTY_ID_TO = 'Company'
  AND p.SALES_REQUEST_ID IS NOT NULL
  AND p.PAYMENT_TYPE_ID IN ('RECEIPT_ADVANCE_PAYMENT', 
                            'RECEIPT_DUE_INSTALLMENT', 
                            'RECEIPT_MAINTENANCE_AMOUNT')
  AND COALESCE(p.AMOUNT, 0) > 0;