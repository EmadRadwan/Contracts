-- ============================================================
-- GL_REPORT  →  Excel column: Report
-- ============================================================
INSERT INTO GL_REPORT (GL_REPORT_ID, DESCRIPTION, DESCRIPTION_ARABIC) VALUES
                                                                          ('BALANCE_SHEET',   'Balance Sheet',   'الميزانية العمومية'),
                                                                          ('PROFIT_AND_LOSS', 'Profit and Loss', 'قائمة الدخل'),
                                                                          ('ADJUSTING',       'Adjusting',       'قيود التسوية');


-- ============================================================
-- GL_CLASS_COURSE  →  Excel column: Class
-- ============================================================
INSERT INTO GL_CLASS_COURSE (GL_CLASS_COURSE_ID, DESCRIPTION, DESCRIPTION_ARABIC) VALUES
                                                                                      ('ASSETS',                          'Assets',                        'الأصول'),
                                                                                      ('LIABILITIES_AND_OWNERS_EQUITY',   'Liabilities and Owners Equity', 'الالتزامات وحقوق الملكية'),
                                                                                      ('TRADING_ACCOUNT',                 'Trading account',               'حساب المتاجرة'),
                                                                                      ('OPERATING_ACCOUNT',               'Operating account',             'حساب التشغيل'),
                                                                                      ('NON_OPERATING',                   'Non-operating',                 'غير تشغيلي'),
                                                                                      ('INTEREST_AND_TAX',                'Interest & Tax',                'الفوائد والضرائب'),
                                                                                      ('ADJUSTING',                       'Adjusting',                     'قيود التسوية');


-- ============================================================
-- GL_SUB_CLASS  →  Excel column: SubClass
-- ============================================================
INSERT INTO GL_SUB_CLASS (GL_SUB_CLASS_ID, DESCRIPTION, DESCRIPTION_ARABIC) VALUES
                                                                                ('ASSETS',                          'Assets',                       'الأصول'),
                                                                                ('LIABILITIES',                     'Liabilities',                  'الالتزامات'),
                                                                                ('OWNERS_EQUITY',                   'Owners Equity',                'حقوق الملكية'),
                                                                                ('SALES',                           'Sales',                        'المبيعات'),
                                                                                ('COST_OF_SALES',                   'Cost of Sales',                'تكلفة المبيعات'),
                                                                                ('OPERATING_EXPENSES',              'Operating Expenses',           'مصروفات تشغيلية'),
                                                                                ('DEPRECIATION_AND_AMORTIZATION',   'Depreciation & Amortization',  'الإهلاك والاستهلاك'),
                                                                                ('INTEREST_INCOME',                 'Interest Income',              'إيرادات الفوائد'),
                                                                                ('GAIN_LOSS_ON_SALES_OF_ASSET',     'Gain/Loss on Sales of Asset',  'أرباح/خسائر بيع الأصول'),
                                                                                ('EXCHANGE_LOSS_GAIN',              'Exchange Loss/Gain',           'خسائر/أرباح العملات الأجنبية'),
                                                                                ('DIVIDEND_INCOME',                 'Dividend Income',              'إيرادات الأرباح الموزعة'),
                                                                                ('INTEREST_EXPENSE',                'Interest Expense',             'مصروفات الفوائد'),
                                                                                ('TAXATION',                        'Taxation',                     'الضرائب'),
                                                                                ('ADJUSTING',                       'Adjusting',                    'قيود التسوية');


