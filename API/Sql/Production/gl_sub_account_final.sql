-- =============================================================================
-- STEP 1: Fix the column definition (type is 'unknown' — needs proper type)
-- =============================================================================

-- =============================================================================
-- STEP 2: UPDATE using ACCOUNT_NAME_ARABIC
-- =============================================================================

-- =====================================================================
-- RECEIVABLES (258 accounts)
-- =====================================================================

UPDATE GL_ACCOUNT ga
JOIN GL_ACCOUNT parent ON parent.GL_ACCOUNT_ID = ga.PARENT_GL_ACCOUNT_ID
    AND parent.ACCOUNT_NAME_ARABIC = 'عملاء مشاريع تحت التنفيذ'
SET ga.GL_SUB_ACCOUNT_COURSE_LABEL_ID = 'Project Receivables'
WHERE ga.GL_ACCOUNT_COURSE_LABEL_ID = 'RECEIVABLES';

UPDATE GL_ACCOUNT ga
JOIN GL_ACCOUNT parent ON parent.GL_ACCOUNT_ID = ga.PARENT_GL_ACCOUNT_ID
    AND parent.ACCOUNT_NAME_ARABIC = 'ذمم الموظفين'
SET ga.GL_SUB_ACCOUNT_COURSE_LABEL_ID = 'Staff Receivables'
WHERE ga.GL_ACCOUNT_COURSE_LABEL_ID = 'RECEIVABLES';

UPDATE GL_ACCOUNT ga
JOIN GL_ACCOUNT parent ON parent.GL_ACCOUNT_ID = ga.PARENT_GL_ACCOUNT_ID
    AND parent.ACCOUNT_NAME_ARABIC = 'مدينو الإيجارات'
SET ga.GL_SUB_ACCOUNT_COURSE_LABEL_ID = 'Rent Receivables'
WHERE ga.GL_ACCOUNT_COURSE_LABEL_ID = 'RECEIVABLES';

UPDATE GL_ACCOUNT ga
JOIN GL_ACCOUNT parent ON parent.GL_ACCOUNT_ID = ga.PARENT_GL_ACCOUNT_ID
    AND parent.ACCOUNT_NAME_ARABIC = 'في الطريق من معالجات بطاقات الائتمان'
SET ga.GL_SUB_ACCOUNT_COURSE_LABEL_ID = 'Credit Card Transit'
WHERE ga.GL_ACCOUNT_COURSE_LABEL_ID = 'RECEIVABLES';

UPDATE GL_ACCOUNT ga
JOIN GL_ACCOUNT parent ON parent.GL_ACCOUNT_ID = ga.PARENT_GL_ACCOUNT_ID
    AND parent.ACCOUNT_NAME_ARABIC IN ('العملاء', 'عملاء إدارة العقارات')
SET ga.GL_SUB_ACCOUNT_COURSE_LABEL_ID = 'Trade Receivables'
WHERE ga.GL_ACCOUNT_COURSE_LABEL_ID = 'RECEIVABLES';

-- =====================================================================
-- TRADE PAYABLES (347 accounts)
-- =====================================================================

UPDATE GL_ACCOUNT ga
JOIN GL_ACCOUNT parent ON parent.GL_ACCOUNT_ID = ga.PARENT_GL_ACCOUNT_ID
    AND parent.ACCOUNT_NAME_ARABIC = 'الدائنون'
SET ga.GL_SUB_ACCOUNT_COURSE_LABEL_ID = 'Trade Payables'
WHERE ga.GL_ACCOUNT_COURSE_LABEL_ID = 'TRADE_PAYABLES';

-- =====================================================================
-- OTHER PAYABLES (135 accounts)
-- =====================================================================

UPDATE GL_ACCOUNT ga
JOIN GL_ACCOUNT parent ON parent.GL_ACCOUNT_ID = ga.PARENT_GL_ACCOUNT_ID
    AND parent.ACCOUNT_NAME_ARABIC = 'المصاريف المستحقة'
SET ga.GL_SUB_ACCOUNT_COURSE_LABEL_ID = 'Accrued Expenses'
WHERE ga.GL_ACCOUNT_COURSE_LABEL_ID = 'OTHER_PAYABLES';

UPDATE GL_ACCOUNT ga
JOIN GL_ACCOUNT parent ON parent.GL_ACCOUNT_ID = ga.PARENT_GL_ACCOUNT_ID
    AND (parent.ACCOUNT_NAME_ARABIC LIKE '%مشاركات ايجار عقارات%'
      OR parent.ACCOUNT_NAME_ARABIC = 'مشاركات ايراد ايجار عقارات')
