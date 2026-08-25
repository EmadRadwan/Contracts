-- ============================================================================================
-- READ-ONLY exposure check: brokers who were VAT-exempt but NOT withholding-tax exempt.
--
-- Until the fix in SalesCommissionCalculator.CalculateAmountsCore (2026-08-25), a VAT exemption
-- short-circuited the withholding-tax deduction entirely:
--
--     if (!hasVatExemption) { net = gross - base * wht/100; }
--     else                  { net = gross; }          <-- WHT never deducted
--
-- So any commission with HAS_VAT_EXEMPTION = 1 AND HAS_WITHHOLDING_TAX_EXEMPTION = 0 paid the
-- broker company its full gross. Under the corrected rule the WHT base for a VAT-exempt broker is
-- the gross itself (nothing is embedded), so the correct net is gross * (1 - WHT%/100).
--
-- This script only SELECTs. It changes nothing. Run it against production to size the exposure.
-- ============================================================================================

-- --------------------------------------------------------------------------------------------
-- 1. Affected commissions, with the overpayment per row.
--    OVERPAID_NET is what the fix would have withheld and did not.
-- --------------------------------------------------------------------------------------------
SELECT
    sc.SALES_COMMISSION_ID,
    sc.SALES_REQUEST_ID,
    sc.STATUS_ID,
    DATE(sc.COMMISSION_DATE)                                   AS COMMISSION_DATE,
    we.PROJECT_NAME,
    sc.EXT_COMPANY_PARTY_ID                                    AS BROKER_PARTY_ID,
    br.DESCRIPTION                                             AS BROKER_NAME,
    sc.WITHHOLDING_TAX_PERCENT                                 AS WHT_PCT,
    sc.EXT_COMPANY_PERCENT                                     AS BROKER_PCT,
    sc.EXT_COMPANY_GROSS_AMOUNT                                AS GROSS,
    sc.EXT_COMPANY_NET_AMOUNT                                  AS STORED_NET,
    -- Corrected net: VAT-exempt means gross IS the WHT base.
    ROUND(sc.EXT_COMPANY_GROSS_AMOUNT
          * (1 - sc.WITHHOLDING_TAX_PERCENT / 100), 2)         AS CORRECTED_NET,
    ROUND(sc.EXT_COMPANY_GROSS_AMOUNT
          * (sc.WITHHOLDING_TAX_PERCENT / 100), 2)             AS OVERPAID_NET,
    -- What the approval actually disbursed. Approval rounds each payment to 0 decimals, so compare
    -- against this, not against STORED_NET. NULL = approved but no payment row found.
    pay.PAID_COUNT,
    pay.PAID_AMOUNT,
    pay.PAYMENT_STATUSES
FROM SALES_COMMISSION sc
LEFT JOIN WORK_EFFORT we
       ON we.WORK_EFFORT_ID = sc.PROJECT_ID
LEFT JOIN PARTY br
       ON br.PARTY_ID = sc.EXT_COMPANY_PARTY_ID
LEFT JOIN (
    SELECT
        p.SALES_REQUEST_ID,
        p.PARTY_ID_TO,
        COUNT(*)                          AS PAID_COUNT,
        SUM(p.AMOUNT)                     AS PAID_AMOUNT,
        GROUP_CONCAT(DISTINCT p.STATUS_ID) AS PAYMENT_STATUSES
    FROM PAYMENT p
    WHERE p.PAYMENT_TYPE_ID = 'COMMISSION_PAYMENT'
    GROUP BY p.SALES_REQUEST_ID, p.PARTY_ID_TO
) pay
       ON pay.SALES_REQUEST_ID = sc.SALES_REQUEST_ID
      AND pay.PARTY_ID_TO      = sc.EXT_COMPANY_PARTY_ID
WHERE sc.HAS_VAT_EXEMPTION            = 1
  AND sc.HAS_WITHHOLDING_TAX_EXEMPTION = 0
  AND sc.WITHHOLDING_TAX_PERCENT      > 0
  AND sc.EXT_COMPANY_GROSS_AMOUNT     > 0
  AND sc.STATUS_ID IN ('COMMISSION_APPROVED', 'COMMISSION_PAID')
ORDER BY OVERPAID_NET DESC;