-- ============================================================
-- GL_SUB_CLASS_2  →  Excel column: SubClass2
-- ============================================================
INSERT INTO GL_SUB_CLASS_2 (GL_SUB_CLASS_2_ID, DESCRIPTION, DESCRIPTION_ARABIC) VALUES
                                                                                    ('CURRENT_ASSETS',              'Current Assets',              'الأصول المتداولة'),
                                                                                    ('NON_CURRENT_ASSETS',          'Non-Current Assets',          'الأصول غير المتداولة'),
                                                                                    ('CURRENT_LIABILITIES',         'Current Liabilities',         'الالتزامات المتداولة'),
                                                                                    ('LONG_TERM_LIABILITIES',       'Long Term Liabilities',       'الالتزامات طويلة الأجل'),
                                                                                    ('SHARE_CAPITAL',               'Share Capital',               'رأس المال المدفوع'),
                                                                                    ('SHARE_PREMIUM',               'Share Premium',               'علاوة الإصدار'),
                                                                                    ('RETAINED_EARNINGS',           'Retained Earnings',           'الأرباح المبقاة'),
                                                                                    ('SALES',                       'Sales',                       'المبيعات'),
                                                                                    ('COST_OF_SALES',               'Cost of Sales',               'تكلفة المبيعات'),
                                                                                    ('SALES_AND_DISTRIBUTION',      'Sales & Distribution',        'المبيعات والتوزيع'),
                                                                                    ('MARKETING',                   'Marketing',                   'التسويق'),
                                                                                    ('ADMINISTRATION',              'Administration',              'الإدارة'),
                                                                                    ('DEPRECIATION',                'Depreciation',                'الإهلاك'),
                                                                                    ('AMORTIZATION',                'Amortization',                'الاستهلاك'),
                                                                                    ('INTEREST_INCOME',             'Interest Income',             'إيرادات الفوائد'),
                                                                                    ('GAIN_LOSS_ON_SALES_OF_ASSET', 'Gain/Loss on Sales of Asset', 'أرباح/خسائر بيع الأصول'),
                                                                                    ('EXCHANGE_LOSS_GAIN',          'Exchange Loss/Gain',          'خسائر/أرباح العملات الأجنبية'),
                                                                                    ('DIVIDEND_INCOME',             'Dividend Income',             'إيرادات الأرباح الموزعة'),
                                                                                    ('INTEREST_EXPENSE',            'Interest Expense',            'مصروفات الفوائد'),
                                                                                    ('TAXATION',                    'Taxation',                    'الضرائب'),
                                                                                    ('ADJUSTING',                   'Adjusting',                   'قيود التسوية');


