-- =============================================================================
-- cashflow_st_view.sql   —   CashFlow_St driver view for Power BI
-- Database: erp_contracts
-- Prepared 2026-07-29
--
-- A driver / bridge view that maps every posting account to a Cash Flow
-- Statement line, keyed on GL_ACCOUNT_ID so it joins straight to Dim_gl_account.
-- Load it in Power BI, relate CashFlow_St[GL_ACCOUNT_ID] -> Dim_gl_account,
-- use LINE_TYPE / LINE_SUBTYPE (sorted by SORT_TYPE / SORT_SUBTYPE) as the
-- visual rows, and one measure ([Cash Flow Amount], below) as the value.
--
-- WHY THIS RECONCILES BY CONSTRUCTION (indirect method, double-entry identity):
--   Every period, total debits = total credits, so across ALL accounts
--   SUM(debit - credit) = 0.  Cash's own (debit - credit) is its net increase.
--   Therefore  net cash increase = -( sum of (debit - credit) over every
--   NON-cash account ).  So each non-cash account contributes exactly
--   -Total_FTP_Raw to the cash movement, and:
--       Cash at start + Operating + Investing + Financing = Cash at end
--   holds automatically. The self-check card can never drift if every non-cash
--   account is bucketed exactly once (this view partitions them cleanly).
--
-- VALUE_TYPE codes used here (see the SWITCH measure at the bottom):
--   Opening_balance -> [Opening TTD]         (cash balance before the period)
--   Closing_balance -> [Total_TTD]           (cash balance through period end)
--   All_FTP         -> -1 * [Total_FTP_Raw]  (period cash contribution / P&L flow)
--
-- NOTE: this is the readable-yet-reconciling BUCKET version (Operating = net
--   profit + working-capital movements; Investing = non-current assets;
--   Financing = equity + long-term debt). The full textbook presentation
--   (profit-before-tax, then non-cash add-backs, then interest/tax paid split
--   out) is a refinement layer of extra rows on top of this — see the sketch
--   doc. This version is the correct, always-tying foundation.
--
-- IMPORTANT — the tie-check is a DATA-QUALITY DETECTOR, not just a proof:
--   Reconciliation holds over the CLASSIFIED universe only. Posting accounts
--   that are excluded from Dim_gl_account (GL_REPORT_ID / classification NULL,
--   or non-posting/adjusting classes) carry real cash-affecting movement that
--   lands in NO bucket. On a 2026 test this residual was ~10.5M, almost all of
--   it unclassified customer receivables (e.g. 120199-120234 'مدينون - <name>').
--   So a non-zero CF Tie Check quantifies exactly how much money sits in
--   unclassified accounts — classify them and the statement ties. Diagnostic
--   query is in the VERIFICATION block below.
-- =============================================================================

CREATE OR REPLACE VIEW `CashFlow_St` AS

-- 1 ── Cash & cash equivalents at the START of the period ---------------------
SELECT 'النقدية وما في حكمها في بداية الفترة' AS LINE_TYPE,
       'رصيد النقدية أول الفترة'              AS LINE_SUBTYPE,
       d.GL_ACCOUNT_ID, d.ACCOUNT_AR,
       'Opening_balance'                       AS VALUE_TYPE,
       1 AS SORT_TYPE, 10 AS SORT_SUBTYPE
FROM Dim_gl_account d
WHERE d.ACCOUNT = 'CASH_AND_CASH_EQUIVALENTS'

UNION ALL
-- 2 ── OPERATING: net profit/loss for the period (all P&L accounts) -----------
SELECT 'التدفقات النقدية من الأنشطة التشغيلية',
       'صافي ربح/خسارة الفترة',
       d.GL_ACCOUNT_ID, d.ACCOUNT_AR,
       'All_FTP', 2, 21
FROM Dim_gl_account d
WHERE d.REPORT = 'PROFIT_AND_LOSS'

