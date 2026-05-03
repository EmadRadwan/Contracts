INSERT INTO GlClassReportMap VALUES

-- =============================================================================
-- BALANCE SHEET — ASSETS
-- =============================================================================

('ASSET', 'Balance Sheet',
 'Assets', 'Assets', 'Assets',
 'الأصول', 'الأصول', 'الأصول',
 'D', 100, 100, 100),

('CURRENT_ASSET', 'Balance Sheet',
 'Assets', 'Current Assets', 'Current Assets',
 'الأصول', 'الأصول المتداولة', 'الأصول المتداولة',
 'D', 100, 110, 110),

('CASH_EQUIVALENT', 'Balance Sheet',
 'Assets', 'Current Assets', 'Cash and Cash Equivalents',
 'الأصول', 'الأصول المتداولة', 'النقد وما في حكمه',
 'D', 100, 110, 120),

('INVENTORY_ASSET', 'Balance Sheet',
 'Assets', 'Current Assets', 'Inventory',
 'الأصول', 'الأصول المتداولة', 'المخزون',
 'D', 100, 110, 130),

('LONGTERM_ASSET', 'Balance Sheet',
 'Assets', 'Non-Current Assets', 'Non-Current Assets',
 'الأصول', 'الأصول غير المتداولة', 'الأصول غير المتداولة',
 'D', 100, 200, 210),

('CONTRA_ASSET', 'Balance Sheet',
 'Assets', 'Contra Assets', 'Contra Assets',
 'الأصول', 'الأصول المقابلة', 'الأصول المقابلة',
 'C', 100, 220, 220),

('ACCUM_DEPRECIATION', 'Balance Sheet',
 'Assets', 'Contra Assets', 'Accumulated Depreciation',
 'الأصول', 'الأصول المقابلة', 'مجمع الإهلاك',
 'C', 100, 220, 230),

('ACCUM_AMORTIZATION', 'Balance Sheet',
 'Assets', 'Contra Assets', 'Accumulated Amortization',
 'الأصول', 'الأصول المقابلة', 'مجمع الإطفاء',
 'C', 100, 220, 240),

-- =============================================================================
-- BALANCE SHEET — LIABILITIES
-- =============================================================================

('LIABILITY', 'Balance Sheet',
 'Liabilities', 'Liabilities', 'Liabilities',
 'الخصوم', 'الخصوم', 'الخصوم',
 'C', 300, 300, 300),

('CURRENT_LIABILITY', 'Balance Sheet',
 'Liabilities', 'Current Liabilities', 'Current Liabilities',
 'الخصوم', 'الخصوم المتداولة', 'الخصوم المتداولة',
 'C', 300, 310, 310),

('LONGTERM_LIABILITY', 'Balance Sheet',
 'Liabilities', 'Non-Current Liabilities', 'Non-Current Liabilities',
 'الخصوم', 'الخصوم غير المتداولة', 'الخصوم غير المتداولة',
 'C', 300, 400, 410),

-- =============================================================================
-- BALANCE SHEET — EQUITY
-- =============================================================================

('EQUITY', 'Balance Sheet',
 'Equity', 'Equity', 'Equity',
 'حقوق الملكية', 'حقوق الملكية', 'حقوق الملكية',
 'C', 500, 500, 500),

('OWNERS_EQUITY', 'Balance Sheet',
 'Equity', 'Equity', 'Owner’s Equity',
 'حقوق الملكية', 'حقوق الملكية', 'حقوق الملكية - المالك',
 'C', 500, 510, 510),

('RETAINED_EARNINGS', 'Balance Sheet',
 'Equity', 'Equity', 'Retained Earnings',
 'حقوق الملكية', 'حقوق الملكية', 'الأرباح المحتجزة',
 'C', 500, 520, 520),

('DISTRIBUTION', 'Balance Sheet',
 'Equity', 'Distributions', 'Distributions',
 'حقوق الملكية', 'التوزيعات', 'التوزيعات',
 'D', 500, 530, 530),

('DIVIDEND', 'Balance Sheet',
 'Equity', 'Distributions', 'Dividends',
 'حقوق الملكية', 'التوزيعات', 'توزيعات الأرباح',
 'D', 500, 530, 540),

('RETURN_OF_CAPITAL', 'Balance Sheet',
 'Equity', 'Distributions', 'Return of Capital',
 'حقوق الملكية', 'التوزيعات', 'رد رأس المال',
 'D', 500, 530, 550),

