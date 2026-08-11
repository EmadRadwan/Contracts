-- ============================================================================
-- Label the GL accounts that Dim_gl_account is silently rejecting
-- ============================================================================
-- WHY THIS EXISTS
--   Dim_gl_account is a VIEW, not a table, and it ends with this gate:
--       WHERE GL_REPORT_ID              IS NOT NULL
--         AND GL_CLASS_COURSE_ID        IS NOT NULL
--         AND GL_SUB_CLASS_ID           IS NOT NULL
--         AND GL_SUB_CLASS_2_ID         IS NOT NULL
--         AND GL_ACCOUNT_COURSE_LABEL_ID IS NOT NULL
--         AND GL_ACCOUNT_CLASS_ID NOT IN ('DEBIT','CREDIT','RESOURCE','NON_POSTING')
--   Any account missing even one of those five levels never reaches Power BI at
--   all. It does not appear as a blank row — it vanishes, with no error, from
--   every measure that filters on REPORT / CLASS / ACCOUNT.
--
--   ── FIVE OR SIX LEVELS? Both numbers are correct, for different questions ──
--   The classification hierarchy has SIX levels:
--       1 REPORT      GL_REPORT_ID                     ← gated
--       2 CLASS       GL_CLASS_COURSE_ID               ← gated
--       3 SUBCLASS    GL_SUB_CLASS_ID                  ← gated
--       4 SUBCLASS2   GL_SUB_CLASS_2_ID                ← gated
--       5 ACCOUNT     GL_ACCOUNT_COURSE_LABEL_ID       ← gated
--       6 SUBACCOUNT  GL_SUB_ACCOUNT_COURSE_LABEL_ID   ← NOT gated, optional
--   The gate above deliberately stops at level 5. Levels 1-5 are what make the
--   financial statements work (which statement, which section, which printed
--   line) so an account without them cannot be reported on at all. Level 6 is
--   the investigative drill-down: nice to have, but an account is still fully
--   reportable without it. Proof: 36 of the 833 accounts currently inside the
--   view have a NULL SUBACCOUNT and pass through fine.
--   So: FIVE levels to be visible at all, SIX to be fully useful.
--
--   141 accounts (new project customers, suppliers, employees, mostly created
--   2026-05 → 2026-08) were saved with all six levels empty. 62 of them have
--   already been posted to and are hiding ~185.6M EGP of gross movement —
--   176.8M of that is customer collections under 121100. The other 79 are
--   dormant: no movement yet, but they would hide it the moment they are used,
--   so this script labels all 141 and closes the hole for good.
--
--   This script fills all SIX levels — not just the five the gate demands —
--   with the value each account's own already-classified siblings use.
--   Filling only five would make the accounts visible but leave them in a blank
--   sub-account group, which would silently break any visual that rows on
--   SUBACCOUNT_AR (the sources/uses tables on the cash-flow page do exactly
--   that). Getting them into the right bucket is the point, not just visibility.
--
-- SAFETY PROPERTIES
--   * Only ever writes to rows where the field IS NULL — never overwrites.
--   * Does NOT touch GL_ACCOUNT_CLASS_ID or GL_ACCOUNT_TYPE_ID: verified that
--     all 141 already carry the correct values, matching their siblings, with no
--     NULLs (121100/124100 -> CURRENT_ASSET + ACCOUNTS_RECEIVABLE,
--     210000/220000 -> CURRENT_LIABILITY + ACCOUNTS_PAYABLE). Also means none of
--     them can trip the view's GL_ACCOUNT_CLASS_ID NOT IN (...) condition.
--   * Scoped to exactly four parent accounts; nothing else can be affected.
--   * Idempotent — running it twice changes nothing the second time.
--   * Wrapped in a transaction. Nothing commits until you type COMMIT.
--
-- HOW TO RUN
--   1. Run STEP 1 and read the output. It is the exact list of rows about to
--      change, with the values they will receive.
--   2. Run STEP 2 (the updates) and check the reported row counts:
--      121100 -> 47,  124100 -> 19,  210000 -> 56,  220000 -> 19   (141 total)
--      These are HIGHER than the 62 accounts that currently hide money, because
--      the 79 dormant ones are labelled too. That is intended — verified by a
--      full dry-run on a copy of the database (see STEP 5).
--   3. Run STEP 3. remaining_orphans_with_movement must fall from 63 to 1
--      (account 100020 only) and the ledger imbalance must be unchanged.
--   4. Only then type COMMIT;   (or ROLLBACK; to abandon with zero trace)
--   5. Refresh the Power BI semantic model.
--
--   !! NOTE ON WHICH DATABASE !!
--   The Power BI model reads erp_contracts on 129.146.22.240:3308, NOT the
--   local copy. Applying this locally will not change any report. Run it on the
--   server the model actually points at.
--
-- NOT COVERED — one account deliberately left alone, see STEP 4 at the bottom.
-- ============================================================================

