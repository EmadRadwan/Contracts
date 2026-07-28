ixing scripts-- ============================================================================
-- Clears the 80.00 stranded in GL 140000, and the 160.00 it was masking.
--
-- WHAT THIS IS NOT
-- Earlier analysis called this a "79.99 landed-cost residual" caused by landed
-- costs being sourced twice (ORDER_ADJUSTMENT for receipts vs the certificate
-- item's Transportation/Gratuities/Discount columns for issuances). That framing
-- was wrong on both counts. The figure is exactly 80.00 -- 79.99 came from
-- comparing two rounded aggregates -- and the two sources do NOT systematically
-- disagree: across all 95 supply certificates exactly ONE drifts.
--
-- WHAT ACTUALLY HAPPENED -- certificate 100-0005 (WE 10201, PO10726, Feb 2026)
-- Line 10202 carries Discount = 80.00 (TOTAL_AMOUNT 16,720 = 16,800 - 80), but
-- PO10726 never got the matching CERTIFICATE_DISCOUNT_ADJUSTMENT row. The code
-- that writes those (CreateProjectCertificate.cs ~line 310) did not exist yet --
-- the earliest certificate carrying adjustments is 2026-03-12, five weeks later.
-- So NO CODE FIX IS NEEDED: certificates created since then are all consistent.
--
-- Two errors followed, pulling in opposite directions, which is why only 80 of
-- the 160 was visible:
--
--   A. Receipts 11369/11371 valued inventory off the un-discounted PO: they
--      debited 140000 with 17,280 while issuance 12258/12260 credited it with
--      17,200. Remediation 18747 later swept the receipt's AP credit (17,280)
--      into 214000, carrying the bad 80 with it.
--
--   B. Purchase invoice 18032 booked the discount with the WRONG SIGN -- entry
--      03, described خصم, is a DEBIT of 80 where it should be a credit. That
--      made the invoice 17,360 instead of 17,200: 160 too high, against a
--      supplier who was invoiced and paid 17,200 (payment 11548).
--
-- END STATE -- four accounts, all reconciling to zero:
--   GL 140000  +80.00       -> 0.00
--   GL 124420  3,458,376.00 -> 3,458,216.00
--   GL 214000  -3,458,296.00 -> -3,458,216.00   (124420 + 214000 = 0)
--   AP 210001 for WE 10201  -160.00 -> 0.00
--
-- Corrected in place, no new ACCTG_TRANS ids minted, every transaction left
-- balanced. Consistent with the in-place approach used by the other fixes in
-- this folder.
--
-- STATUS
--   localhost   APPLIED 2026-07-28. Verified after commit: GL 140000 = 0.00,
--               124420 + 214000 offset to 0.00, AP 210001 for WE 10201 = 0.00,
--               0 certificates drifting, 0 newly unbalanced transactions.
--   production  NOT APPLIED. Run the BEFORE queries first -- this is a
--               single-certificate legacy defect and production may differ.
--
-- REVIEW BEFORE RUNNING. Take a backup first:
--   mysqldump erp_contracts ACCTG_TRANS_ENTRY ORDER_ADJUSTMENT \
--     > backup_before_discount_fix.sql
-- ============================================================================

START TRANSACTION;

-- ---------------------------------------------------------------------------
-- BEFORE 1: the four balances -- expect 80.00 / 3458376.00 / -3458296.00 / -160.00
-- ---------------------------------------------------------------------------
SELECT 'BEFORE' phase, 'GL 140000' label,
       ROUND(SUM(CASE WHEN ate.DEBIT_CREDIT_FLAG='D' THEN ate.AMOUNT ELSE -ate.AMOUNT END),2) balance
FROM ACCTG_TRANS act JOIN ACCTG_TRANS_ENTRY ate ON ate.ACCTG_TRANS_ID=act.ACCTG_TRANS_ID AND ate.GL_ACCOUNT_ID='140000'
WHERE act.IS_POSTED='Y'
UNION ALL
SELECT 'BEFORE', 'GL 124420',
       ROUND(SUM(CASE WHEN ate.DEBIT_CREDIT_FLAG='D' THEN ate.AMOUNT ELSE -ate.AMOUNT END),2)
FROM ACCTG_TRANS act JOIN ACCTG_TRANS_ENTRY ate ON ate.ACCTG_TRANS_ID=act.ACCTG_TRANS_ID AND ate.GL_ACCOUNT_ID='124420'
WHERE act.IS_POSTED='Y'
UNION ALL
SELECT 'BEFORE', 'GL 214000',
       ROUND(SUM(CASE WHEN ate.DEBIT_CREDIT_FLAG='D' THEN ate.AMOUNT ELSE -ate.AMOUNT END),2)
FROM ACCTG_TRANS act JOIN ACCTG_TRANS_ENTRY ate ON ate.ACCTG_TRANS_ID=act.ACCTG_TRANS_ID AND ate.GL_ACCOUNT_ID='214000'
WHERE act.IS_POSTED='Y'
UNION ALL
SELECT 'BEFORE', 'AP 210001 for WE 10201',
       ROUND(SUM(CASE WHEN ate.DEBIT_CREDIT_FLAG='D' THEN ate.AMOUNT ELSE -ate.AMOUNT END),2)
FROM ACCTG_TRANS act JOIN ACCTG_TRANS_ENTRY ate ON ate.ACCTG_TRANS_ID=act.ACCTG_TRANS_ID AND ate.GL_ACCOUNT_ID='210001'
WHERE act.IS_POSTED='Y' AND act.WORK_EFFORT_ID='10201';

-- ---------------------------------------------------------------------------
-- BEFORE 2: the detector -- supply certificates whose PO adjustments disagree
-- with their certificate-item add-ons. Expect ONLY WE 10201, drift 80.00.
-- Any other row is a case this script does not cover.
-- ---------------------------------------------------------------------------
SELECT 'DRIFT' flag, hdr.WORK_EFFORT_ID we, hdr.CERTIFICATE_NUMBER cert, hdr.RELATED_ORDER_ID po,
       ROUND(COALESCE(ci.addons,0),2) cert_addons,
       ROUND(COALESCE(oa.adjustments,0),2) order_adjustments,
       ROUND(COALESCE(oa.adjustments,0)-COALESCE(ci.addons,0),2) drift
FROM WORK_EFFORT hdr
LEFT JOIN (SELECT WORK_EFFORT_PARENT_ID p,
                  SUM(COALESCE(TransportationExpenses,0)+COALESCE(Gratuities,0)-COALESCE(Discount,0)) addons
           FROM WORK_EFFORT WHERE WORK_EFFORT_TYPE_ID='CERTIFICATE_ITEM' GROUP BY 1) ci
       ON ci.p = hdr.WORK_EFFORT_ID
LEFT JOIN (SELECT ORDER_ID o, SUM(AMOUNT) adjustments FROM ORDER_ADJUSTMENT GROUP BY 1) oa
       ON oa.o = hdr.RELATED_ORDER_ID
WHERE hdr.CERTIFICATE_CATEGORY='SUPPLY_PROCUREMENT_CERTIFICATE'
  AND ABS(COALESCE(oa.adjustments,0)-COALESCE(ci.addons,0)) > 0.001;

-- ---------------------------------------------------------------------------
-- STEP 1 -- correct invoice 18032: the خصم line becomes a credit, and the AP
-- credit drops from 17,360 to the 17,200 actually invoiced and paid.
-- Debits 16,800 + 480 = 17,280; credits 80 + 17,200 = 17,280. Still balanced.
-- ---------------------------------------------------------------------------

UPDATE ACCTG_TRANS_ENTRY
   SET DEBIT_CREDIT_FLAG = 'C', LAST_UPDATED_STAMP = NOW()
 WHERE ACCTG_TRANS_ID = '18032' AND ACCTG_TRANS_ENTRY_SEQ_ID = '03'
   AND DEBIT_CREDIT_FLAG = 'D' AND AMOUNT = 80.000;

UPDATE ACCTG_TRANS_ENTRY
   SET AMOUNT = 17200.000, ORIG_AMOUNT = 17200.000, LAST_UPDATED_STAMP = NOW()
 WHERE ACCTG_TRANS_ID = '18032' AND ACCTG_TRANS_ENTRY_SEQ_ID = '04'
   AND DEBIT_CREDIT_FLAG = 'C' AND AMOUNT = 17360.000;

-- ---------------------------------------------------------------------------
-- STEP 2 -- value receipt 11369 at the discounted cost (16,800 -> 16,720),
-- both legs, so inventory in matches inventory out.
-- ---------------------------------------------------------------------------

UPDATE ACCTG_TRANS_ENTRY
   SET AMOUNT = 16720.000, ORIG_AMOUNT = 16720.000, LAST_UPDATED_STAMP = NOW()
 WHERE ACCTG_TRANS_ID = '11369' AND AMOUNT = 16800.000;

-- ---------------------------------------------------------------------------
-- STEP 3 -- realign remediation 18747, which swept the receipt's AP credit into
-- 214000, to the corrected receipt total (17,280 -> 17,200), both legs.
-- ---------------------------------------------------------------------------

UPDATE ACCTG_TRANS_ENTRY
   SET AMOUNT = 17200.000, ORIG_AMOUNT = 17200.000, LAST_UPDATED_STAMP = NOW()
 WHERE ACCTG_TRANS_ID = '18747' AND AMOUNT = 17280.000;

-- ---------------------------------------------------------------------------
-- STEP 4 -- add the ORDER_ADJUSTMENT the PO never got, so the source data
-- matches the certificate and anything re-derived from PO10726 is correct.
-- ---------------------------------------------------------------------------

INSERT INTO ORDER_ADJUSTMENT
    (ORDER_ADJUSTMENT_ID, ORDER_ADJUSTMENT_TYPE_ID, ORDER_ID, ORDER_ITEM_SEQ_ID,
     AMOUNT, CORRESPONDING_PRODUCT_ID, COMMENTS,
     CREATED_STAMP, LAST_UPDATED_STAMP)
SELECT UUID(), 'CERTIFICATE_DISCOUNT_ADJUSTMENT', 'PO10726', '0001',
       -80.000, '000059',
       'Backfilled: certificate 100-0005 line 10202 Discount, predates CreateProjectCertificate adjustment logic',
       NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM (SELECT * FROM ORDER_ADJUSTMENT) x
     WHERE x.ORDER_ID = 'PO10726'
       AND x.ORDER_ADJUSTMENT_TYPE_ID = 'CERTIFICATE_DISCOUNT_ADJUSTMENT');

