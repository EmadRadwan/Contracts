-- Data-only change, 2026-07-19 (console_9.sql, no impact on this view's
-- SQL): added GL_SUB_ACCOUNT_COURSE_LABEL 'Partner Investment Participations'
-- (جارى شركاء - مشاركات استثمارية, SORT_ORDER 275) and reassigned 14
-- GL_ACCOUNT rows to it — 250280-283, 250310-390, 250440. Note: parent
-- account 250270 (parent of 250280-283) was intentionally left on its
-- old label 'Project Partnerships' (مشاركات المشاريع), so it no longer
-- matches its own children's SUBACCOUNT — harmless for reporting
-- since visuals should filter on IS_LEAF = 1, but worth knowing if
-- browsing the hierarchy by SUBACCOUNT_AR looks inconsistent there.

-- v2.2, 2026-09-07 — added SUBACCOUNT_DISPLAY_AR and SUBACCOUNT_DISPLAY_SORT.
-- 39 GL accounts carry no GL_SUB_ACCOUNT_COURSE_LABEL_ID, so SUBACCOUNT_AR is
-- NULL for them and any matrix grouping on it renders an unlabelled row. On the
-- الحركة النقدية الشاملة page that row was 743,860, made up of 600030
-- لادريس اكتوبر, 600031 سوا, 970000 المساهمة التكافلية and 860040 اتصالات.
-- All 39 are leaf accounts and every one has ACCOUNT_AR populated, so the new
-- column falls back cleanly.
--
-- SUBACCOUNT_AR itself is deliberately UNCHANGED. A NULL there is a true fact —
-- the account has not been given a sub-account label — and anything auditing
-- that gap must keep seeing it. Use SUBACCOUNT_AR to find unlabelled accounts,
-- SUBACCOUNT_DISPLAY_AR to put a row on a page.
--
-- This is a display fallback, not a substitute for labelling the accounts.
-- Section 2 at the foot of this file lists the ones still needing a real
-- GL_SUB_ACCOUNT_COURSE_LABEL_ID.

