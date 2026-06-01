-- =============================================================================
-- UPDATE gl_account SET gl_sub_account_course_label_id
-- Based on: parent account name + account classification
-- Covers all 1,000 accounts in GolderLand GLAccounts
-- =============================================================================
-- IMPORTANT: Run the INSERT statements (gl_sub_account_inserts.sql) FIRST
-- Assumes your gl_account table has columns:
--   gl_account_id, parent_gl_account_id (or parent name joinable),
--   gl_account_course_label_id (Account level FK),
--   gl_sub_account_course_label_id (the new FK column)
-- Adjust table/column names to match your actual OFBiz schema.
-- =============================================================================

-- =====================================================================
-- RECEIVABLES (258 accounts)
-- =====================================================================

-- 163 accounts: عملاء مشاريع تحت التنفيذ
UPDATE gl_account
SET gl_sub_account_course_label_id = 'Project Receivables'
WHERE gl_account_course_label_id = 'RECEIVABLES'
  AND parent_gl_account_id IN (
      SELECT gl_account_id FROM gl_account
      WHERE account_name = 'عملاء مشاريع تحت التنفيذ'
  );

-- 82 accounts: ذمم الموظفين
UPDATE gl_account
SET gl_sub_account_course_label_id = 'Staff Receivables'
WHERE gl_account_course_label_id = 'RECEIVABLES'
  AND parent_gl_account_id IN (
      SELECT gl_account_id FROM gl_account
      WHERE account_name = 'ذمم الموظفين'
  );

-- 5 accounts: مدينو الإيجارات
UPDATE gl_account
SET gl_sub_account_course_label_id = 'Rent Receivables'
WHERE gl_account_course_label_id = 'RECEIVABLES'
  AND parent_gl_account_id IN (
      SELECT gl_account_id FROM gl_account
      WHERE account_name = 'مدينو الإيجارات'
  );

-- 4 accounts: في الطريق من معالجات بطاقات الائتمان
UPDATE gl_account
SET gl_sub_account_course_label_id = 'Credit Card Transit'
WHERE gl_account_course_label_id = 'RECEIVABLES'
  AND parent_gl_account_id IN (
      SELECT gl_account_id FROM gl_account
      WHERE account_name = 'في الطريق من معالجات بطاقات الائتمان'
  );

-- 1 account: العملاء (regular customers)
-- 1 account: عملاء إدارة العقارات (property management customers)
-- Both map to Trade Receivables — the value the DAX measure filters on
UPDATE gl_account
SET gl_sub_account_course_label_id = 'Trade Receivables'
WHERE gl_account_course_label_id = 'RECEIVABLES'
  AND parent_gl_account_id IN (
      SELECT gl_account_id FROM gl_account
      WHERE account_name IN ('العملاء', 'عملاء إدارة العقارات')
  );

-- 2 accounts: الخصوم (WRONG SIDE — needs reclassification fix first)
-- Flagged here so you don't miss them; fix classification before setting SubAccount
-- UPDATE gl_account
-- SET gl_sub_account_course_label_id = 'Trade Receivables'  -- or other value after fix
-- WHERE gl_account_id = 126000;  -- الحسابات المستحقة القبض - المدفوعات غير المعالجة

-- =====================================================================
-- TRADE PAYABLES (348 accounts)
-- =====================================================================

-- 347 accounts: الدائنون
UPDATE gl_account
SET gl_sub_account_course_label_id = 'Trade Payables'
WHERE gl_account_course_label_id = 'TRADE_PAYABLES'
  AND parent_gl_account_id IN (
      SELECT gl_account_id FROM gl_account
      WHERE account_name = 'الدائنون'
  );

-- 1 account: الأصول (WRONG SIDE — account 216000, needs fix first)
-- UPDATE gl_account
-- SET gl_sub_account_course_label_id = 'Trade Payables'
-- WHERE gl_account_id = 216000;  -- run after fixing classification

-- =====================================================================
-- OTHER PAYABLES (135 accounts)
-- =====================================================================