-- --------------------------------------------------------------------------------------------
-- 2. One-line total exposure.
-- --------------------------------------------------------------------------------------------
SELECT
    COUNT(*)                                                         AS AFFECTED_COMMISSIONS,
    COUNT(DISTINCT sc.EXT_COMPANY_PARTY_ID)                          AS AFFECTED_BROKERS,
    ROUND(SUM(sc.EXT_COMPANY_GROSS_AMOUNT), 2)                       AS TOTAL_GROSS,
    ROUND(SUM(sc.EXT_COMPANY_GROSS_AMOUNT
              * (sc.WITHHOLDING_TAX_PERCENT / 100)), 2)              AS TOTAL_OVERPAID
FROM SALES_COMMISSION sc
WHERE sc.HAS_VAT_EXEMPTION            = 1
  AND sc.HAS_WITHHOLDING_TAX_EXEMPTION = 0
  AND sc.WITHHOLDING_TAX_PERCENT      > 0
  AND sc.EXT_COMPANY_GROSS_AMOUNT     > 0
  AND sc.STATUS_ID IN ('COMMISSION_APPROVED', 'COMMISSION_PAID');


-- --------------------------------------------------------------------------------------------
-- 3. Still-pending commissions with the same flag combination.
--    These are NOT exposure — they are recalculated on save or approval, so the fix corrects them
--    on its own. Listed only so the two sets are not confused when reconciling.
-- --------------------------------------------------------------------------------------------
SELECT
    sc.SALES_COMMISSION_ID,
    sc.SALES_REQUEST_ID,
    br.DESCRIPTION                                             AS BROKER_NAME,
    sc.EXT_COMPANY_GROSS_AMOUNT                                AS GROSS,
    sc.EXT_COMPANY_NET_AMOUNT                                  AS STORED_NET,
    ROUND(sc.EXT_COMPANY_GROSS_AMOUNT
          * (1 - sc.WITHHOLDING_TAX_PERCENT / 100), 2)         AS NET_AFTER_FIX
FROM SALES_COMMISSION sc
LEFT JOIN PARTY br
       ON br.PARTY_ID = sc.EXT_COMPANY_PARTY_ID
WHERE sc.HAS_VAT_EXEMPTION            = 1
  AND sc.HAS_WITHHOLDING_TAX_EXEMPTION = 0
  AND sc.WITHHOLDING_TAX_PERCENT      > 0
  AND sc.EXT_COMPANY_GROSS_AMOUNT     > 0
  AND sc.STATUS_ID = 'COMMISSION_PENDING'
ORDER BY sc.SALES_COMMISSION_ID;


-- --------------------------------------------------------------------------------------------
-- 4. Sanity counter-check: commissions whose stored net already differs from the gross.
--    Every row here was calculated on the non-exempt path, so it should be UNAFFECTED by the fix.
--    If any row from query 1 also appears here, the assumption behind this script is wrong —
--    stop and re-check before acting on the numbers above.
-- --------------------------------------------------------------------------------------------
SELECT
    sc.SALES_COMMISSION_ID,
    sc.HAS_VAT_EXEMPTION,
    sc.HAS_WITHHOLDING_TAX_EXEMPTION,
    sc.EXT_COMPANY_GROSS_AMOUNT AS GROSS,
    sc.EXT_COMPANY_NET_AMOUNT   AS STORED_NET
FROM SALES_COMMISSION sc
WHERE sc.HAS_VAT_EXEMPTION        = 1
  AND sc.EXT_COMPANY_GROSS_AMOUNT > 0
  AND sc.EXT_COMPANY_NET_AMOUNT  <> sc.EXT_COMPANY_GROSS_AMOUNT;


SELECT p.SALES_REQUEST_ID, p.PAYMENT_ID, p.PARTY_ID_TO, pt.DESCRIPTION AS PARTY_NAME,
       p.AMOUNT, p.STATUS_ID, p.EFFECTIVE_DATE, p.COMMENTS
FROM PAYMENT p
         LEFT JOIN PARTY pt ON pt.PARTY_ID = p.PARTY_ID_TO
WHERE p.PAYMENT_TYPE_ID = 'COMMISSION_PAYMENT'
  AND p.SALES_REQUEST_ID IN ('10885', '10886')
ORDER BY p.SALES_REQUEST_ID, p.STATUS_ID, p.AMOUNT DESC;
