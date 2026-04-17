-- =============================================================
-- DIM PAYMENT METHODS – Clean dimension table (Power BI DimPaymentMethods)
-- =============================================================
DROP VIEW IF EXISTS DimPaymentMethods;
CREATE OR REPLACE VIEW DimPaymentMethods AS
SELECT
    -- Surrogate key – used in Payment fact table
    pm.PAYMENT_METHOD_ID                                    AS PaymentMethodId,          -- KEY

    -- Main display name (Arabic description is usually richer in your system)
    COALESCE(pm.DESCRIPTION, pm.PAYMENT_METHOD_ID)          AS PaymentMethodName,        -- e.g. "بنك أبو ظبي الإسلامي" or "كاش"

    -- English fallback (derived from PaymentMethodType description)
    COALESCE(pmt.DESCRIPTION, pm.PAYMENT_METHOD_ID)         AS PaymentMethodNameEnglish,

    -- Payment Method Type (link to DimPaymentMethodTypes)
    pm.PAYMENT_METHOD_TYPE_ID                               AS PaymentMethodTypeId,
    pmt.DESCRIPTION                                         AS PaymentMethodTypeName,
    pmt.DESCRIPTION_ARABIC                                  AS PaymentMethodTypeNameArabic,

    -- Bank / Financial Account details
    pm.FIN_ACCOUNT_ID                                       AS FinAccountId,
    pm.GL_ACCOUNT_ID                                        AS GlAccountId,

    -- Party (almost always "Company" in your case, but kept for completeness)
    pm.PARTY_ID                                             AS PartyId,

    -- Active status (slowly changing dimension type 2 support)
    pm.FROM_DATE                                            AS ValidFrom,
    pm.THRU_DATE                                            AS ValidThru,
    CASE WHEN pm.THRU_DATE IS NULL THEN 'Y' ELSE 'N' END    AS IsActive,

    -- Helpful categorization flags
    CASE
        WHEN pm.PAYMENT_METHOD_TYPE_ID = 'CASH'                  THEN 'Y' ELSE 'N'
        END                                                     AS IsCash,
    CASE
        WHEN pm.PAYMENT_METHOD_TYPE_ID IN ('COMPANY_CHECK', 'CERTIFIED_CHECK')
            OR pm.FIN_ACCOUNT_ID IS NOT NULL                    THEN 'Y' ELSE 'N'
        END                                                     AS IsBankAccount,
    CASE
        WHEN pm.PAYMENT_METHOD_TYPE_ID = 'FIN_ACCOUNT'           THEN 'Y' ELSE 'N'
        END                                                     AS IsCustody,  -- عهدة مستديمة

    -- Audit
    pm.LAST_UPDATED_STAMP                                   AS LastUpdatedStamp,
    pm.CREATED_STAMP                                        AS CreatedStamp

FROM PAYMENT_METHOD pm
         LEFT JOIN PAYMENT_METHOD_TYPE pmt
                   ON pm.PAYMENT_METHOD_TYPE_ID = pmt.PAYMENT_METHOD_TYPE_ID

WHERE (pm.THRU_DATE IS NULL OR pm.THRU_DATE >= CURDATE());  -- Only currently active methods