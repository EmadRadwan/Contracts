-- ============================================================================================
-- Corrects EXT_COMPANY_NET_AMOUNT on the nine APPROVED commissions, under the current rule:
-- both taxes come off what the broker company is paid.
--
--     base = HAS_VAT_EXEMPTION ? gross : gross * 100 / (100 + VAT%)
--     net  = base * (1 - (HAS_WITHHOLDING_TAX_EXEMPTION ? 0 : WHT%) / 100)
--
-- Two separate corrections are folded in here:
--   * WHT was skipped entirely for VAT-exempt brokers (11116, 11119) — fixed 2026-08-25.
--   * VAT is now deducted as well as WHT (the other seven)          — changed 2026-08-26.
-- Both are already live in SalesCommissionCalculator; an approved commission is never
-- recalculated and is not editable in the UI, hence this script.
--
-- Supersedes fix_vat_exempt_broker_nets_2026_08_25.sql, which covered only 11116 and 11119.
-- Do not run both.
--
-- NOT INCLUDED: 11117. It is COMMISSION_PENDING, so saving or approving it recalculates through
-- the fixed code on its own. Touching it here would be redundant.
--
-- DO NOT run this unreviewed. Work through the steps in order and read each result.
-- ============================================================================================


-- --------------------------------------------------------------------------------------------
-- Step 0. Prerequisite, done outside this script, through the payments UI.
--   Eight broker payments are PMNT_NOT_PAID with no AcctgTrans behind them (CreatePayment only
--   posts to the ledger at PMNT_SENT / PMNT_RECEIVED), so editing the amount has no GL impact:
--
--     11116   O17584   204,955 -> 194,707
--     11118   O17775   192,240 -> 167,549
--     11119   O17606   249,861 -> 237,368
--     11120   O17589   256,864 -> 223,872
--     11121   O17600   219,416 -> 191,234
--     11122   O17595   176,562 -> 153,884
--     11123   O17611   194,324 -> 169,365
--     11125   O17799   100,204 ->  87,333
--
--   11124 / O17762 is PMNT_SENT — 104,561 already disbursed against a corrected entitlement of
--   91,131. It CANNOT be edited and must not be reset (reset purges every COMMISSION_PAYMENT on
--   the sales request, disbursed ones included). This script still corrects its entitlement; the
--   13,430 overpayment then stands as a recoverable balance. See step 5.
-- --------------------------------------------------------------------------------------------


-- --------------------------------------------------------------------------------------------
-- Step 1. PRE-CHECK. Expect exactly 9 rows. Confirm NEW_NET against the figures below before
--         running step 2. If 0 rows come back, the correction is already applied — stop.
--
--           11116  194,707.38      11121  191,233.79
--           11118  167,548.82      11122  153,884.33
--           11119  237,368.09      11123  169,365.23
--           11120  223,872.48      11124   91,131.00
--                                  11125   87,333.33
--         Total reduction across the nine: 182,542.59
-- --------------------------------------------------------------------------------------------
SELECT
    sc.SALES_COMMISSION_ID,
    sc.SALES_REQUEST_ID,
    sc.STATUS_ID,
    sc.HAS_VAT_EXEMPTION                                          AS VAT_EX,
    sc.HAS_WITHHOLDING_TAX_EXEMPTION                              AS WHT_EX,
    sc.VAT_PERCENT,
    sc.WITHHOLDING_TAX_PERCENT                                    AS WHT_PCT,
    sc.EXT_COMPANY_GROSS_AMOUNT                                   AS GROSS,
    sc.EXT_COMPANY_NET_AMOUNT                                     AS CURRENT_NET,
    ROUND(IF(sc.HAS_VAT_EXEMPTION = 1,
             sc.EXT_COMPANY_GROSS_AMOUNT,
             sc.EXT_COMPANY_GROSS_AMOUNT * 100
               / (100 + IF(sc.VAT_PERCENT > 0, sc.VAT_PERCENT, 14)))
          * (1 - IF(sc.HAS_WITHHOLDING_TAX_EXEMPTION = 1, 0, sc.WITHHOLDING_TAX_PERCENT) / 100),
          2)                                                      AS NEW_NET,
    ROUND(sc.EXT_COMPANY_NET_AMOUNT
          - IF(sc.HAS_VAT_EXEMPTION = 1,
               sc.EXT_COMPANY_GROSS_AMOUNT,
               sc.EXT_COMPANY_GROSS_AMOUNT * 100
                 / (100 + IF(sc.VAT_PERCENT > 0, sc.VAT_PERCENT, 14)))
            * (1 - IF(sc.HAS_WITHHOLDING_TAX_EXEMPTION = 1, 0, sc.WITHHOLDING_TAX_PERCENT) / 100),
          2)                                                      AS REDUCTION
