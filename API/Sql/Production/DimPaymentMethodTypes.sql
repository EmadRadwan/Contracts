-- =============================================================
-- DIM PAYMENT METHOD TYPES – Clean dimension table (Power BI DimPaymentMethodTypes)
-- =============================================================
DROP VIEW IF EXISTS DimPaymentMethodTypes;
CREATE OR REPLACE VIEW DimPaymentMethodTypes AS
SELECT
    -- Surrogate key – unique identifier used in Payment and PaymentMethod tables
    pmt.PAYMENT_METHOD_TYPE_ID                              AS PaymentMethodTypeId,     -- KEY

    -- English name (primary display name)
    COALESCE(pmt.DESCRIPTION, pmt.PAYMENT_METHOD_TYPE_ID)   AS PaymentMethodTypeName,   -- e.g. "Cash"

    -- Arabic name (for bilingual reports)
    pmt.DESCRIPTION_ARABIC                                  AS PaymentMethodTypeNameArabic, -- e.g. "نق

    -- Default GL Account (useful for finance users & reconciliation reports)
    pmt.DEFAULT_GL_ACCOUNT_ID                               AS DefaultGlAccountId,

    -- Optional: Bring GL account description if you have a chart of accounts view/table
    -- coa.ACCOUNT_NAME                                     AS DefaultGlAccountName,   -- uncomment if you join later

    -- Payment category / grouping (common business grouping)
    CASE pmt.PAYMENT_METHOD_TYPE_ID
        WHEN 'CASH'              THEN 'Cash'
        WHEN 'CERTIFIED_CHECK'   THEN 'Check'
        WHEN 'COMPANY_CHECK'     THEN 'Check'
        WHEN 'PERSONAL_CHECK'    THEN 'Check'
        WHEN 'COMPANY_ACCOUNT'   THEN 'Bank Transfer'
        WHEN 'ELECTRONIC'        THEN 'Electronic'
        WHEN 'CREDIT_CARD'       THEN 'Credit Card'
        ELSE 'Other'
        END                                                     AS PaymentCategory,

    -- Audit fields (optional but helpful)
    pmt.LAST_UPDATED_STAMP                                  AS LastUpdatedStamp,
    pmt.CREATED_STAMP                                      AS CreatedStamp

FROM PAYMENT_METHOD_TYPE pmt;

-- =============================================================
-- USAGE NOTES
-- =============================================================
-- • Join fact tables (Payment, PaymentMethod) on PaymentMethodTypeId
-- • Use PaymentMethodTypeName for English dashboards
-- • Use PaymentMethodTypeNameArabic for Arabic dashboards
-- • PaymentCategory column enables quick grouping (e.g., "All Check payments")