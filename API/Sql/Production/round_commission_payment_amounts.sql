-- ============================================================================
-- Round existing COMMISSION_PAYMENT amounts to whole numbers (0 decimals)
-- ============================================================================
-- WHY THIS EXISTS
--   Commission payments are now paid as whole amounts — ApproveSalesCommission
--   rounds each payment's Amount to 0 decimals (MidpointRounding.AwayFromZero)
--   at creation time. This script back-fills that rounding onto commission
--   payments that were created BEFORE that change.
--
-- SAFETY — READ BEFORE RUNNING
--   A commission payment is created with STATUS_ID = 'PMNT_NOT_PAID' and is only
--   posted to the general ledger later, when it is marked sent/confirmed. Once
--   posted, its ACCTG_TRANS_ENTRY rows carry the amount at ledger precision and
--   the debit/credit entries balance against each other. Rounding ONLY the
--   PAYMENT row of an already-posted payment would leave the payment and its GL
--   entries inconsistent.
--
--   Therefore this script:
--     • Section 1 — previews the unposted commission payments it will change.
--     • Section 2 — updates ONLY unposted ('PMNT_NOT_PAID') commission payments
--                   that actually have a fractional amount. Safe: no GL entries
--                   exist for these yet.
--     • Section 3 — reports any commission payments that are already posted /
--                   past PMNT_NOT_PAID and still have decimals. These are NOT
--                   touched here — handle them manually (round the payment AND
--                   its ACCTG_TRANS_ENTRY amounts together, keeping debits =
--                   credits), or reset+re-approve the commission so they are
--                   recreated with the new rounding.
--
--   MySQL ROUND() rounds halves away from zero for positive values, matching the
--   application's MidpointRounding.AwayFromZero. Commission amounts are positive.
--
--   Run the SELECTs first, review, then run the UPDATE inside the transaction.
-- ============================================================================

-- ── Section 1: PREVIEW — unposted commission payments that will be rounded ──
SELECT
    p.PAYMENT_ID,
    p.STATUS_ID,
    p.AMOUNT                          AS CURRENT_AMOUNT,
    ROUND(p.AMOUNT, 0)               AS NEW_AMOUNT,
    p.ACTUAL_CURRENCY_AMOUNT         AS CURRENT_ACTUAL_AMOUNT,
    ROUND(p.ACTUAL_CURRENCY_AMOUNT, 0) AS NEW_ACTUAL_AMOUNT,
    p.SALES_REQUEST_ID,
    p.COMMENTS
FROM PAYMENT p
WHERE p.PAYMENT_TYPE_ID = 'COMMISSION_PAYMENT'
  AND p.STATUS_ID = 'PMNT_NOT_PAID'
  AND (p.AMOUNT <> ROUND(p.AMOUNT, 0)
       OR p.ACTUAL_CURRENCY_AMOUNT <> ROUND(p.ACTUAL_CURRENCY_AMOUNT, 0))
ORDER BY p.PAYMENT_ID;

-- ── Section 3 (run this too, before deciding): posted / advanced payments ──
--    with decimals that this script deliberately does NOT change.
SELECT
    p.PAYMENT_ID,
    p.STATUS_ID,
    p.AMOUNT,
    p.SALES_REQUEST_ID,
    (SELECT COUNT(*) FROM ACCTG_TRANS at WHERE at.PAYMENT_ID = p.PAYMENT_ID) AS ACCTG_TRANS_COUNT
FROM PAYMENT p
WHERE p.PAYMENT_TYPE_ID = 'COMMISSION_PAYMENT'
  AND p.AMOUNT <> ROUND(p.AMOUNT, 0)
  AND (p.STATUS_ID <> 'PMNT_NOT_PAID'
       OR EXISTS (SELECT 1 FROM ACCTG_TRANS at WHERE at.PAYMENT_ID = p.PAYMENT_ID))
ORDER BY p.PAYMENT_ID;

-- ── Section 2: UPDATE — round unposted commission payments only ──
--    Uncomment and run after reviewing Sections 1 and 3.
 START TRANSACTION;

 UPDATE PAYMENT
 SET AMOUNT                 = ROUND(AMOUNT, 0),
     ACTUAL_CURRENCY_AMOUNT = ROUND(ACTUAL_CURRENCY_AMOUNT, 0),
     LAST_UPDATED_STAMP     = UTC_TIMESTAMP()
 WHERE PAYMENT_TYPE_ID = 'COMMISSION_PAYMENT'
   AND STATUS_ID = 'PMNT_NOT_PAID'
   AND (AMOUNT <> ROUND(AMOUNT, 0)
        OR ACTUAL_CURRENCY_AMOUNT <> ROUND(ACTUAL_CURRENCY_AMOUNT, 0));

-- -- Verify row count matches Section 1, then:
 COMMIT;
-- -- (or ROLLBACK; if anything looks wrong)
