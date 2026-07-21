-- =====================================================================
-- Consolidate partner/investment participation sub-accounts under a
-- single SUBACCOUNT label: جارى شركاء - مشاركات استثمارية
-- Database: erp_contracts @ 129.146.22.240:3308
--
-- Scope (14 GL_ACCOUNT rows, confirmed via Dim_gl_account):
--   - 250280, 250281, 250282, 250283  (parent 250270)
--       currently SUBACCOUNT = 'Project Partnerships' (مشاركات المشاريع)
--   - 250310, 250320, 250330, 250340, 250350, 250360  (parent 250300)
--       currently SUBACCOUNT = 'Temp Partnerships' (مشاركات مؤقتة استثمار عقاري)
--   - 250370, 250380, 250390  (parent 250300)
--       currently SUBACCOUNT = NULL (unlabeled)
--   - 250440  (parent 250400, different ACCOUNT: دائنو شراء أسهم /
--       Stock Purchase Payables — only its SUBACCOUNT changes here,
--       its ACCOUNT/parent hierarchy is left untouched)
--     currently SUBACCOUNT = 'Other Payables' (ذمم دائنة أخرى)
--
-- Not yet run against the database — review before executing.
-- =====================================================================

START TRANSACTION;

-- 1. New reference label
--    SORT_ORDER 275 — between 'Temp Partnerships' (270) and
--    'Land Owner Payables' (280), no collision with existing values.
INSERT INTO GL_SUB_ACCOUNT_COURSE_LABEL
    (GL_SUB_ACCOUNT_COURSE_LABEL_ID, DESCRIPTION, DESCRIPTION_ARABIC, SORT_ORDER)
VALUES
    ('Partner Investment Participations', 'Partner Investment Participations', 'جارى شركاء - مشاركات استثمارية', 275);

-- 2. Reassign all 14 accounts to the new label
UPDATE GL_ACCOUNT
SET GL_SUB_ACCOUNT_COURSE_LABEL_ID = 'Partner Investment Participations'
WHERE GL_ACCOUNT_ID IN (
    '250280','250281','250282','250283',
    '250310','250320','250330','250340','250350','250360','250370','250380','250390',
    '250440'
);
-- expect: 14 rows affected

COMMIT;
-- (ROLLBACK instead if the row count above was unexpected)


-- =====================================================================
-- POST-CHANGE VERIFICATION
-- =====================================================================

SELECT GL_ACCOUNT_ID, ACCOUNT_NAME_ARABIC, PARENT_GL_ACCOUNT_ID, ACCOUNT_AR, SUBACCOUNT_AR, SUBACCOUNT_SORT
FROM Dim_gl_account
WHERE GL_ACCOUNT_ID IN (
    '250280','250281','250282','250283',
    '250310','250320','250330','250340','250350','250360','250370','250380','250390',
    '250440'
)
ORDER BY GL_ACCOUNT_ID;
-- expect: all 14 rows show SUBACCOUNT_AR = 'جارى شركاء - مشاركات استثمارية', SUBACCOUNT_SORT = 275

-- 'مشاركات مؤقتة استثمار عقاري' was used ONLY by the 6 accounts moved
-- above (250310-250360) — expect 0 rows left under it.
-- 'مشاركات المشاريع' is NOT exclusive to our scope: 7 other, unrelated
-- project accounts (250210-250260, and 250270 itself — the PARENT of
-- 250280-250283) also carry it and are intentionally left unchanged
-- here, so expect those 7 rows to remain.
SELECT GL_ACCOUNT_ID, ACCOUNT_NAME_ARABIC, SUBACCOUNT_AR
FROM Dim_gl_account
WHERE SUBACCOUNT_AR IN ('مشاركات المشاريع', 'مشاركات مؤقتة استثمار عقاري');
-- expect: 7 rows, all SUBACCOUNT_AR = 'مشاركات المشاريع'
-- (250210, 250220, 250230, 250240, 250250, 250260, 250270)
