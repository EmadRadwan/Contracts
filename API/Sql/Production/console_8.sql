-- =====================================================================
-- Dim_gl_account — classification fixes
-- Database: erp_contracts @ 129.146.22.240:3308
-- Source: Project-19/dim-gl-account-classification-fixes.sql
--
-- Fixes three defect classes found in the GL classification data:
--   A. Misclassified account  (124410 tagged as cash-in-banks)
--   B. Missing sub-account labels (5 accounts backfilled here, incl. 3 live banks)
--   C. No leaf/posting flag  (parent rollup accounts indistinguishable
--      from real posting accounts — e.g. 110000)
--
-- RUN ORDER: 1 -> 4 (wrapped in a transaction), then 5 separately (DDL,
-- implicit commit). Not yet run against the database — review first.
--
-- !! BEFORE RUNNING: confirm the column list of
--    GL_SUB_ACCOUNT_COURSE_LABEL. The INSERT in Section 1 assumes
--    (ID, DESCRIPTION, DESCRIPTION_ARABIC, SORT_ORDER).
-- =====================================================================

START TRANSACTION;

-- 1. New reference label: Cheques Under Collection
--    (sort order 15 — between Cash at Bank (10) and Cash in Hand (20))
INSERT INTO GL_SUB_ACCOUNT_COURSE_LABEL
    (GL_SUB_ACCOUNT_COURSE_LABEL_ID, DESCRIPTION, DESCRIPTION_ARABIC, SORT_ORDER)
VALUES
    ('Cheques Under Collection', 'Cheques Under Collection', 'شيكات تحت التحصيل', 15);

-- 2. Fix A: reclassify 124410 (شيكات تحت التحصيل) off "Cash at Bank"
--    Its parent is 100010 (current assets), not 110000 (banks) — hierarchy
--    already disagreed with the sub-account label.
UPDATE GL_ACCOUNT
SET GL_SUB_ACCOUNT_COURSE_LABEL_ID = 'Cheques Under Collection'
WHERE GL_ACCOUNT_ID = '124410';
-- expect: 1 row affected

-- 3. Fix B1: backfill 3 banks that were silently dropped from every
--    SUBACCOUNT_AR = 'نقدية في البنوك' filter (NULL sub-account label)
--      113300 البنك الاهلي المصري NBE
--      114300 QNB - USD
--      115300 SAIB
UPDATE GL_ACCOUNT
SET GL_SUB_ACCOUNT_COURSE_LABEL_ID = 'Cash at Bank'
WHERE GL_ACCOUNT_ID IN ('113300', '114300', '115300')
  AND GL_SUB_ACCOUNT_COURSE_LABEL_ID IS NULL;
-- expect: 3 rows affected

-- 4. Fix B2: backfill 2 missing cash-on-hand accounts
--      111701 النقدية بالصندوق USD
--      111702 النقدية بالصندوق SAR
UPDATE GL_ACCOUNT
SET GL_SUB_ACCOUNT_COURSE_LABEL_ID = 'Cash in Hand'
WHERE GL_ACCOUNT_ID IN ('111701', '111702')
  AND GL_SUB_ACCOUNT_COURSE_LABEL_ID IS NULL;
-- expect: 2 rows affected

COMMIT;
-- (ROLLBACK instead if any row count above was unexpected)


-- =====================================================================
-- 5. Fix C: add leaf/posting flags to Dim_gl_account (DDL, separate —
--    implicit commit, run only after the above is committed)
--
-- WHY NOT JUST RE-ADD THE LEAF FILTER:
-- The documented view (PowerBI-Projects-MySQL-View-Definitions.md) has a
-- "leaf accounts only" NOT EXISTS clause. That clause is NOT active in
-- the deployed view (see dim_gl_account_v2.sql, the current production
-- definition) — 45 non-leaf accounts are currently in Dim_gl_account,
-- and 11 of them carry their own postings (e.g. 111010 النقدية, 1098
-- rows). Re-adding the filter would delete those 11 accounts' balances
-- from every report on the model.
--
-- So: expose the distinction as flags instead of filtering. Nothing is
-- removed; every existing visual keeps working; new visuals can opt in
-- with IS_LEAF = 1.
--
-- Changes vs. dim_gl_account_v2.sql:
--   + HAS_CHILDREN     count of direct children
--   + IS_LEAF          1 = real posting account, 0 = rollup header
--   + SUBACCOUNT_KEY   unique grouping key (see NOTES)
-- =====================================================================