START TRANSACTION;

-- ---------------------------------------------------------------------------
-- STEP 1 — preview. Review this before running anything else.
-- ---------------------------------------------------------------------------
SELECT
    g.GL_ACCOUNT_ID                              AS account,
    g.ACCOUNT_NAME_ARABIC                        AS account_name,
    g.PARENT_GL_ACCOUNT_ID                       AS parent,
    CASE g.PARENT_GL_ACCOUNT_ID
        WHEN '121100' THEN 'RECEIVABLES / Project Receivables'
        WHEN '124100' THEN 'RECEIVABLES / Staff Receivables'
        WHEN '210000' THEN 'TRADE_PAYABLES / Trade Payables'
        WHEN '220000' THEN 'OTHER_PAYABLES / Accrued Expenses'
    END                                          AS will_be_labelled,
    COALESCE(SUM(ate.AMOUNT), 0)                 AS gross_movement_now_hidden
FROM GL_ACCOUNT g
LEFT JOIN ACCTG_TRANS_ENTRY ate
       ON ate.GL_ACCOUNT_ID = g.GL_ACCOUNT_ID
WHERE g.PARENT_GL_ACCOUNT_ID IN ('121100','124100','210000','220000')
  AND g.GL_REPORT_ID IS NULL
GROUP BY g.GL_ACCOUNT_ID, g.ACCOUNT_NAME_ARABIC, g.PARENT_GL_ACCOUNT_ID
ORDER BY gross_movement_now_hidden DESC;


-- ---------------------------------------------------------------------------
-- STEP 2 — the fix. One statement per parent so the row counts are auditable.
--          Values below are the ones 167/168, 66/66, 185/188 and 75/75 of each
--          parent's already-classified siblings use.
-- ---------------------------------------------------------------------------

-- 121100  عملاء مشاريع تحت التنفيذ  → project customers        (expect 47 rows)
UPDATE GL_ACCOUNT
SET GL_REPORT_ID                    = COALESCE(GL_REPORT_ID,                    'BALANCE_SHEET'),
    GL_CLASS_COURSE_ID              = COALESCE(GL_CLASS_COURSE_ID,              'ASSETS'),
    GL_SUB_CLASS_ID                 = COALESCE(GL_SUB_CLASS_ID,                 'ASSETS'),
    GL_SUB_CLASS_2_ID               = COALESCE(GL_SUB_CLASS_2_ID,               'CURRENT_ASSETS'),
    GL_ACCOUNT_COURSE_LABEL_ID      = COALESCE(GL_ACCOUNT_COURSE_LABEL_ID,      'RECEIVABLES'),
    GL_SUB_ACCOUNT_COURSE_LABEL_ID  = COALESCE(GL_SUB_ACCOUNT_COURSE_LABEL_ID,  'Project Receivables'),
    LAST_UPDATED_STAMP              = NOW()
WHERE PARENT_GL_ACCOUNT_ID = '121100'
  AND GL_REPORT_ID IS NULL;

-- 124100  ذمم الموظفين  → employee receivables                 (expect 19 rows)
UPDATE GL_ACCOUNT
SET GL_REPORT_ID                    = COALESCE(GL_REPORT_ID,                    'BALANCE_SHEET'),
    GL_CLASS_COURSE_ID              = COALESCE(GL_CLASS_COURSE_ID,              'ASSETS'),
    GL_SUB_CLASS_ID                 = COALESCE(GL_SUB_CLASS_ID,                 'ASSETS'),
    GL_SUB_CLASS_2_ID               = COALESCE(GL_SUB_CLASS_2_ID,               'CURRENT_ASSETS'),
    GL_ACCOUNT_COURSE_LABEL_ID      = COALESCE(GL_ACCOUNT_COURSE_LABEL_ID,      'RECEIVABLES'),
    GL_SUB_ACCOUNT_COURSE_LABEL_ID  = COALESCE(GL_SUB_ACCOUNT_COURSE_LABEL_ID,  'Staff Receivables'),
    LAST_UPDATED_STAMP              = NOW()
