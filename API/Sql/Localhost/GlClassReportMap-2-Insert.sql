INSERT INTO GlClassReportMap VALUES

-- =============================================================================
-- BALANCE SHEET — ASSETS
-- =============================================================================

('ASSET', 'Balance Sheet', 'Assets', 'Assets', 'Assets', 'D', 100, 100, 100),

('CURRENT_ASSET', 'Balance Sheet', 'Assets', 'Current Assets', 'Current Assets', 'D', 100, 110, 110),

('CASH_EQUIVALENT', 'Balance Sheet', 'Assets', 'Current Assets', 'Cash and Cash Equivalents', 'D', 100, 110, 120),

('INVENTORY_ASSET', 'Balance Sheet', 'Assets', 'Current Assets', 'Inventory', 'D', 100, 110, 130),

('LONGTERM_ASSET', 'Balance Sheet', 'Assets', 'Non-Current Assets', 'Non-Current Assets', 'D', 100, 200, 210),

('CONTRA_ASSET', 'Balance Sheet', 'Assets', 'Contra Assets', 'Contra Assets', 'C', 100, 220, 220),

('ACCUM_DEPRECIATION', 'Balance Sheet', 'Assets', 'Contra Assets', 'Accumulated Depreciation', 'C', 100, 220, 230),

('ACCUM_AMORTIZATION', 'Balance Sheet', 'Assets', 'Contra Assets', 'Accumulated Amortization', 'C', 100, 220, 240),

-- =============================================================================
-- BALANCE SHEET — LIABILITIES
-- =============================================================================

('LIABILITY', 'Balance Sheet', 'Liabilities', 'Liabilities', 'Liabilities', 'C', 300, 300, 300),

('CURRENT_LIABILITY', 'Balance Sheet', 'Liabilities', 'Current Liabilities', 'Current Liabilities', 'C', 300, 310, 310),

('LONGTERM_LIABILITY', 'Balance Sheet', 'Liabilities', 'Non-Current Liabilities', 'Non-Current Liabilities', 'C', 300, 400, 410),

-- =============================================================================
-- BALANCE SHEET — EQUITY
-- =============================================================================

('EQUITY', 'Balance Sheet', 'Equity', 'Equity', 'Equity', 'C', 500, 500, 500),

('OWNERS_EQUITY', 'Balance Sheet', 'Equity', 'Equity', 'Owner’s Equity', 'C', 500, 510, 510),

('RETAINED_EARNINGS', 'Balance Sheet', 'Equity', 'Equity', 'Retained Earnings', 'C', 500, 520, 520),

('DISTRIBUTION', 'Balance Sheet', 'Equity', 'Distributions', 'Distributions', 'D', 500, 530, 530),

('DIVIDEND', 'Balance Sheet', 'Equity', 'Distributions', 'Dividends', 'D', 500, 530, 540),

('RETURN_OF_CAPITAL', 'Balance Sheet', 'Equity', 'Distributions', 'Return of Capital', 'D', 500, 530, 550),

-- =============================================================================
-- PROFIT & LOSS — REVENUE
-- =============================================================================

('REVENUE', 'Profit and Loss', 'Revenue', 'Revenue', 'Revenue', 'C', 600, 600, 600),

('CONTRA_REVENUE', 'Profit and Loss', 'Revenue', 'Contra Revenue', 'Contra Revenue', 'D', 600, 610, 610),

('INCOME', 'Profit and Loss', 'Revenue', 'Other Income', 'Other Income', 'C', 600, 620, 620),

('CASH_INCOME', 'Profit and Loss', 'Revenue', 'Other Income', 'Cash Income', 'C', 600, 620, 630),

('NON_CASH_INCOME', 'Profit and Loss', 'Revenue', 'Other Income', 'Non-Cash Income', 'C', 600, 620, 640),

-- =============================================================================
-- PROFIT & LOSS — EXPENSES
-- =============================================================================

('EXPENSE', 'Profit and Loss', 'Expenses', 'Expenses', 'Expenses', 'D', 700, 700, 700),

('CASH_EXPENSE', 'Profit and Loss', 'Expenses', 'Cash Expenses', 'Cash Expenses', 'D', 700, 710, 710),

('COGS_EXPENSE', 'Profit and Loss', 'Expenses', 'Cash Expenses', 'Cost of Goods Sold', 'D', 700, 710, 720),

('SGA_EXPENSE', 'Profit and Loss', 'Expenses', 'Cash Expenses', 'Selling, General and Administrative', 'D', 700, 710, 730),

('INTEREST_EXPENSE', 'Profit and Loss', 'Expenses', 'Cash Expenses', 'Interest Expense', 'D', 700, 710, 740),

('NON_CASH_EXPENSE', 'Profit and Loss', 'Expenses', 'Non-Cash Expenses', 'Non-Cash Expenses', 'D', 700, 800, 800),

('DEPRECIATION', 'Profit and Loss', 'Expenses', 'Non-Cash Expenses', 'Depreciation', 'D', 700, 800, 810),

('AMORTIZATION', 'Profit and Loss', 'Expenses', 'Non-Cash Expenses', 'Amortization', 'D', 700, 800, 820),

('INVENTORY_ADJUST', 'Profit and Loss', 'Expenses', 'Non-Cash Expenses', 'Inventory Adjustment', 'D', 700, 800, 830),

-- =============================================================================
-- EXCLUDED (kept for FK integrity)
-- =============================================================================

('DEBIT', 'Excluded', 'Excluded', 'Excluded', 'Excluded', 'D', 9000, 9000, 9000),

('CREDIT', 'Excluded', 'Excluded', 'Excluded', 'Excluded', 'C', 9000, 9000, 9000),

('NON_POSTING', 'Excluded', 'Excluded', 'Excluded', 'Excluded', 'D', 9000, 9000, 9000),

('RESOURCE', 'Excluded', 'Excluded', 'Excluded', 'Excluded', 'D', 9000, 9000, 9000);