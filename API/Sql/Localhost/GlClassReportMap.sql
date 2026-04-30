-- Create the lookup table once
CREATE TABLE GlClassReportMap (
                                  gl_account_class_id   VARCHAR(50) PRIMARY KEY,
                                  report                VARCHAR(50),
                                  class                 VARCHAR(50),
                                  sub_class             VARCHAR(100),
                                  sub_class2            VARCHAR(100),
                                  normal_balance        CHAR(1),        -- D or C
                                  is_header             CHAR(1),        -- Y = grouping row, no direct accounts
                                  sort_order            INT
);

INSERT INTO GlClassReportMap VALUES
-- ── BALANCE SHEET : ASSETS ──────────────────────────────────────────
('CURRENT_ASSET',       'Balance Sheet', 'Assets',      'Current Assets',         NULL,                          'D', 'Y', 110),
('CASH_EQUIVALENT',     'Balance Sheet', 'Assets',      'Current Assets',         'Cash & Cash Equivalents',     'D', 'N', 120),
('INVENTORY_ASSET',     'Balance Sheet', 'Assets',      'Current Assets',         'Inventory',                   'D', 'N', 130),
('LONGTERM_ASSET',      'Balance Sheet', 'Assets',      'Non-Current Assets',     'Property, Plant & Equipment', 'D', 'N', 210),
('CONTRA_ASSET',        'Balance Sheet', 'Assets',      'Non-Current Assets',     'Contra Assets',               'C', 'Y', 220),
('ACCUM_DEPRECIATION',  'Balance Sheet', 'Assets',      'Non-Current Assets',     'Accumulated Depreciation',    'C', 'N', 230),
('ACCUM_AMORTIZATION',  'Balance Sheet', 'Assets',      'Non-Current Assets',     'Accumulated Amortization',    'C', 'N', 240),

-- ── BALANCE SHEET : LIABILITIES ─────────────────────────────────────
('CURRENT_LIABILITY',   'Balance Sheet', 'Liabilities', 'Current Liabilities',    NULL,                          'C', 'N', 310),
('LONGTERM_LIABILITY',  'Balance Sheet', 'Liabilities', 'Non-Current Liabilities',NULL,                          'C', 'N', 410),

-- ── BALANCE SHEET : EQUITY ──────────────────────────────────────────
('EQUITY',              'Balance Sheet', 'Equity',      'Equity',                 NULL,                          'C', 'Y', 510),
('OWNERS_EQUITY',       'Balance Sheet', 'Equity',      'Equity',                 'Owners Equity',               'C', 'N', 520),
('RETAINED_EARNINGS',   'Balance Sheet', 'Equity',      'Equity',                 'Retained Earnings',           'C', 'N', 530),
('DISTRIBUTION',        'Balance Sheet', 'Equity',      'Equity Distributions',   NULL,                          'D', 'Y', 540),
('DIVIDEND',            'Balance Sheet', 'Equity',      'Equity Distributions',   'Dividends',                   'D', 'N', 550),
('RETURN_OF_CAPITAL',   'Balance Sheet', 'Equity',      'Equity Distributions',   'Return of Capital',           'D', 'N', 560),

-- ── P&L : REVENUE ───────────────────────────────────────────────────
('INCOME',              'P&L',           'Revenue',     'Income',                 NULL,                          'C', 'Y', 610),
('REVENUE',             'P&L',           'Revenue',     'Revenue',                'Operating Revenue',           'C', 'N', 620),
('CASH_INCOME',         'P&L',           'Revenue',     'Other Income',           'Cash Income',                 'C', 'N', 630),
('NON_CASH_INCOME',     'P&L',           'Revenue',     'Other Income',           'Non-Cash Income',             'C', 'N', 640),
('CONTRA_REVENUE',      'P&L',           'Revenue',     'Revenue Deductions',     'Returns & Allowances',        'D', 'N', 650),

-- ── P&L : COST OF SALES ─────────────────────────────────────────────
('COGS_EXPENSE',        'P&L',           'Cost of Sales','Cost of Goods Sold',   'COGS',                        'D', 'N', 710),

-- ── P&L : OPERATING EXPENSES ────────────────────────────────────────
('EXPENSE',             'P&L',           'Expenses',    'Operating Expenses',     NULL,                          'D', 'Y', 810),
('CASH_EXPENSE',        'P&L',           'Expenses',    'Operating Expenses',     NULL,                          'D', 'Y', 820),
('SGA_EXPENSE',         'P&L',           'Expenses',    'Operating Expenses',     'Selling, General & Admin',    'D', 'N', 830),
('INTEREST_EXPENSE',    'P&L',           'Expenses',    'Finance Costs',          'Interest Expense',            'D', 'N', 840),

-- ── P&L : NON-CASH CHARGES ──────────────────────────────────────────
('NON_CASH_EXPENSE',    'P&L',           'Expenses',    'Non-Cash Charges',       NULL,                          'D', 'Y', 910),
('DEPRECIATION',        'P&L',           'Expenses',    'Non-Cash Charges',       'Depreciation',                'D', 'N', 920),
('AMORTIZATION',        'P&L',           'Expenses',    'Non-Cash Charges',       'Amortization',                'D', 'N', 930),
('INVENTORY_ADJUST',    'P&L',           'Expenses',    'Non-Cash Charges',       'Inventory Adjustment',        'D', 'N', 940),

-- ── EXCLUDED ────────────────────────────────────────────────────────
('NON_POSTING',         NULL,            NULL,          NULL,                     NULL,                          NULL,'Y', 999),
('RESOURCE',            NULL,            NULL,          NULL,                     NULL,                          NULL,'Y', 999),
('ASSET',               NULL,            NULL,          NULL,                     NULL,                          'D', 'Y', 100),
('LIABILITY',           NULL,            NULL,          NULL,                     NULL,                          'C', 'Y', 300),
('DEBIT',               NULL,            NULL,          NULL,                     NULL,                          'D', 'Y',   0),
('CREDIT',              NULL,            NULL,          NULL,                     NULL,                          'C', 'Y',   0);