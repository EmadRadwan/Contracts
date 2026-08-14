CREATE OR REPLACE VIEW Fact_Project_DirectPayments_2 AS
SELECT
    pyt.PAYMENT_ID                                      AS PaymentId,
    pyt.PAYMENT_TYPE_ID,
    ptt.DESCRIPTION_ARABIC                              AS PaymentTypeDescription,

    pyt.PAYMENT_METHOD_ID                               AS PaymentMethodId,
    pyt.PAYMENT_METHOD_TYPE_ID,
    pmt.DESCRIPTION_ARABIC                              AS PaymentMethodTypeDescription,

    pyt.PARTY_ID_FROM                                   AS PartyIdFrom,
    pty_from.DESCRIPTION                                AS PartyIdFromName,

    pyt.PARTY_ID_TO                                     AS PartyIdTo,
    COALESCE(pty_to.DESCRIPTION,
             CASE WHEN pyt.PARTY_ID_TO = 'Company' THEN 'Golden Land'
                  ELSE pyt.PARTY_ID_TO END)              AS PartyIdToName,

    pyt.STATUS_ID,
    sts.DESCRIPTION_ARABIC                              AS StatusDescription,
    sts.DESCRIPTION                                     AS StatusDescriptionEnglish,

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

    NULL                                                AS OrganizationPartyId,

    -- ✅ Now driven by dimProject
    dp.ProjectId,
    dp.ProjectName,

    pyt.OVERRIDE_GL_ACCOUNT_ID                          AS OverrideGlAccountId,

    pyt.COST_CENTER_ID                                  AS CostCenterId,
    cc.DESCRIPTION                                      AS CostCenterDescription,

    sr.SALES_REQUEST_ID,
    prod.PRODUCT_ID,
    prod.BUILDING_NUMBER,

    pyt.APPROVED_BY_PARTY_ID                            AS ApprovedByPartyId,
    approved.DESCRIPTION                                AS ApprovedByPartyName,

    pyt.CREATED_BY_PARTY_ID                             AS CreatedByPartyId,
    created_by.DESCRIPTION                              AS CreatedByPartyName,

    pyt.CHEQUENUMBER                                    AS ChequeNumber,
    pyt.CHEQUEDATE                                      AS ChequeDate,

    -- Due Status (unchanged)
    CASE
        WHEN pyt.STATUS_ID != 'PMNT_NOT_PAID' AND pyt.STATUS_ID IS NOT NULL
    THEN COALESCE(sts.DESCRIPTION_ARABIC, pyt.STATUS_ID)

        WHEN pyt.EFFECTIVE_DATE IS NULL
            THEN 'غير محدد'

        ELSE
            CASE
                WHEN DATEDIFF(pyt.EFFECTIVE_DATE, CURDATE()) < 0 THEN
                    CASE
                        WHEN ABS(DATEDIFF(pyt.EFFECTIVE_DATE, CURDATE())) = 1 THEN 'دفعة مستحقة متأخرة بيوم واحد'
                        WHEN ABS(DATEDIFF(pyt.EFFECTIVE_DATE, CURDATE())) = 2 THEN 'دفعة مستحقة متأخرة بيومين'
                        WHEN ABS(DATEDIFF(pyt.EFFECTIVE_DATE, CURDATE())) <= 30
                            THEN CONCAT('دفعة مستحقة متأخرة منذ ', ABS(DATEDIFF(pyt.EFFECTIVE_DATE, CURDATE())), ' يوم')
                        ELSE CONCAT('دفعة مستحقة متأخرة جداً (الربع ',
                                    CASE ((MONTH(pyt.EFFECTIVE_DATE)-1) DIV 3 + 1)
                                        WHEN 1 THEN 'الأول' WHEN 2 THEN 'الثاني'
                                        WHEN 3 THEN 'الثالث' WHEN 4 THEN 'الرابع'
                                    END, ' ', YEAR(pyt.EFFECTIVE_DATE), ')')
END

WHEN DATEDIFF(pyt.EFFECTIVE_DATE, CURDATE()) = 0 THEN 'دفعة مستحقة اليوم'
                WHEN DATEDIFF(pyt.EFFECTIVE_DATE, CURDATE()) = 1 THEN 'دفعة مستحقة غداً'
                WHEN DATEDIFF(pyt.EFFECTIVE_DATE, CURDATE()) <= 3
                    THEN CONCAT('دفعة مستحقة بعد ', DATEDIFF(pyt.EFFECTIVE_DATE, CURDATE()), ' أيام')
                WHEN DATEDIFF(pyt.EFFECTIVE_DATE, CURDATE()) <= 7 THEN 'دفعة مستحقة هذا الأسبوع'
                WHEN DATEDIFF(pyt.EFFECTIVE_DATE, CURDATE()) <= 30 THEN 'دفعة مستحقة خلال الشهر'
                WHEN DATEDIFF(pyt.EFFECTIVE_DATE, CURDATE()) <= 90
                    THEN CONCAT('دفعة مستحقة خلال 3 أشهر (الربع ',
                                CASE ((MONTH(pyt.EFFECTIVE_DATE)-1) DIV 3 + 1)
                                    WHEN 1 THEN 'الأول' WHEN 2 THEN 'الثاني'
                                    WHEN 3 THEN 'الثالث' WHEN 4 THEN 'الرابع'
                                END, ' ', YEAR(pyt.EFFECTIVE_DATE), ')')
                ELSE CONCAT('دفعة مستحقة لاحقاً (الربع ',
                            CASE ((MONTH(pyt.EFFECTIVE_DATE)-1) DIV 3 + 1)
                                WHEN 1 THEN 'الأول' WHEN 2 THEN 'الثاني'
                                WHEN 3 THEN 'الثالث' WHEN 4 THEN 'الرابع'
                            END, ' ', YEAR(pyt.EFFECTIVE_DATE), ')')
