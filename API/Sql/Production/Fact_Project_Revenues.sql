CREATE OR REPLACE VIEW Fact_Project_Revenues AS
SELECT
    p.PAYMENT_ID                                      AS PaymentId,
    p.SALES_REQUEST_ID                                AS SalesRequestId,
    sr.PRODUCT_ID                                     AS ApartmentId,
    apt.BUILDING_NUMBER                               AS BuildingNumber,          -- Added from C#

    -- Project
    COALESCE(apt.PROJECT_ID, p.WORK_EFFORT_ID)        AS ProjectId,
    proj.PROJECT_NAME                                 AS ProjectName,             -- From WorkEffort to match C#

    -- Customer
    p.PARTY_ID_FROM                                   AS CustomerPartyId,
    pf.DESCRIPTION                                    AS CustomerName,

    -- Payment Type
    p.PAYMENT_TYPE_ID,
    pt_type.DESCRIPTION_ARABIC                        AS PaymentTypeArabic,

    CASE p.PAYMENT_TYPE_ID
        WHEN 'RECEIPT_ADVANCE_PAYMENT'    THEN 'Advance Payment'
        WHEN 'RECEIPT_DUE_INSTALLMENT'    THEN 'Installment'
        WHEN 'RECEIPT_MAINTENANCE_AMOUNT' THEN 'Maintenance Deposit'
        ELSE 'Other'
        END                                               AS RevenueCategory,

    -- Amounts
    COALESCE(p.AMOUNT, 0)                             AS ScheduledAmount,

    CASE WHEN p.STATUS_ID = 'PMNT_RECEIVED'
             THEN COALESCE(p.AMOUNT, 0) ELSE 0 END    AS CollectedAmount,

    CASE WHEN p.STATUS_ID != 'PMNT_RECEIVED' 
             THEN COALESCE(p.AMOUNT, 0) ELSE 0 END    AS OutstandingAmount,

    CASE WHEN p.STATUS_ID != 'PMNT_RECEIVED'
        AND p.EFFECTIVE_DATE < CURDATE()
    THEN COALESCE(p.AMOUNT, 0) ELSE 0 END    AS LateAmount,

    CASE WHEN p.STATUS_ID != 'PMNT_RECEIVED' 
             AND p.EFFECTIVE_DATE >= CURDATE() 
             THEN COALESCE(p.AMOUNT, 0) ELSE 0 END    AS FutureAmount,

    -- Status
    p.STATUS_ID                                       AS StatusId,
    COALESCE(sts.DESCRIPTION_ARABIC, p.STATUS_ID)     AS StatusDescription,      -- Added from C#

    -- Payment Status (English)
    CASE
        WHEN p.STATUS_ID = 'PMNT_RECEIVED'                  THEN 'Received'
        WHEN p.EFFECTIVE_DATE < CURDATE()                   THEN 'Late'
        ELSE 'Upcoming'
