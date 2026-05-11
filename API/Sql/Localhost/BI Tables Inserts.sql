INSERT INTO GL_REPORT (GL_REPORT_ID, DESCRIPTION, DESCRIPTION_ARABIC) VALUES
                                                                          ('BS', 'Balance Sheet', 'الميزانية العمومية'),
                                                                          ('PL', 'Profit and Loss', 'قائمة الدخل');

INSERT INTO GL_CLASS_COURSE (GL_CLASS_COURSE_ID, DESCRIPTION, DESCRIPTION_ARABIC) VALUES
                                                                                      ('ASSET', 'Assets', 'الأصول'),
                                                                                      ('LIABILITY', 'Liabilities', 'الالتزامات'),
                                                                                      ('EQUITY', 'Equity', 'حقوق الملكية'),
                                                                                      ('TRADING', 'Trading account', 'حساب المتاجرة'),
                                                                                      ('OPERATING', 'Operating account', 'حساب التشغيل'),
                                                                                      ('NON_OPERATING', 'Non-operating account', 'الحسابات غير التشغيلية');

INSERT INTO GL_SUB_CLASS (GL_SUB_CLASS_ID, DESCRIPTION, DESCRIPTION_ARABIC) VALUES
                                                                                ('CURR_ASSET', 'Current Assets', 'الأصول المتداولة'),
                                                                                ('FIXED_ASSET', 'Fixed Assets', 'الأصول الثابتة'),
                                                                                ('CURR_LIAB', 'Current Liabilities', 'الالتزامات المتداولة'),
                                                                                ('REVENUE', 'Sales', 'المبيعات'),
                                                                                ('COGS', 'Cost of Sales', 'تكلفة المبيعات'),
                                                                                ('OP_EXPENSE', 'Operating Expenses', 'مصروفات تشغيلية'),
                                                                                ('OTHER_INC_EXP', 'Other Income and Expenses', 'إيرادات ومصروفات أخرى');

INSERT INTO GL_SUB_CLASS_2 (GL_SUB_CLASS_2_ID, DESCRIPTION, DESCRIPTION_ARABIC) VALUES
                                                                                    ('MARKETING', 'Marketing', 'التسويق'),
                                                                                    ('ADMIN', 'Administration', 'الإدارة'),
                                                                                    ('SALES_DIST', 'Sales & Distribution', 'المبيعات والتوزيع'),
                                                                                    ('FINANCE', 'Finance', 'المالية'),
                                                                                    ('DEPRECIATION', 'Depreciation', 'الإهلاك'),
                                                                                    ('DIRECT_COST', 'Direct Cost', 'تكلفة مباشرة'),
                                                                                    ('NA', 'Not Applicable', 'غير متاح');

INSERT INTO GL_ACCOUNT_COURSE_LABEL (GL_ACCOUNT_COURSE_LABEL_ID, DESCRIPTION, DESCRIPTION_ARABIC, SIGN_MULTIPLIER) VALUES
                                                                                                                       ('CASH_EQ', 'Cash & Cash Equivalents', 'النقدية وما في حكمها', 1),
                                                                                                                       ('RECEIVABLES', 'Receivables', 'الذمم المدينة', 1),
                                                                                                                       ('INVENTORY', 'Inventory', 'المخزون', 1),
                                                                                                                       ('PAYABLES', 'Payables', 'الذمم الدائنة', -1),
                                                                                                                       ('SALES_REV', 'Sales Revenue', 'إيرادات المبيعات', -1),
                                                                                                                       ('COMMISSIONS', 'Commissions', 'العمولات', 1),
                                                                                                                       ('ADVERTISING', 'Advertising', 'الدعاية والإعلان', 1),
                                                                                                                       ('SALARIES', 'Salaries and Wages', 'الرواتب والأجور', 1),
                                                                                                                       ('RENT_UTIL', 'Rent and Utilities', 'الإيجار والمرافق', 1);