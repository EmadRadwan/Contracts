-- Land purchase item type for purchase invoices + vendor GL mapping for Issa Al-Nomisi (party 193).
-- Client request 2026-10-05: "اضافة بند شراء اراضى - مدين اراضى بغرض الاستثمار - دائن عيسى النميسى حساب رقم 125100"
--
-- Resulting posting for a purchase invoice from party 193 with a PINV_LAND_ITEM line:
--   D 140704  اراضى بغرض الاستثمار                     (item type default GL)
--   C 125100  عيسى النميسى بيع 1120 متر ارض الثروة ...  (PARTY_GL_ACCOUNT, replaces generic AP 210000)
-- Cheques O18281 (2,204,000) + O18282 (10,000,000) already debit 125100, so INV1775 (12,204,000) nets it to zero.
--
-- No code change needed: GetInvoiceItemTypesByInvoiceId lists children of PINV_PROD_ITEM for
-- purchase invoices, and the GL resolver checks PARTY_GL_ACCOUNT before the AP default.
-- UPPERCASE names: prod Linux MySQL is case-sensitive.

START TRANSACTION;

-- 1. Item type: debit side
INSERT INTO INVOICE_ITEM_TYPE
    (INVOICE_ITEM_TYPE_ID, PARENT_TYPE_ID, HAS_TABLE, DESCRIPTION, DESCRIPTION_ARABIC,
     DEFAULT_GL_ACCOUNT_ID, IS_POSITIVE_AMOUNT,
     LAST_UPDATED_STAMP, LAST_UPDATED_TX_STAMP, CREATED_STAMP, CREATED_TX_STAMP)
VALUES
    ('PINV_LAND_ITEM', 'PINV_PROD_ITEM', 'N', 'Invoice Land Purchase Item', 'بند شراء أراضي',
     '140704', 1,
     NOW(), NOW(), NOW(), NOW());

-- 2. Vendor role for party 193 (required by FK PRTYGLACCT_PTRL; party is currently customer-only)
INSERT IGNORE INTO PARTY_ROLE
    (PARTY_ID, ROLE_TYPE_ID, LAST_UPDATED_STAMP, CREATED_STAMP)
VALUES
    ('193', 'BILL_FROM_VENDOR', NOW(), NOW());

-- 3. Credit side: party 193's payables go to 125100 instead of 210000
INSERT INTO PARTY_GL_ACCOUNT
    (ORGANIZATION_PARTY_ID, PARTY_ID, ROLE_TYPE_ID, GL_ACCOUNT_TYPE_ID, GL_ACCOUNT_ID,
     LAST_UPDATED_STAMP, LAST_UPDATED_TX_STAMP, CREATED_STAMP, CREATED_TX_STAMP)
VALUES
    ('Company', '193', 'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '125100',
     NOW(), NOW(), NOW(), NOW());

-- 4. Re-type the INV1775 line (still INVOICE_IN_PROCESS, no ledger rows yet). Can also be done
--    from the UI item-type dialog instead; skip this statement if the user already did it.
UPDATE INVOICE_ITEM
SET INVOICE_ITEM_TYPE_ID = 'PINV_LAND_ITEM', LAST_UPDATED_STAMP = NOW()
WHERE INVOICE_ID = 'INV1775' AND INVOICE_ITEM_SEQ_ID = '01'
  AND INVOICE_ITEM_TYPE_ID = 'PINV_FXASTPRD_ITEM';

-- Verify before committing
SELECT INVOICE_ITEM_TYPE_ID, PARENT_TYPE_ID, DEFAULT_GL_ACCOUNT_ID, DESCRIPTION_ARABIC
FROM INVOICE_ITEM_TYPE WHERE INVOICE_ITEM_TYPE_ID = 'PINV_LAND_ITEM';
SELECT * FROM PARTY_GL_ACCOUNT WHERE PARTY_ID = '193';
SELECT INVOICE_ID, INVOICE_ITEM_SEQ_ID, INVOICE_ITEM_TYPE_ID, OVERRIDE_GL_ACCOUNT_ID, AMOUNT
FROM INVOICE_ITEM WHERE INVOICE_ID = 'INV1775';

COMMIT;
