-- Create (or recreate) the Power BI user
DROP USER IF EXISTS 'powerbi2_user'@'%';
CREATE USER 'powerbi2_user'@'%' IDENTIFIED BY 'baba1934';


GRANT SELECT ON DimProductCategories     TO 'powerbi2_user'@'%';
GRANT SELECT ON DimProductRawMaterials   TO 'powerbi2_user'@'%';
GRANT SELECT ON DimProductServices       TO 'powerbi2_user'@'%';
GRANT SELECT ON DimProjects              TO 'powerbi2_user'@'%';
GRANT SELECT ON DimSuppliers             TO 'powerbi2_user'@'%';
GRANT SELECT ON ProjectCertificates_2    TO 'powerbi2_user'@'%';
GRANT SELECT ON ProjectCertificatesV2      TO 'powerbi2_user'@'%';
GRANT SELECT ON ProjectCertificateItemsV2  TO 'powerbi2_user'@'%';
GRANT SELECT ON Payments_2  TO 'powerbi2_user'@'%';
GRANT SELECT ON DimParties  TO 'powerbi2_user'@'%';
GRANT SELECT ON DimProducts  TO 'powerbi2_user'@'%';
GRANT SELECT ON Fact_Projects_Expenses  TO 'powerbi2_user'@'%';
GRANT SELECT ON Fact_Apartment_Payments  TO 'powerbi2_user'@'%';


-- =============================================================
-- Flush privileges (recommended)
-- =============================================================
FLUSH PRIVILEGES;

-- =============================================================
-- Verify the grants
-- =============================================================
SHOW GRANTS FOR 'powerbi2_user'@'%';