-- 67 accounts: المصاريف المستحقة
UPDATE gl_account
SET gl_sub_account_course_label_id = 'Accrued Expenses'
WHERE gl_account_course_label_id = 'OTHER_PAYABLES'
  AND parent_gl_account_id IN (
      SELECT gl_account_id FROM gl_account
      WHERE account_name = 'المصاريف المستحقة'
  );

-- 9+7+4+2 = 22 accounts: rental partnership payables across all properties
-- (جولدن ووك، مطعم السدة، سيتى ووك، مول الصحراوى)
UPDATE gl_account
SET gl_sub_account_course_label_id = 'Rental Partnerships'
WHERE gl_account_course_label_id = 'OTHER_PAYABLES'
  AND parent_gl_account_id IN (
      SELECT gl_account_id FROM gl_account
      WHERE account_name LIKE '%مشاركات ايجار عقارات%'
         OR account_name = 'مشاركات ايراد ايجار عقارات'
  );

-- 6+4 = 10 accounts: project partnership payables
UPDATE gl_account
SET gl_sub_account_course_label_id = 'Project Partnerships'
WHERE gl_account_course_label_id = 'OTHER_PAYABLES'
  AND parent_gl_account_id IN (
      SELECT gl_account_id FROM gl_account
      WHERE account_name IN ('مشاركات المشاريع تحت التنفيذ', 'مشاركات مشروع الواحات')
  );

-- 6 accounts: مشاركات مؤقتة استثمار عقاري
UPDATE gl_account
SET gl_sub_account_course_label_id = 'Temp Partnerships'
WHERE gl_account_course_label_id = 'OTHER_PAYABLES'
  AND parent_gl_account_id IN (
      SELECT gl_account_id FROM gl_account
      WHERE account_name LIKE '%مشاركات مؤقتة%'
  );

-- 3 accounts: ملاك الاراضي المشتراة
UPDATE gl_account
SET gl_sub_account_course_label_id = 'Land Owner Payables'
WHERE gl_account_course_label_id = 'OTHER_PAYABLES'
  AND parent_gl_account_id IN (
      SELECT gl_account_id FROM gl_account
      WHERE account_name = 'ملاك الاراضي المشتراة'
  );

-- 2 accounts: مصلحة الضرائب المصرية
-- 1 account:  ضريبة المبيعات المحصلة كندا (Canadian GST — same category)
UPDATE gl_account
SET gl_sub_account_course_label_id = 'Tax Payables'
WHERE gl_account_course_label_id = 'OTHER_PAYABLES'
  AND parent_gl_account_id IN (
      SELECT gl_account_id FROM gl_account
      WHERE account_name IN ('مصلحة الضرائب المصرية', 'ضريبة المبيعات المحصلة كندا')
  );

-- 1 account: مقاولي الباطن
UPDATE gl_account
SET gl_sub_account_course_label_id = 'Subcontractor Payables'
WHERE gl_account_course_label_id = 'OTHER_PAYABLES'
  AND parent_gl_account_id IN (
      SELECT gl_account_id FROM gl_account
      WHERE account_name = 'مقاولي الباطن'
  );

-- 1 account: ائتمانات العملاء
UPDATE gl_account
SET gl_sub_account_course_label_id = 'Customer Credits'
WHERE gl_account_course_label_id = 'OTHER_PAYABLES'
  AND parent_gl_account_id IN (
      SELECT gl_account_id FROM gl_account
      WHERE account_name = 'ائتمانات العملاء'
  );

-- Remaining OTHER_PAYABLES: الدائنون(1), الالتزامات المتداولة(8),
-- الالتزامات الجارية المستحقة الدفع(1), الخصوم(1), ذمم دائنة أخرى(10)
UPDATE gl_account
SET gl_sub_account_course_label_id = 'Other Payables'
WHERE gl_account_course_label_id = 'OTHER_PAYABLES'
  AND gl_sub_account_course_label_id IS NULL;

-- =====================================================================
-- INVENTORY (50 accounts)
-- =====================================================================

-- 26 accounts: مشاريع تحت التنفيذ
UPDATE gl_account
SET gl_sub_account_course_label_id = 'WIP Own Projects'
WHERE gl_account_course_label_id = 'INVENTORY'
  AND parent_gl_account_id IN (
      SELECT gl_account_id FROM gl_account
      WHERE account_name = 'مشاريع تحت التنفيذ'
  );