-- ---------------------------------------------------------------------------
-- AFTER: checks 1-2 must return ZERO rows; the balances must all reconcile.
-- ---------------------------------------------------------------------------

-- 1. Every transaction touched still balances.
SELECT 'UNBALANCED' flag, ACCTG_TRANS_ID,
       SUM(CASE WHEN DEBIT_CREDIT_FLAG='D' THEN AMOUNT ELSE -AMOUNT END) diff
FROM ACCTG_TRANS_ENTRY
WHERE ACCTG_TRANS_ID IN ('11369','11371','12258','12260','18032','18747','11548')
GROUP BY ACCTG_TRANS_ID
HAVING ABS(diff) > 0.001;

-- 2. No certificate drifts between its PO adjustments and its item add-ons.
SELECT 'STILL DRIFTING' flag, hdr.WORK_EFFORT_ID we, hdr.CERTIFICATE_NUMBER cert
FROM WORK_EFFORT hdr
LEFT JOIN (SELECT WORK_EFFORT_PARENT_ID p,
                  SUM(COALESCE(TransportationExpenses,0)+COALESCE(Gratuities,0)-COALESCE(Discount,0)) addons
           FROM WORK_EFFORT WHERE WORK_EFFORT_TYPE_ID='CERTIFICATE_ITEM' GROUP BY 1) ci
       ON ci.p = hdr.WORK_EFFORT_ID
