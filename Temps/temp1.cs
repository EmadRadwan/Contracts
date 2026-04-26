CREATE OR REPLACE VIEW Fact_Project_DirectPayments AS
SELECT
pyt.PAYMENT_ID                                      AS PaymentId,
-- ... [Keep other columns same] ...

-- 🔴 ADJUSTED LOGIC: 
-- 1. Take the direct Project ID if it exists
-- 2. If null, find the project that owns this GL account
COALESCE(pyt.WORK_EFFORT_ID, gl_proj.WORK_EFFORT_ID) AS ProjectId,
    
    pyt.OVERRIDE_GL_ACCOUNT_ID                          AS OverrideGlAccountId,
    COALESCE(proj.PROJECT_NAME, gl_proj.PROJECT_NAME)   AS ProjectName,

-- ... [Keep other columns same] ...

FROM PAYMENT pyt
    JOIN PAYMENT_TYPE ptt          ON pyt.PAYMENT_TYPE_ID = ptt.PAYMENT_TYPE_ID
JOIN STATUS_ITEM sts           ON pyt.STATUS_ID = sts.STATUS_ID
JOIN PARTY pty_from            ON pyt.PARTY_ID_FROM = pty_from.PARTY_ID

    -- Join for direct Project link
    LEFT JOIN WORK_EFFORT proj     ON pyt.WORK_EFFORT_ID = proj.WORK_EFFORT_ID
         
    -- 🟢 NEW JOIN: Join via the GL Account to ensure "inheritance"
LEFT JOIN WORK_EFFORT gl_proj  ON pyt.OVERRIDE_GL_ACCOUNT_ID = gl_proj.GL_ACCOUNT_ID

    -- ... [Keep other joins same] ...

WHERE ptt.PARENT_TYPE_ID = 'DISBURSEMENT'
AND pyt.STATUS_ID = 'PMNT_SENT';