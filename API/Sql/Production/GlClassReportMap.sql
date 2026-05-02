-- =============================================================================
-- GlClassReportMap  v3
--
-- SOURCE OF TRUTH: OFBiz GlAccountClass table only.
-- No COA sheet labels. No assumptions.
-- Every value in class / sub_class / sub_class2 is the DESCRIPTION
-- field from GlAccountClass, resolved by walking the parent chain.
--
-- Column derivation rules (applied consistently to every row):
--   report     = derived from the L1 ancestor (first child of DEBIT/CREDIT root)
--                ASSET / LIABILITY / EQUITY / CONTRA_ASSET / DISTRIBUTION → Balance Sheet
--                EXPENSE / INCOME / REVENUE / CONTRA_REVENUE              → Profit and Loss
--                DEBIT / CREDIT / RESOURCE / NON_POSTING                  → Excluded
--
--   class      = DESCRIPTION of L1 ancestor
--                (the first meaningful level below DEBIT/CREDIT)
--
--   sub_class  = DESCRIPTION of L2 ancestor
--                (parent of this class; falls back to class if no L2)
--
--   sub_class2 = DESCRIPTION of this class itself  ← ALWAYS POPULATED, never null
--                This is the value Power BI groups and subtotals on.
--
--   normal_balance = D if root is DEBIT, C if root is CREDIT
--                    Exception: CONTRA_ASSET branch → C (credit reduces assets)
--                               DISTRIBUTION branch → C (distributions reduce equity)
--                               CONTRA_REVENUE      → D (reduces revenue)
--
--   sort_order = manually assigned, follows standard financial statement order
--
-- HOW Power BI USES THIS TABLE:
--   JOIN GlAccount.glAccountClassId = GlClassReportMap.gl_account_class_id
--   Use report as page filter, class → sub_class → sub_class2 as matrix rows.
--   signed_amount = CASE WHEN normal_balance = debitCreditFlag THEN +amount ELSE -amount END
-- =============================================================================

DROP TABLE IF EXISTS GlClassReportMap;

CREATE TABLE GlClassReportMap (
                                  gl_account_class_id  VARCHAR(50)  NOT NULL PRIMARY KEY,
                                  report               VARCHAR(50)  NOT NULL,
                                  class                VARCHAR(100) NOT NULL,
                                  sub_class            VARCHAR(100) NOT NULL,
                                  sub_class2           VARCHAR(100) NOT NULL,  -- never null by design
                                  normal_balance       CHAR(1)      NOT NULL,  -- D or C
                                  sort_order           INT          NOT NULL
);


INSERT INTO GlClassReportMap
(gl_account_class_id, report, class, sub_class, sub_class2, normal_balance, sort_order)
VALUES

-- =============================================================================
-- BALANCE SHEET — ASSETS
-- Root chain: DEBIT → ASSET → ...
-- class = 'Asset'  (OFBiz description of ASSET)
-- =============================================================================

-- ASSET itself (parent node — accounts rarely assigned directly)
('ASSET',
 'Balance Sheet', 'Asset', 'Asset', 'Asset',
 'D', 100),

-- DEBIT → ASSET → CURRENT_ASSET
('CURRENT_ASSET',
 'Balance Sheet', 'Asset', 'Asset', 'Current Asset',
 'D', 110),

-- DEBIT → ASSET → CURRENT_ASSET → CASH_EQUIVALENT
('CASH_EQUIVALENT',
 'Balance Sheet', 'Asset', 'Current Asset', 'Cash and Equivalent',
 'D', 120),

-- DEBIT → ASSET → CURRENT_ASSET → INVENTORY_ASSET
('INVENTORY_ASSET',
 'Balance Sheet', 'Asset', 'Current Asset', 'Inventory Asset',
 'D', 130),

-- DEBIT → ASSET → LONGTERM_ASSET
('LONGTERM_ASSET',
 'Balance Sheet', 'Asset', 'Asset', 'Long Term Asset',
 'D', 210),

-- CREDIT → CONTRA_ASSET  (credit-balance — reduces total assets)
('CONTRA_ASSET',
 'Balance Sheet', 'Contra Asset', 'Contra Asset', 'Contra Asset',
 'C', 220),

-- CREDIT → CONTRA_ASSET → ACCUM_DEPRECIATION
('ACCUM_DEPRECIATION',
 'Balance Sheet', 'Contra Asset', 'Contra Asset', 'Accumulated Depreciation',
 'C', 230),

-- CREDIT → CONTRA_ASSET → ACCUM_AMORTIZATION
('ACCUM_AMORTIZATION',
 'Balance Sheet', 'Contra Asset', 'Contra Asset', 'Accumulated Amortization',
 'C', 240),


-- =============================================================================
-- BALANCE SHEET — LIABILITIES
-- Root chain: CREDIT → LIABILITY → ...
-- class = 'Liability'
-- =============================================================================

('LIABILITY',
 'Balance Sheet', 'Liability', 'Liability', 'Liability',
 'C', 300),

-- CREDIT → LIABILITY → CURRENT_LIABILITY
('CURRENT_LIABILITY',
 'Balance Sheet', 'Liability', 'Liability', 'Current Liability',
 'C', 310),

-- CREDIT → LIABILITY → LONGTERM_LIABILITY
('LONGTERM_LIABILITY',
 'Balance Sheet', 'Liability', 'Liability', 'Long Term Liability',
 'C', 410),


-- =============================================================================
-- BALANCE SHEET — EQUITY
-- Root chain: CREDIT → EQUITY → ...
-- class = 'Equity'
-- =============================================================================

('EQUITY',
 'Balance Sheet', 'Equity', 'Equity', 'Equity',
 'C', 500),