LEFT JOIN (SELECT ORDER_ID o, SUM(AMOUNT) adjustments FROM ORDER_ADJUSTMENT GROUP BY 1) oa
       ON oa.o = hdr.RELATED_ORDER_ID
WHERE hdr.CERTIFICATE_CATEGORY='SUPPLY_PROCUREMENT_CERTIFICATE'
  AND ABS(COALESCE(oa.adjustments,0)-COALESCE(ci.addons,0)) > 0.001;

-- 3. The four balances -- expect 0.00 / 3458216.00 / -3458216.00 / 0.00
SELECT 'AFTER' phase, 'GL 140000' label,
       ROUND(SUM(CASE WHEN ate.DEBIT_CREDIT_FLAG='D' THEN ate.AMOUNT ELSE -ate.AMOUNT END),2) balance
FROM ACCTG_TRANS act JOIN ACCTG_TRANS_ENTRY ate ON ate.ACCTG_TRANS_ID=act.ACCTG_TRANS_ID AND ate.GL_ACCOUNT_ID='140000'
WHERE act.IS_POSTED='Y'
UNION ALL
SELECT 'AFTER', 'GL 124420',
       ROUND(SUM(CASE WHEN ate.DEBIT_CREDIT_FLAG='D' THEN ate.AMOUNT ELSE -ate.AMOUNT END),2)
