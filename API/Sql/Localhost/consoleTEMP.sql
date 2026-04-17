DROP VIEW IF EXISTS Fact_Apartment_Installment_Payment_Link;

CREATE OR REPLACE VIEW Fact_Apartment_Installment_Payment_Link AS
SELECT
    -- Installment side (original plan)
    i.SALES_REQUEST_ID,
    i.INSTALLMENT_NUMBER,
    i.DUE_DATE                                      AS PlannedDueDate,
    DATE(i.DUE_DATE)                                AS PlannedDueDateKey,
    i.AMOUNT                                        AS PlannedAmount,
    i.IS_ADVANCE,

    -- Payment side (what actually exists)
    p.PAYMENT_ID,
    p.STATUS_ID                                     AS PaymentStatusId,
    p.EFFECTIVE_DATE                                AS ActualEffectiveDate,
    DATE(p.EFFECTIVE_DATE)                          AS ActualDueDateKey,
    p.AMOUNT                                        AS ActualAmount,

    -- Customer Info (Added)
    p.PARTY_ID_FROM                                 AS CustomerPartyId,
    pf.DESCRIPTION                                  AS CustomerName,           -- ← Added

    -- Matching & Difference flags
    CASE
        WHEN p.PAYMENT_ID IS NULL THEN 'Missing Payment'
        WHEN ABS(COALESCE(p.AMOUNT, 0) - i.AMOUNT) > 0.01 THEN 'Amount Mismatch'
        WHEN DATE(p.EFFECTIVE_DATE) <> DATE(i.DUE_DATE) THEN 'Date Mismatch'
        WHEN p.STATUS_ID = 'PMNT_RECEIVED' THEN 'Paid'
        ELSE 'Matched - Not Paid'
        END                                             AS MatchStatus,

    -- Useful calculated fields
    COALESCE(p.AMOUNT, 0) - i.AMOUNT                AS AmountDifference,
    DATEDIFF(DATE(p.EFFECTIVE_DATE), DATE(i.DUE_DATE)) AS DaysDifference,

    -- Extra info from Payment
    p.COMMENTS                                      AS PaymentComments,
    p.PAYMENT_TYPE_ID,
    p.ChequeNumber,
    p.ChequeDate,

    -- From Sales Request (for context)
    sr.PRODUCT_ID                                   AS ApartmentId,
    sr.STATUS_ID                                    AS SalesRequestStatus

FROM SALES_REQUEST_INSTALLMENT i

         LEFT JOIN PAYMENT p
                   ON p.SALES_REQUEST_ID = i.SALES_REQUEST_ID
                       AND DATE(p.EFFECTIVE_DATE) = DATE(i.DUE_DATE)
                       AND ABS(COALESCE(p.AMOUNT, 0) - i.AMOUNT) <= 0.01
                       AND p.PAYMENT_TYPE_ID IN ('RECEIPT_ADVANCE_PAYMENT',
                                                 'RECEIPT_DUE_INSTALLMENT',
                                                 'RECEIPT_MAINTENANCE_AMOUNT')

         LEFT JOIN PARTY pf
                   ON p.PARTY_ID_FROM = pf.PARTY_ID                     -- ← Added join for Customer Name

         LEFT JOIN SALES_REQUEST sr
                   ON i.SALES_REQUEST_ID = sr.SALES_REQUEST_ID

WHERE i.SALES_REQUEST_ID IS NOT NULL

ORDER BY i.SALES_REQUEST_ID, i.INSTALLMENT_NUMBER, p.PAYMENT_ID;