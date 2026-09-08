-- ############################################################################################
-- VOID — DO NOT RUN. Superseded 2026-09-07 by set_broker_tax_invoice_flag_2026_09_07.sql.
--
-- This script applies the "VAT always deducted" rule to every row. The users' workbook shows the
-- VAT is deducted ONLY while the broker's tax invoice is outstanding (رفع الفاتورة الضريبية = لم
-- ترفع). Running this would corrupt 11117 and 11118, which are currently correct.
-- Kept for history only.
-- ############################################################################################

-- ============================================================================================
-- Corrects EXT_COMPANY_NET_AMOUNT on the eight APPROVED commissions that still hold a pre-fix
-- net, under the current rule: both taxes come off what the broker company is paid.
--
--     base = HAS_VAT_EXEMPTION ? gross : gross * 100 / (100 + VAT%)
--     vat  = gross - base
--     wht  = base * (HAS_WITHHOLDING_TAX_EXEMPTION ? 0 : WHT%) / 100
--     net  = gross - vat - wht
--
-- Regenerated 2026-09-07 against current data. Supersedes:
--   * fix_vat_exempt_broker_nets_2026_08_25.sql   (2 rows, pre-VAT-deduction values)
--   * fix_broker_commission_nets_2026_08_26.sql   (9 rows, now stale — see below)
-- Both are marked DO NOT RUN. Run only this one.
--
-- WHAT CHANGED SINCE 2026-08-26, and why regenerating mattered:
--   * 11125 was reset to COMMISSION_PENDING and its VAT exemption switched ON. It has since been
--     recalculated by the fixed code (net 99,560.00) and is CORRECT. The old script asserted
--     87,333.33 for it — a value that is now wrong on both counts.
--   * 11117 was approved on 2026-08-29 through the fixed code. Net 128,822.58, payment O17841
--     128,823 — both CORRECT. It is excluded.
--   * 11118's payment O17775 moved from PMNT_NOT_PAID to PMNT_SENT. Its 24,691 overpayment is
--     now disbursed and can no longer be fixed by editing the payment.
--
-- EXCLUDED, deliberately: 11117 and 11125. Both are already correct.
--
-- DO NOT run this unreviewed. Work through the steps in order and read each result.
-- ============================================================================================


-- --------------------------------------------------------------------------------------------
-- Step 0. Prerequisite, done outside this script, through the payments UI.
--   SIX broker payments are PMNT_NOT_PAID with no AcctgTrans behind them (CreatePayment only
--   posts to the ledger at PMNT_SENT / PMNT_RECEIVED), so editing the amount has no GL impact:
--
--     11116   O17584   204,955 -> 194,707
--     11119   O17606   249,861 -> 237,368
--     11120   O17589   256,864 -> 223,872
--     11121   O17600   219,416 -> 191,234
--     11122   O17595   176,562 -> 153,884
--     11123   O17611   194,324 -> 169,365
--
--   TWO are already disbursed (PMNT_SENT) and CANNOT be edited. This script still corrects their
--   entitlement; the overpayment then stands as a recoverable balance — see step 5:
--
--     11118   O17775   paid 192,240  vs entitlement 167,549   -> over by 24,691
--     11124   O17762   paid 104,561  vs entitlement  91,131   -> over by 13,430
--                                                       total    over by 38,121
--
--   Never reset a commission to "fix" these. CommissionPaymentCleanup.PurgeAsync deletes every
--   COMMISSION_PAYMENT on the sales request, disbursed ones included.
-- --------------------------------------------------------------------------------------------


-- --------------------------------------------------------------------------------------------
-- Step 1. PRE-CHECK. Expect exactly 8 rows. Confirm NEW_NET against the figures below.
--         If 0 rows come back, the correction is already applied — stop.
--
--           11116  194,707.38      11121  191,233.79
--           11118  167,548.82      11122  153,884.33
--           11119  237,368.09      11123  169,365.23
--           11120  223,872.48      11124   91,131.00
--         Total reduction across the eight: 169,672.41
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
      ('11116','11118','11119','11120','11121','11122','11123','11124')
  AND sc.STATUS_ID = 'COMMISSION_APPROVED'
