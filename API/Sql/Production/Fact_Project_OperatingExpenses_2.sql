CREATE OR REPLACE VIEW Fact_Project_OperatingExpenses_2 AS
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

    -- ✅ Unified Project resolution
    dp.ProjectId,
    dp.ProjectName,

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

-- ✅ CORE FIX (same as DirectPayments)
         JOIN DimProject dp
              ON pyt.OVERRIDE_GL_ACCOUNT_ID = dp.GlAccountId

WHERE (ptt.PARENT_TYPE_ID = 'DISBURSEMENT'
    OR ptt.PAYMENT_TYPE_ID = 'DISBURSEMENT') AND dp.GlAccountType <> 'PROJECT_MAIN'
  AND pyt.OVERRIDE_GL_ACCOUNT_ID IS NOT NULL
  -- Exclude payments already counted in the expenses or direct-payments sections
  AND NOT EXISTS (
      SELECT 1 FROM Fact_Project_Expenses fpe
      WHERE fpe.PaymentId = pyt.PAYMENT_ID
  )
  AND NOT EXISTS (
      SELECT 1 FROM Fact_Project_DirectPayments_2 fdp
      WHERE fdp.PaymentId = pyt.PAYMENT_ID
  );