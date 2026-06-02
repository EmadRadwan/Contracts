-- =============================================================================
-- GlSubAccountCourseLabel INSERT statements
-- Table: gl_sub_account_course_label
-- Columns: gl_sub_account_course_label_id, description, description_arabic
-- =============================================================================
-- 
-- NAMING CONVENTION:
--   IDs match the course file naming style (Title Case, same as SubAccount 
--   column values in tbl_ChartofAccounts) so Power BI filters work directly.
--   Arabic descriptions derived from your OFBiz parent account names.
--
-- SOURCE NOTES:
--   "Trade Receivables" — exact value used in course DAX measure:
--     CALCULATE([Total_TTD], tbl_ChartofAccounts[SubAccount] = "Trade Receivables")
--   All other values derived from GolderLand GLAccounts parent groupings.
-- =============================================================================

INSERT INTO GL_SUB_ACCOUNT_COURSE_LABEL
    (gl_sub_account_course_label_id, description, description_arabic)
VALUES

-- =====================================================================
-- RECEIVABLES GROUP
-- Source: 258 accounts under Account = RECEIVABLES
-- =====================================================================

-- CRITICAL: This exact string is used in the course DAX Receivables measure
('Trade Receivables',       'Trade Receivables',            'ذمم العملاء التجارية'),

-- Your project-based customers (163 accounts - largest group)
('Project Receivables',     'Project Receivables',          'ذمم عملاء المشاريع'),

-- Employee advances and salary receivables (82 accounts)
('Staff Receivables',       'Staff Receivables',            'ذمم الموظفين'),

-- Rent customers (5 accounts - مدينو الإيجارات)
('Rent Receivables',        'Rent Receivables',             'ذمم مدينو الإيجارات'),

-- Credit card payments in transit (4 accounts)
('Credit Card Transit',     'Credit Card in Transit',       'مدفوعات بطاقات ائتمان في الطريق'),

-- =====================================================================
-- PAYABLES GROUP
-- Source: 348 accounts under Account = TRADE_PAYABLES
--         135 accounts under Account = OTHER_PAYABLES
-- =====================================================================

-- Regular trade suppliers (347 accounts - الدائنون)
('Trade Payables',          'Trade Payables',               'ذمم الدائنون التجاريون'),

-- Accrued but unpaid expenses (67 accounts - المصاريف المستحقة)
('Accrued Expenses',        'Accrued Expenses',             'المصاريف المستحقة'),

-- Project partnership liabilities - مشاركات المشاريع تحت التنفيذ (6+4 accounts)
('Project Partnerships',    'Project Partnership Payables', 'مشاركات المشاريع'),

-- Rental revenue partnerships - سيتي ووك، مطعم السدة، جولدن ووك، الصحراوى (35 accounts)
('Rental Partnerships',     'Rental Partnership Payables',  'مشاركات ايجار العقارات'),

-- Temporary real estate investment partnerships (6 accounts - مشاركات مؤقتة)
('Temp Partnerships',       'Temporary Investment Partnerships', 'مشاركات مؤقتة استثمار عقاري'),

-- Land owners payables - ملاك الاراضي المشتراة (3 accounts)
('Land Owner Payables',     'Land Owner Payables',          'ذمم ملاك الأراضي المشتراة'),

-- Egyptian Tax Authority (2 accounts - مصلحة الضرائب المصرية)
('Tax Payables',            'Tax Payables',                 'ذمم مصلحة الضرائب'),

-- Subcontractors (1 account - مقاولي الباطن)
('Subcontractor Payables',  'Subcontractor Payables',       'ذمم مقاولو الباطن'),

-- Customer credit balances (1 account - ائتمانات العملاء)
('Customer Credits',        'Customer Credit Balances',     'أرصدة ائتمانات العملاء'),

-- Remaining miscellaneous payables
('Other Payables',          'Other Payables',               'ذمم دائنة أخرى'),

-- =====================================================================
-- INVENTORY GROUP
-- Source: 50 accounts under Account = INVENTORY
--         22 accounts under Account = INVENTORY_LANDS
-- =====================================================================

-- Own projects under construction (26 accounts - مشاريع تحت التنفيذ)
('WIP Own Projects',        'Work in Progress - Own Projects',       'مشاريع تحت التنفيذ'),

-- Third party construction work (12 accounts - اعمال تحت التنفيذ للغير)
('WIP Third Party',         'Work in Progress - Third Party',        'اعمال تحت التنفيذ للغير'),

-- Completed units ready for sale (6 accounts - المخزون العقاري وحدات للبيع)
('Units for Sale',          'Real Estate Units for Sale',            'المخزون العقاري - وحدات للبيع'),

-- Construction materials inventory (6 accounts - مخزون المواد)
('Materials Inventory',     'Materials Inventory',                   'مخزون المواد'),

-- Land held as inventory (22 accounts - INVENTORY_LANDS)
('Land Inventory',          'Land Inventory',                        'مخزون الأراضي'),

-- =====================================================================
-- CASH GROUP
-- Source: 8 accounts under Account = CASH_AND_CASH_EQUIVALENTS
-- =====================================================================

('Cash at Bank',            'Cash at Bank',                 'نقدية في البنوك'),
('Cash in Hand',            'Cash in Hand',                 'نقدية في الصندوق'),

-- Partner/owner cash accounts (نقدية الملاك)
('Owners Cash',             'Owners Cash Accounts',         'نقدية الملاك'),

-- =====================================================================
-- OTHER CURRENT ASSETS GROUP
-- Source: 62 accounts under Account = OTHER_CURRENT_ASSETS
-- =====================================================================

