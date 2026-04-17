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