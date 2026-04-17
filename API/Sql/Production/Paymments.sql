DROP VIEW IF EXISTS Payments;
CREATE OR REPLACE VIEW Payments AS
SELECT
    -- =================================================================
    -- Core Keys (keep for relationships in Power BI / reporting tools)
    -- =================================================================
    p.PAYMENT_ID AS PaymentId,

    -- =================================================================
    -- Amounts & Currency
    -- =================================================================
    p.AMOUNT AS Amount,
    p.ACTUAL_CURRENCY_AMOUNT AS ActualAmount,
    COALESCE(p.CURRENCY_UOM_ID, 'EGP') AS CurrencyUomId,

    -- =================================================================
    -- Parties – Raw IDs + Names
    -- =================================================================
    p.PARTY_ID_FROM AS PartyIdFrom,
    pf.DESCRIPTION AS PartyNameFrom,                  -- e.g. "الشركة", "محمد أحمد"

    p.PARTY_ID_TO AS PartyIdTo,
    COALESCE(pt.DESCRIPTION,
             CASE WHEN p.PARTY_ID_TO = 'Company' THEN 'Company' ELSE p.PARTY_ID_TO END,
             'Unknown') AS PartyNameTo,               -- Handles "Company" literal and missing parties

    -- =================================================================
    -- Payment Classification + Names
    -- =================================================================
    p.PAYMENT_TYPE_ID AS PaymentTypeId,
    pt_type.DESCRIPTION AS PaymentTypeDescription,    -- REFACTOR: Added join to PaymentType for readable type name
    pt_type.DESCRIPTION_ARABIC AS PaymentTypeDescriptionArabic,

    p.PAYMENT_METHOD_TYPE_ID AS PaymentMethodTypeId,
    pmt_type.DESCRIPTION AS PaymentMethodTypeName,
    pmt_type.DESCRIPTION_ARABIC AS PaymentMethodTypeNameArabic,
    p.PAYMENT_REF_NUM AS PaymentRefNum,
    p.PAYMENT_METHOD_ID AS PaymentMethodId,
    pm.DESCRIPTION AS PaymentMethodName,              -- e.g. "بنك أبوظبي الإسلامي", "كاش"

    -- =================================================================
    -- Project & Cost Center + Names
    -- =================================================================
    p.WORK_EFFORT_ID AS ProjectId,
    we.PROJECT_NAME AS ProjectName,

    p.COST_CENTER_ID AS CostCenterId,
    cc.DESCRIPTION AS CostCenterName,

    -- =================================================================
    -- Status + Names (English & Arabic)
    -- =================================================================
    p.STATUS_ID AS StatusId,
    si.DESCRIPTION AS StatusNameEnglish,
    si.DESCRIPTION_ARABIC AS StatusNameArabic,

    -- =================================================================
    -- Dates & Core Due Calculation
    -- =================================================================
    p.EFFECTIVE_DATE AS EffectiveDate,
    DATEDIFF(p.EFFECTIVE_DATE, CURDATE()) AS DaysUntilDue,  -- Positive = future, 0 = today, negative = overdue

    -- =================================================================
    -- REFACTOR: Arabic Due Status – now ONLY applied to unpaid payments
    -- This exactly matches the React frontend logic (getDueStatusArabic)
    -- Previously applied to all records → misleading for paid/confirmed payments
    -- =================================================================
    CASE
        WHEN p.STATUS_ID <> 'PMNT_NOT_PAID' THEN si.DESCRIPTION_ARABIC
        ELSE
            CASE
                WHEN DATEDIFF(p.EFFECTIVE_DATE, CURDATE()) < 0 THEN
                    CASE
                        WHEN ABS(DATEDIFF(p.EFFECTIVE_DATE, CURDATE())) <= 30 THEN
                            CONCAT(
                                    CASE WHEN pt_type.PARENT_TYPE_ID = 'DISBURSEMENT' THEN 'دفعة' ELSE 'مستحق' END,
                                    ' متأخرة منذ ',
                                    ABS(DATEDIFF(p.EFFECTIVE_DATE, CURDATE())),
                                    ' يوم'
                                )
                        ELSE
                            CONCAT(
                                    CASE WHEN pt_type.PARENT_TYPE_ID = 'DISBURSEMENT' THEN 'دفعة' ELSE 'مستحق' END,
                                    ' متأخرة جداً'
                                )
                        END
                WHEN DATEDIFF(p.EFFECTIVE_DATE, CURDATE()) = 0 THEN
                    CONCAT(CASE WHEN pt_type.PARENT_TYPE_ID = 'DISBURSEMENT' THEN 'دفعة مستحقة' ELSE 'مستحق' END, ' اليوم')
                WHEN DATEDIFF(p.EFFECTIVE_DATE, CURDATE()) = 1 THEN
                    CONCAT(CASE WHEN pt_type.PARENT_TYPE_ID = 'DISBURSEMENT' THEN 'دفعة مستحقة' ELSE 'مستحق' END, ' غداً')
                WHEN DATEDIFF(p.EFFECTIVE_DATE, CURDATE()) <= 3 THEN
                    CONCAT(
                            CASE WHEN pt_type.PARENT_TYPE_ID = 'DISBURSEMENT' THEN 'دفعة مستحقة' ELSE 'مستحق' END,
                            ' بعد ',
                            DATEDIFF(p.EFFECTIVE_DATE, CURDATE()),
                            ' أيام'
                        )
                WHEN DATEDIFF(p.EFFECTIVE_DATE, CURDATE()) <= 7 THEN
                    CONCAT(CASE WHEN pt_type.PARENT_TYPE_ID = 'DISBURSEMENT' THEN 'دفعة مستحقة' ELSE 'مستحق' END, ' هذا الأسبوع')
                WHEN DATEDIFF(p.EFFECTIVE_DATE, CURDATE()) <= 30 THEN
                    CONCAT(CASE WHEN pt_type.PARENT_TYPE_ID = 'DISBURSEMENT' THEN 'دفعة مستحقة' ELSE 'مستحق' END, ' خلال الشهر')
                WHEN DATEDIFF(p.EFFECTIVE_DATE, CURDATE()) <= 90 THEN
                    CONCAT(CASE WHEN pt_type.PARENT_TYPE_ID = 'DISBURSEMENT' THEN 'دفعة مستحقة' ELSE 'مستحق' END, ' خلال 3 أشهر')
                ELSE
                    CONCAT(CASE WHEN pt_type.PARENT_TYPE_ID = 'DISBURSEMENT' THEN 'دفعة مستحقة' ELSE 'مستحق' END, ' لاحقاً')
                END
        END AS DueStatusArabic,

    -- =================================================================
    -- Helpful Flags
    -- =================================================================
    CASE WHEN pt_type.PARENT_TYPE_ID = 'DISBURSEMENT' THEN 1 ELSE 0 END AS IsDisbursement,  -- REFACTOR: More reliable than PartyId check
    CASE
        WHEN pt_type.PARENT_TYPE_ID = 'DISBURSEMENT' THEN p.PARTY_ID_FROM   -- Outgoing: organization is From
        ELSE p.PARTY_ID_TO                                                 -- Incoming: organization is To
        END AS OrganizationPartyId,

    CASE
        WHEN pt_type.PARENT_TYPE_ID = 'DISBURSEMENT' THEN 'Outbound'
        WHEN p.PARTY_ID_TO = 'Company' THEN 'Inbound'
        ELSE 'Unknown'
        END AS PaymentDirection,

    -- =================================================================
    -- Additional fields (kept for completeness)
    -- =================================================================
    p.COMMENTS AS Comments,
    p.ChequeNumber AS ChequeNumber,
    p.ChequeDate AS ChequeDate,
    p.OVERRIDE_GL_ACCOUNT_ID AS OverrideGlAccountId,
    p.CREATED_STAMP AS CreatedDate