CREATE OR REPLACE VIEW `Dim_gl_account` AS
SELECT
    ao.GL_ACCOUNT_ID                                      AS GL_ACCOUNT_ID,
    a.ACCOUNT_NAME_ARABIC                                 AS ACCOUNT_NAME_ARABIC,
    a.PARENT_GL_ACCOUNT_ID                                AS PARENT_GL_ACCOUNT_ID,
    pa.ACCOUNT_NAME_ARABIC                                AS PARENT_ACCOUNT_NAME_ARABIC,
    acl.SIGN_MULTIPLIER                                   AS SIGN_MULTIPLIER,

    -- Financial statement hierarchy (5 levels)
    a.GL_REPORT_ID                                        AS REPORT,
    gr.DESCRIPTION_ARABIC                                 AS REPORT_AR,
    a.GL_CLASS_COURSE_ID                                  AS CLASS,
    gcc.DESCRIPTION_ARABIC                                AS CLASS_AR,
    a.GL_SUB_CLASS_ID                                     AS SUBCLASS,
    gsc.DESCRIPTION_ARABIC                                AS SUBCLASS_AR,
    a.GL_SUB_CLASS_2_ID                                   AS SUBCLASS2,
    gsc2.DESCRIPTION_ARABIC                               AS SUBCLASS2_AR,
    a.GL_ACCOUNT_COURSE_LABEL_ID                          AS ACCOUNT,
    acl.DESCRIPTION_ARABIC                                AS ACCOUNT_AR,
    a.GL_SUB_ACCOUNT_COURSE_LABEL_ID                      AS SUBACCOUNT,
    gsa.DESCRIPTION_ARABIC                                AS SUBACCOUNT_AR,

    -- NEW: unique grouping key. The same sub-account label is reused
    -- under different ACCOUNT parents (e.g. 'Other Receivables' appears
    -- under both INVENTORY and OTHER_CURRENT_ASSETS), so grouping on
    -- SUBACCOUNT_AR alone silently merges unrelated accounts.
    CONCAT(
        COALESCE(a.GL_ACCOUNT_COURSE_LABEL_ID, ''), '|',
        COALESCE(a.GL_SUB_ACCOUNT_COURSE_LABEL_ID, '')
    )                                                     AS SUBACCOUNT_KEY,

    -- Sort orders for each level
    gr.SORT_ORDER                                         AS REPORT_SORT,
    gcc.SORT_ORDER                                        AS CLASS_SORT,
    gsc.SORT_ORDER                                        AS SUBCLASS_SORT,
    gsc2.SORT_ORDER                                       AS SUBCLASS2_SORT,
    acl.SORT_ORDER                                        AS ACCOUNT_SORT,
    gsa.SORT_ORDER                                        AS SUBACCOUNT_SORT,

    -- DAX hint: cumulative (TTD) vs period (FTP) aggregation
    CASE a.GL_REPORT_ID
        WHEN 'BALANCE_SHEET'   THEN 'TTD'
        WHEN 'PROFIT_AND_LOSS' THEN 'FTP'
        ELSE NULL
    END                                                   AS MEASURE_TYPE,

    -- Helper flags
    CASE WHEN a.GL_SUB_CLASS_2_ID IN ('CURRENT_ASSETS', 'CURRENT_LIABILITIES')
         THEN 1 ELSE 0
    END                                                   AS IS_CURRENT,
    CASE WHEN a.GL_CLASS_COURSE_ID IN ('TRADING_ACCOUNT', 'OPERATING_ACCOUNT')
         THEN 1 ELSE 0
    END                                                   AS IS_OPERATING,

    -- NEW: hierarchy position
    COALESCE(kids.CHILD_COUNT, 0)                         AS HAS_CHILDREN,
    CASE WHEN COALESCE(kids.CHILD_COUNT, 0) = 0
         THEN 1 ELSE 0
    END                                                   AS IS_LEAF,

    a.LAST_UPDATED_STAMP                                  AS LAST_UPDATED_STAMP

FROM GL_ACCOUNT_ORGANIZATION ao
JOIN GL_ACCOUNT a
    ON  a.GL_ACCOUNT_ID = ao.GL_ACCOUNT_ID
    AND ao.FROM_DATE <= NOW()
    AND (ao.THRU_DATE IS NULL OR ao.THRU_DATE > NOW())

LEFT JOIN GL_ACCOUNT                  pa   ON pa.GL_ACCOUNT_ID   = a.PARENT_GL_ACCOUNT_ID
LEFT JOIN GL_REPORT                   gr   ON gr.GL_REPORT_ID    = a.GL_REPORT_ID
LEFT JOIN GL_CLASS_COURSE             gcc  ON gcc.GL_CLASS_COURSE_ID = a.GL_CLASS_COURSE_ID
LEFT JOIN GL_SUB_CLASS                gsc  ON gsc.GL_SUB_CLASS_ID    = a.GL_SUB_CLASS_ID
LEFT JOIN GL_SUB_CLASS_2              gsc2 ON gsc2.GL_SUB_CLASS_2_ID = a.GL_SUB_CLASS_2_ID
LEFT JOIN GL_ACCOUNT_COURSE_LABEL     acl  ON acl.GL_ACCOUNT_COURSE_LABEL_ID = a.GL_ACCOUNT_COURSE_LABEL_ID
LEFT JOIN GL_SUB_ACCOUNT_COURSE_LABEL gsa  ON gsa.GL_SUB_ACCOUNT_COURSE_LABEL_ID = a.GL_SUB_ACCOUNT_COURSE_LABEL_ID

