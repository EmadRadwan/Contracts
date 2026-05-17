WITH RECURSIVE GlTree AS (
    -- Start with the requested account
    SELECT
        ga.GL_ACCOUNT_ID,
        ga.PARENT_GL_ACCOUNT_ID,
        ga.ACCOUNT_NAME_ARABIC,
        0 AS LEVEL,
        CAST(ga.ACCOUNT_NAME_ARABIC AS CHAR(2000)) AS TREE_PATH
    FROM GL_ACCOUNT ga
    WHERE ga.GL_ACCOUNT_ID = '124600'   -- << your target account

    UNION ALL

    -- Traverse upward to parents
    SELECT
        parent.GL_ACCOUNT_ID,
        parent.PARENT_GL_ACCOUNT_ID,
        parent.ACCOUNT_NAME_ARABIC,
        gt.LEVEL + 1,
        CONCAT(parent.ACCOUNT_NAME_ARABIC, ' > ', gt.TREE_PATH) AS TREE_PATH
    FROM GL_ACCOUNT parent
             INNER JOIN GlTree gt
                        ON gt.PARENT_GL_ACCOUNT_ID = parent.GL_ACCOUNT_ID
)

SELECT
    LEVEL,
    GL_ACCOUNT_ID,
    PARENT_GL_ACCOUNT_ID,
    ACCOUNT_NAME_ARABIC,
    REPEAT('    ', LEVEL) AS INDENTATION,
    CONCAT(REPEAT('    ', LEVEL), ACCOUNT_NAME_ARABIC) AS TREE_VIEW,
    TREE_PATH
FROM GlTree
ORDER BY LEVEL DESC;