FROM SALES_COMMISSION sc
WHERE sc.SALES_COMMISSION_ID IN
      ('11116','11118','11119','11120','11121','11122','11123','11124','11125')
  AND sc.STATUS_ID = 'COMMISSION_APPROVED'
ORDER BY REDUCTION DESC;


-- --------------------------------------------------------------------------------------------
-- Step 2. THE UPDATE.
--   The new value is derived from each row rather than typed in, so it cannot be mistranscribed.
--   The WHERE requires the stored net to actually differ from the computed one, which makes the
--   statement idempotent: a second run matches nothing.
--   Expect "9 rows affected". Anything else: ROLLBACK and re-check step 1.
-- --------------------------------------------------------------------------------------------
START TRANSACTION;

UPDATE SALES_COMMISSION sc
SET sc.EXT_COMPANY_NET_AMOUNT =
        ROUND(IF(sc.HAS_VAT_EXEMPTION = 1,
                 sc.EXT_COMPANY_GROSS_AMOUNT,
                 sc.EXT_COMPANY_GROSS_AMOUNT * 100
                   / (100 + IF(sc.VAT_PERCENT > 0, sc.VAT_PERCENT, 14)))
              * (1 - IF(sc.HAS_WITHHOLDING_TAX_EXEMPTION = 1, 0, sc.WITHHOLDING_TAX_PERCENT) / 100),
              2),
    sc.LAST_UPDATED_STAMP = UTC_TIMESTAMP()
WHERE sc.SALES_COMMISSION_ID IN
      ('11116','11118','11119','11120','11121','11122','11123','11124','11125')
  AND sc.STATUS_ID = 'COMMISSION_APPROVED'
  AND ABS(sc.EXT_COMPANY_NET_AMOUNT
          - ROUND(IF(sc.HAS_VAT_EXEMPTION = 1,
                     sc.EXT_COMPANY_GROSS_AMOUNT,
                     sc.EXT_COMPANY_GROSS_AMOUNT * 100
                       / (100 + IF(sc.VAT_PERCENT > 0, sc.VAT_PERCENT, 14)))
                  * (1 - IF(sc.HAS_WITHHOLDING_TAX_EXEMPTION = 1, 0, sc.WITHHOLDING_TAX_PERCENT) / 100),
                  2)) > 0.005;


-- Step 3. VERIFY BEFORE COMMITTING, with the transaction still open.
--   STORED_NET must equal EXPECTED_NET on all nine rows.
--   MATCHES_PAYMENT should read 1 for the eight edited in step 0, and 0 for 11124.
SELECT
    sc.SALES_COMMISSION_ID,
    sc.EXT_COMPANY_GROSS_AMOUNT                       AS GROSS,
    sc.EXT_COMPANY_NET_AMOUNT                         AS STORED_NET,
    CAST(CASE sc.SALES_COMMISSION_ID
             WHEN '11116' THEN 194707.38
             WHEN '11118' THEN 167548.82
             WHEN '11119' THEN 237368.09
             WHEN '11120' THEN 223872.48
             WHEN '11121' THEN 191233.79
             WHEN '11122' THEN 153884.33
             WHEN '11123' THEN 169365.23
             WHEN '11124' THEN  91131.00
             WHEN '11125' THEN  87333.33
         END AS DECIMAL(20, 2))                       AS EXPECTED_NET,
    p.PAYMENT_ID,
    p.AMOUNT                                          AS PAYMENT_AMOUNT,
    p.STATUS_ID                                       AS PAYMENT_STATUS,
    (p.AMOUNT = ROUND(sc.EXT_COMPANY_NET_AMOUNT, 0))  AS MATCHES_PAYMENT
