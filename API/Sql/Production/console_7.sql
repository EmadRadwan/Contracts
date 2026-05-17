-- 1. Update Land Inventory Label (Multiplier = 1)
UPDATE `GL_ACCOUNT_COURSE_LABEL`
SET
    `DESCRIPTION` = 'Land Inventory',
    `DESCRIPTION_ARABIC` = 'مخزون أراضي',
    `SIGN_MULTIPLIER` = 1
WHERE `GL_ACCOUNT_COURSE_LABEL_ID` = 'INVENTORY_LANDS';

-- 2. Update Land Liability Label (Multiplier = -1)
UPDATE `GL_ACCOUNT_COURSE_LABEL`
SET
    `DESCRIPTION` = 'Land Payables / Creditors',
    `DESCRIPTION_ARABIC` = 'دائنو أراضي',
    `SIGN_MULTIPLIER` = -1
WHERE `GL_ACCOUNT_COURSE_LABEL_ID` = 'PAYABLES_LANDS';

-- 3. Update Stock Liability Label (Multiplier = -1)
UPDATE `GL_ACCOUNT_COURSE_LABEL`
SET
    `DESCRIPTION` = 'Stock Purchase Payables',
    `DESCRIPTION_ARABIC` = 'دائنو شراء أسهم',
    `SIGN_MULTIPLIER` = -1
WHERE `GL_ACCOUNT_COURSE_LABEL_ID` = 'PAYABLES_STOCKS';