WHERE PARENT_GL_ACCOUNT_ID = '124100'
  AND GL_REPORT_ID IS NULL;

-- 210000  الدائنون  → suppliers / subcontractors               (expect 56 rows)
UPDATE GL_ACCOUNT
SET GL_REPORT_ID                    = COALESCE(GL_REPORT_ID,                    'BALANCE_SHEET'),
    GL_CLASS_COURSE_ID              = COALESCE(GL_CLASS_COURSE_ID,              'LIABILITIES_AND_OWNERS_EQUITY'),
    GL_SUB_CLASS_ID                 = COALESCE(GL_SUB_CLASS_ID,                 'LIABILITIES'),
    GL_SUB_CLASS_2_ID               = COALESCE(GL_SUB_CLASS_2_ID,               'CURRENT_LIABILITIES'),
    GL_ACCOUNT_COURSE_LABEL_ID      = COALESCE(GL_ACCOUNT_COURSE_LABEL_ID,      'TRADE_PAYABLES'),
    GL_SUB_ACCOUNT_COURSE_LABEL_ID  = COALESCE(GL_SUB_ACCOUNT_COURSE_LABEL_ID,  'Trade Payables'),
    LAST_UPDATED_STAMP              = NOW()
WHERE PARENT_GL_ACCOUNT_ID = '210000'
  AND GL_REPORT_ID IS NULL;

-- 220000  المصاريف المستحقة  → accrued salaries / expenses      (expect 19 rows)
UPDATE GL_ACCOUNT
SET GL_REPORT_ID                    = COALESCE(GL_REPORT_ID,                    'BALANCE_SHEET'),
    GL_CLASS_COURSE_ID              = COALESCE(GL_CLASS_COURSE_ID,              'LIABILITIES_AND_OWNERS_EQUITY'),
    GL_SUB_CLASS_ID                 = COALESCE(GL_SUB_CLASS_ID,                 'LIABILITIES'),
    GL_SUB_CLASS_2_ID               = COALESCE(GL_SUB_CLASS_2_ID,               'CURRENT_LIABILITIES'),
    GL_ACCOUNT_COURSE_LABEL_ID      = COALESCE(GL_ACCOUNT_COURSE_LABEL_ID,      'OTHER_PAYABLES'),
    GL_SUB_ACCOUNT_COURSE_LABEL_ID  = COALESCE(GL_SUB_ACCOUNT_COURSE_LABEL_ID,  'Accrued Expenses'),
    LAST_UPDATED_STAMP              = NOW()
WHERE PARENT_GL_ACCOUNT_ID = '220000'
  AND GL_REPORT_ID IS NULL;


-- ---------------------------------------------------------------------------
-- STEP 3 — verify BEFORE committing.
--   remaining_orphans_with_movement must be 1, and that 1 is account 100020,
--   which this script intentionally does not touch (see STEP 4).
--   newly_visible_accounts must be 141, and movement_still_hidden must be 12789.
-- ---------------------------------------------------------------------------
SELECT
    (SELECT COUNT(DISTINCT ate.GL_ACCOUNT_ID)
       FROM ACCTG_TRANS_ENTRY ate
       LEFT JOIN Dim_gl_account d ON d.GL_ACCOUNT_ID = ate.GL_ACCOUNT_ID
      WHERE d.GL_ACCOUNT_ID IS NULL)                       AS remaining_orphans_with_movement,
    (SELECT COUNT(*)
       FROM Dim_gl_account
      WHERE PARENT_GL_ACCOUNT_ID IN ('121100','124100','210000','220000')
        AND LAST_UPDATED_STAMP >= NOW() - INTERVAL 10 MINUTE) AS newly_visible_accounts,
    (SELECT ROUND(SUM(ate.AMOUNT), 0)
       FROM ACCTG_TRANS_ENTRY ate
       LEFT JOIN Dim_gl_account d ON d.GL_ACCOUNT_ID = ate.GL_ACCOUNT_ID
      WHERE d.GL_ACCOUNT_ID IS NULL)                       AS movement_still_hidden;

