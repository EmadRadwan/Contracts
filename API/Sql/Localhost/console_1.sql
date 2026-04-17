SELECT
    g.GL_ACCOUNT_ID,
    g.ACCOUNT_CODE,
    g.ACCOUNT_NAME,
    g.ACCOUNT_NAME_ARABIC,
    g.DESCRIPTION,
    g.GL_ACCOUNT_TYPE_ID,
    g.GL_ACCOUNT_CLASS_ID
FROM
    GL_ACCOUNT g
        INNER JOIN
    GL_ACCOUNT_ORGANIZATION org
    ON g.GL_ACCOUNT_ID = org.GL_ACCOUNT_ID
WHERE
        g.PARENT_GL_ACCOUNT_ID = '210000';

SELECT
    party_id,
    description
FROM
    Party
WHERE
        description LIKE '%احمد صالح%'
   OR description LIKE '%اشرف كمال%'
   OR description LIKE '%محمد أبو جامع%'
   OR description LIKE '%رضا فتحى%'
   OR description LIKE '%احمد سراج%'
   OR description LIKE '%احمد عاشور%'
   OR description LIKE '%فاطمة نبيل%'
   OR description LIKE '%محمد الصعيدى%'
   OR description LIKE '%علاء خاطر%'
   OR description LIKE '%رامى حسين%'
   OR description LIKE '%قطب مختار%'
   OR description LIKE '%محمد القاضي%'
ORDER BY
    description;

INSERT INTO PARTY_GL_ACCOUNT (
    ORGANIZATION_PARTY_ID,
    PARTY_ID,
    ROLE_TYPE_ID,
    GL_ACCOUNT_TYPE_ID,
    GL_ACCOUNT_ID,
    LAST_UPDATED_STAMP,
    LAST_UPDATED_TX_STAMP,
    CREATED_STAMP,
    CREATED_TX_STAMP
) VALUES
      ('Company', '34', 'BILL_TO_CUSTOMER', 'ACCOUNTS_RECEIVABLE', '1211005', '2026-01-30 17:00:00', '2026-01-30 17:00:00', '2026-01-30 17:00:00', '2026-01-30 17:00:00'),
      ('Company', '30', 'BILL_TO_CUSTOMER', 'ACCOUNTS_RECEIVABLE', '1211001', '2026-01-30 17:00:00', '2026-01-30 17:00:00', '2026-01-30 17:00:00', '2026-01-30 17:00:00'),
      ('Company', '35', 'BILL_TO_CUSTOMER', 'ACCOUNTS_RECEIVABLE', '1211006', '2026-01-30 17:00:00', '2026-01-30 17:00:00', '2026-01-30 17:00:00', '2026-01-30 17:00:00'),
      ('Company', '31', 'BILL_TO_CUSTOMER', 'ACCOUNTS_RECEIVABLE', '1211002', '2026-01-30 17:00:00', '2026-01-30 17:00:00', '2026-01-30 17:00:00', '2026-01-30 17:00:00'),
      ('Company', '39', 'BILL_TO_CUSTOMER', 'ACCOUNTS_RECEIVABLE', '1211010', '2026-01-30 17:00:00', '2026-01-30 17:00:00', '2026-01-30 17:00:00', '2026-01-30 17:00:00'),
      ('Company', '33', 'BILL_TO_CUSTOMER', 'ACCOUNTS_RECEIVABLE', '1211004', '2026-01-30 17:00:00', '2026-01-30 17:00:00', '2026-01-30 17:00:00', '2026-01-30 17:00:00'),
      ('Company', '38', 'BILL_TO_CUSTOMER', 'ACCOUNTS_RECEIVABLE', '1211009', '2026-01-30 17:00:00', '2026-01-30 17:00:00', '2026-01-30 17:00:00', '2026-01-30 17:00:00'),
      ('Company', '36', 'BILL_TO_CUSTOMER', 'ACCOUNTS_RECEIVABLE', '1211007', '2026-01-30 17:00:00', '2026-01-30 17:00:00', '2026-01-30 17:00:00', '2026-01-30 17:00:00'),
      ('Company', '40', 'BILL_TO_CUSTOMER', 'ACCOUNTS_RECEIVABLE', '1211011', '2026-01-30 17:00:00', '2026-01-30 17:00:00', '2026-01-30 17:00:00', '2026-01-30 17:00:00'),
      ('Company', '32', 'BILL_TO_CUSTOMER', 'ACCOUNTS_RECEIVABLE', '1211003', '2026-01-30 17:00:00', '2026-01-30 17:00:00', '2026-01-30 17:00:00', '2026-01-30 17:00:00'),
      ('Company', '37', 'BILL_TO_CUSTOMER', 'ACCOUNTS_RECEIVABLE', '1211008', '2026-01-30 17:00:00', '2026-01-30 17:00:00', '2026-01-30 17:00:00', '2026-01-30 17:00:00'),
      ('Company', '41', 'BILL_TO_CUSTOMER', 'ACCOUNTS_RECEIVABLE', '1211012', '2026-01-30 17:00:00', '2026-01-30 17:00:00', '2026-01-30 17:00:00', '2026-01-30 17:00:00');

SELECT
    pga.ORGANIZATION_PARTY_ID,
    pga.PARTY_ID,
    p.description                  AS party_description,
    pga.ROLE_TYPE_ID,
    pga.GL_ACCOUNT_TYPE_ID,
    pga.GL_ACCOUNT_ID,
    g.ACCOUNT_CODE,
    g.ACCOUNT_NAME_ARABIC          AS gl_account_name_arabic,
    g.PARENT_GL_ACCOUNT_ID,
    pga.LAST_UPDATED_STAMP,
    pga.CREATED_STAMP
FROM
    PARTY_GL_ACCOUNT pga
        INNER JOIN
    Party p
    ON pga.PARTY_ID = p.party_id
        INNER JOIN
    GL_ACCOUNT g
    ON pga.GL_ACCOUNT_ID = g.GL_ACCOUNT_ID
WHERE
        pga.ORGANIZATION_PARTY_ID = 'Company'
  AND pga.ROLE_TYPE_ID = 'BILL_TO_CUSTOMER'
  AND pga.GL_ACCOUNT_TYPE_ID = 'ACCOUNTS_RECEIVABLE'
  AND g.PARENT_GL_ACCOUNT_ID = '121100'          -- only the sub-accounts you're working with
ORDER BY
    p.description,
    g.ACCOUNT_CODE;

SELECT
    PARTY_ID,
    DESCRIPTION
FROM Party
WHERE PARTY_ID IN ('182', '205', '222', '185', '181')
ORDER BY FIELD(PARTY_ID, '182', '205', '222', '185', '181');