FROM SALES_COMMISSION sc
LEFT JOIN PAYMENT p
       ON p.SALES_REQUEST_ID = sc.SALES_REQUEST_ID
      AND p.PARTY_ID_TO      = sc.EXT_COMPANY_PARTY_ID
      AND p.PAYMENT_TYPE_ID  = 'COMMISSION_PAYMENT'
WHERE sc.SALES_COMMISSION_ID IN
      ('11116','11118','11119','11120','11121','11122','11123','11124','11125')
ORDER BY sc.SALES_COMMISSION_ID;

-- Step 4. If step 3 looks right:
COMMIT;
-- If anything is off:
-- ROLLBACK;


-- --------------------------------------------------------------------------------------------
-- Step 5. Post-commit. Surfaces every broker payment that no longer matches its entitlement.
--   After step 0 and this script, exactly ONE row should remain: 11124 / O17762, PMNT_SENT,
--   paid 104,561 against an entitlement of 91,131 — a 13,430 overpayment already out the door.
--   That is a real recoverable balance, not a data error. Decide how to book it (recovery from
--   the broker, or an adjusting entry); do NOT reset the commission to "fix" it.
-- --------------------------------------------------------------------------------------------
SELECT
    sc.SALES_COMMISSION_ID,
    sc.EXT_COMPANY_NET_AMOUNT                            AS ENTITLEMENT,
    p.PAYMENT_ID,
    p.AMOUNT                                             AS PAID,
    p.STATUS_ID                                          AS PAYMENT_STATUS,
    ROUND(p.AMOUNT - sc.EXT_COMPANY_NET_AMOUNT, 2)       AS OVERPAID
FROM SALES_COMMISSION sc
JOIN PAYMENT p
       ON p.SALES_REQUEST_ID = sc.SALES_REQUEST_ID
      AND p.PARTY_ID_TO      = sc.EXT_COMPANY_PARTY_ID
      AND p.PAYMENT_TYPE_ID  = 'COMMISSION_PAYMENT'
WHERE ABS(p.AMOUNT - ROUND(sc.EXT_COMPANY_NET_AMOUNT, 0)) > 0.5
ORDER BY OVERPAID DESC;


-- ============================================================================================
-- ROLLBACK AFTER COMMIT, if ever needed. Restores the pre-correction values verbatim.
--
--   UPDATE SALES_COMMISSION SET EXT_COMPANY_NET_AMOUNT = 204955.14 WHERE SALES_COMMISSION_ID = '11116';
--   UPDATE SALES_COMMISSION SET EXT_COMPANY_NET_AMOUNT = 192240.23 WHERE SALES_COMMISSION_ID = '11118';
--   UPDATE SALES_COMMISSION SET EXT_COMPANY_NET_AMOUNT = 249861.15 WHERE SALES_COMMISSION_ID = '11119';
--   UPDATE SALES_COMMISSION SET EXT_COMPANY_NET_AMOUNT = 256864.22 WHERE SALES_COMMISSION_ID = '11120';
--   UPDATE SALES_COMMISSION SET EXT_COMPANY_NET_AMOUNT = 219415.61 WHERE SALES_COMMISSION_ID = '11121';
--   UPDATE SALES_COMMISSION SET EXT_COMPANY_NET_AMOUNT = 176562.02 WHERE SALES_COMMISSION_ID = '11122';
--   UPDATE SALES_COMMISSION SET EXT_COMPANY_NET_AMOUNT = 194324.32 WHERE SALES_COMMISSION_ID = '11123';
--   UPDATE SALES_COMMISSION SET EXT_COMPANY_NET_AMOUNT = 104560.83 WHERE SALES_COMMISSION_ID = '11124';
--   UPDATE SALES_COMMISSION SET EXT_COMPANY_NET_AMOUNT = 100203.51 WHERE SALES_COMMISSION_ID = '11125';
--
-- These restore the pre-fix (incorrect) nets. Operational safety only.
-- ============================================================================================
