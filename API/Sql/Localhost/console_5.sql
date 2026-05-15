CREATE OR REPLACE VIEW view_gl_account_audit_org AS
SELECT
    -- Organization Mapping
    gao.ORGANIZATION_PARTY_ID,

    -- Primary Identifiers
    g.GL_ACCOUNT_ID,
    g.ACCOUNT_CODE,
    g.ACCOUNT_NAME,
    g.ACCOUNT_NAME_ARABIC,

    -- The "Internal Hierarchy" (Power BI Columns)
    g.GL_REPORT_ID,
    g.GL_CLASS_COURSE_ID,
    g.GL_SUB_CLASS_ID,
    g.GL_SUB_CLASS_2_ID,
    g.GL_ACCOUNT_COURSE_LABEL_ID,

    -- Functional Types
    g.GL_ACCOUNT_TYPE_ID,
    g.GL_ACCOUNT_CLASS_ID,

    -- Parent Context
    g.PARENT_GL_ACCOUNT_ID,
    (SELECT p.ACCOUNT_NAME FROM gl_account p WHERE p.GL_ACCOUNT_ID = g.PARENT_GL_ACCOUNT_ID) AS PARENT_ACCOUNT_NAME,

    -- Metadata
    g.DESCRIPTION
FROM
    gl_account g
-- The INNER JOIN ensures we only see accounts assigned to an organization
        INNER JOIN
    gl_account_organization gao ON g.GL_ACCOUNT_ID = gao.GL_ACCOUNT_ID
ORDER BY
    gao.ORGANIZATION_PARTY_ID,
    g.GL_REPORT_ID,
    g.GL_CLASS_COURSE_ID,
    g.GL_SUB_CLASS_ID,
    -- Keeps sub-ledgers grouped with their parents
    COALESCE(g.PARENT_GL_ACCOUNT_ID, g.GL_ACCOUNT_ID),
    g.ACCOUNT_CODE;