-- 12 accounts: اعمال تحت التنفيذ للغير
UPDATE gl_account
SET gl_sub_account_course_label_id = 'WIP Third Party'
WHERE gl_account_course_label_id = 'INVENTORY'
  AND parent_gl_account_id IN (
      SELECT gl_account_id FROM gl_account
      WHERE account_name = 'اعمال تحت التنفيذ للغير'
  );

-- 6 accounts: المخزون العقاري وحدات للبيع
UPDATE gl_account
SET gl_sub_account_course_label_id = 'Units for Sale'
WHERE gl_account_course_label_id = 'INVENTORY'
  AND parent_gl_account_id IN (
      SELECT gl_account_id FROM gl_account
      WHERE account_name LIKE '%وحدات للبيع%'
  );

-- 6 accounts: مخزون المواد مشاريع تحت التنفيذ
UPDATE gl_account
SET gl_sub_account_course_label_id = 'Materials Inventory'
WHERE gl_account_course_label_id = 'INVENTORY'
  AND parent_gl_account_id IN (
      SELECT gl_account_id FROM gl_account
      WHERE account_name LIKE '%مخزون المواد%'
  );

-- =====================================================================
-- INVENTORY LANDS (22 accounts)
-- =====================================================================

-- All 22 under ذمم مدينة أخرى
UPDATE gl_account
SET gl_sub_account_course_label_id = 'Land Inventory'
WHERE gl_account_course_label_id = 'INVENTORY_LANDS';

-- =====================================================================
-- CASH AND CASH EQUIVALENTS (8 accounts)
-- =====================================================================

-- 4 accounts: البنوك
-- 2 accounts: الأصول المتداولة (generic bank accounts)
UPDATE gl_account
SET gl_sub_account_course_label_id = 'Cash at Bank'
WHERE gl_account_course_label_id = 'CASH_AND_CASH_EQUIVALENTS'
  AND parent_gl_account_id IN (
      SELECT gl_account_id FROM gl_account
      WHERE account_name IN ('البنوك', 'الأصول المتداولة')
  );

-- 1 account: النقدية
UPDATE gl_account
SET gl_sub_account_course_label_id = 'Cash in Hand'
WHERE gl_account_course_label_id = 'CASH_AND_CASH_EQUIVALENTS'
  AND parent_gl_account_id IN (
      SELECT gl_account_id FROM gl_account
      WHERE account_name = 'النقدية'
  );

-- 1 account: نقدية الملاك
UPDATE gl_account
SET gl_sub_account_course_label_id = 'Owners Cash'
WHERE gl_account_course_label_id = 'CASH_AND_CASH_EQUIVALENTS'
  AND parent_gl_account_id IN (
      SELECT gl_account_id FROM gl_account
      WHERE account_name = 'نقدية الملاك'
  );

-- =====================================================================
-- OTHER CURRENT ASSETS (62 accounts)
-- =====================================================================

-- 42 accounts: ذمم مدينة أخرى
UPDATE gl_account
SET gl_sub_account_course_label_id = 'Other Receivables'
WHERE gl_account_course_label_id = 'OTHER_CURRENT_ASSETS'
  AND parent_gl_account_id IN (
      SELECT gl_account_id FROM gl_account
      WHERE account_name = 'ذمم مدينة أخرى'
  );

-- 14 accounts: العهدة المستديمة
UPDATE gl_account
SET gl_sub_account_course_label_id = 'Permanent Advances'
WHERE gl_account_course_label_id = 'OTHER_CURRENT_ASSETS'
  AND parent_gl_account_id IN (
      SELECT gl_account_id FROM gl_account
      WHERE account_name = 'العهدة المستديمة'
  );

-- 6 accounts: الأصول المتداولة (generic)
UPDATE gl_account
SET gl_sub_account_course_label_id = 'Other Current Assets'
WHERE gl_account_course_label_id = 'OTHER_CURRENT_ASSETS'
  AND gl_sub_account_course_label_id IS NULL;

-- =====================================================================
-- FIXED ASSETS
-- =====================================================================

