CREATE OR REPLACE VIEW DimProject AS
WITH RECURSIVE gl_hierarchy AS (
    -- Anchor: operating parent
    SELECT
        we.WORK_EFFORT_ID AS ProjectId,
        we.PROJECT_NAME AS ProjectName,
        ga.GL_ACCOUNT_ID,
        ga.PARENT_GL_ACCOUNT_ID,
        0 AS Level
    FROM WORK_EFFORT we
             JOIN GL_ACCOUNT ga
                  ON ga.GL_ACCOUNT_ID = we.OPERATING_EXPENSE_GL_ACCOUNT_ID
    WHERE we.WORK_EFFORT_TYPE_ID = 'PROJECT'
      AND we.OPERATING_EXPENSE_GL_ACCOUNT_ID IS NOT NULL

    UNION ALL

    -- Recursive: children
    SELECT
        gh.ProjectId,
        gh.ProjectName,
        ga.GL_ACCOUNT_ID,
        ga.PARENT_GL_ACCOUNT_ID,
        gh.Level + 1
    FROM GL_ACCOUNT ga
             JOIN gl_hierarchy gh
                  ON ga.PARENT_GL_ACCOUNT_ID = gh.GL_ACCOUNT_ID
)

SELECT DISTINCT
    ProjectId,
    ProjectName,
    GlAccountId,
    GlAccountType
FROM (

         -- 1. Main project GL
         SELECT
             WORK_EFFORT_ID AS ProjectId,
             PROJECT_NAME AS ProjectName,
             GlAccountId,
             'PROJECT_MAIN' AS GlAccountType
         FROM WORK_EFFORT
         WHERE WORK_EFFORT_TYPE_ID = 'PROJECT'
           AND GlAccountId IS NOT NULL

         UNION ALL

         -- 2. Operating parent
         SELECT
             WORK_EFFORT_ID AS ProjectId,
             PROJECT_NAME AS ProjectName,
             OPERATING_EXPENSE_GL_ACCOUNT_ID AS GlAccountId,
             'OPERATING_PARENT' AS GlAccountType
         FROM WORK_EFFORT
         WHERE WORK_EFFORT_TYPE_ID = 'PROJECT'
           AND OPERATING_EXPENSE_GL_ACCOUNT_ID IS NOT NULL

         UNION ALL

         -- 3. Operating children
         SELECT
             ProjectId,
             ProjectName,
             GL_ACCOUNT_ID AS GlAccountId,
             CASE
                 WHEN Level = 0 THEN 'OPERATING_PARENT'   -- safety (already included, but kept consistent)
                 ELSE 'OPERATING_CHILD'
                 END AS GlAccountType
         FROM gl_hierarchy

     ) t;