SET ga.GL_SUB_ACCOUNT_COURSE_LABEL_ID = 'Rental Partnerships'
WHERE ga.GL_ACCOUNT_COURSE_LABEL_ID = 'OTHER_PAYABLES';

UPDATE GL_ACCOUNT ga
JOIN GL_ACCOUNT parent ON parent.GL_ACCOUNT_ID = ga.PARENT_GL_ACCOUNT_ID
    AND parent.ACCOUNT_NAME_ARABIC IN ('مشاركات المشاريع تحت التنفيذ', 'مشاركات مشروع الواحات')
SET ga.GL_SUB_ACCOUNT_COURSE_LABEL_ID = 'Project Partnerships'
WHERE ga.GL_ACCOUNT_COURSE_LABEL_ID = 'OTHER_PAYABLES';

UPDATE GL_ACCOUNT ga
JOIN GL_ACCOUNT parent ON parent.GL_ACCOUNT_ID = ga.PARENT_GL_ACCOUNT_ID
    AND parent.ACCOUNT_NAME_ARABIC LIKE '%مشاركات مؤقتة%'
SET ga.GL_SUB_ACCOUNT_COURSE_LABEL_ID = 'Temp Partnerships'
WHERE ga.GL_ACCOUNT_COURSE_LABEL_ID = 'OTHER_PAYABLES';

UPDATE GL_ACCOUNT ga
JOIN GL_ACCOUNT parent ON parent.GL_ACCOUNT_ID = ga.PARENT_GL_ACCOUNT_ID
    AND parent.ACCOUNT_NAME_ARABIC = 'ملاك الاراضي المشتراة'
SET ga.GL_SUB_ACCOUNT_COURSE_LABEL_ID = 'Land Owner Payables'
WHERE ga.GL_ACCOUNT_COURSE_LABEL_ID = 'OTHER_PAYABLES';

UPDATE GL_ACCOUNT ga
JOIN GL_ACCOUNT parent ON parent.GL_ACCOUNT_ID = ga.PARENT_GL_ACCOUNT_ID
    AND parent.ACCOUNT_NAME_ARABIC IN ('مصلحة الضرائب المصرية', 'ضريبة المبيعات المحصلة كندا')
SET ga.GL_SUB_ACCOUNT_COURSE_LABEL_ID = 'Tax Payables'
WHERE ga.GL_ACCOUNT_COURSE_LABEL_ID = 'OTHER_PAYABLES';

UPDATE GL_ACCOUNT ga
JOIN GL_ACCOUNT parent ON parent.GL_ACCOUNT_ID = ga.PARENT_GL_ACCOUNT_ID
    AND parent.ACCOUNT_NAME_ARABIC = 'مقاولي الباطن'
SET ga.GL_SUB_ACCOUNT_COURSE_LABEL_ID = 'Subcontractor Payables'
WHERE ga.GL_ACCOUNT_COURSE_LABEL_ID = 'OTHER_PAYABLES';

UPDATE GL_ACCOUNT ga
JOIN GL_ACCOUNT parent ON parent.GL_ACCOUNT_ID = ga.PARENT_GL_ACCOUNT_ID
    AND parent.ACCOUNT_NAME_ARABIC = 'ائتمانات العملاء'
SET ga.GL_SUB_ACCOUNT_COURSE_LABEL_ID = 'Customer Credits'
WHERE ga.GL_ACCOUNT_COURSE_LABEL_ID = 'OTHER_PAYABLES';

-- Catch-all — run last in this group
UPDATE GL_ACCOUNT ga
SET ga.GL_SUB_ACCOUNT_COURSE_LABEL_ID = 'Other Payables'
WHERE ga.GL_ACCOUNT_COURSE_LABEL_ID = 'OTHER_PAYABLES'
  AND ga.GL_SUB_ACCOUNT_COURSE_LABEL_ID IS NULL;

-- =====================================================================
-- INVENTORY (50 accounts)
-- =====================================================================

UPDATE GL_ACCOUNT ga
JOIN GL_ACCOUNT parent ON parent.GL_ACCOUNT_ID = ga.PARENT_GL_ACCOUNT_ID
    AND parent.ACCOUNT_NAME_ARABIC = 'مشاريع تحت التنفيذ'
SET ga.GL_SUB_ACCOUNT_COURSE_LABEL_ID = 'WIP Own Projects'
WHERE ga.GL_ACCOUNT_COURSE_LABEL_ID = 'INVENTORY';