UNION ALL
-- 2 ── OPERATING: working-capital movements (current assets excl. cash) -------
SELECT 'التدفقات النقدية من الأنشطة التشغيلية',
       CASE d.ACCOUNT
            WHEN 'RECEIVABLES'          THEN 'التغير في الذمم المدينة'
            WHEN 'INVENTORY'            THEN 'التغير في المخزون'
            WHEN 'INVENTORY_LANDS'      THEN 'التغير في المخزون'
            ELSE 'التغير في الأصول المتداولة الأخرى'
       END,
       d.GL_ACCOUNT_ID, d.ACCOUNT_AR,
       'All_FTP', 2,
       CASE d.ACCOUNT WHEN 'RECEIVABLES' THEN 22
                      WHEN 'INVENTORY' THEN 23 WHEN 'INVENTORY_LANDS' THEN 23
                      ELSE 24 END
FROM Dim_gl_account d
WHERE d.REPORT = 'BALANCE_SHEET'
  AND d.SUBCLASS = 'ASSETS'
  AND d.SUBCLASS2 = 'CURRENT_ASSETS'
  AND d.ACCOUNT <> 'CASH_AND_CASH_EQUIVALENTS'

UNION ALL
-- 2 ── OPERATING: working-capital movements (current liabilities) -------------
SELECT 'التدفقات النقدية من الأنشطة التشغيلية',
       'التغير في الدائنين والالتزامات المتداولة',
       d.GL_ACCOUNT_ID, d.ACCOUNT_AR,
       'All_FTP', 2, 25
FROM Dim_gl_account d
WHERE d.REPORT = 'BALANCE_SHEET'
  AND d.SUBCLASS = 'LIABILITIES'
  AND d.SUBCLASS2 <> 'LONG_TERM_LIABILITIES'   -- current liabs (incl. a stray SUBCLASS2=CURRENT_ASSETS payable)

UNION ALL
-- 3 ── INVESTING: movements in non-current assets -----------------------------
SELECT 'التدفقات النقدية من الأنشطة الاستثمارية',
       'التغير في الأصول غير المتداولة',
       d.GL_ACCOUNT_ID, d.ACCOUNT_AR,
       'All_FTP', 3, 31
FROM Dim_gl_account d
WHERE d.REPORT = 'BALANCE_SHEET'
  AND d.SUBCLASS = 'ASSETS'
  AND d.SUBCLASS2 = 'NON_CURRENT_ASSETS'

UNION ALL
-- 4 ── FINANCING: movements in owners' equity (capital, dividends) ------------
SELECT 'التدفقات النقدية من الأنشطة التمويلية',
       'التغير في حقوق الملكية',
       d.GL_ACCOUNT_ID, d.ACCOUNT_AR,
       'All_FTP', 4, 41
FROM Dim_gl_account d
WHERE d.SUBCLASS = 'OWNERS_EQUITY'

UNION ALL
-- 4 ── FINANCING: movements in long-term liabilities --------------------------
SELECT 'التدفقات النقدية من الأنشطة التمويلية',
       'التغير في الالتزامات طويلة الأجل',
       d.GL_ACCOUNT_ID, d.ACCOUNT_AR,
       'All_FTP', 4, 42
FROM Dim_gl_account d
WHERE d.REPORT = 'BALANCE_SHEET'
  AND d.SUBCLASS = 'LIABILITIES'
  AND d.SUBCLASS2 = 'LONG_TERM_LIABILITIES'

UNION ALL
-- 5 ── Cash & cash equivalents at the END of the period -----------------------
SELECT 'النقدية وما في حكمها في نهاية الفترة',
       'رصيد النقدية آخر الفترة',
       d.GL_ACCOUNT_ID, d.ACCOUNT_AR,
       'Closing_balance', 5, 50
FROM Dim_gl_account d
WHERE d.ACCOUNT = 'CASH_AND_CASH_EQUIVALENTS';