END                                               AS PaymentStatus,

    -- Overdue Bucket + DaysOverdue (exact match to C# logic)
    CASE
        WHEN p.STATUS_ID = 'PMNT_RECEIVED'                  THEN 'Received'
        WHEN p.EFFECTIVE_DATE >= CURDATE()                  THEN 'Upcoming'
        WHEN DATEDIFF(CURDATE(), p.EFFECTIVE_DATE) BETWEEN 1 AND 30  THEN 'Late (1-30 Days)'
        WHEN DATEDIFF(CURDATE(), p.EFFECTIVE_DATE) BETWEEN 31 AND 90 THEN 'Late (31-90 Days)'
        ELSE 'Late (Over 90 Days)'
END                                               AS OverdueBucket,

    CASE
        WHEN p.STATUS_ID != 'PMNT_RECEIVED' AND p.EFFECTIVE_DATE < CURDATE()
            THEN DATEDIFF(CURDATE(), p.EFFECTIVE_DATE)
        ELSE 0
END                                               AS DaysOverdue,

    -- Dates
    p.EFFECTIVE_DATE                                  AS DueDate,                 -- DATE type to match C#
    p.CREATED_STAMP                                   AS CreatedDate,

    p.COMMENTS,
    p.CHEQUENUMBER,

    -- Rich Arabic Due Status (translated from C# CalculateDueStatusArabic)
    CASE
        WHEN p.STATUS_ID != 'PMNT_NOT_PAID' AND p.STATUS_ID IS NOT NULL 
            THEN COALESCE(sts.DESCRIPTION_ARABIC, p.STATUS_ID)

        WHEN p.EFFECTIVE_DATE IS NULL 
            THEN 'غير محدد'

        ELSE 
            CASE
                WHEN DATEDIFF(p.EFFECTIVE_DATE, CURDATE()) < 0 THEN
                    CASE
                        WHEN ABS(DATEDIFF(p.EFFECTIVE_DATE, CURDATE())) = 1 THEN 'مستحق متأخر بيوم واحد'
                        WHEN ABS(DATEDIFF(p.EFFECTIVE_DATE, CURDATE())) = 2 THEN 'مستحق متأخر بيومين'
                        WHEN ABS(DATEDIFF(p.EFFECTIVE_DATE, CURDATE())) <= 30 
                            THEN CONCAT('مستحق متأخر منذ ', ABS(DATEDIFF(p.EFFECTIVE_DATE, CURDATE())), ' يوم')
                        ELSE CONCAT('مستحق متأخر جداً (الربع ', 
                                    CASE ((MONTH(p.EFFECTIVE_DATE)-1) DIV 3 + 1)
                                        WHEN 1 THEN 'الأول' WHEN 2 THEN 'الثاني' 
                                        WHEN 3 THEN 'الثالث' WHEN 4 THEN 'الرابع' 
                                    END, ' ', YEAR(p.EFFECTIVE_DATE), ')')
END

WHEN DATEDIFF(p.EFFECTIVE_DATE, CURDATE()) = 0 THEN 'مستحق اليوم'
                WHEN DATEDIFF(p.EFFECTIVE_DATE, CURDATE()) = 1 THEN 'مستحق غداً'
                WHEN DATEDIFF(p.EFFECTIVE_DATE, CURDATE()) <= 3 THEN CONCAT('مستحق بعد ', DATEDIFF(p.EFFECTIVE_DATE, CURDATE()), ' أيام')
                WHEN DATEDIFF(p.EFFECTIVE_DATE, CURDATE()) <= 7 THEN 'مستحق هذا الأسبوع'
                WHEN DATEDIFF(p.EFFECTIVE_DATE, CURDATE()) <= 30 THEN 'مستحق خلال الشهر'
                WHEN DATEDIFF(p.EFFECTIVE_DATE, CURDATE()) <= 90 THEN CONCAT('مستحق خلال 3 أشهر (الربع ', 
                                    CASE ((MONTH(p.EFFECTIVE_DATE)-1) DIV 3 + 1)
                                        WHEN 1 THEN 'الأول' WHEN 2 THEN 'الثاني' 
                                        WHEN 3 THEN 'الثالث' WHEN 4 THEN 'الرابع' 
                                    END, ' ', YEAR(p.EFFECTIVE_DATE), ')')
                ELSE CONCAT('مستحق لاحقاً (الربع ', 
                                    CASE ((MONTH(p.EFFECTIVE_DATE)-1) DIV 3 + 1)
                                        WHEN 1 THEN 'الأول' WHEN 2 THEN 'الثاني' 
                                        WHEN 3 THEN 'الثالث' WHEN 4 THEN 'الرابع' 
                                    END, ' ', YEAR(p.EFFECTIVE_DATE), ')')
END
END                                               AS DueStatusArabic

FROM PAYMENT p
LEFT JOIN PARTY pf              ON p.PARTY_ID_FROM = pf.PARTY_ID
LEFT JOIN PAYMENT_TYPE pt_type  ON p.PAYMENT_TYPE_ID = pt_type.PAYMENT_TYPE_ID
LEFT JOIN STATUS_ITEM sts       ON p.STATUS_ID = sts.STATUS_ID                     -- Added for Arabic status
LEFT JOIN SALES_REQUEST sr      ON p.SALES_REQUEST_ID = sr.SALES_REQUEST_ID
LEFT JOIN PRODUCT apt           ON sr.PRODUCT_ID = apt.PRODUCT_ID
LEFT JOIN WORK_EFFORT proj      ON COALESCE(apt.PROJECT_ID, p.WORK_EFFORT_ID) = proj.WORK_EFFORT_ID

WHERE p.PARTY_ID_TO = 'Company'
  AND p.SALES_REQUEST_ID IS NOT NULL
  AND p.PAYMENT_TYPE_ID IN ('RECEIPT_ADVANCE_PAYMENT', 'RECEIPT_DUE_INSTALLMENT', 'RECEIPT_MAINTENANCE_AMOUNT')
  AND COALESCE(p.AMOUNT, 0) > 0;