-- Data-only change, 2026-07-19 (console_9.sql, no impact on this view's
-- SQL): added GL_SUB_ACCOUNT_COURSE_LABEL 'Partner Investment Participations'
-- (جارى شركاء - مشاركات استثمارية, SORT_ORDER 275) and reassigned 14
-- GL_ACCOUNT rows to it — 250280-283, 250310-390, 250440. Note: parent
-- account 250270 (parent of 250280-283) was intentionally left on its
-- old label 'Project Partnerships' (مشاركات المشاريع), so it no longer
-- matches its own children's SUBACCOUNT — harmless for reporting
-- since visuals should filter on IS_LEAF = 1, but worth knowing if
-- browsing the hierarchy by SUBACCOUNT_AR looks inconsistent there.

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