ORDER BY REDUCTION DESC;


-- --------------------------------------------------------------------------------------------
-- Step 2. THE UPDATE.
--   The new value is derived from each row rather than typed in, so it cannot be mistranscribed.
--
--   The 0.02 tolerance in the WHERE is deliberate, not sloppiness. C# decimal and MySQL DECIMAL
--   round this expression differently at the half-piastre boundary — 11117 sits at exactly
--   .575 and the two engines disagree by 0.01. A tighter tolerance would make this script fight
--   the application over one piastre forever. It also means a value written here may differ by up
--   to 0.01 from what a future recalculation produces. Payments are whole EGP, so unaffected.
--
--   The tolerance also makes the statement idempotent: a second run matches nothing.
--   Expect "8 rows affected". Anything else: ROLLBACK and re-check step 1.
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
      ('11116','11118','11119','11120','11121','11122','11123','11124')
  AND sc.STATUS_ID = 'COMMISSION_APPROVED'
  AND ABS(sc.EXT_COMPANY_NET_AMOUNT
          - ROUND(IF(sc.HAS_VAT_EXEMPTION = 1,
                     sc.EXT_COMPANY_GROSS_AMOUNT,
                     sc.EXT_COMPANY_GROSS_AMOUNT * 100
                       / (100 + IF(sc.VAT_PERCENT > 0, sc.VAT_PERCENT, 14)))
                  * (1 - IF(sc.HAS_WITHHOLDING_TAX_EXEMPTION = 1, 0, sc.WITHHOLDING_TAX_PERCENT) / 100),
                  2)) > 0.02;


-- Step 3. VERIFY BEFORE COMMITTING, with the transaction still open.
--   STORED_NET must equal EXPECTED_NET on all eight rows.
--   MATCHES_PAYMENT: 1 for the six edited in step 0, 0 for 11118 and 11124 (already disbursed).
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
      ('11116','11118','11119','11120','11121','11122','11123','11124')
ORDER BY sc.SALES_COMMISSION_ID;

-- Step 4. If step 3 looks right:
COMMIT;
-- If anything is off:
-- ROLLBACK;


-- --------------------------------------------------------------------------------------------
-- Step 5. Post-commit. Every broker payment that no longer matches its entitlement.
--   After step 0 and this script, exactly TWO rows should remain — 11118 / O17775 and
--   11124 / O17762, both PMNT_SENT, 38,121 overpaid in total and already out the door.
--   Real recoverable balances, not data errors. Book them as a recovery from the broker or an
--   adjusting entry; do NOT reset the commissions.
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


-- --------------------------------------------------------------------------------------------
-- Step 6. Whole-set confirmation. Lists every commission with its correct net, so the two that
--   were already right (11117, 11125) can be seen to be untouched and still right.
--   GAP should be 0.00 everywhere, or 0.01 on 11117 for the rounding reason in step 2.
-- --------------------------------------------------------------------------------------------
SELECT
    sc.SALES_COMMISSION_ID,
    sc.STATUS_ID,
    sc.EXT_COMPANY_GROSS_AMOUNT                                   AS GROSS,
    sc.EXT_COMPANY_NET_AMOUNT                                     AS STORED_NET,
    ROUND(sc.EXT_COMPANY_NET_AMOUNT
          - IF(sc.HAS_VAT_EXEMPTION = 1,
               sc.EXT_COMPANY_GROSS_AMOUNT,
               sc.EXT_COMPANY_GROSS_AMOUNT * 100
                 / (100 + IF(sc.VAT_PERCENT > 0, sc.VAT_PERCENT, 14)))
            * (1 - IF(sc.HAS_WITHHOLDING_TAX_EXEMPTION = 1, 0, sc.WITHHOLDING_TAX_PERCENT) / 100),
          2)                                                      AS GAP
FROM SALES_COMMISSION sc
WHERE sc.EXT_COMPANY_GROSS_AMOUNT > 0
ORDER BY ABS(GAP) DESC, sc.SALES_COMMISSION_ID;


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
--
-- These restore the pre-fix (incorrect) nets. Operational safety only.
-- ============================================================================================