END
END AS DueStatusArabic

FROM PAYMENT pyt

JOIN PAYMENT_TYPE ptt          ON pyt.PAYMENT_TYPE_ID = ptt.PAYMENT_TYPE_ID
JOIN STATUS_ITEM sts           ON pyt.STATUS_ID = sts.STATUS_ID
JOIN PARTY pty_from            ON pyt.PARTY_ID_FROM = pty_from.PARTY_ID

LEFT JOIN PAYMENT_METHOD_TYPE pmt ON pyt.PAYMENT_METHOD_TYPE_ID = pmt.PAYMENT_METHOD_TYPE_ID
LEFT JOIN PARTY pty_to            ON pyt.PARTY_ID_TO = pty_to.PARTY_ID
LEFT JOIN COST_CENTER cc          ON pyt.COST_CENTER_ID = cc.COST_CENTER_ID
LEFT JOIN SALES_REQUEST sr        ON pyt.SALES_REQUEST_ID = sr.SALES_REQUEST_ID
LEFT JOIN PRODUCT prod            ON sr.PRODUCT_ID = prod.PRODUCT_ID
LEFT JOIN PARTY approved          ON pyt.APPROVED_BY_PARTY_ID = approved.PARTY_ID
LEFT JOIN PARTY created_by        ON pyt.CREATED_BY_PARTY_ID = created_by.PARTY_ID

-- ✅ CORE FIX
JOIN DimProject dp
    ON pyt.OVERRIDE_GL_ACCOUNT_ID = dp.GlAccountId

WHERE (ptt.PARENT_TYPE_ID = 'DISBURSEMENT'
    OR ptt.PAYMENT_TYPE_ID = 'DISBURSEMENT')
  -- NO STATUS FILTER — deliberate. `AND pyt.STATUS_ID = 'PMNT_SENT'` used to sit here and
  -- was removed on review, because it made Power BI the only place that disagreed with the
  -- accounts. The excluded rows are signed post-dated cheques for government licence fees
  -- and land-authority instalments (maturities 2026-12 → 2031-06) whose cost the ledger has
  -- ALREADY recognised: each one carries a CHECK_ISSUED transaction debiting the project
  -- account and crediting 250100 شيكات صادرة مؤجلة. GetProjectReport.cs includes them too
  -- (its own status filter is commented out), and the sibling view
  -- Fact_Project_OperatingExpenses_2 has never had a status filter either.
  --
  -- Measured impact of the removal: +14 payments, +82,472,646 — which is 73% of نسيم -
  -- الثروة الخضراء's direct project cost and 81% of سوا's. Before the change Power BI showed
  -- roughly a quarter of Naseem's direct cost and a fifth of Sawa's, so anyone reconciling a
  -- project against its GL trial balance found a 70–80% gap.
  --
  -- If a cash-only view is ever wanted, do NOT reinstate this filter here in isolation: add a
  -- separate measure for committed-not-yet-paid, and apply the same rule in all three places
  -- (this view, Fact_Project_OperatingExpenses_2, and GetProjectReport.cs) so they agree.
  AND dp.GlAccountType = 'PROJECT_MAIN'
  AND pyt.OVERRIDE_GL_ACCOUNT_ID IS NOT NULL
  -- Project labour cost belongs to Fact_Project_Payroll, which claims every payroll
  -- charge to a project account — including ad-hoc PAYROL_PAYMENTs made outside the
  -- monthly accrual. Without this line that view and this one would both count the
  -- same row, and [Total Spent] would sum them. GetProjectReport.cs applies the same
  -- exclusion (lines 462 and 696), so this brings the view back in step with the C#.
  --
  -- !! DEPLOY THIS TOGETHER WITH Fact_Project_Payroll — neither half is safe alone:
  --      payroll view + no exclusion  -> the payment is counted TWICE
  --      exclusion + no payroll view  -> the payment is lost entirely
  --    Today's exposure is zero rows in THIS view (the one live case, payment 14757,
  --    is OPERATING_CHILD and lands in the sibling). The line is here so the rule is
  --    the same in both views and the next such payment cannot slip through whichever
  --    one it happens to hit.
  AND pyt.PAYMENT_TYPE_ID <> 'PAYROL_PAYMENT'
  -- Exclude payments already counted in the expenses section
  AND NOT EXISTS (
      SELECT 1 FROM Fact_Project_Expenses fpe
      WHERE fpe.PaymentId = pyt.PAYMENT_ID
  );