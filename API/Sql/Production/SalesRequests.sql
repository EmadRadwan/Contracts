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