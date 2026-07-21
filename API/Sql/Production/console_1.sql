SELECT
    GL_ACCOUNT_ID,
    ACCOUNT_NAME_ARABIC,
    PARENT_GL_ACCOUNT_ID,
    REPORT_AR,
    CLASS_AR,
    SUBCLASS_AR,
    SUBCLASS2_AR,
    ACCOUNT_AR,
    SUBACCOUNT_AR,
    IS_LEAF,
    HAS_CHILDREN
FROM Dim_gl_account
WHERE GL_ACCOUNT_ID IN (
                        '250280','250281','250282','250283',
                        '250310','250320','250330','250340','250350','250360',
                        '250370','250380','250390',
                        '250440'
    )
ORDER BY GL_ACCOUNT_ID;