FROM ACCTG_TRANS act JOIN ACCTG_TRANS_ENTRY ate ON ate.ACCTG_TRANS_ID=act.ACCTG_TRANS_ID AND ate.GL_ACCOUNT_ID='124420'
WHERE act.IS_POSTED='Y'
UNION ALL
SELECT 'AFTER', 'GL 214000',
       ROUND(SUM(CASE WHEN ate.DEBIT_CREDIT_FLAG='D' THEN ate.AMOUNT ELSE -ate.AMOUNT END),2)
FROM ACCTG_TRANS act JOIN ACCTG_TRANS_ENTRY ate ON ate.ACCTG_TRANS_ID=act.ACCTG_TRANS_ID AND ate.GL_ACCOUNT_ID='214000'
WHERE act.IS_POSTED='Y'
UNION ALL
SELECT 'AFTER', 'AP 210001 for WE 10201',
       ROUND(SUM(CASE WHEN ate.DEBIT_CREDIT_FLAG='D' THEN ate.AMOUNT ELSE -ate.AMOUNT END),2)
FROM ACCTG_TRANS act JOIN ACCTG_TRANS_ENTRY ate ON ate.ACCTG_TRANS_ID=act.ACCTG_TRANS_ID AND ate.GL_ACCOUNT_ID='210001'
WHERE act.IS_POSTED='Y' AND act.WORK_EFFORT_ID='10201';

-- 4. 124420 and 214000 must now offset exactly -- expect 0.00
SELECT '124420 + 214000' label,
       ROUND(SUM(CASE WHEN ate.DEBIT_CREDIT_FLAG='D' THEN ate.AMOUNT ELSE -ate.AMOUNT END),2) net
FROM ACCTG_TRANS act JOIN ACCTG_TRANS_ENTRY ate ON ate.ACCTG_TRANS_ID=act.ACCTG_TRANS_ID
WHERE act.IS_POSTED='Y' AND ate.GL_ACCOUNT_ID IN ('124420','214000');

-- ============================================================================
-- Review the output above, then run ONE of:
--   COMMIT;
--   ROLLBACK;
-- ============================================================================