-- CREDIT → EQUITY → OWNERS_EQUITY
('OWNERS_EQUITY',
 'Balance Sheet', 'Equity', 'Equity', 'Owners Equity',
 'C', 510),

-- CREDIT → EQUITY → RETAINED_EARNINGS
('RETAINED_EARNINGS',
 'Balance Sheet', 'Equity', 'Equity', 'Retained Earnings',
 'C', 520),

-- DEBIT → DISTRIBUTION  (reduces equity — debit normal balance)
('DISTRIBUTION',
 'Balance Sheet', 'Equity Distribution', 'Equity Distribution', 'Equity Distribution',
 'D', 530),

-- DEBIT → DISTRIBUTION → DIVIDEND
('DIVIDEND',
 'Balance Sheet', 'Equity Distribution', 'Equity Distribution', 'Dividends',
 'D', 540),

-- DEBIT → DISTRIBUTION → RETURN_OF_CAPITAL
('RETURN_OF_CAPITAL',
 'Balance Sheet', 'Equity Distribution', 'Equity Distribution', 'Return of Capital',
 'D', 550),


-- =============================================================================
-- PROFIT AND LOSS — REVENUE
-- Root chain: CREDIT → REVENUE
--             CREDIT → INCOME → ...
--             DEBIT  → CONTRA_REVENUE
-- =============================================================================

-- CREDIT → REVENUE (direct, no children in OFBiz standard data)
('REVENUE',
 'Profit and Loss', 'Revenue', 'Revenue', 'Revenue',
 'C', 600),

-- DEBIT → CONTRA_REVENUE (reduces revenue — debit normal balance)
('CONTRA_REVENUE',
 'Profit and Loss', 'Contra Revenue', 'Contra Revenue', 'Contra Revenue',
 'D', 610),

-- CREDIT → INCOME
('INCOME',
 'Profit and Loss', 'Income', 'Income', 'Income',
 'C', 620),

-- CREDIT → INCOME → CASH_INCOME
('CASH_INCOME',
 'Profit and Loss', 'Income', 'Income', 'Cash Income',
 'C', 630),

-- CREDIT → INCOME → NON_CASH_INCOME
('NON_CASH_INCOME',
 'Profit and Loss', 'Income', 'Income', 'Non-Cash Income',
 'C', 640),


-- =============================================================================
-- PROFIT AND LOSS — EXPENSES
-- Root chain: DEBIT → EXPENSE → CASH_EXPENSE → ...
--                              → NON_CASH_EXPENSE → ...
-- class = 'Expense'
-- =============================================================================

-- DEBIT → EXPENSE (top-level parent)
('EXPENSE',
 'Profit and Loss', 'Expense', 'Expense', 'Expense',
 'D', 700),

-- DEBIT → EXPENSE → CASH_EXPENSE
('CASH_EXPENSE',
 'Profit and Loss', 'Expense', 'Expense', 'Cash Expense',
 'D', 710),

-- DEBIT → EXPENSE → CASH_EXPENSE → COGS_EXPENSE
('COGS_EXPENSE',
 'Profit and Loss', 'Expense', 'Cash Expense', 'Cost of Goods Sold Expense',
 'D', 720),

-- DEBIT → EXPENSE → CASH_EXPENSE → SGA_EXPENSE
('SGA_EXPENSE',
 'Profit and Loss', 'Expense', 'Cash Expense', 'Selling, General, and Administrative Expense',
 'D', 730),

-- DEBIT → EXPENSE → CASH_EXPENSE → INTEREST_EXPENSE
('INTEREST_EXPENSE',
 'Profit and Loss', 'Expense', 'Cash Expense', 'Interest Expense',
 'D', 740),

-- DEBIT → EXPENSE → NON_CASH_EXPENSE
('NON_CASH_EXPENSE',
 'Profit and Loss', 'Expense', 'Expense', 'Non-Cash Expense',
 'D', 800),

-- DEBIT → EXPENSE → NON_CASH_EXPENSE → DEPRECIATION
('DEPRECIATION',
 'Profit and Loss', 'Expense', 'Non-Cash Expense', 'Depreciation',
 'D', 810),

-- DEBIT → EXPENSE → NON_CASH_EXPENSE → AMORTIZATION
('AMORTIZATION',
 'Profit and Loss', 'Expense', 'Non-Cash Expense', 'Amortization',
 'D', 820),

-- DEBIT → EXPENSE → NON_CASH_EXPENSE → INVENTORY_ADJUST
('INVENTORY_ADJUST',
 'Profit and Loss', 'Expense', 'Non-Cash Expense', 'Inventory Adjustment',
 'D', 830),


-- =============================================================================
-- EXCLUDED — root nodes and non-posting classes
-- Kept in the table so the FK is always satisfied.
-- WHERE report <> 'Excluded' filters them from all BI queries.
-- =============================================================================

('DEBIT',       'Excluded', 'Debit',       'Debit',       'Debit',       'D', 9000),
('CREDIT',      'Excluded', 'Credit',      'Credit',      'Credit',      'C', 9000),
('NON_POSTING', 'Excluded', 'Non-Posting', 'Non-Posting', 'Non-Posting', 'D', 9000),
('RESOURCE',    'Excluded', 'Resource',    'Resource',    'Resource',    'D', 9000);


-- =============================================================================
-- VERIFICATION — run after INSERT, must return 0 rows
-- =============================================================================
/*
SELECT gl_account_class_id
FROM   GlClassReportMap
WHERE  report      IS NULL
    OR class       IS NULL
    OR sub_class   IS NULL
    OR sub_class2  IS NULL
    OR normal_balance IS NULL;
*/