-- ============================================================
-- GL_ACCOUNT_COURSE_LABEL  →  Excel column: Account
-- ============================================================
INSERT INTO GL_ACCOUNT_COURSE_LABEL (GL_ACCOUNT_COURSE_LABEL_ID, DESCRIPTION, DESCRIPTION_ARABIC, SIGN_MULTIPLIER) VALUES
-- Assets (debit-normal → +1)
('CASH_AND_CASH_EQUIVALENTS',           'Cash & Cash Equivalents',               'النقدية وما في حكمها',                  1),
('RECEIVABLES',                         'Receivables',                           'الذمم المدينة',                          1),
('INVENTORY',                           'Inventory',                             'المخزون',                                1),
('OTHER_CURRENT_ASSETS',                'Other Current Assets',                  'أصول متداولة أخرى',                      1),
('INVESTMENTS',                         'Investments',                           'الاستثمارات',                            1),
('PROPERTY_PLANT_AND_EQUIPMENT',        'Property, Plant, & Equipment',          'العقارات والمنشآت والمعدات',              1),
('INTANGIBLE_ASSETS',                   'Intangible Assets',                     'الأصول غير الملموسة',                    1),
-- Liabilities (credit-normal → -1)
('TRADE_PAYABLES',                      'Trade Payables',                        'الذمم الدائنة التجارية',                 -1),
('OTHER_PAYABLES',                      'Other Payables',                        'الذمم الدائنة الأخرى',                  -1),
('OTHER_CURRENT_LIABILITIES',           'Other Current Liabilities',             'التزامات متداولة أخرى',                 -1),
('CURRENT_INSTALLMENTS_OF_LT_DEBT',     'Current Installments of Long-term Debt','الأقساط الحالية للديون طويلة الأجل',    -1),
('LONG_TERM_OBLIGATIONS',               'Long Term Obligations',                 'الالتزامات طويلة الأجل',                -1),
('OTHER_LONG_TERM_LIABILITIES',         'Other Long Term Liabilities',           'التزامات طويلة الأجل أخرى',             -1),
-- Equity (credit-normal → -1); dividends paid is a debit reduction of equity → +1
('SHARE_CAPITAL',                       'Share Capital',                         'رأس المال المدفوع',                     -1),
('SHARE_PREMIUM',                       'Share Premium',                         'علاوة الإصدار',                         -1),
('RETAINED_EARNINGS',                   'Retained Earnings',                     'الأرباح المبقاة',                       -1),
('DIVIDENDS_PAID',                      'Dividends paid',                        'الأرباح الموزعة المدفوعة',               1),
-- Revenue (credit-normal → -1); sales return is a debit contra → +1
('SALES',                               'Sales',                                 'المبيعات',                              -1),
('SALES_RETURN',                        'Sales Return',                          'مردودات المبيعات',                       1),
-- Cost of sales & all operating expenses (debit-normal → +1)
('COST_OF_SALES',                       'Cost of Sales',                         'تكلفة المبيعات',                         1),
('STAFF_COSTS',                         'Staff Costs',                           'تكاليف الموظفين',                        1),
('BAD_DEBT_EXPENSE',                    'Bad Debt Expense',                      'مصروف الديون المعدومة',                  1),
('COMMISSIONS',                         'Commissions',                           'العمولات',                               1),
('CONFERENCES',                         'Conferences',                           'المؤتمرات',                              1),
('ADVERTISEMENTS',                      'Advertisements',                        'الدعاية والإعلان',                       1),
('TRAVEL',                              'Travel',                                'السفر والتنقل',                          1),
('ENTERTAINMENT',                       'Entertainment',                         'الضيافة والترفيه',                       1),
('OFFICE_SUPPLIES',                     'Office Supplies',                       'مستلزمات المكتب',                        1),
('PROFESSIONAL_SERVICES',               'Professional Services',                 'الخدمات المهنية',                        1),
('TELEPHONE',                           'Telephone',                             'الاتصالات',                              1),
('UTILITIES',                           'Utilities',                             'المرافق العامة',                         1),
('OTHER_EXPENSES',                      'Other Expenses',                        'مصروفات أخرى',                           1),
('RENT',                                'Rent',                                  'الإيجار',                                1),
('VEHICLES',                            'Vehicles',                              'المركبات',                               1),
('EQUIPMENT',                           'Equipment',                             'المعدات',                                1),
('FURNITURE_AND_FIXTURES',              'Furniture and Fixtures',                'الأثاث والتجهيزات',                      1),
('AMORTIZATION_OF_INTANGIBLE_ASSETS',   'Amortization of Intangible Assets',     'استهلاك الأصول غير الملموسة',            1),
-- Non-operating income (credit-normal → -1)
('INTEREST_INCOME',                     'Interest Income',                       'إيرادات الفوائد',                       -1),
('GAIN_LOSS_ON_SALES_OF_ASSET',         'Gain/Loss on Sales of Asset',           'أرباح/خسائر بيع الأصول',               -1),
('EXCHANGE_LOSS_GAIN',                  'Exchange Loss/Gain',                    'خسائر/أرباح العملات الأجنبية',          -1),
('DIVIDEND_INCOME',                     'Dividend Income',                       'إيرادات الأرباح الموزعة',               -1),
-- Non-operating expense & tax (debit-normal → +1)
('INTEREST_EXPENSE',                    'Interest Expense',                      'مصروفات الفوائد',                        1),
('TAXATION',                            'Taxation',                              'الضرائب',                                1),
('ADJUSTING',                           'Adjusting',                             'قيود التسوية',                           1);