UPDATE GL_ACCOUNT ga
JOIN GL_ACCOUNT parent ON parent.GL_ACCOUNT_ID = ga.PARENT_GL_ACCOUNT_ID
    AND parent.ACCOUNT_NAME_ARABIC = 'اعمال تحت التنفيذ للغير'
SET ga.GL_SUB_ACCOUNT_COURSE_LABEL_ID = 'WIP Third Party'
WHERE ga.GL_ACCOUNT_COURSE_LABEL_ID = 'INVENTORY';

UPDATE GL_ACCOUNT ga
JOIN GL_ACCOUNT parent ON parent.GL_ACCOUNT_ID = ga.PARENT_GL_ACCOUNT_ID
    AND parent.ACCOUNT_NAME_ARABIC LIKE '%وحدات للبيع%'
SET ga.GL_SUB_ACCOUNT_COURSE_LABEL_ID = 'Units for Sale'
WHERE ga.GL_ACCOUNT_COURSE_LABEL_ID = 'INVENTORY';

UPDATE GL_ACCOUNT ga
JOIN GL_ACCOUNT parent ON parent.GL_ACCOUNT_ID = ga.PARENT_GL_ACCOUNT_ID
    AND parent.ACCOUNT_NAME_ARABIC LIKE '%مخزون المواد%'
SET ga.GL_SUB_ACCOUNT_COURSE_LABEL_ID = 'Materials Inventory'
WHERE ga.GL_ACCOUNT_COURSE_LABEL_ID = 'INVENTORY';

-- =====================================================================
-- INVENTORY LANDS (22 accounts)
-- =====================================================================

UPDATE GL_ACCOUNT ga
SET ga.GL_SUB_ACCOUNT_COURSE_LABEL_ID = 'Land Inventory'
WHERE ga.GL_ACCOUNT_COURSE_LABEL_ID = 'INVENTORY_LANDS';

-- =====================================================================
-- CASH (8 accounts)
-- =====================================================================

UPDATE GL_ACCOUNT ga
JOIN GL_ACCOUNT parent ON parent.GL_ACCOUNT_ID = ga.PARENT_GL_ACCOUNT_ID
    AND parent.ACCOUNT_NAME_ARABIC IN ('البنوك', 'الأصول المتداولة')
SET ga.GL_SUB_ACCOUNT_COURSE_LABEL_ID = 'Cash at Bank'
WHERE ga.GL_ACCOUNT_COURSE_LABEL_ID = 'CASH_AND_CASH_EQUIVALENTS';

UPDATE GL_ACCOUNT ga
JOIN GL_ACCOUNT parent ON parent.GL_ACCOUNT_ID = ga.PARENT_GL_ACCOUNT_ID
    AND parent.ACCOUNT_NAME_ARABIC = 'النقدية'
SET ga.GL_SUB_ACCOUNT_COURSE_LABEL_ID = 'Cash in Hand'
WHERE ga.GL_ACCOUNT_COURSE_LABEL_ID = 'CASH_AND_CASH_EQUIVALENTS';

UPDATE GL_ACCOUNT ga
JOIN GL_ACCOUNT parent ON parent.GL_ACCOUNT_ID = ga.PARENT_GL_ACCOUNT_ID
    AND parent.ACCOUNT_NAME_ARABIC = 'نقدية الملاك'
SET ga.GL_SUB_ACCOUNT_COURSE_LABEL_ID = 'Owners Cash'
WHERE ga.GL_ACCOUNT_COURSE_LABEL_ID = 'CASH_AND_CASH_EQUIVALENTS';

-- =====================================================================
-- OTHER CURRENT ASSETS (62 accounts)
-- =====================================================================

UPDATE GL_ACCOUNT ga
JOIN GL_ACCOUNT parent ON parent.GL_ACCOUNT_ID = ga.PARENT_GL_ACCOUNT_ID
    AND parent.ACCOUNT_NAME_ARABIC = 'ذمم مدينة أخرى'
SET ga.GL_SUB_ACCOUNT_COURSE_LABEL_ID = 'Other Receivables'
WHERE ga.GL_ACCOUNT_COURSE_LABEL_ID = 'OTHER_CURRENT_ASSETS';

UPDATE GL_ACCOUNT ga
JOIN GL_ACCOUNT parent ON parent.GL_ACCOUNT_ID = ga.PARENT_GL_ACCOUNT_ID
    AND parent.ACCOUNT_NAME_ARABIC = 'العهدة المستديمة'