FROM PAYMENT p

-- Required joins (inner – assumed always present)
         LEFT JOIN PARTY pf ON p.PARTY_ID_FROM = pf.PARTY_ID
         LEFT JOIN PARTY pt ON p.PARTY_ID_TO = pt.PARTY_ID
         LEFT JOIN PAYMENT_METHOD pm ON p.PAYMENT_METHOD_ID = pm.PAYMENT_METHOD_ID
         LEFT JOIN COST_CENTER cc ON p.COST_CENTER_ID = cc.COST_CENTER_ID
         LEFT JOIN STATUS_ITEM si ON p.STATUS_ID = si.STATUS_ID
         LEFT JOIN WORK_EFFORT we ON p.WORK_EFFORT_ID = we.WORK_EFFORT_ID

-- REFACTOR: Added essential joins for accurate direction and descriptions
         LEFT JOIN PAYMENT_TYPE pt_type ON p.PAYMENT_TYPE_ID = pt_type.PAYMENT_TYPE_ID
         LEFT JOIN PAYMENT_METHOD_TYPE pmt_type ON p.PAYMENT_METHOD_TYPE_ID = pmt_type.PAYMENT_METHOD_TYPE_ID

ORDER BY p.EFFECTIVE_DATE DESC, p.PAYMENT_ID DESC;