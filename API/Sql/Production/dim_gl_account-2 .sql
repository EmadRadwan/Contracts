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
    acl.DESCRIPTION_ARABIC                          AS ACCOUNT_AR

FROM GL_ACCOUNT_ORGANIZATION ao

         INNER JOIN GL_ACCOUNT a
                    ON  a.GL_ACCOUNT_ID = ao.GL_ACCOUNT_ID
                        -- Active org assignment only
                        AND ao.FROM_DATE   <= NOW()
                        AND (ao.THRU_DATE IS NULL OR ao.THRU_DATE > NOW())

    -- Parent account name (left: root accounts have no parent)
         LEFT JOIN GL_ACCOUNT pa
                   ON pa.GL_ACCOUNT_ID = a.PARENT_GL_ACCOUNT_ID

    -- Classification hierarchy
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

WHERE
  -- ── Exclude header/summary accounts ──────────────────────────────────────
  -- A posting account must have all 5 classifiers resolved.
  -- Headers are either partially filled (inherited report/class only)
  -- or deliberately left null by the update scripts.
    a.GL_REPORT_ID               IS NOT NULL
  AND a.GL_CLASS_COURSE_ID     IS NOT NULL
  AND a.GL_SUB_CLASS_ID        IS NOT NULL
  AND a.GL_SUB_CLASS_2_ID      IS NOT NULL
  AND a.GL_ACCOUNT_COURSE_LABEL_ID IS NOT NULL

  -- ── Exclude structural OFBiz placeholders ─────────────────────────────────
  -- DEBIT/CREDIT/RESOURCE/NON_POSTING are meta-classes with no P&L or BS meaning
  AND a.GL_ACCOUNT_CLASS_ID NOT IN ('DEBIT', 'CREDIT', 'RESOURCE', 'NON_POSTING')

  -- ── Exclude accounts that have posting children (true header accounts) ─────
  -- If an account is a parent of another org-linked account it is a subtotal
  -- node, not a leaf — even if it somehow got all 5 classifiers filled.
  AND NOT EXISTS (
    SELECT 1
    FROM GL_ACCOUNT child
    WHERE child.PARENT_GL_ACCOUNT_ID = a.GL_ACCOUNT_ID
);


-- ── Audit companion: what got excluded and why ───────────────────────────────
-- Run this to review headers and decide if any need manual fixing:
--
 /*SELECT
     a.GL_ACCOUNT_ID,
     a.ACCOUNT_NAME_ARABIC,
     a.GL_ACCOUNT_CLASS_ID,
     a.GL_ACCOUNT_TYPE_ID,
     a.GL_REPORT_ID,
     a.GL_CLASS_COURSE_ID,
     a.GL_SUB_CLASS_ID,
     a.GL_SUB_CLASS_2_ID,
     a.GL_ACCOUNT_COURSE_LABEL_ID,
     CASE
         WHEN a.GL_ACCOUNT_CLASS_ID IN ('DEBIT','CREDIT','RESOURCE','NON_POSTING')
             THEN 'Structural OFBiz placeholder'
         WHEN EXISTS (SELECT 1 FROM GL_ACCOUNT c WHERE c.PARENT_GL_ACCOUNT_ID = a.GL_ACCOUNT_ID)
             THEN 'Has children — header/subtotal account'
         WHEN a.GL_REPORT_ID IS NULL OR a.GL_SUB_CLASS_ID IS NULL
             OR a.GL_SUB_CLASS_2_ID IS NULL OR a.GL_ACCOUNT_COURSE_LABEL_ID IS NULL
             THEN 'Incomplete classifiers — needs manual review'
         ELSE 'Unknown'
     END AS exclusion_reason
 FROM GL_ACCOUNT_ORGANIZATION ao
 INNER JOIN GL_ACCOUNT a ON a.GL_ACCOUNT_ID = ao.GL_ACCOUNT_ID
     AND ao.FROM_DATE <= NOW()
     AND (ao.THRU_DATE IS NULL OR ao.THRU_DATE > NOW())
 WHERE
     a.GL_ACCOUNT_CLASS_ID IN ('DEBIT','CREDIT','RESOURCE','NON_POSTING')
     OR EXISTS (SELECT 1 FROM GL_ACCOUNT c WHERE c.PARENT_GL_ACCOUNT_ID = a.GL_ACCOUNT_ID)
     OR a.GL_REPORT_ID IS NULL OR a.GL_SUB_CLASS_ID IS NULL
     OR a.GL_SUB_CLASS_2_ID IS NULL OR a.GL_ACCOUNT_COURSE_LABEL_ID IS NULL
 ORDER BY a.GL_ACCOUNT_ID;


SELECT DISTINCT
    ate.GL_ACCOUNT_ID,
    ate.ORGANIZATION_PARTY_ID,
    a.ACCOUNT_NAME_ARABIC,
    a.ACCOUNT_CODE,
    a.GL_ACCOUNT_CLASS_ID,
    a.GL_ACCOUNT_TYPE_ID,
    a.GL_REPORT_ID,
    a.GL_CLASS_COURSE_ID,
    a.GL_SUB_CLASS_ID,
    a.GL_SUB_CLASS_2_ID,
    a.GL_ACCOUNT_COURSE_LABEL_ID,
    CASE
        WHEN a.GL_ACCOUNT_ID IS NULL
            THEN 'Account missing from GL_ACCOUNT entirely'
        WHEN a.GL_ACCOUNT_CLASS_ID IN ('DEBIT','CREDIT','RESOURCE','NON_POSTING')
            THEN 'Structural OFBiz placeholder class'
        WHEN EXISTS (SELECT 1 FROM GL_ACCOUNT c WHERE c.PARENT_GL_ACCOUNT_ID = a.GL_ACCOUNT_ID)
            THEN 'Has children — header/subtotal account'
        WHEN a.GL_REPORT_ID               IS NULL
            OR a.GL_CLASS_COURSE_ID         IS NULL
            OR a.GL_SUB_CLASS_ID            IS NULL
            OR a.GL_SUB_CLASS_2_ID          IS NULL
            OR a.GL_ACCOUNT_COURSE_LABEL_ID IS NULL
            THEN 'Incomplete classifiers — needs manual fix'
        WHEN NOT EXISTS (
            SELECT 1 FROM GL_ACCOUNT_ORGANIZATION ao
            WHERE ao.GL_ACCOUNT_ID         = ate.GL_ACCOUNT_ID
              AND ao.ORGANIZATION_PARTY_ID = ate.ORGANIZATION_PARTY_ID
              AND ao.FROM_DATE            <= NOW()
              AND (ao.THRU_DATE IS NULL OR ao.THRU_DATE > NOW())
        )
            THEN 'Not in GL_ACCOUNT_ORGANIZATION (or expired)'
        ELSE 'Excluded by view — unknown reason'
        END AS gap_reason

FROM ACCTG_TRANS_ENTRY ate

         LEFT JOIN GL_ACCOUNT a
                   ON a.GL_ACCOUNT_ID = ate.GL_ACCOUNT_ID

WHERE ate.ORGANIZATION_PARTY_ID = 'Company'   -- adjust if needed

  -- The account has actual transaction entries but is absent from the view
  AND NOT EXISTS (
    SELECT 1
    FROM dim_gl_account2 d
    WHERE d.GL_ACCOUNT_ID         = ate.GL_ACCOUNT_ID
      AND d.ORGANIZATION_PARTY_ID = ate.ORGANIZATION_PARTY_ID
)

ORDER BY gap_reason, ate.GL_ACCOUNT_ID;*/