-- Miscellaneous receivables not fitting main RECEIVABLES (42 accounts)
('Other Receivables',       'Other Receivables',            'ذمم مدينة أخرى'),

-- Permanent custody / petty cash advances (14 accounts - العهدة المستديمة)
('Permanent Advances',      'Permanent Advances (Custody)', 'العهدة المستديمة'),

-- Generic other current assets
('Other Current Assets',    'Other Current Assets',         'أصول متداولة أخرى'),

-- =====================================================================
-- FIXED ASSETS GROUP
-- Source: PPE, Equipment (depreciation), Vehicles
-- =====================================================================

('Property Plant Equipment','Property, Plant & Equipment',  'العقارات والمنشآت والمعدات'),
('Accumulated Depreciation','Accumulated Depreciation',     'مجمع الإهلاك المتراكم'),
('Vehicles',                'Vehicles',                     'المركبات'),

-- =====================================================================
-- EQUITY GROUP
-- Source: SHARE_CAPITAL accounts
-- =====================================================================

('Share Capital',           'Share Capital',                'رأس المال المدفوع'),

-- Partner current accounts (جارى الشركاء) - drawings, not fixed capital
('Partner Current Accounts','Partner Current Accounts',     'جارى الشركاء'),

('Retained Earnings',       'Retained Earnings',            'الأرباح المبقاة'),

-- =====================================================================
-- INCOME GROUP
-- Source: 11 accounts under Account = SALES
-- =====================================================================

-- All revenue lines collapse to single SubAccount for P&L 
-- (individual revenue types already distinguishable by account name)
('Sales',                   'Sales Revenue',                'إيرادات المبيعات'),
('Interest Income',         'Interest Income',              'إيرادات الفوائد'),
('Dividend Income',         'Dividend Income',              'إيرادات الأرباح الموزعة'),

-- =====================================================================
-- EXPENSE GROUP
-- Source: Various P&L expense accounts
-- =====================================================================

('Cost of Sales',           'Cost of Sales',                'تكلفة المبيعات'),
('Staff Costs',             'Staff Costs',                  'تكاليف الموظفين'),
('Sales Return',            'Sales Return',                 'مردودات المبيعات'),
('Interest Expense',        'Interest Expense',             'مصروفات الفوائد'),
('Other Expenses',          'Other Expenses',               'مصروفات أخرى'),
('Depreciation Expense',    'Depreciation Expense',         'مصروف الإهلاك'),
('Advertisements',          'Advertisements',               'الدعاية والإعلان'),
('Internet and Telecom',    'Internet and Telecom',         'الإنترنت والاتصالات'),
('Utilities',               'Utilities',                    'المرافق العامة'),
('Rent Expense',            'Rent Expense',                 'مصروف الإيجار'),

-- =====================================================================
-- LONG TERM / OTHER
-- =====================================================================

('Long Term Obligations',   'Long Term Obligations',        'الالتزامات طويلة الأجل'),
('Dividends Paid',          'Dividends Paid',               'الأرباح الموزعة المدفوعة');

-- =============================================================================
-- TOTAL: 46 SubAccount values
-- =============================================================================
-- 
-- NEXT STEP — Update GLAccounts table:
--   After inserting these values, run UPDATE statements on gl_account table
--   to set gl_sub_account_course_label_id for each account based on its 
--   parent group. The mapping is:
--
--   parent_name = 'عملاء مشاريع تحت التنفيذ'  -> 'Project Receivables'
--   parent_name = 'ذمم الموظفين'               -> 'Staff Receivables'
--   parent_name = 'مدينو الإيجارات'            -> 'Rent Receivables'
--   parent_name LIKE '%بطاقات%'                -> 'Credit Card Transit'
--   parent_name = 'العملاء'                    -> 'Trade Receivables'
--   parent_name = 'عملاء إدارة العقارات'       -> 'Trade Receivables'
--   parent_name = 'الدائنون'                   -> 'Trade Payables'
--   parent_name = 'المصاريف المستحقة'          -> 'Accrued Expenses'
--   parent_name LIKE '%مشاركات ايجار%'         -> 'Rental Partnerships'
--   parent_name LIKE '%مشاركات المشاريع%'      -> 'Project Partnerships'
--   parent_name LIKE '%مشاركات مؤقتة%'         -> 'Temp Partnerships'
--   parent_name = 'ملاك الاراضي المشتراة'      -> 'Land Owner Payables'
--   parent_name = 'مصلحة الضرائب المصرية'      -> 'Tax Payables'
--   parent_name = 'مقاولي الباطن'              -> 'Subcontractor Payables'
--   parent_name = 'ائتمانات العملاء'           -> 'Customer Credits'
--   parent_name = 'مشاريع تحت التنفيذ'         -> 'WIP Own Projects'
--   parent_name = 'اعمال تحت التنفيذ للغير'    -> 'WIP Third Party'
--   parent_name LIKE '%وحدات للبيع%'           -> 'Units for Sale'
--   parent_name LIKE '%مخزون المواد%'           -> 'Materials Inventory'
--   parent_name = 'البنوك'                     -> 'Cash at Bank'
--   parent_name = 'النقدية'                    -> 'Cash in Hand'
--   parent_name = 'نقدية الملاك'               -> 'Owners Cash'
--   parent_name = 'العهدة المستديمة'           -> 'Permanent Advances'
--   parent_name = 'الإهلاك المتراكم'           -> 'Accumulated Depreciation'
--   parent_name = 'الأصول الثابتة'            -> 'Property Plant Equipment'
--   parent_name = 'جارى الشركاء'              -> 'Partner Current Accounts'
--   parent_name = 'رأس المال'                 -> 'Share Capital'
-- =============================================================================