CREATE OR REPLACE VIEW Dim_gl_account AS
SELECT
    -- ── Grain ────────────────────────────────────────────────────────────────
    ao.GL_ACCOUNT_ID,

    -- ── Account identifiers ──────────────────────────────────────────────────
    a.ACCOUNT_NAME_ARABIC,
    a.PARENT_GL_ACCOUNT_ID,
    pa.ACCOUNT_NAME_ARABIC                          AS PARENT_ACCOUNT_NAME_ARABIC,

    -- ── Normal balance & sign ────────────────────────────────────────────────
    acl.SIGN_MULTIPLIER,

    -- ── Classification hierarchy — Arabic set, broad → narrow ────────────────
    gr.DESCRIPTION_ARABIC                           AS REPORT_AR,
    gcc.DESCRIPTION_ARABIC                          AS CLASS_AR,
    gsc.DESCRIPTION_ARABIC                          AS SUBCLASS_AR,
    gsc2.DESCRIPTION_ARABIC                         AS SUBCLASS2_AR,
    acl.DESCRIPTION_ARABIC                          AS ACCOUNT_AR,
    gsa.DESCRIPTION_ARABIC                          AS SUBACCOUNT_AR,

    -- v2.2 — never NULL. Falls back sub-account -> account -> account name, so
    -- a matrix row always carries a label. See the header note before using
    -- this in place of SUBACCOUNT_AR for anything other than display.
    COALESCE(
        NULLIF(TRIM(gsa.DESCRIPTION_ARABIC), ''),
        NULLIF(TRIM(acl.DESCRIPTION_ARABIC), ''),
        NULLIF(TRIM(a.ACCOUNT_NAME_ARABIC), ''),
        a.GL_ACCOUNT_ID
    )                                                AS SUBACCOUNT_DISPLAY_AR,

    -- ── Classification hierarchy — English/ID set, broad → narrow ────────────
    a.GL_REPORT_ID                                  AS REPORT,
    a.GL_CLASS_COURSE_ID                            AS CLASS,
    a.GL_SUB_CLASS_ID                               AS SUBCLASS,
    a.GL_SUB_CLASS_2_ID                             AS SUBCLASS2,
    a.GL_ACCOUNT_COURSE_LABEL_ID                    AS ACCOUNT,
    a.GL_SUB_ACCOUNT_COURSE_LABEL_ID                AS SUBACCOUNT,

    -- ── Unique grouping key (v2.1) ────────────────────────────────────────────
    -- The same SUBACCOUNT label is reused under different ACCOUNT parents
    -- (e.g. 'Other Receivables' appears under both INVENTORY and
    -- OTHER_CURRENT_ASSETS), so grouping on SUBACCOUNT_AR alone silently
    -- merges unrelated accounts. Use this key instead when a stable,
    -- unambiguous grouping is required.
    CONCAT(
        COALESCE(a.GL_ACCOUNT_COURSE_LABEL_ID, ''), '|',
        COALESCE(a.GL_SUB_ACCOUNT_COURSE_LABEL_ID, '')
    )                                                AS SUBACCOUNT_KEY,

    -- ── Sort keys (drive correct P&L / BS ordering in Power BI matrix) ───────
    gr.SORT_ORDER                                   AS REPORT_SORT,
    gcc.SORT_ORDER                                  AS CLASS_SORT,
    gsc.SORT_ORDER                                  AS SUBCLASS_SORT,
    gsc2.SORT_ORDER                                 AS SUBCLASS2_SORT,
    acl.SORT_ORDER                                  AS ACCOUNT_SORT,
    gsa.SORT_ORDER                                  AS SUBACCOUNT_SORT,

    -- v2.2 — sort key for SUBACCOUNT_DISPLAY_AR. Without it every fallback row
    -- sorts as NULL and they clump together at one end of the matrix.
    COALESCE(gsa.SORT_ORDER, acl.SORT_ORDER)        AS SUBACCOUNT_DISPLAY_SORT,

    -- ── Derived helper flags (useful for DAX measure branching) ──────────────
    -- Tells Power BI which base measure to use without filtering in DAX
    CASE a.GL_REPORT_ID
        WHEN 'BALANCE_SHEET'   THEN 'TTD'   -- Total To Date (cumulative)
        WHEN 'PROFIT_AND_LOSS' THEN 'FTP'   -- For The Period
        ELSE NULL
    END                                             AS MEASURE_TYPE,

    -- True for current assets/liabilities — drives Current Ratio, Quick Ratio
    CASE
        WHEN a.GL_SUB_CLASS_2_ID IN ('CURRENT_ASSETS', 'CURRENT_LIABILITIES')
        THEN 1 ELSE 0
    END                                             AS IS_CURRENT,

    -- Separates operating from non-operating for EBIT calculation
    CASE
        WHEN a.GL_CLASS_COURSE_ID IN ('TRADING_ACCOUNT', 'OPERATING_ACCOUNT')
        THEN 1 ELSE 0
    END                                             AS IS_OPERATING,

    -- ── Hierarchy position (v2.1) ─────────────────────────────────────────────
    -- The view intentionally includes both leaf (posting) accounts and
    -- parent rollup accounts — some parents (e.g. 110000, 111010) carry
    -- their own postings, so a leaf-only filter would drop real balances.
    -- These flags let Power BI opt into leaf-only filtering per visual
    -- instead (e.g. Banks slicer: SUBACCOUNT = 'Cash at Bank' AND IS_LEAF = 1).
    COALESCE(kids.CHILD_COUNT, 0)                   AS HAS_CHILDREN,
    CASE WHEN COALESCE(kids.CHILD_COUNT, 0) = 0
         THEN 1 ELSE 0
    END                                             AS IS_LEAF,

    -- Timestamp for incremental refresh in Power BI / Power Query
    a.LAST_UPDATED_STAMP

FROM GL_ACCOUNT_ORGANIZATION ao

         INNER JOIN GL_ACCOUNT a
                    ON  a.GL_ACCOUNT_ID = ao.GL_ACCOUNT_ID
                        AND ao.FROM_DATE   <= NOW()
                        AND (ao.THRU_DATE IS NULL OR ao.THRU_DATE > NOW())

         LEFT JOIN GL_ACCOUNT pa
                   ON pa.GL_ACCOUNT_ID = a.PARENT_GL_ACCOUNT_ID

         LEFT JOIN GL_REPORT gr
                   ON gr.GL_REPORT_ID = a.GL_REPORT_ID
         LEFT JOIN GL_CLASS_COURSE gcc
                   ON gcc.GL_CLASS_COURSE_ID = a.GL_CLASS_COURSE_ID
         LEFT JOIN GL_SUB_CLASS gsc
                   ON gsc.GL_SUB_CLASS_ID = a.GL_SUB_CLASS_ID
         LEFT JOIN GL_SUB_CLASS_2 gsc2
                   ON gsc2.GL_SUB_CLASS_2_ID = a.GL_SUB_CLASS_2_ID
         LEFT JOIN GL_ACCOUNT_COURSE_LABEL acl
                   ON acl.GL_ACCOUNT_COURSE_LABEL_ID = a.GL_ACCOUNT_COURSE_LABEL_ID

         -- SubAccount lookup
         LEFT JOIN GL_SUB_ACCOUNT_COURSE_LABEL gsa
                   ON gsa.GL_SUB_ACCOUNT_COURSE_LABEL_ID = a.GL_SUB_ACCOUNT_COURSE_LABEL_ID

         -- NEW (v2.1): direct-child count per account, for HAS_CHILDREN/IS_LEAF
         LEFT JOIN (
             SELECT PARENT_GL_ACCOUNT_ID, COUNT(*) AS CHILD_COUNT
             FROM GL_ACCOUNT
             WHERE PARENT_GL_ACCOUNT_ID IS NOT NULL
             GROUP BY PARENT_GL_ACCOUNT_ID
         ) kids ON kids.PARENT_GL_ACCOUNT_ID = a.GL_ACCOUNT_ID

