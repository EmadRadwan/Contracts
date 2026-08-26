-- ############################################################################################
-- SUPERSEDED 2026-08-26 by fix_broker_commission_nets_2026_08_26.sql — DO NOT RUN.
--
-- This script predates the change that deducts VAT as well as WHT from the broker's net, so its
-- target values for 11119 and 11116 are no longer correct, and it covers only those two of the
-- nine approved commissions that need correcting. Kept for history.
-- ############################################################################################

-- ============================================================================================
-- Corrects EXT_COMPANY_NET_AMOUNT on the two commissions affected by the VAT-exempt / WHT bug.
--
-- Context: until 2026-08-25, SalesCommissionCalculator.CalculateAmountsCore let a VAT exemption
-- short-circuit the withholding-tax deduction (`else { net = gross; }`), so a VAT-exempt broker
-- that was NOT WHT-exempt had its full gross stored as the net. The calculator is fixed, but an
-- approved commission is never recalculated and is not editable through the UI — hence this script.
-- Exposure query: API/Sql/Production/vat_exempt_wht_exposure_2026_08_25.sql
--
--   11119  اركان للتطوير والتسويق العقاري   249,861.15 -> 237,368.09   (SR 10885)
--   11116  جياد الخليج                      204,955.14 -> 194,707.38   (SR 10886)
--
-- SCOPE: this script touches SALES_COMMISSION only. The two broker PAYMENT rows (O17606 and
-- O17584) are corrected separately through the payments UI — see step 0 below. It does NOT reset
-- either commission: both sales requests carry PMNT_SENT sibling payments and hand-made
-- adjustments that CommissionPaymentCleanup.PurgeAsync would destroy.
--
-- DO NOT run this unreviewed. Run each step, read the output, then decide.
-- ============================================================================================


-- --------------------------------------------------------------------------------------------
-- Step 0. Prerequisite, done outside this script.
--   Through the payments UI, set:
--     O17606  249,861 -> 237,368
--     O17584  204,955 -> 194,707
--   Both are PMNT_NOT_PAID with no AcctgTrans behind them, so the edit has no ledger impact.
--   Order does not matter, but both halves must happen or the report and the payments disagree.
-- --------------------------------------------------------------------------------------------


-- --------------------------------------------------------------------------------------------
-- Step 1. PRE-CHECK. Read this before running step 2.
--   WILL_CHANGE must be exactly 2 rows, and NEW_NET must read 237368.09 and 194707.38.
--   If it returns 0 rows the correction is already applied — stop, there is nothing to do.
-- --------------------------------------------------------------------------------------------
SELECT
    sc.SALES_COMMISSION_ID,
    sc.SALES_REQUEST_ID,
    sc.STATUS_ID,
    sc.HAS_VAT_EXEMPTION,
    sc.HAS_WITHHOLDING_TAX_EXEMPTION,
    sc.WITHHOLDING_TAX_PERCENT,
    sc.EXT_COMPANY_GROSS_AMOUNT                                   AS GROSS,
    sc.EXT_COMPANY_NET_AMOUNT                                     AS CURRENT_NET,
    ROUND(sc.EXT_COMPANY_GROSS_AMOUNT
          * (1 - sc.WITHHOLDING_TAX_PERCENT / 100), 2)            AS NEW_NET,
    ROUND(sc.EXT_COMPANY_NET_AMOUNT
          - sc.EXT_COMPANY_GROSS_AMOUNT
            * (1 - sc.WITHHOLDING_TAX_PERCENT / 100), 2)          AS REDUCTION
FROM SALES_COMMISSION sc
WHERE sc.SALES_COMMISSION_ID IN ('11119', '11116')
  AND sc.STATUS_ID                     = 'COMMISSION_APPROVED'
  AND sc.HAS_VAT_EXEMPTION             = 1
  AND sc.HAS_WITHHOLDING_TAX_EXEMPTION = 0
  AND sc.WITHHOLDING_TAX_PERCENT       > 0
  AND sc.EXT_COMPANY_NET_AMOUNT        = sc.EXT_COMPANY_GROSS_AMOUNT;


-- --------------------------------------------------------------------------------------------
-- Step 2. THE UPDATE.
--   The new value is computed from the row itself rather than typed in, so it cannot be
--   mistranscribed. The WHERE repeats every condition from step 1, including
--   `net = gross` — the fingerprint of the bug — so the statement is idempotent: running it a
--   second time matches nothing and changes nothing.
--   Expect "2 rows affected". Anything else: roll back and re-check step 1.
-- --------------------------------------------------------------------------------------------
START TRANSACTION;