SET ga.GL_SUB_ACCOUNT_COURSE_LABEL_ID = 'Permanent Advances'
WHERE ga.GL_ACCOUNT_COURSE_LABEL_ID = 'OTHER_CURRENT_ASSETS';

-- Catch-all
UPDATE GL_ACCOUNT ga
SET ga.GL_SUB_ACCOUNT_COURSE_LABEL_ID = 'Other Current Assets'
WHERE ga.GL_ACCOUNT_COURSE_LABEL_ID = 'OTHER_CURRENT_ASSETS'
  AND ga.GL_SUB_ACCOUNT_COURSE_LABEL_ID IS NULL;

-- =====================================================================
-- FIXED ASSETS
-- =====================================================================

UPDATE GL_ACCOUNT ga
SET ga.GL_SUB_ACCOUNT_COURSE_LABEL_ID = 'Property Plant Equipment'
WHERE ga.GL_ACCOUNT_COURSE_LABEL_ID = 'PROPERTY_PLANT_AND_EQUIPMENT';

UPDATE GL_ACCOUNT ga
JOIN GL_ACCOUNT parent ON parent.GL_ACCOUNT_ID = ga.PARENT_GL_ACCOUNT_ID
    AND parent.ACCOUNT_NAME_ARABIC = 'الإهلاك المتراكم'
SET ga.GL_SUB_ACCOUNT_COURSE_LABEL_ID = 'Accumulated Depreciation'
WHERE ga.GL_ACCOUNT_COURSE_LABEL_ID = 'EQUIPMENT';

UPDATE GL_ACCOUNT ga
JOIN GL_ACCOUNT parent ON parent.GL_ACCOUNT_ID = ga.PARENT_GL_ACCOUNT_ID
    AND parent.ACCOUNT_NAME_ARABIC = 'نفقات الإهلاك'
SET ga.GL_SUB_ACCOUNT_COURSE_LABEL_ID = 'Depreciation Expense'
WHERE ga.GL_ACCOUNT_COURSE_LABEL_ID = 'EQUIPMENT';

UPDATE GL_ACCOUNT ga
SET ga.GL_SUB_ACCOUNT_COURSE_LABEL_ID = 'Vehicles'
WHERE ga.GL_ACCOUNT_COURSE_LABEL_ID = 'VEHICLES';

-- =====================================================================
-- EQUITY
-- =====================================================================

UPDATE GL_ACCOUNT ga
JOIN GL_ACCOUNT parent ON parent.GL_ACCOUNT_ID = ga.PARENT_GL_ACCOUNT_ID
    AND parent.ACCOUNT_NAME_ARABIC IN ('رأس المال', 'رأس المال وحقوق الملكية')
SET ga.GL_SUB_ACCOUNT_COURSE_LABEL_ID = 'Share Capital'
WHERE ga.GL_ACCOUNT_COURSE_LABEL_ID = 'SHARE_CAPITAL';

UPDATE GL_ACCOUNT ga
JOIN GL_ACCOUNT parent ON parent.GL_ACCOUNT_ID = ga.PARENT_GL_ACCOUNT_ID
    AND parent.ACCOUNT_NAME_ARABIC = 'جارى الشركاء'
SET ga.GL_SUB_ACCOUNT_COURSE_LABEL_ID = 'Partner Current Accounts'
WHERE ga.GL_ACCOUNT_COURSE_LABEL_ID = 'SHARE_CAPITAL';

UPDATE GL_ACCOUNT ga
SET ga.GL_SUB_ACCOUNT_COURSE_LABEL_ID = 'Partner Current Accounts'
WHERE ga.GL_ACCOUNT_COURSE_LABEL_ID = 'DIVIDENDS_PAID';

UPDATE GL_ACCOUNT ga
SET ga.GL_SUB_ACCOUNT_COURSE_LABEL_ID = 'Retained Earnings'
WHERE ga.GL_ACCOUNT_COURSE_LABEL_ID = 'RETAINED_EARNINGS';

-- =====================================================================
-- LONG TERM & SPECIALTY PAYABLES
-- =====================================================================

UPDATE GL_ACCOUNT ga
SET ga.GL_SUB_ACCOUNT_COURSE_LABEL_ID = 'Long Term Obligations'
WHERE ga.GL_ACCOUNT_COURSE_LABEL_ID = 'LONG_TERM_OBLIGATIONS';

