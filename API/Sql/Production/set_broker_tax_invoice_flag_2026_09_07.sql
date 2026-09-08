-- ============================================================================================
-- PRODUCTION data script for the رفع الفاتورة الضريبية refactor.
--
-- Classifies every existing broker commission against the new EXT_COMPANY_TAX_INVOICE_RAISED
-- column, untangles the three rows where إعفاء من ضريبة القيمة المضافة was ticked as a stand-in
-- for it, and recomputes the two approved nets that are wrong.
--
-- The rule the application now implements (matching the users' workbook, sheet "CO 2"):
--     base = gross * 100 / (100 + VAT%)        -- always; there is no VAT exemption in this business
--     wht  = base * WHT%
--     net  = EXT_COMPANY_TAX_INVOICE_RAISED ? gross - wht        (تم رفعها)
--                                           : gross - wht - vat  (لم ترفع)
--
-- RUN ORDER
--   1. Deploy the application (backend + frontend).
--   2. Apply migration 20260907160152_AddExtCompanyTaxInvoiceRaised.
--   3. Run this script.
--   4. Edit two payments in the UI — see step 6.
--
-- Verified end to end on the dev copy of production taken 2026-09-07: all ten commissions agree
-- with the users' sheet afterwards.
--
-- Supersedes and replaces fix_vat_exempt_broker_nets_2026_08_25.sql,
-- fix_broker_commission_nets_2026_08_26.sql and fix_broker_commission_nets_2026_09_07.sql,
-- all of which are banner-marked DO NOT RUN.
-- ============================================================================================


-- --------------------------------------------------------------------------------------------
-- Step 1. PRE-CHECK. Read this before running anything else.
--
--   EXPECTED_INV is what step 2 will set. Confirm against the users' sheet column
--   رفع الفاتورة الضريبية before proceeding:
--       لم ترفع  (0) — 11116, 11117
--       تم رفعها (1) — 11118, 11119, 11120, 11121, 11122, 11123, 11124, 11125
--
--   UNLISTED = 'CHECK ME' flags any broker commission this script does not classify. The new
--   column defaults to 0 (لم ترفع), so an unlisted row will have VAT deducted the next time it is
--   saved or approved. Production must return zero such rows — if it does not, STOP and classify
--   them before continuing.
-- --------------------------------------------------------------------------------------------
SELECT
    sc.SALES_COMMISSION_ID,
    sc.STATUS_ID,
    br.DESCRIPTION                     AS BROKER,
    sc.HAS_VAT_EXEMPTION               AS VAT_EX_NOW,
    sc.VAT_PERCENT,
    sc.EXT_COMPANY_TAX_INVOICE_RAISED  AS INV_NOW,
    CASE WHEN sc.SALES_COMMISSION_ID IN ('11116','11117') THEN 0
         WHEN sc.SALES_COMMISSION_ID IN ('11118','11119','11120','11121',
                                         '11122','11123','11124','11125') THEN 1
    END                                AS EXPECTED_INV,
    sc.EXT_COMPANY_GROSS_AMOUNT        AS GROSS,
    sc.EXT_COMPANY_NET_AMOUNT          AS NET_NOW,
    CASE WHEN sc.SALES_COMMISSION_ID NOT IN
              ('11116','11117','11118','11119','11120','11121','11122','11123','11124','11125')
         THEN 'CHECK ME' END           AS UNLISTED
FROM SALES_COMMISSION sc
LEFT JOIN PARTY br ON br.PARTY_ID = sc.EXT_COMPANY_PARTY_ID
WHERE sc.EXT_COMPANY_GROSS_AMOUNT > 0
ORDER BY sc.SALES_COMMISSION_ID;


-- --------------------------------------------------------------------------------------------
-- Step 2. Set the flags.
--
--   A broker commission always carries VAT — the users' sheet fills in اصل العمولة بدون الضريبة for
--   every row. HAS_VAT_EXEMPTION is now DEPRECATED and ignored by the application; it is zeroed here
--   only so nobody reads a stale 1 and misreads history. VAT_PERCENT is restored to 14 on the rows
--   the old "zero it when exempt" behaviour left at 0 (11116, 11119) — that one still matters,
--   because it is the divisor for the base.
-- --------------------------------------------------------------------------------------------
START TRANSACTION;

-- لم ترفع — tax invoice still outstanding, VAT withheld
UPDATE SALES_COMMISSION
SET EXT_COMPANY_TAX_INVOICE_RAISED = 0,
    HAS_VAT_EXEMPTION              = 0,
    VAT_PERCENT                    = 14.0000,
    LAST_UPDATED_STAMP             = UTC_TIMESTAMP()
WHERE SALES_COMMISSION_ID IN ('11116','11117');

-- تم رفعها — tax invoice issued, VAT paid over to the broker
UPDATE SALES_COMMISSION
SET EXT_COMPANY_TAX_INVOICE_RAISED = 1,
    HAS_VAT_EXEMPTION              = 0,
    VAT_PERCENT                    = 14.0000,
    LAST_UPDATED_STAMP             = UTC_TIMESTAMP()
WHERE SALES_COMMISSION_ID IN ('11118','11119','11120','11121','11122','11123','11124','11125');


-- --------------------------------------------------------------------------------------------
-- Step 3. Recompute the stored net on APPROVED rows that need it.
--
--   Approved commissions are never recalculated by the application, so their net is set here.
--   Only 11116 and 11119 should change — the other seven approved rows already hold the correct
--   figure. 11125 is COMMISSION_PENDING and is deliberately left alone: it recalculates itself
--   (to 100,203.51) on the next save.
--
--   The 0.02 tolerance keeps this idempotent and stops it fighting the application over the
--   half-piastre where C# decimal and MySQL DECIMAL round differently (11117 sits exactly there).
--
--   Restricted to the ten classified IDs on purpose: an unlisted row would have an unset invoice
--   flag, and recomputing it here would write a number nobody has checked. VAT_PERCENT > 0 guards
--   the divisor — a row still holding 0 would make base = gross and produce a wrong net.
--   Expect "2 rows affected".
-- --------------------------------------------------------------------------------------------
UPDATE SALES_COMMISSION sc
SET sc.EXT_COMPANY_NET_AMOUNT = ROUND(
        sc.EXT_COMPANY_GROSS_AMOUNT
        - sc.EXT_COMPANY_GROSS_AMOUNT * 100 / (100 + sc.VAT_PERCENT)
          * (IF(sc.HAS_WITHHOLDING_TAX_EXEMPTION = 1, 0, sc.WITHHOLDING_TAX_PERCENT) / 100)
        - IF(sc.EXT_COMPANY_TAX_INVOICE_RAISED = 1, 0,
             sc.EXT_COMPANY_GROSS_AMOUNT
             - sc.EXT_COMPANY_GROSS_AMOUNT * 100 / (100 + sc.VAT_PERCENT)),
    2),
    sc.LAST_UPDATED_STAMP = UTC_TIMESTAMP()
WHERE sc.SALES_COMMISSION_ID IN
      ('11116','11117','11118','11119','11120','11121','11122','11123','11124','11125')
  AND sc.STATUS_ID = 'COMMISSION_APPROVED'
  AND sc.EXT_COMPANY_GROSS_AMOUNT > 0
  AND sc.VAT_PERCENT > 0
  AND ABS(sc.EXT_COMPANY_NET_AMOUNT - ROUND(
        sc.EXT_COMPANY_GROSS_AMOUNT
        - sc.EXT_COMPANY_GROSS_AMOUNT * 100 / (100 + sc.VAT_PERCENT)
          * (IF(sc.HAS_WITHHOLDING_TAX_EXEMPTION = 1, 0, sc.WITHHOLDING_TAX_PERCENT) / 100)
        - IF(sc.EXT_COMPANY_TAX_INVOICE_RAISED = 1, 0,
             sc.EXT_COMPANY_GROSS_AMOUNT
             - sc.EXT_COMPANY_GROSS_AMOUNT * 100 / (100 + sc.VAT_PERCENT)),
    2)) > 0.02;


-- --------------------------------------------------------------------------------------------
-- Step 4. VERIFY BEFORE COMMITTING, with the transaction still open.
--
--   AGREES must read OK on every approved row. 11125 will read PENDING — that is expected and
--   correct; it recalculates on its next save.
-- --------------------------------------------------------------------------------------------
SELECT
    sc.SALES_COMMISSION_ID,
    sc.STATUS_ID,
    sc.EXT_COMPANY_TAX_INVOICE_RAISED AS INV,
    sc.EXT_COMPANY_GROSS_AMOUNT       AS GROSS,
    sc.EXT_COMPANY_NET_AMOUNT         AS NET,
    ROUND(sc.EXT_COMPANY_GROSS_AMOUNT
          - sc.EXT_COMPANY_GROSS_AMOUNT * 100 / (100 + sc.VAT_PERCENT)
            * (IF(sc.HAS_WITHHOLDING_TAX_EXEMPTION = 1, 0, sc.WITHHOLDING_TAX_PERCENT) / 100)
          - IF(sc.EXT_COMPANY_TAX_INVOICE_RAISED = 1, 0,
               sc.EXT_COMPANY_GROSS_AMOUNT
               - sc.EXT_COMPANY_GROSS_AMOUNT * 100 / (100 + sc.VAT_PERCENT)),
      2)                              AS SHOULD_BE,
    CASE
        WHEN sc.STATUS_ID <> 'COMMISSION_APPROVED' THEN 'PENDING'
        WHEN ABS(sc.EXT_COMPANY_NET_AMOUNT - ROUND(
                 sc.EXT_COMPANY_GROSS_AMOUNT
                 - sc.EXT_COMPANY_GROSS_AMOUNT * 100 / (100 + sc.VAT_PERCENT)
                   * (IF(sc.HAS_WITHHOLDING_TAX_EXEMPTION = 1, 0, sc.WITHHOLDING_TAX_PERCENT) / 100)
                 - IF(sc.EXT_COMPANY_TAX_INVOICE_RAISED = 1, 0,
                      sc.EXT_COMPANY_GROSS_AMOUNT
                      - sc.EXT_COMPANY_GROSS_AMOUNT * 100 / (100 + sc.VAT_PERCENT)),
             2)) <= 0.02 THEN 'OK'
        ELSE 'MISMATCH'
    END                               AS AGREES
FROM SALES_COMMISSION sc
WHERE sc.EXT_COMPANY_GROSS_AMOUNT > 0
ORDER BY sc.SALES_COMMISSION_ID;

-- Step 5. If step 4 shows OK everywhere (and PENDING on 11125):
COMMIT;
-- If anything reads MISMATCH:
-- ROLLBACK;


-- --------------------------------------------------------------------------------------------
-- Step 6. Payment edits — done in the payments UI, not here.
--
--   Two broker payments no longer match their entitlement. Both are PMNT_NOT_PAID with no
--   AcctgTrans behind them (CreatePayment only posts to the ledger at PMNT_SENT / PMNT_RECEIVED),
--   so editing the amount has no GL impact:
--
--       11116   O17584   204,955 -> 170,796
--       11119   O17606   249,861 -> 238,902
--
--   Never reset a commission to "recalculate" it. CommissionPaymentCleanup.PurgeAsync deletes
--   every COMMISSION_PAYMENT on the sales request — disbursed ones included, along with the
--   manual adjustments the users have added by hand.
-- --------------------------------------------------------------------------------------------


-- --------------------------------------------------------------------------------------------
-- Step 7. Post-commit reconciliation. Run after the two payment edits.
--   Should return ZERO rows. Anything listed is a payment that disagrees with its entitlement.
-- --------------------------------------------------------------------------------------------
SELECT
    sc.SALES_COMMISSION_ID,
    sc.EXT_COMPANY_NET_AMOUNT                      AS ENTITLEMENT,
    p.PAYMENT_ID,
    p.AMOUNT                                       AS PAID,
    p.STATUS_ID                                    AS PAYMENT_STATUS,
    ROUND(p.AMOUNT - sc.EXT_COMPANY_NET_AMOUNT, 2) AS DIFFERENCE
FROM SALES_COMMISSION sc
JOIN PAYMENT p
       ON p.SALES_REQUEST_ID = sc.SALES_REQUEST_ID
      AND p.PARTY_ID_TO      = sc.EXT_COMPANY_PARTY_ID
      AND p.PAYMENT_TYPE_ID  = 'COMMISSION_PAYMENT'
WHERE ABS(p.AMOUNT - ROUND(sc.EXT_COMPANY_NET_AMOUNT, 0)) > 0.5
ORDER BY DIFFERENCE DESC;


-- ============================================================================================
-- EXPECTED FINAL STATE — cross-checked against the users' sheet
--
--     ID      INV   GROSS        NET
--     11116    0    204,955.14   170,795.95   (لم ترفع)
--     11117    0    154,587.09   128,822.58   (لم ترفع)
--     11118    1    201,058.59   192,240.23
--     11119    1    249,861.15   238,902.33
--     11120    1    268,646.98   256,864.22
--     11121    1    229,480.55   219,415.61
--     11122    1    184,661.19   176,562.02
--     11123    1    203,238.28   194,324.32
--     11124    1    109,357.20   104,560.83
--     11125    1    104,800.00    99,560.00 -> 100,203.51 on its next save (pending)
--
-- ROLLBACK AFTER COMMIT, if ever needed:
--   UPDATE SALES_COMMISSION SET HAS_VAT_EXEMPTION=1, VAT_PERCENT=0,  EXT_COMPANY_NET_AMOUNT=204955.14 WHERE SALES_COMMISSION_ID='11116';
--   UPDATE SALES_COMMISSION SET HAS_VAT_EXEMPTION=1, VAT_PERCENT=0,  EXT_COMPANY_NET_AMOUNT=249861.15 WHERE SALES_COMMISSION_ID='11119';
--   UPDATE SALES_COMMISSION SET HAS_VAT_EXEMPTION=1, VAT_PERCENT=14, EXT_COMPANY_NET_AMOUNT=99560.00  WHERE SALES_COMMISSION_ID='11125';
--   UPDATE SALES_COMMISSION SET EXT_COMPANY_TAX_INVOICE_RAISED=0
--    WHERE SALES_COMMISSION_ID IN ('11116','11117','11118','11119','11120','11121','11122','11123','11124','11125');
-- ============================================================================================