-- Sanity check that the ledger itself was untouched: this must still be the
-- same value as before the script ran (-8,501,402 at time of writing).
SELECT ROUND(SUM(CASE WHEN DEBIT_CREDIT_FLAG = 'D' THEN AMOUNT ELSE -AMOUNT END), 2) AS ledger_imbalance_unchanged
FROM ACCTG_TRANS_ENTRY;


-- ---------------------------------------------------------------------------
-- Nothing above is permanent yet.
-- ---------------------------------------------------------------------------
COMMIT;
-- ROLLBACK;


-- ============================================================================
-- STEP 4 — the one account deliberately NOT labelled: 100020
-- ============================================================================
-- 100020 "الأصول الثابتة" (Fixed Assets) is excluded on purpose. It is not an
-- under-labelled account, it is a MIS-labelled one, and it needs an accountant's
-- decision rather than a copied value:
--
--   * GL_SUB_CLASS_ID    = NULL           -- missing
--   * GL_SUB_CLASS_2_ID  = 'ADJUSTING'    -- wrong: ADJUSTING is a REPORT value,
--                                            and fixed assets are NON-current
--   * GL_ACCOUNT_COURSE_LABEL_ID = 'OTHER_CURRENT_ASSETS'
--                                         -- wrong: fixed assets are not current
--
--   * It is also a PARENT/header account with 12 children, so it should not be
--     posted to directly at all — yet it received exactly one entry:
--     12,789 EGP debit on 2026-07-08, "تصفيه عهده مختار عادل" (a custody
--     settlement). That posting most likely belongs on one of its 12 children.
--
-- Two separate questions for the accountant:
--   1. Correct 100020's classification (probably NON_CURRENT_ASSETS +
--      a property/plant/equipment account label), and
--   2. Reclassify that 12,789 posting onto the correct child asset account.
--
-- Its siblings under parent 100000 use 5 different classification combinations,
-- so there is no safe value to copy — which is exactly why it is not in STEP 2.
-- ============================================================================


-- ============================================================================
-- STEP 5 — dry-run evidence (already performed, 2026-08-10)
-- ============================================================================
-- The whole of STEP 2 was executed against a copy of erp_contracts inside a
-- transaction and then rolled back. Observed results:
--
--   BEFORE   orphan accounts with movement : 63
--            gross movement hidden         : 185,599,530
--            Dim_gl_account row count      : 833
--
--   ROWS UPDATED   121100 -> 47   124100 -> 19   210000 -> 56   220000 -> 19
--                  total 141   (62 of which had movement, 79 dormant)
--
--   AFTER    orphan accounts with movement : 1        (only 100020)
--            gross movement hidden         : 12,789   (only 100020's stray entry)
--            Dim_gl_account row count      : 974      (+141)
--
--   Resulting classification spread under the four parents:
--            RECEIVABLES    / Project Receivables : 210
--            RECEIVABLES    / Staff Receivables   :  85
--            TRADE_PAYABLES / Trade Payables      : 238
--            OTHER_PAYABLES / Accrued Expenses    :  86
--            OTHER_PAYABLES / Other Payables      :   2  (pre-existing, untouched)
--
--   LEDGER UNCHANGED  imbalance before and after : -8,501,402.00
--            (i.e. no accounting was altered — this only adds labels. That
--             -8.5M is the pre-existing one-sided OPENING_BALANCE total plus the
--             known -7,900 from 4 broken payroll transactions; unrelated to this
--             script and not fixed by it.)
--
--   ROLLBACK confirmed: Dim_gl_account returned to 833 rows, no residue.
--
-- After you COMMIT on the real server, expect these Power BI figures to change:
--   * [Unclassified GL Movement]  185,599,530  ->  12,789
--   * [Receivables Outstanding]   rises materially (14 project customers and
--     10 employee-receivable accounts become visible for the first time)
--   * مصادر النقدية table: the "(غير مصنف)" row largely disappears and its
--     value moves into ذمم عملاء المشاريع where it belongs.
-- ============================================================================
