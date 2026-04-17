-- Create (or recreate) the Power BI user
DROP USER IF EXISTS 'powerbi_user'@'%';
CREATE USER 'powerbi_user'@'%' IDENTIFIED BY 'baba1934';

-- Give access ONLY to this view
GRANT SELECT ON InventoryItemDetails TO 'powerbi_user'@'%';
GRANT SELECT ON ProjectCertificates TO 'powerbi_user'@'%';
GRANT SELECT ON ProjectCertificateItems TO 'powerbi_user'@'%';
GRANT SELECT ON ProjectCertificatesWithItems TO 'powerbi_user'@'%';

-- Clean slate – remove any accidental broader privileges
GRANT USAGE ON *.* TO 'powerbi_user'@'%';

FLUSH PRIVILEGES;