-- 15 accounts: الأصول الثابتة
UPDATE gl_account
SET gl_sub_account_course_label_id = 'Property Plant Equipment'
WHERE gl_account_course_label_id = 'PROPERTY_PLANT_AND_EQUIPMENT';

-- 12 accounts: الإهلاك المتراكم (accumulated depreciation — Balance Sheet)
UPDATE gl_account
SET gl_sub_account_course_label_id = 'Accumulated Depreciation'
WHERE gl_account_course_label_id = 'EQUIPMENT'
  AND parent_gl_account_id IN (
      SELECT gl_account_id FROM gl_account
      WHERE account_name = 'الإهلاك المتراكم'
  );

-- 1 account: نفقات الإهلاك (depreciation expense — P&L)
UPDATE gl_account
SET gl_sub_account_course_label_id = 'Depreciation Expense'
WHERE gl_account_course_label_id = 'EQUIPMENT'
  AND parent_gl_account_id IN (
      SELECT gl_account_id FROM gl_account
      WHERE account_name = 'نفقات الإهلاك'
  );

-- 5 accounts: Vehicles
UPDATE gl_account
SET gl_sub_account_course_label_id = 'Vehicles'
WHERE gl_account_course_label_id = 'VEHICLES';

-- =====================================================================
-- EQUITY
-- =====================================================================

-- 2 accounts: رأس المال (fixed partner capital)
-- 1 account:  رأس المال وحقوق الملكية (general equity header)
UPDATE gl_account
SET gl_sub_account_course_label_id = 'Share Capital'
WHERE gl_account_course_label_id = 'SHARE_CAPITAL'
  AND parent_gl_account_id IN (
      SELECT gl_account_id FROM gl_account
      WHERE account_name IN ('رأس المال', 'رأس المال وحقوق الملكية')
  );

-- 2 accounts: جارى الشركاء (partner current/drawings accounts)
UPDATE gl_account
SET gl_sub_account_course_label_id = 'Partner Current Accounts'
WHERE gl_account_course_label_id = 'SHARE_CAPITAL'
  AND parent_gl_account_id IN (
      SELECT gl_account_id FROM gl_account
      WHERE account_name = 'جارى الشركاء'
  );

-- 2 accounts: جارى الشركاء under DIVIDENDS_PAID
UPDATE gl_account
SET gl_sub_account_course_label_id = 'Partner Current Accounts'
WHERE gl_account_course_label_id = 'DIVIDENDS_PAID';

-- 1 account: Retained Earnings
UPDATE gl_account
SET gl_sub_account_course_label_id = 'Retained Earnings'
WHERE gl_account_course_label_id = 'RETAINED_EARNINGS';

-- =====================================================================
-- LONG TERM OBLIGATIONS (2 accounts)
-- =====================================================================

UPDATE gl_account
SET gl_sub_account_course_label_id = 'Long Term Obligations'
WHERE gl_account_course_label_id = 'LONG_TERM_OBLIGATIONS';

-- =====================================================================
-- PAYABLES LANDS & STOCKS
-- =====================================================================

UPDATE gl_account
SET gl_sub_account_course_label_id = 'Land Owner Payables'
WHERE gl_account_course_label_id = 'PAYABLES_LANDS';

UPDATE gl_account
SET gl_sub_account_course_label_id = 'Other Payables'
WHERE gl_account_course_label_id = 'PAYABLES_STOCKS';

-- =====================================================================
-- P&L — INCOME
-- =====================================================================

UPDATE gl_account
SET gl_sub_account_course_label_id = 'Sales'
WHERE gl_account_course_label_id = 'SALES';

UPDATE gl_account
SET gl_sub_account_course_label_id = 'Sales Return'
WHERE gl_account_course_label_id = 'SALES_RETURN';

UPDATE gl_account
SET gl_sub_account_course_label_id = 'Interest Income'
WHERE gl_account_course_label_id = 'INTEREST_INCOME';

UPDATE gl_account
SET gl_sub_account_course_label_id = 'Dividend Income'
WHERE gl_account_course_label_id = 'DIVIDEND_INCOME';

-- =====================================================================
-- P&L — EXPENSES
-- =====================================================================

