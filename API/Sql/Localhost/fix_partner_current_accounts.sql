-- =============================================================================
-- fix_partner_current_accounts.sql
-- Reclassify the "جارى الشركاء" (partner current account) accounts 321000 / 321100
-- / 321200 out of the DIVIDENDS_PAID (الأرباح الموزعة المدفوعة) matrix line and into
-- the owners'-equity SHARE_CAPITAL line, so they match the identically-named
-- 304000 family that is ALREADY classified correctly.
--
-- WHY (the problem):
--   The chart of accounts has TWO parallel "جارى الشركاء" structures:
--     * 302000 / 303000 / 304000  -> GL_ACCOUNT_CLASS_ID = EQUITY
--         -> OWNERS_EQUITY / SHARE_CAPITAL / SHARE_CAPITAL   (credit-normal, sign -1)  CORRECT
--     * 321000 / 321100 / 321200  -> GL_ACCOUNT_CLASS_ID = RETURN_OF_CAPITAL
--         -> OWNERS_EQUITY / RETAINED_EARNINGS / DIVIDENDS_PAID (debit-normal, sign +1)  WRONG
--   A partner current account is an equity component, not a dividend/return of capital.
--   The auto-classifier only put them on the Dividends-Paid line because their root
--   GL_ACCOUNT_CLASS_ID was RETURN_OF_CAPITAL — so the durable fix corrects the root
--   class as well as the presentation columns.
--
-- SAFETY: all three accounts have zero balance and zero posted transactions — no restatement.
--
-- DELIBERATELY NOT TOUCHED (they are correctly on the DIVIDENDS_PAID line):
--   * 321300 السحوبات - الشريك 3  -> a genuine DRAWING / return of capital. Stays.
--   * 311000 السحوبات, 343000/343100/343200/343300 السحوبات -> genuine drawings.
--   * 334000 / 335000 / 342000 / 342100 / 342200 / 342300 الأرباح الموزعة -> genuine dividends.
--   After this fix the DIVIDENDS_PAID line still holds all of the above, unchanged.
--
-- Idempotent: the UPDATE is deterministic and safe to re-run.
-- =============================================================================

START TRANSACTION;

-- ── OPTION A (active) — align 321xxx to the existing 304000 "Share Capital" pattern ──
-- Root nature: RETURN_OF_CAPITAL -> EQUITY  (matches 304000 family; stops the
--   auto-classifier (gl_account_classifier_update.sql) from reverting this fix).
-- Presentation: SUBCLASS2 RETAINED_EARNINGS -> SHARE_CAPITAL,
--               matrix line DIVIDENDS_PAID -> SHARE_CAPITAL.
-- Unchanged (already correct): GL_REPORT_ID=BALANCE_SHEET,
--   GL_CLASS_COURSE_ID=LIABILITIES_AND_OWNERS_EQUITY, GL_SUB_CLASS_ID=OWNERS_EQUITY,
--   GL_ACCOUNT_TYPE_ID=OWNERS_EQUITY, GL_SUB_ACCOUNT_COURSE_LABEL_ID='Partner Current Accounts'.
-- Sign flips from +1 (debit) to -1 (credit) automatically, because the view derives
--   SIGN_MULTIPLIER from the matrix-line label and SHARE_CAPITAL is credit-normal.
UPDATE GL_ACCOUNT
SET GL_ACCOUNT_CLASS_ID         = 'EQUITY',
    GL_SUB_CLASS_2_ID           = 'SHARE_CAPITAL',
    GL_ACCOUNT_COURSE_LABEL_ID  = 'SHARE_CAPITAL'
WHERE GL_ACCOUNT_ID IN ('321000','321100','321200');

COMMIT;

-- ── Verification: the three should now read identically to the 304000 family ─────────
SELECT d.GL_ACCOUNT_ID, d.ACCOUNT_NAME_ARABIC,
       d.SUBCLASS2, d.ACCOUNT AS matrix_line, d.ACCOUNT_AR, d.SIGN_MULTIPLIER,
       d.SUBACCOUNT_AR
FROM Dim_gl_account d
WHERE d.GL_ACCOUNT_ID IN ('302000','303000','304000','321000','321100','321200','321300')
ORDER BY d.ACCOUNT_SORT, d.GL_ACCOUNT_ID;

-- Confirm the DIVIDENDS_PAID line still holds the genuine drawing (321300) and is otherwise intact:
SELECT GL_ACCOUNT_ID, ACCOUNT_NAME_ARABIC, GL_ACCOUNT_COURSE_LABEL_ID
FROM GL_ACCOUNT WHERE GL_ACCOUNT_COURSE_LABEL_ID = 'DIVIDENDS_PAID' ORDER BY GL_ACCOUNT_ID;


-- =============================================================================
-- OPTION B (alternative — leave COMMENTED unless you prefer it)
-- Give partner current accounts their OWN dedicated equity matrix line instead of
-- folding them into "Share Capital", and move BOTH families onto it for consistency.
-- More textbook-correct presentation (a current account is not paid-up capital),
-- but it adds a new matrix line and also re-points 302000/303000/304000.
-- =============================================================================
-- START TRANSACTION;
--
-- INSERT INTO GL_ACCOUNT_COURSE_LABEL
--   (GL_ACCOUNT_COURSE_LABEL_ID, DESCRIPTION, DESCRIPTION_ARABIC, SIGN_MULTIPLIER, SORT_ORDER)
--   VALUES ('PARTNER_CURRENT_ACCOUNTS', 'Partner Current Accounts', 'جارى الشركاء', -1, 205)
--   ON DUPLICATE KEY UPDATE DESCRIPTION=VALUES(DESCRIPTION),
--     DESCRIPTION_ARABIC=VALUES(DESCRIPTION_ARABIC),
--     SIGN_MULTIPLIER=VALUES(SIGN_MULTIPLIER), SORT_ORDER=VALUES(SORT_ORDER);
--
-- -- 321xxx: fix root nature + move to the new line
-- UPDATE GL_ACCOUNT
-- SET GL_ACCOUNT_CLASS_ID        = 'EQUITY',
--     GL_SUB_CLASS_2_ID          = 'SHARE_CAPITAL',
--     GL_ACCOUNT_COURSE_LABEL_ID = 'PARTNER_CURRENT_ACCOUNTS'
-- WHERE GL_ACCOUNT_ID IN ('321000','321100','321200');
--
-- -- 302000/303000/304000: move the already-equity family onto the same dedicated line
-- UPDATE GL_ACCOUNT
-- SET GL_ACCOUNT_COURSE_LABEL_ID = 'PARTNER_CURRENT_ACCOUNTS'
-- WHERE GL_ACCOUNT_ID IN ('302000','303000','304000');
--
-- COMMIT;
-- =============================================================================