-- NEW: direct-child count per account
LEFT JOIN (
    SELECT PARENT_GL_ACCOUNT_ID, COUNT(*) AS CHILD_COUNT
    FROM GL_ACCOUNT
    WHERE PARENT_GL_ACCOUNT_ID IS NOT NULL
    GROUP BY PARENT_GL_ACCOUNT_ID
) kids ON kids.PARENT_GL_ACCOUNT_ID = a.GL_ACCOUNT_ID

WHERE
    a.GL_REPORT_ID                   IS NOT NULL
    AND a.GL_CLASS_COURSE_ID         IS NOT NULL
    AND a.GL_SUB_CLASS_ID            IS NOT NULL
    AND a.GL_SUB_CLASS_2_ID          IS NOT NULL
    AND a.GL_ACCOUNT_COURSE_LABEL_ID IS NOT NULL
    AND a.GL_ACCOUNT_CLASS_ID NOT IN ('DEBIT', 'CREDIT', 'RESOURCE', 'NON_POSTING');


-- =====================================================================
-- POST-CHANGE VERIFICATION
-- =====================================================================

-- The bank list the Power BI slicer should now show (expect 7 rows,
-- no 110000, no 124410)
SELECT GL_ACCOUNT_ID, ACCOUNT_NAME_ARABIC, IS_LEAF, HAS_CHILDREN
FROM Dim_gl_account
WHERE SUBACCOUNT = 'Cash at Bank'
  AND IS_LEAF = 1
ORDER BY GL_ACCOUNT_ID;
-- expect: 110100, 110200, 110300, 112300, 113300, 114300, 115300

-- 110000 still present but now flagged as a rollup
SELECT GL_ACCOUNT_ID, ACCOUNT_NAME_ARABIC, IS_LEAF, HAS_CHILDREN
FROM Dim_gl_account WHERE GL_ACCOUNT_ID = '110000';
-- expect: IS_LEAF = 0, HAS_CHILDREN = 7

-- 124410 now on its own label
SELECT GL_ACCOUNT_ID, ACCOUNT_NAME_ARABIC, SUBACCOUNT, SUBACCOUNT_AR, SUBACCOUNT_SORT
FROM Dim_gl_account WHERE GL_ACCOUNT_ID = '124410';
-- expect: 'Cheques Under Collection' / شيكات تحت التحصيل / 15

-- Row count unchanged (expect 829 — nothing added or dropped)
SELECT COUNT(*) AS TOTAL_ROWS FROM Dim_gl_account;

-- Remaining accounts with no sub-account label (was 42, now ~37)
SELECT ACCOUNT, COUNT(*) AS STILL_MISSING
FROM Dim_gl_account
WHERE SUBACCOUNT IS NULL
GROUP BY ACCOUNT ORDER BY STILL_MISSING DESC;


-- =====================================================================
-- NOTES / NOT FIXED HERE
-- =====================================================================
--
-- 1. 37 accounts still have no sub-account label after this script
--    (biggest groups: OTHER_PAYABLES 9, INVENTORY 8, RECEIVABLES 4,
--    OTHER_EXPENSES 3). Each needs a business call on which label
--    applies — only the 5 where the correct label was unambiguous from
--    the parent account were backfilled here.
--
-- 2. SORT_ORDER collision at 200: 'Trade Payables' (ذمم الدائنون
--    التجاريون) and 'Travel' (السفر) share it. Travel sits among the
--    expense labels (390-460), so 200 looks like a typo for ~470.
--    Not changed — it would reorder an existing report.
--
-- 3. 'Other Receivables' (ذمم مدينة أخرى) is attached to 5 accounts
--    under ACCOUNT label INVENTORY. Receivables classified under
--    inventory is worth a look — likely a mis-tag, but it affects the
--    balance sheet so it was left alone.
--
-- 4. 124410 currently sits under ACCOUNT label CASH_AND_CASH_EQUIVALENTS,
--    so cheques not yet collected are reported inside "cash & equivalents"
--    on the balance sheet. Under IFRS an uncollected cheque is normally a
--    receivable / cash-in-transit, not cash. There is already a
--    precedent for in-transit items under RECEIVABLES: 'Credit Card
--    Transit' / مدفوعات بطاقات ائتمان في الطريق (sort 80). Moving 124410
--    there would change reported cash — needs accountant sign-off, not
--    applied here.
--
-- 5. The doc PowerBI-Projects-MySQL-View-Definitions.md describes a
--    leaf-only filter that production does not have. After applying
--    Section 5, update that doc — otherwise the next person re-adds the
--    filter and silently drops 11 posting accounts.
--
-- 6. After running: refresh the Power BI model, then change the Banks
--    slicer filter to  SUBACCOUNT = 'Cash at Bank' AND IS_LEAF = 1.
-- =====================================================================
