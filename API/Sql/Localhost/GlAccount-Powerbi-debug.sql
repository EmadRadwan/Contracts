WITH RECURSIVE gl_hierarchy AS (
    -- 1. Base Case: Find the specific starting account
    SELECT
        g.GL_ACCOUNT_ID,
        g.ACCOUNT_CODE,
        g.ACCOUNT_NAME,
        g.ACCOUNT_NAME_ARABIC,
        g.PARENT_GL_ACCOUNT_ID,
        g.GL_REPORT_ID,
        g.GL_CLASS_COURSE_ID,
        g.GL_SUB_CLASS_ID,
        g.GL_SUB_CLASS_2_ID,
        g.GL_ACCOUNT_COURSE_LABEL_ID,
        1 AS depth -- Tracks how deep the nesting goes
    FROM gl_account g
             INNER JOIN gl_account_organization gao ON g.GL_ACCOUNT_ID = gao.GL_ACCOUNT_ID
    WHERE g.GL_ACCOUNT_ID = '180000' -- Replace with your starting GL_ACCOUNT_ID

    UNION ALL

    -- 2. Recursive Step: Find children of the accounts already found
    SELECT
        c.GL_ACCOUNT_ID,
        c.ACCOUNT_CODE,
        c.ACCOUNT_NAME,
        c.ACCOUNT_NAME_ARABIC,
        c.PARENT_GL_ACCOUNT_ID,
        c.GL_REPORT_ID,
        c.GL_CLASS_COURSE_ID,
        c.GL_SUB_CLASS_ID,
        c.GL_SUB_CLASS_2_ID,
        c.GL_ACCOUNT_COURSE_LABEL_ID,
        h.depth + 1
    FROM gl_account c
             INNER JOIN gl_hierarchy h ON c.PARENT_GL_ACCOUNT_ID = h.GL_ACCOUNT_ID
             INNER JOIN gl_account_organization gao ON c.GL_ACCOUNT_ID = gao.GL_ACCOUNT_ID
)
SELECT * FROM gl_hierarchy
ORDER BY depth, ACCOUNT_CODE;