UPDATE GL_ACCOUNT ga
SET ga.GL_SUB_ACCOUNT_COURSE_LABEL_ID = 'Land Owner Payables'
WHERE ga.GL_ACCOUNT_COURSE_LABEL_ID = 'PAYABLES_LANDS';

UPDATE GL_ACCOUNT ga
SET ga.GL_SUB_ACCOUNT_COURSE_LABEL_ID = 'Other Payables'
WHERE ga.GL_ACCOUNT_COURSE_LABEL_ID = 'PAYABLES_STOCKS';

-- =====================================================================
-- P&L — INCOME & EXPENSES
-- =====================================================================

UPDATE GL_ACCOUNT ga SET ga.GL_SUB_ACCOUNT_COURSE_LABEL_ID = 'Sales'
WHERE ga.GL_ACCOUNT_COURSE_LABEL_ID = 'SALES';

UPDATE GL_ACCOUNT ga SET ga.GL_SUB_ACCOUNT_COURSE_LABEL_ID = 'Sales Return'
WHERE ga.GL_ACCOUNT_COURSE_LABEL_ID = 'SALES_RETURN';

UPDATE GL_ACCOUNT ga SET ga.GL_SUB_ACCOUNT_COURSE_LABEL_ID = 'Interest Income'
WHERE ga.GL_ACCOUNT_COURSE_LABEL_ID = 'INTEREST_INCOME';

UPDATE GL_ACCOUNT ga SET ga.GL_SUB_ACCOUNT_COURSE_LABEL_ID = 'Dividend Income'
WHERE ga.GL_ACCOUNT_COURSE_LABEL_ID = 'DIVIDEND_INCOME';

UPDATE GL_ACCOUNT ga SET ga.GL_SUB_ACCOUNT_COURSE_LABEL_ID = 'Cost of Sales'
WHERE ga.GL_ACCOUNT_COURSE_LABEL_ID = 'COST_OF_SALES';

UPDATE GL_ACCOUNT ga SET ga.GL_SUB_ACCOUNT_COURSE_LABEL_ID = 'Interest Expense'
WHERE ga.GL_ACCOUNT_COURSE_LABEL_ID = 'INTEREST_EXPENSE';

UPDATE GL_ACCOUNT ga SET ga.GL_SUB_ACCOUNT_COURSE_LABEL_ID = 'Staff Costs'
WHERE ga.GL_ACCOUNT_COURSE_LABEL_ID = 'STAFF_COSTS';

UPDATE GL_ACCOUNT ga SET ga.GL_SUB_ACCOUNT_COURSE_LABEL_ID = 'Advertisements'
WHERE ga.GL_ACCOUNT_COURSE_LABEL_ID = 'ADVERTISEMENTS';

UPDATE GL_ACCOUNT ga SET ga.GL_SUB_ACCOUNT_COURSE_LABEL_ID = 'Internet and Telecom'
WHERE ga.GL_ACCOUNT_COURSE_LABEL_ID = 'INTERNET_AND_TELECOM';

UPDATE GL_ACCOUNT ga SET ga.GL_SUB_ACCOUNT_COURSE_LABEL_ID = 'Utilities'
WHERE ga.GL_ACCOUNT_COURSE_LABEL_ID = 'UTILITIES';

UPDATE GL_ACCOUNT ga SET ga.GL_SUB_ACCOUNT_COURSE_LABEL_ID = 'Rent Expense'
WHERE ga.GL_ACCOUNT_COURSE_LABEL_ID = 'RENT';

UPDATE GL_ACCOUNT ga SET ga.GL_SUB_ACCOUNT_COURSE_LABEL_ID = 'Other Expenses'
WHERE ga.GL_ACCOUNT_COURSE_LABEL_ID = 'OTHER_EXPENSES';

-- =====================================================================
-- VERIFICATION — uncomment and run after all updates
-- =====================================================================

-- SELECT
--     ga.GL_ACCOUNT_ID,
--     ga.ACCOUNT_NAME_ARABIC,
--     ga.GL_ACCOUNT_COURSE_LABEL_ID         AS ACCOUNT_LEVEL,
--     ga.GL_SUB_ACCOUNT_COURSE_LABEL_ID     AS SUB_ACCOUNT_LEVEL
-- FROM GL_ACCOUNT ga
-- WHERE ga.GL_SUB_ACCOUNT_COURSE_LABEL_ID IS NULL
-- ORDER BY ga.GL_ACCOUNT_COURSE_LABEL_ID, ga.GL_ACCOUNT_ID;

