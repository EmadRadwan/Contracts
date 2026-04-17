-- =============================================================
-- FACT PAYMENT APPLICATIONS – Shows every payment and what it was applied to
-- Includes unapplied payments too (via LEFT JOIN)
-- NO dimension joins – pure fact, relationships done in Power BI
-- =============================================================
DROP VIEW IF EXISTS PaymentApplications;
CREATE OR REPLACE VIEW PaymentApplications AS

SELECT
    -- =================================================================
    -- Keys
    -- =================================================================
    p.PAYMENT_ID                                            AS PaymentId,                     -- Main key (links to FactPayments)
    COALESCE(pa.PAYMENT_APPLICATION_ID, CONCAT('UNAPPLIED_', p.PAYMENT_ID))
                                                            AS PaymentApplicationKey,         -- Surrogate for unapplied rows

    pa.PAYMENT_APPLICATION_ID                               AS PaymentApplicationId,
    pa.INVOICE_ID                                           AS InvoiceId,
    pa.INVOICE_ITEM_SEQ_ID                                  AS InvoiceItemSeqId,
    pa.TO_PAYMENT_ID                                        AS ToPaymentId,                   -- for payment-to-payment applications
    pa.BILLING_ACCOUNT_ID                                   AS BillingAccountId,

    -- =================================================================
    -- Application Amount
    -- =================================================================
    COALESCE(pa.AMOUNT_APPLIED, 0)                          AS AmountApplied,
    p.AMOUNT                                                AS PaymentAmount,

    -- Remaining unapplied amount (super useful KPI – no DAX needed)
    p.AMOUNT - COALESCE((
                            SELECT SUM(pa2.AMOUNT_APPLIED)
                            FROM PAYMENT_APPLICATION pa2
                            WHERE pa2.PAYMENT_ID = p.PAYMENT_ID
                        ), 0)                                                   AS AmountUnapplied,

    -- =================================================================
    -- Flags – instant filtering in Power BI
    -- =================================================================
    CASE WHEN pa.PAYMENT_APPLICATION_ID IS NOT NULL THEN 'Y' ELSE 'N' END
        AS IsApplied,
    CASE WHEN pa.INVOICE_ID IS NOT NULL              THEN 'Y' ELSE 'N' END
        AS IsAppliedToInvoice,
    CASE WHEN pa.TO_PAYMENT_ID IS NOT NULL           THEN 'Y' ELSE 'N' END
        AS IsAppliedToPayment,

    -- =================================================================
    -- Raw Payment Details (same structure as FactPayments)
    -- =================================================================
    p.PAYMENT_TYPE_ID,
    p.PAYMENT_METHOD_TYPE_ID,
    p.PAYMENT_METHOD_ID,
    p.PAYMENT_REF_NUM                                       AS PaymentRefNum,
    p.EFFECTIVE_DATE                                        AS EffectiveDate,
    p.STATUS_ID                                             AS PaymentStatusId,
    p.PARTY_ID_FROM,
    p.PARTY_ID_TO,
    p.CURRENCY_UOM_ID,
    p.WORK_EFFORT_ID                                        AS ProjectId,
    p.COST_CENTER_ID                                        AS CostCenterId,
    p.COMMENTS,

    -- =================================================================
    -- Audit
    -- =================================================================
    GREATEST(
            COALESCE(p.LAST_UPDATED_STAMP, p.CREATED_STAMP),
            COALESCE(pa.LAST_UPDATED_STAMP, pa.CREATED_STAMP, p.CREATED_STAMP)
        )                                                       AS RowLastUpdated

FROM PAYMENT p
         LEFT JOIN PAYMENT_APPLICATION pa
                   ON pa.PAYMENT_ID = p.PAYMENT_ID

-- Optional: remove fully cancelled payments
WHERE COALESCE(p.STATUS_ID, '') NOT IN ('PMNT_CANCELLED', 'PMNT_VOID')

ORDER BY p.EFFECTIVE_DATE DESC, p.PAYMENT_ID;