-- =============================================================================
-- PROFIT & LOSS — REVENUE
-- =============================================================================

('REVENUE', 'Profit and Loss',
 'Revenue', 'Revenue', 'Revenue',
 'الإيرادات', 'الإيرادات', 'الإيرادات',
 'C', 600, 600, 600),

('CONTRA_REVENUE', 'Profit and Loss',
 'Revenue', 'Contra Revenue', 'Contra Revenue',
 'الإيرادات', 'الإيرادات المقابلة', 'الإيرادات المقابلة',
 'D', 600, 610, 610),

('INCOME', 'Profit and Loss',
 'Revenue', 'Other Income', 'Other Income',
 'الإيرادات', 'إيرادات أخرى', 'إيرادات أخرى',
 'C', 600, 620, 620),

('CASH_INCOME', 'Profit and Loss',
 'Revenue', 'Other Income', 'Cash Income',
 'الإيرادات', 'إيرادات أخرى', 'إيرادات نقدية',
 'C', 600, 620, 630),

('NON_CASH_INCOME', 'Profit and Loss',
 'Revenue', 'Other Income', 'Non-Cash Income',
 'الإيرادات', 'إيرادات أخرى', 'إيرادات غير نقدية',
 'C', 600, 620, 640),

-- =============================================================================
-- PROFIT & LOSS — EXPENSES
-- =============================================================================

('EXPENSE', 'Profit and Loss',
 'Expenses', 'Expenses', 'Expenses',
 'المصروفات', 'المصروفات', 'المصروفات',
 'D', 700, 700, 700),

('CASH_EXPENSE', 'Profit and Loss',
 'Expenses', 'Cash Expenses', 'Cash Expenses',
 'المصروفات', 'مصروفات نقدية', 'مصروفات نقدية',
 'D', 700, 710, 710),

('COGS_EXPENSE', 'Profit and Loss',
 'Expenses', 'Cash Expenses', 'Cost of Goods Sold',
 'المصروفات', 'مصروفات نقدية', 'تكلفة البضاعة المباعة',
 'D', 700, 710, 720),

('SGA_EXPENSE', 'Profit and Loss',
 'Expenses', 'Cash Expenses', 'Selling, General and Administrative',
 'المصروفات', 'مصروفات نقدية', 'مصروفات عمومية وإدارية',
 'D', 700, 710, 730),

('INTEREST_EXPENSE', 'Profit and Loss',
 'Expenses', 'Cash Expenses', 'Interest Expense',
 'المصروفات', 'مصروفات نقدية', 'مصروفات فوائد',
 'D', 700, 710, 740),

('NON_CASH_EXPENSE', 'Profit and Loss',
 'Expenses', 'Non-Cash Expenses', 'Non-Cash Expenses',
 'المصروفات', 'مصروفات غير نقدية', 'مصروفات غير نقدية',
 'D', 700, 800, 800),

('DEPRECIATION', 'Profit and Loss',
 'Expenses', 'Non-Cash Expenses', 'Depreciation',
 'المصروفات', 'مصروفات غير نقدية', 'الإهلاك',
 'D', 700, 800, 810),

('AMORTIZATION', 'Profit and Loss',
 'Expenses', 'Non-Cash Expenses', 'Amortization',
 'المصروفات', 'مصروفات غير نقدية', 'الإطفاء',
 'D', 700, 800, 820),

('INVENTORY_ADJUST', 'Profit and Loss',
 'Expenses', 'Non-Cash Expenses', 'Inventory Adjustment',
 'المصروفات', 'مصروفات غير نقدية', 'تسوية المخزون',
 'D', 700, 800, 830),

-- =============================================================================
-- EXCLUDED
-- =============================================================================

('DEBIT', 'Excluded',
 'Excluded', 'Excluded', 'Excluded',
 'مستبعد', 'مستبعد', 'مستبعد',
 'D', 9000, 9000, 9000),

('CREDIT', 'Excluded',
 'Excluded', 'Excluded', 'Excluded',
 'مستبعد', 'مستبعد', 'مستبعد',
 'C', 9000, 9000, 9000),

('NON_POSTING', 'Excluded',
 'Excluded', 'Excluded', 'Excluded',
 'مستبعد', 'مستبعد', 'مستبعد',
 'D', 9000, 9000, 9000),

('RESOURCE', 'Excluded',
 'Excluded', 'Excluded', 'Excluded',
 'مستبعد', 'مستبعد', 'مستبعد',
 'D', 9000, 9000, 9000);