UPDATE gl_account
SET gl_sub_account_course_label_id = 'Cost of Sales'
WHERE gl_account_course_label_id = 'COST_OF_SALES';

UPDATE gl_account
SET gl_sub_account_course_label_id = 'Interest Expense'
WHERE gl_account_course_label_id = 'INTEREST_EXPENSE';

-- Staff costs across all parent groups
UPDATE gl_account
SET gl_sub_account_course_label_id = 'Staff Costs'
WHERE gl_account_course_label_id = 'STAFF_COSTS';

-- Advertisements (3 accounts under مشروع نسيم operating expenses)
UPDATE gl_account
SET gl_sub_account_course_label_id = 'Advertisements'
WHERE gl_account_course_label_id = 'ADVERTISEMENTS';

-- Internet & Telecom (4 accounts)
UPDATE gl_account
SET gl_sub_account_course_label_id = 'Internet and Telecom'
WHERE gl_account_course_label_id = 'INTERNET_AND_TELECOM';

-- Utilities (2 accounts)
UPDATE gl_account
SET gl_sub_account_course_label_id = 'Utilities'
WHERE gl_account_course_label_id = 'UTILITIES';

-- Rent expense (1 account)
UPDATE gl_account
SET gl_sub_account_course_label_id = 'Rent Expense'
WHERE gl_account_course_label_id = 'RENT';

-- Other expenses — multiple parent groups, all map to same SubAccount
UPDATE gl_account
SET gl_sub_account_course_label_id = 'Other Expenses'
WHERE gl_account_course_label_id = 'OTHER_EXPENSES';

-- =====================================================================
-- VERIFICATION QUERY
-- Run after all updates to check for any accounts still unclassified
-- =====================================================================

-- SELECT
--     ga.gl_account_id,
--     ga.account_name,
--     ga.gl_account_course_label_id      AS account_level,
--     ga.gl_sub_account_course_label_id  AS sub_account_level
-- FROM gl_account ga
-- WHERE ga.is_leaf = 1                          -- only posting accounts
--   AND ga.gl_sub_account_course_label_id IS NULL
-- ORDER BY ga.gl_account_course_label_id, ga.gl_account_id;

-- =====================================================================
-- TOTAL ACCOUNTS AFFECTED BY EACH UPDATE (expected row counts)
-- =====================================================================
-- Project Receivables      163
-- Staff Receivables         82
-- Trade Receivables          2  (العملاء + عملاء إدارة العقارات)
-- Rent Receivables           5
-- Credit Card Transit        4
-- Trade Payables           347
-- Accrued Expenses          67
-- Rental Partnerships       22
-- Project Partnerships      10
-- Temp Partnerships          6
-- Land Owner Payables        3
-- Tax Payables               3
-- Subcontractor Payables     1
-- Customer Credits           1
-- Other Payables            22  (الدائنون+الالتزامات المتداولة+ذمم دائنة أخرى+others)
-- WIP Own Projects          26
-- WIP Third Party           12
-- Units for Sale             6
-- Materials Inventory        6
-- Land Inventory            22
-- Cash at Bank               6
-- Cash in Hand               1
-- Owners Cash                1
-- Other Receivables         42
-- Permanent Advances        14
-- Other Current Assets       6
-- Property Plant Equipment  15
-- Accumulated Depreciation  12
-- Depreciation Expense       1
-- Vehicles                   5
-- Share Capital              3
-- Partner Current Accounts   4  (2 SHARE_CAPITAL + 2 DIVIDENDS_PAID)
-- Retained Earnings          1
-- Long Term Obligations      2
-- Land Owner Payables (PAYABLES_LANDS)  1
-- Other Payables (PAYABLES_STOCKS)      3
-- Sales                     11
-- Sales Return               4
-- Interest Income            1
-- Dividend Income            1
-- Cost of Sales              2
-- Interest Expense           1
-- Staff Costs                4
-- Advertisements             3
-- Internet and Telecom       4
-- Utilities                  2
-- Rent Expense               1
-- Other Expenses            36
-- ---------------------------------
-- TOTAL COVERED:           998
-- NOT COVERED (need fix):    2  (accounts 126000 and 216000 — wrong side)
-- =============================================================================
