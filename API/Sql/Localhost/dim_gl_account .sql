CREATE OR REPLACE VIEW vw_dim_gl_account AS
SELECT
    -- ── Grain ────────────────────────────────────────────────────────────────
    ao.GL_ACCOUNT_ID,
    ao.ORGANIZATION_PARTY_ID,

    -- ── Account identifiers ───────────────────────────────────────────────────
    a.ACCOUNT_CODE,
    a.ACCOUNT_NAME_ARABIC,
    a.PARENT_GL_ACCOUNT_ID,
    pa.ACCOUNT_NAME_ARABIC                          AS PARENT_ACCOUNT_NAME_ARABIC,

    -- ── Normal balance & sign ─────────────────────────────────────────────────
    acl.SIGN_MULTIPLIER                AS SIGN_MULTIPLIER,

    -- ── GL_REPORT (top of hierarchy) ─────────────────────────────────────────
    a.GL_REPORT_ID,
    gr.DESCRIPTION_ARABIC                           AS GL_REPORT_DESCRIPTION_AR,

    -- ── GL_CLASS_COURSE ───────────────────────────────────────────────────────
    a.GL_CLASS_COURSE_ID,
    gcc.DESCRIPTION_ARABIC                          AS GL_CLASS_COURSE_DESCRIPTION_AR,

    -- ── GL_SUB_CLASS ──────────────────────────────────────────────────────────
    a.GL_SUB_CLASS_ID,
    gsc.DESCRIPTION_ARABIC                          AS GL_SUB_CLASS_DESCRIPTION_AR,

    -- ── GL_SUB_CLASS_2 ────────────────────────────────────────────────────────
    a.GL_SUB_CLASS_2_ID,
    gsc2.DESCRIPTION_ARABIC                         AS GL_SUB_CLASS_2_DESCRIPTION_AR,

    -- ── GL_ACCOUNT_COURSE_LABEL (leaf of hierarchy) ───────────────────────────
    a.GL_ACCOUNT_COURSE_LABEL_ID,
    acl.DESCRIPTION_ARABIC                          AS GL_ACCOUNT_COURSE_LABEL_DESCRIPTION_AR,

    -- ── Legacy OFBiz classification (kept for backwards compat) ───────────────
    a.GL_ACCOUNT_TYPE_ID,
    at_.DESCRIPTION                                 AS GL_ACCOUNT_TYPE_DESCRIPTION,
    a.GL_ACCOUNT_CLASS_ID,
    ac.DESCRIPTION                                  AS GL_ACCOUNT_CLASS_DESCRIPTION

FROM GL_ACCOUNT_ORGANIZATION ao

-- Inner join: only org-linked accounts
         INNER JOIN GL_ACCOUNT a
                    ON a.GL_ACCOUNT_ID = ao.GL_ACCOUNT_ID

-- Parent account name (left: root accounts have no parent)
         LEFT JOIN GL_ACCOUNT pa
                   ON pa.GL_ACCOUNT_ID = a.PARENT_GL_ACCOUNT_ID

-- Classification hierarchy (all left: new columns may not be filled yet)
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

-- Legacy OFBiz joins (left: may not be populated)
         LEFT JOIN GL_ACCOUNT_TYPE at_
                   ON at_.GL_ACCOUNT_TYPE_ID = a.GL_ACCOUNT_TYPE_ID

         LEFT JOIN GL_ACCOUNT_CLASS ac
                   ON ac.GL_ACCOUNT_CLASS_ID = a.GL_ACCOUNT_CLASS_ID;