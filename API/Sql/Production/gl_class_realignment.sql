-- =============================================================================
-- gl_class_realignment.sql
-- Database: erp_contracts  (dev mirrors production @ 129.146.22.240:3308)
-- Prepared 2026-07-29
--
-- Realigns 6 accounts to the reference chart-of-accounts taxonomy that
-- Dim_gl_account was designed from (the training "Chart of Accounts" sheet):
--   * 3 sales-commission accounts  -> Trading account / Cost of Sales
--   * 3 loan-interest accounts      -> Interest & Tax
--
-- -----------------------------------------------------------------------------
-- PLAIN-LANGUAGE EXPLANATION (for the non-accountant)
-- -----------------------------------------------------------------------------
-- Think of the income statement as a staircase you walk DOWN from Sales to
-- final profit. Each step subtracts a different kind of cost, and each step
-- has a name:
--
--     Sales
--      └─ minus  Cost of Sales ........... = GROSS PROFIT      (step 1: "Trading")
--          └─ minus  Running-the-business . = OPERATING PROFIT  (step 2: "Operating")
--              └─ minus  Interest & Tax .... = NET PROFIT        (step 3: "Interest & Tax")
--
-- Every account carries a tag ("Class") saying which step it belongs to.
-- Six accounts were tagged on the WRONG step:
--
--  1) Three "Naseem sales commission" accounts (what we pay agents/staff to
--     SELL the units) were tagged as ordinary running-the-business costs.
--     They are really a direct cost of making the sale, so they belong on
--     step 1 (Cost of Sales). Putting them there means GROSS PROFIT finally
--     reflects the true cost of selling.
--
--  2) Three "loan interest / financing" accounts were also tagged as
--     running-the-business costs. But interest is the cost of BORROWING money,
--     not of operating — it belongs on its own step 3 (Interest & Tax), below
--     operating profit. Moving them there stops interest from wrongly dragging
--     down OPERATING PROFIT.
--
-- Net effect: the three profit subtotals (gross / operating / net) each line up
-- with what they are supposed to mean, exactly like the textbook layout the
-- whole model was built from. Total net profit does NOT change — only which
-- step each cost sits on.
-- -----------------------------------------------------------------------------
--
-- SAFETY: only classification tags change; no amounts, dates, or postings are
-- touched. Idempotent (deterministic UPDATEs, safe to re-run).
--
-- DURABILITY NOTE: this also updates GL_ACCOUNT_CLASS_ID (the account's root
-- nature) so the tags are correct at the source. However, the auto-classifier
-- script (gl_account_classifier_update.sql) currently maps class INTEREST_EXPENSE
-- to "Operating account", so if that script is ever re-run it would push the
-- 3 interest accounts back. Either don't re-run it, or update its
-- INTEREST_EXPENSE -> GL_CLASS_COURSE_ID mapping to 'INTEREST_AND_TAX' first.
-- =============================================================================

START TRANSACTION;

-- =============================================================================
-- SECTION 1 — Sales commissions -> Trading account / Cost of Sales
-- Accounts: 600025, 600026, 600027 (Naseem external / sales-staff / internal commissions)
-- From: Operating account · Administration · Advertisements   (class was CASH_EXPENSE)
-- To:   Trading account · Cost of Sales · Cost of Sales        (matches the 4 clean COGS accounts)
-- =============================================================================
UPDATE GL_ACCOUNT
SET GL_ACCOUNT_CLASS_ID         = 'COGS_EXPENSE',     -- root nature: was CASH_EXPENSE
    GL_CLASS_COURSE_ID          = 'TRADING_ACCOUNT',  -- CLASS: was OPERATING_ACCOUNT
    GL_SUB_CLASS_ID             = 'COST_OF_SALES',    -- (already COST_OF_SALES)
    GL_SUB_CLASS_2_ID           = 'COST_OF_SALES',    -- SUBCLASS2: was ADMINISTRATION
    GL_ACCOUNT_COURSE_LABEL_ID  = 'COST_OF_SALES'     -- ACCOUNT line: was ADVERTISEMENTS
WHERE GL_ACCOUNT_ID IN ('600025','600026','600027');
-- expect: 3 rows affected

-- =============================================================================
-- SECTION 2 — Loan interest / financing -> Interest & Tax
-- Accounts: 600010, 600011, 600012 (financing expenses / loan interest / other financing)
-- From: Operating account · Financial Expenses · (Other Expenses/Interest Expense)
-- To:   Interest & Tax · Interest Expense · Interest Expense   (reference row 440)
-- (Keeps GL_SUB_CLASS_ID = INTEREST_EXPENSE, already correct.)
-- =============================================================================
UPDATE GL_ACCOUNT
SET GL_ACCOUNT_CLASS_ID         = 'INTEREST_EXPENSE', -- root nature: was CASH_EXPENSE
    GL_CLASS_COURSE_ID          = 'INTEREST_AND_TAX', -- CLASS: was OPERATING_ACCOUNT
    GL_SUB_CLASS_2_ID           = 'INTEREST_EXPENSE', -- SUBCLASS2: was FINANCIAL_EXPENSES ‡
    GL_ACCOUNT_COURSE_LABEL_ID  = 'INTEREST_EXPENSE'  -- ACCOUNT line: was OTHER_EXPENSES (600010/600012)
WHERE GL_ACCOUNT_ID IN ('600010','600011','600012');
-- expect: 3 rows affected
-- ‡ If you prefer to KEEP your "Financial Expenses" sub-grouping, drop the
--   GL_SUB_CLASS_2_ID line above — 'FINANCIAL_EXPENSES' sits fine under Interest & Tax.

COMMIT;

-- =============================================================================
-- VERIFICATION
-- =============================================================================
-- The 6 realigned accounts, now on the correct class/tier:
SELECT GL_ACCOUNT_ID, ACCOUNT_NAME_ARABIC, CLASS, SUBCLASS, SUBCLASS2, ACCOUNT
FROM Dim_gl_account
WHERE GL_ACCOUNT_ID IN ('600010','600011','600012','600025','600026','600027')
ORDER BY CLASS, GL_ACCOUNT_ID;

-- Cost of Sales should now sit entirely in Trading account (expect: 1 row, TRADING_ACCOUNT):
SELECT CLASS, COUNT(*) accts FROM Dim_gl_account WHERE SUBCLASS='COST_OF_SALES' GROUP BY CLASS;

-- The Interest & Tax class should no longer be empty (expect the 3 interest accounts):
SELECT GL_ACCOUNT_ID, ACCOUNT_NAME_ARABIC FROM Dim_gl_account WHERE CLASS='INTEREST_AND_TAX' ORDER BY GL_ACCOUNT_ID;

-- =============================================================================
-- After running: refresh the Power BI model. The subclass-based DAX measures
-- already produce correct totals (Gross Profit 20.79M, EBIT -5.62M, Net -6.21M);
-- this change makes the CLASS-level P&L presentation match, and makes the
-- "Interest & Tax" section appear below operating profit as in the reference.
-- =============================================================================
