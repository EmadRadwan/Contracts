-- =========================================
-- DELETE ALL PAYROLL INVOICES SAFELY
-- =========================================

-- Start transaction
START TRANSACTION;

-- -----------------------------------------
-- STEP 1: Preview (VERY IMPORTANT)
-- -----------------------------------------
SELECT COUNT(*) AS invoice_count
FROM INVOICE
WHERE INVOICE_TYPE_ID = 'PAYROL_INVOICE';

-- Optional: preview actual records
SELECT INVOICE_ID
FROM INVOICE
WHERE INVOICE_TYPE_ID = 'PAYROL_INVOICE';


-- -----------------------------------------
-- STEP 2: Create temp table (avoids MySQL issues)
-- -----------------------------------------
CREATE TEMPORARY TABLE TMP_PAYROLL_INV AS
SELECT INVOICE_ID
FROM INVOICE
WHERE INVOICE_TYPE_ID = 'PAYROL_INVOICE';


-- -----------------------------------------
-- STEP 3: Delete child records
-- -----------------------------------------

-- Invoice items
DELETE FROM INVOICE_ITEM
WHERE INVOICE_ID IN (SELECT INVOICE_ID FROM TMP_PAYROLL_INV);

-- Invoice roles
DELETE FROM INVOICE_ROLE
WHERE INVOICE_ID IN (SELECT INVOICE_ID FROM TMP_PAYROLL_INV);

-- Invoice status
DELETE FROM INVOICE_STATUS
WHERE INVOICE_ID IN (SELECT INVOICE_ID FROM TMP_PAYROLL_INV);


-- -----------------------------------------
-- STEP 4: Delete accounting data
-- -----------------------------------------

-- Accounting entries
DELETE ate
FROM ACCTG_TRANS_ENTRY ate
         INNER JOIN ACCTG_TRANS at
                    ON ate.ACCTG_TRANS_ID = at.ACCTG_TRANS_ID
WHERE at.INVOICE_ID IN (SELECT INVOICE_ID FROM TMP_PAYROLL_INV);

-- Accounting transactions
DELETE FROM ACCTG_TRANS
WHERE INVOICE_ID IN (SELECT INVOICE_ID FROM TMP_PAYROLL_INV);


-- -----------------------------------------
-- STEP 5: Delete invoices
-- -----------------------------------------
DELETE FROM INVOICE
WHERE INVOICE_ID IN (SELECT INVOICE_ID FROM TMP_PAYROLL_INV);


-- -----------------------------------------
-- STEP 6: VERIFY BEFORE COMMIT
-- -----------------------------------------
SELECT COUNT(*) AS remaining
FROM INVOICE
WHERE INVOICE_TYPE_ID = 'PAYROL_INVOICE';


-- -----------------------------------------
-- FINAL STEP: COMMIT OR ROLLBACK
-- -----------------------------------------

-- If everything looks correct:
COMMIT;

-- If something is wrong:
-- ROLLBACK;