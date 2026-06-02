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

    -- ── GL_REPORT (top of hierarchy) ─────────────────────────────────────────
    a.GL_REPORT_ID                                  AS REPORT,
    gr.DESCRIPTION_ARABIC                           AS REPORT_AR,

    -- ── GL_CLASS_COURSE ──────────────────────────────────────────────────────
    a.GL_CLASS_COURSE_ID                            AS CLASS,
    gcc.DESCRIPTION_ARABIC                          AS CLASS_AR,

    -- ── GL_SUB_CLASS ─────────────────────────────────────────────────────────
    a.GL_SUB_CLASS_ID                               AS SUBCLASS,
    gsc.DESCRIPTION_ARABIC                          AS SUBCLASS_AR,

    -- ── GL_SUB_CLASS_2 ───────────────────────────────────────────────────────
    a.GL_SUB_CLASS_2_ID                             AS SUBCLASS2,
    gsc2.DESCRIPTION_ARABIC                         AS SUBCLASS2_AR,

    -- ── GL_ACCOUNT_COURSE_LABEL (leaf of hierarchy) ──────────────────────────
    a.GL_ACCOUNT_COURSE_LABEL_ID                    AS ACCOUNT,
    acl.DESCRIPTION_ARABIC                          AS ACCOUNT_AR,

    -- ── GL_SUB_ACCOUNT_COURSE_LABEL (new — ratio & drill-down level) ─────────
    a.GL_SUB_ACCOUNT_COURSE_LABEL_ID                AS SUBACCOUNT,
    gsa.DESCRIPTION_ARABIC                          AS SUBACCOUNT_AR,

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

         -- NEW: SubAccount lookup
         LEFT JOIN GL_SUB_ACCOUNT_COURSE_LABEL gsa
                   ON gsa.GL_SUB_ACCOUNT_COURSE_LABEL_ID = a.GL_SUB_ACCOUNT_COURSE_LABEL_ID

WHERE
    a.GL_REPORT_ID                  IS NOT NULL
  AND a.GL_CLASS_COURSE_ID          IS NOT NULL
  AND a.GL_SUB_CLASS_ID             IS NOT NULL
  AND a.GL_SUB_CLASS_2_ID           IS NOT NULL
  AND a.GL_ACCOUNT_COURSE_LABEL_ID  IS NOT NULL

  AND a.GL_ACCOUNT_CLASS_ID NOT IN ('DEBIT', 'CREDIT', 'RESOURCE', 'NON_POSTING')

  AND NOT EXISTS (
    SELECT 1
    FROM GL_ACCOUNT child
    WHERE child.PARENT_GL_ACCOUNT_ID = a.GL_ACCOUNT_ID
);