WHERE
    a.GL_REPORT_ID                  IS NOT NULL
  AND a.GL_CLASS_COURSE_ID          IS NOT NULL
  AND a.GL_SUB_CLASS_ID             IS NOT NULL
  AND a.GL_SUB_CLASS_2_ID           IS NOT NULL
  AND a.GL_ACCOUNT_COURSE_LABEL_ID  IS NOT NULL

  AND a.GL_ACCOUNT_CLASS_ID NOT IN ('DEBIT', 'CREDIT', 'RESOURCE', 'NON_POSTING');


-- =============================================================================
-- 1 · VERIFICATION — run after replacing the view
-- =============================================================================
-- SUBACCOUNT_DISPLAY_AR must never be NULL or blank. Must return 0.
SELECT 'Display label never empty' AS check_name,
       COUNT(*)                    AS blank_rows,
       CASE WHEN COUNT(*) = 0 THEN 'PASS' ELSE 'FAIL' END AS verdict
FROM Dim_gl_account
WHERE SUBACCOUNT_DISPLAY_AR IS NULL OR TRIM(SUBACCOUNT_DISPLAY_AR) = '';

-- Where a real label exists the display column must equal it untouched, so no
-- existing grouping shifts. Must return 0.
SELECT 'Labelled rows unchanged' AS check_name,
       COUNT(*)                  AS mismatched,
       CASE WHEN COUNT(*) = 0 THEN 'PASS' ELSE 'FAIL' END AS verdict
FROM Dim_gl_account
WHERE SUBACCOUNT_AR IS NOT NULL AND TRIM(SUBACCOUNT_AR) <> ''
  AND SUBACCOUNT_DISPLAY_AR <> SUBACCOUNT_AR;

-- How many rows are relying on the fallback, and from which source.
-- Dev copy 2026-09-07: 39 rows, all falling back to ACCOUNT_AR.
SELECT CASE WHEN SUBACCOUNT_AR IS NOT NULL AND TRIM(SUBACCOUNT_AR) <> ''
              THEN 'real sub-account label'
            WHEN ACCOUNT_AR   IS NOT NULL AND TRIM(ACCOUNT_AR)   <> ''
              THEN 'fell back to ACCOUNT_AR'
            ELSE 'fell back to the account name' END AS label_source,
       COUNT(*) AS accounts
FROM Dim_gl_account
GROUP BY label_source
ORDER BY accounts DESC;


-- =============================================================================
-- 2 · THE REAL FIX — accounts still needing a sub-account label
-- =============================================================================
-- The view now guarantees a readable row, but these accounts are genuinely
-- unclassified at the sub-account level. Assigning a proper
-- GL_SUB_ACCOUNT_COURSE_LABEL_ID is an accounting decision, not a reporting
-- one, so nothing here is updated automatically.
--
-- "has_movement" marks the ones already carrying postings — fix those first,
-- because they are the rows appearing on pages today.
--
-- Precedent for the update itself: console_9.sql (2026-07-19) added a label and
-- reassigned 14 accounts to it. Follow that pattern, one UPDATE per label:
--     UPDATE GL_ACCOUNT SET GL_SUB_ACCOUNT_COURSE_LABEL_ID = '<label id>'
--      WHERE GL_ACCOUNT_ID IN (...);
--
-- Note 600030 and 600031 in particular: their siblings 600021-600028 each have
-- their own project operating label, so these two look like a gap left when the
-- لادريس اكتوبر and سوا projects were added.
SELECT d.GL_ACCOUNT_ID,
       d.ACCOUNT_NAME_ARABIC,
       d.CLASS_AR,
       d.SUBCLASS_AR,
       d.ACCOUNT_AR                       AS falls_back_to,
       d.PARENT_GL_ACCOUNT_ID,
       d.IS_LEAF,
       CASE WHEN mv.GL_ACCOUNT_ID IS NULL THEN '' ELSE 'has_movement' END AS has_movement
FROM Dim_gl_account d
LEFT JOIN (SELECT DISTINCT GL_ACCOUNT_ID FROM ACCTG_TRANS_ENTRY) mv
       ON mv.GL_ACCOUNT_ID = d.GL_ACCOUNT_ID
WHERE d.SUBACCOUNT_AR IS NULL OR TRIM(d.SUBACCOUNT_AR) = ''
ORDER BY has_movement DESC, d.CLASS_AR, d.GL_ACCOUNT_ID;