UPDATE SALES_COMMISSION sc
SET sc.EXT_COMPANY_NET_AMOUNT = ROUND(sc.EXT_COMPANY_GROSS_AMOUNT
                                      * (1 - sc.WITHHOLDING_TAX_PERCENT / 100), 2),
    sc.LAST_UPDATED_STAMP     = UTC_TIMESTAMP()
WHERE sc.SALES_COMMISSION_ID IN ('11119', '11116')
  AND sc.STATUS_ID                     = 'COMMISSION_APPROVED'
  AND sc.HAS_VAT_EXEMPTION             = 1
  AND sc.HAS_WITHHOLDING_TAX_EXEMPTION = 0
  AND sc.WITHHOLDING_TAX_PERCENT       > 0
  AND sc.EXT_COMPANY_NET_AMOUNT        = sc.EXT_COMPANY_GROSS_AMOUNT;

-- Step 3. VERIFY BEFORE COMMITTING. Run this while the transaction is still open.
--   Expect STORED_NET to equal EXPECTED_NET on both rows, and MATCHES_PAYMENT = 1 once step 0 is
--   done (the payment is the net rounded to whole EGP, which is how approval writes it).
SELECT
    sc.SALES_COMMISSION_ID,
    sc.EXT_COMPANY_GROSS_AMOUNT                                   AS GROSS,
    sc.EXT_COMPANY_NET_AMOUNT                                     AS STORED_NET,
    CAST(CASE sc.SALES_COMMISSION_ID
             WHEN '11119' THEN 237368.09
             WHEN '11116' THEN 194707.38
         END AS DECIMAL(20, 2))                                   AS EXPECTED_NET,
    p.PAYMENT_ID,
    p.AMOUNT                                                      AS PAYMENT_AMOUNT,
    p.STATUS_ID                                                   AS PAYMENT_STATUS,
    (p.AMOUNT = ROUND(sc.EXT_COMPANY_NET_AMOUNT, 0))              AS MATCHES_PAYMENT
FROM SALES_COMMISSION sc
LEFT JOIN PAYMENT p
       ON p.SALES_REQUEST_ID  = sc.SALES_REQUEST_ID
      AND p.PARTY_ID_TO       = sc.EXT_COMPANY_PARTY_ID
      AND p.PAYMENT_TYPE_ID   = 'COMMISSION_PAYMENT'
WHERE sc.SALES_COMMISSION_ID IN ('11119', '11116');

-- Step 4. If step 3 looks right:
COMMIT;
-- If anything is off:
-- ROLLBACK;


-- --------------------------------------------------------------------------------------------
-- Step 5. Post-commit confirmation. The exposure query should now come back empty.
--   (Query 1 of vat_exempt_wht_exposure_2026_08_25.sql, inlined here for convenience.)
-- --------------------------------------------------------------------------------------------
SELECT COUNT(*) AS REMAINING_EXPOSURE
FROM SALES_COMMISSION sc
WHERE sc.HAS_VAT_EXEMPTION             = 1
  AND sc.HAS_WITHHOLDING_TAX_EXEMPTION = 0
  AND sc.WITHHOLDING_TAX_PERCENT       > 0
  AND sc.EXT_COMPANY_GROSS_AMOUNT      > 0
  AND sc.EXT_COMPANY_NET_AMOUNT        = sc.EXT_COMPANY_GROSS_AMOUNT
  AND sc.STATUS_ID IN ('COMMISSION_APPROVED', 'COMMISSION_PAID');


-- ============================================================================================
-- ROLLBACK AFTER COMMIT, if ever needed. Restores the pre-fix values verbatim.
--
--   UPDATE SALES_COMMISSION SET EXT_COMPANY_NET_AMOUNT = 249861.15, LAST_UPDATED_STAMP = UTC_TIMESTAMP()
--    WHERE SALES_COMMISSION_ID = '11119';
--   UPDATE SALES_COMMISSION SET EXT_COMPANY_NET_AMOUNT = 204955.14, LAST_UPDATED_STAMP = UTC_TIMESTAMP()
--    WHERE SALES_COMMISSION_ID = '11116';
--
-- Note this restores the *incorrect* nets. It is here for operational safety only.
-- ============================================================================================