-- =============================================================================
-- VERIFICATION (read-only)
-- =============================================================================
-- Row counts per section, and confirm every non-cash posting account is
-- bucketed exactly once (movement rows should equal the count of non-cash
-- accounts in Dim_gl_account):
SELECT SORT_TYPE, LINE_TYPE, COUNT(*) rows FROM CashFlow_St GROUP BY 1,2 ORDER BY 1;

SELECT
  (SELECT COUNT(*) FROM CashFlow_St WHERE VALUE_TYPE='All_FTP')                     AS movement_rows,
  (SELECT COUNT(*) FROM Dim_gl_account WHERE ACCOUNT <> 'CASH_AND_CASH_EQUIVALENTS'
     AND (REPORT='PROFIT_AND_LOSS'
          OR (SUBCLASS IN ('ASSETS','LIABILITIES','OWNERS_EQUITY'))))              AS noncash_accounts;
-- movement_rows should equal noncash_accounts (clean partition, no gaps/overlaps).
-- Verified 2026: 818 non-cash accounts = 91 P&L + 341 curr-assets + 355 curr-liab
--                + 17 investing + 12 equity + 2 long-term-liab.  Exact cover.

-- Reconciliation LEAK diagnostic — posting accounts with movement but NOT in the
-- classified chart (these are what a non-zero CF Tie Check is measuring).
-- Set the date range to your reporting period:
SELECT ga.GL_ACCOUNT_ID, ga.ACCOUNT_NAME_ARABIC, ga.GL_ACCOUNT_CLASS_ID,
       ROUND(SUM(e.AMOUNT * IF(e.DEBIT_CREDIT_FLAG='D',1,-1))) AS movement
FROM ACCTG_TRANS_ENTRY e
JOIN ACCTG_TRANS t ON t.ACCTG_TRANS_ID = e.ACCTG_TRANS_ID AND t.IS_POSTED='Y'
                  AND t.TRANSACTION_DATE >= '2026-01-01' AND t.TRANSACTION_DATE < '2027-01-01'
LEFT JOIN Dim_gl_account d ON d.GL_ACCOUNT_ID = e.GL_ACCOUNT_ID
WHERE d.GL_ACCOUNT_ID IS NULL
GROUP BY 1,2,3 HAVING ABS(movement) > 0 ORDER BY ABS(movement) DESC;


-- =============================================================================
-- POWER BI SIDE — measures to add (Fact_GL_Transactions)
-- =============================================================================
-- PeriodStartDate = MIN ( DateTbl[Date] )              -- from the date slicer
-- Opening TTD =
--     CALCULATE ( [Total_TTD],
--         DATESBETWEEN ( DateTbl[Date], [MinDateAcross], [PeriodStartDate] - 1 ) )
--
-- Cash Flow Amount =
--     SUMX ( VALUES ( CashFlow_St[VALUE_TYPE] ),
--         SWITCH ( CashFlow_St[VALUE_TYPE],
--             "Opening_balance", [Opening TTD],
--             "Closing_balance", [Total_TTD],
--             "All_FTP",         -1 * [Total_FTP_Raw],
--             BLANK () ) )
--
-- SELF-CHECK card (0 = perfectly classified; any residual = money sitting in
-- unclassified/excluded posting accounts — see the LEAK diagnostic above):
-- CF Tie Check =
--     VAR sections = CALCULATE ( [Cash Flow Amount],
--                        CashFlow_St[VALUE_TYPE] = "All_FTP" )
--     VAR opening  = CALCULATE ( [Cash Flow Amount],
--                        CashFlow_St[VALUE_TYPE] = "Opening_balance" )
--     VAR closing  = CALCULATE ( [Cash Flow Amount],
--                        CashFlow_St[VALUE_TYPE] = "Closing_balance" )
--     RETURN ( opening + sections ) - closing
-- =============================================================================
