CREATE OR REPLACE VIEW Fact_Project_OperatingExpenses AS
WITH RECURSIVE OperatingGlAccounts AS (
    -- Anchor: Get the OperatingExpenseGlAccountId for each project
    SELECT 
        w.WORK_EFFORT_ID AS ProjectId,
        w.OPERATING_EXPENSE_GL_ACCOUNT_ID AS GlAccountId
    FROM WORK_EFFORT w
    WHERE w.OPERATING_EXPENSE_GL_ACCOUNT_ID IS NOT NULL

    UNION ALL

    -- Recursive: Add all child GL accounts
    SELECT 
        parent.ProjectId,
        g.GL_ACCOUNT_ID
    FROM GL_ACCOUNT g
    INNER JOIN OperatingGlAccounts parent 
        ON g.PARENT_GL_ACCOUNT_ID = parent.GlAccountId
)
SELECT
    pyt.PAYMENT_ID                                      AS PaymentId,
    pyt.PAYMENT_TYPE_ID,
    ptt.DESCRIPTION_ARABIC                              AS PaymentTypeDescription,
    pyt.PAYMENT_METHOD_ID                               AS PaymentMethodId,
    pyt.PAYMENT_METHOD_TYPE_ID,
    pmt.DESCRIPTION_ARABIC                              AS PaymentMethodTypeDescription,

    pyt.PARTY_ID_FROM                                   AS PartyIdFrom,
    COALESCE(pty_from.DESCRIPTION, '')                  AS PartyIdFromName,

    pyt.PARTY_ID_TO                                     AS PartyIdTo,
    COALESCE(pty_to.DESCRIPTION,
             CASE WHEN pyt.PARTY_ID_TO = 'Company' THEN 'Golden Land'
                  ELSE pyt.PARTY_ID_TO END)              AS PartyIdToName,

    pyt.STATUS_ID,
    COALESCE(sts.DESCRIPTION_ARABIC, pyt.STATUS_ID)     AS StatusDescription,
    COALESCE(sts.DESCRIPTION, pyt.STATUS_ID)            AS StatusDescriptionEnglish,

    pyt.EFFECTIVE_DATE,
    pyt.CREATED_STAMP                                   AS CreatedStamp,
    pyt.COMMENTS,
    pyt.PAYMENT_REF_NUM                                 AS PaymentRefNum,
    pyt.PAYMENT_PREFERENCE_ID                           AS PaymentPreferenceId,

    pyt.IS_BANK_TRANSFER                                AS IsBankTransfer,
    pyt.AMOUNT,
    COALESCE(pyt.ACTUAL_CURRENCY_AMOUNT, pyt.AMOUNT)    AS ActualCurrencyAmount,
    COALESCE(pyt.CURRENCY_UOM_ID, 'EGP')                AS CurrencyUomId,

    TRUE                                                AS IsDisbursement,
    TRUE                                                AS IsOperatingExpense,

    pyt.PARTY_ID_FROM                                   AS OrganizationPartyId,
    pyt.WORK_EFFORT_ID                                  AS ProjectId,
    pyt.OVERRIDE_GL_ACCOUNT_ID                          AS OverrideGlAccountId,

    pyt.COST_CENTER_ID                                  AS CostCenterId,
    cc.DESCRIPTION                                      AS CostCenterDescription,

    pyt.CHEQUENUMBER                                    AS ChequeNumber,
    pyt.CHEQUEDATE                                      AS ChequeDate



FROM PAYMENT pyt
JOIN PAYMENT_TYPE ptt 
    ON pyt.PAYMENT_TYPE_ID = ptt.PAYMENT_TYPE_ID

LEFT JOIN STATUS_ITEM sts 
    ON pyt.STATUS_ID = sts.STATUS_ID

LEFT JOIN PARTY pty_from 
    ON pyt.PARTY_ID_FROM = pty_from.PARTY_ID

LEFT JOIN PAYMENT_METHOD_TYPE pmt 
    ON pyt.PAYMENT_METHOD_TYPE_ID = pmt.PAYMENT_METHOD_TYPE_ID

LEFT JOIN PARTY pty_to 
    ON pyt.PARTY_ID_TO = pty_to.PARTY_ID

LEFT JOIN COST_CENTER cc 
    ON pyt.COST_CENTER_ID = cc.COST_CENTER_ID


-- Link to the recursive hierarchy (this makes it per-project)
INNER JOIN OperatingGlAccounts ogl 
    ON pyt.WORK_EFFORT_ID = ogl.ProjectId 
   AND pyt.OVERRIDE_GL_ACCOUNT_ID = ogl.GlAccountId

WHERE (ptt.PARENT_TYPE_ID = 'DISBURSEMENT' OR ptt.PAYMENT_TYPE_ID = 'DISBURSEMENT')
  AND pyt.OVERRIDE_GL_ACCOUNT_ID IS NOT NULL;

CREATE OR REPLACE VIEW Dim_Project_OperatingExpense_GL_Hierarchy AS
WITH RECURSIVE OperatingGlHierarchy AS (
    -- Anchor: Projects that have an Operating Expense GL Account
    SELECT
        w.WORK_EFFORT_ID AS ProjectId,
        w.OPERATING_EXPENSE_GL_ACCOUNT_ID AS GlAccountId,
        0 AS HierarchyLevel,
        CAST(w.OPERATING_EXPENSE_GL_ACCOUNT_ID AS CHAR(255)) AS GlAccountPath
    FROM WORK_EFFORT w
    WHERE w.OPERATING_EXPENSE_GL_ACCOUNT_ID IS NOT NULL

    UNION ALL

    -- Recursive: Add all child GL accounts
    SELECT
        h.ProjectId,
        g.GL_ACCOUNT_ID,
        h.HierarchyLevel + 1,
        CONCAT(h.GlAccountPath, ' > ', g.GL_ACCOUNT_ID) AS GlAccountPath
    FROM GL_ACCOUNT g
             INNER JOIN OperatingGlHierarchy h
                        ON g.PARENT_GL_ACCOUNT_ID = h.GlAccountId
)
SELECT
    ProjectId,
    GlAccountId,
    HierarchyLevel,
    GlAccountPath
FROM OperatingGlHierarchy;