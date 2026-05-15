-- 1. Fix the Label Mismatch so Dokki Apartment appears in Inventory reports
UPDATE GL_ACCOUNT
SET GL_ACCOUNT_COURSE_LABEL_ID = 'INVENTORY',
    ACCOUNT_NAME = 'DOKKI APT - SHEIKH ABDULAZIZ'
WHERE GL_ACCOUNT_ID = '124438';

-- 2. Consistency fix for Zayed 3 Arabic name
UPDATE GL_ACCOUNT
SET ACCOUNT_NAME_ARABIC = 'زايد 3'
WHERE GL_ACCOUNT_ID = '140701';

-- 3. Professional Spelling for Parent (تنفيد -> تنفيذ)
UPDATE GL_ACCOUNT
SET ACCOUNT_NAME_ARABIC = 'اعمال تحت التنفيذ للغير'
WHERE GL_ACCOUNT_ID = '140700';