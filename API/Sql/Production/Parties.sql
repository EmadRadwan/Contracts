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