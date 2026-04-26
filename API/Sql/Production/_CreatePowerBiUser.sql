-- Create (or recreate) the Power BI user
DROP USER IF EXISTS 'powerbi_user'@'%';
CREATE USER 'powerbi_user'@'%' IDENTIFIED BY 'baba1934';

-- Give access ONLY to this view
GRANT SELECT ON InventoryItemsDetails TO 'powerbi_user'@'%';
GRANT SELECT ON DimApartments TO 'powerbi_user'@'%';
GRANT SELECT ON DimContractors TO 'powerbi_user'@'%';
GRANT SELECT ON DimEmployees TO 'powerbi_user'@'%';
GRANT SELECT ON DimCostCenters TO 'powerbi_user'@'%';
GRANT SELECT ON DimProductCategories TO 'powerbi_user'@'%';
GRANT SELECT ON DimPaymentMethods TO 'powerbi_user'@'%';
GRANT SELECT ON DimPaymentMethodTypes TO 'powerbi_user'@'%';
GRANT SELECT ON DimPaymentTypes TO 'powerbi_user'@'%';
GRANT SELECT ON DimProductRawMaterials TO 'powerbi_user'@'%';
GRANT SELECT ON DimProductServices TO 'powerbi_user'@'%';
GRANT SELECT ON DimProjects TO 'powerbi_user'@'%';
GRANT SELECT ON DimProject TO 'powerbi_user'@'%';
GRANT SELECT ON DimSuppliers TO 'powerbi_user'@'%';
GRANT SELECT ON DimStatusItems TO 'powerbi_user'@'%';
GRANT SELECT ON Payments TO 'powerbi_user'@'%';
GRANT SELECT ON BillingAccounts TO 'powerbi_user'@'%';
GRANT SELECT ON AcctgTransactions TO 'powerbi_user'@'%';
GRANT SELECT ON AcctgTransEntries TO 'powerbi_user'@'%';
GRANT SELECT ON PaymentCertificates TO 'powerbi_user'@'%';
GRANT SELECT ON PaymentCertificateItems TO 'powerbi_user'@'%';
GRANT SELECT ON SalesRequests TO 'powerbi_user'@'%';
GRANT SELECT ON ReserveRequests TO 'powerbi_user'@'%';
GRANT SELECT ON Payments_2 TO 'powerbi_user'@'%';
GRANT SELECT ON DimParties TO 'powerbi_user'@'%';
GRANT SELECT ON DimProducts TO 'powerbi_user'@'%';
GRANT SELECT ON Fact_Project_Expenses TO 'powerbi_user'@'%';
GRANT SELECT ON Fact_Project_OperatingExpenses TO 'powerbi_user'@'%';
GRANT SELECT ON Fact_Project_OperatingExpenses_2 TO 'powerbi_user'@'%';
GRANT SELECT ON Fact_Project_DirectPayments TO 'powerbi_user'@'%';
GRANT SELECT ON Fact_Project_DirectPayments_2 TO 'powerbi_user'@'%';
GRANT SELECT ON Fact_Project_Revenues TO 'powerbi_user'@'%';
GRANT SELECT ON Fact_Apartment_Payments TO 'powerbi_user'@'%';
GRANT SELECT ON Fact_Apartment_Installment_Payment_Link TO 'powerbi_user'@'%';

-- Clean slate – remove any accidental broader privileges
GRANT USAGE ON *.* TO 'powerbi_user'@'%';

FLUSH PRIVILEGES;