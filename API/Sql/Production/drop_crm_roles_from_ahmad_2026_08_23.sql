-- ============================================================
-- Remove all CRM roles from Ahmad (aagiba@gmail.com)
-- Date: 2026-08-23
-- ============================================================
-- CONTEXT
--   Supersedes step 2b of crm_admin_roles_and_copy_nevine_to_rmostafa_2026_08_23.sql,
--   which was deliberately NOT executed. That step would have revoked CRM
--   roles from all 11 non-designated users; the decision instead is:
--
--     * Ahmad            -> loses ALL CRM roles          (this script)
--     * The other 10     -> keep their CRM roles as-is   (no change)
--     * Emad, Ayman      -> keep the full 9-role set     (no change)
--
--   The admin / non-admin split for the other 10 is deferred: the CRM
--   Admin's defining job (assigning leads to CRM users) has no feature
--   and therefore no role to grant or withhold. Revisit once lead
--   assignment ships.
--
-- SCOPE
--   Ahmad holds 41 roles, 8 of them CRM. This removes only the 8 CRM
--   roles. His other 33 roles are untouched.
--
--   Roles removed:
--     CRM_View
--     CRM_Leads_View, CRM_Leads_Create, CRM_Leads_Edit, CRM_Leads_Delete
--     CRM_Contacts_View, CRM_Contacts_Create, CRM_Contacts_Delete
--   (He does not currently hold CRM_Contacts_Edit - 8, not 9.)
--
-- EFFECT
--   Losing CRM_View removes his access to the whole CRM module, since
--   the application gates /leads and /sales-opportunities on that role
--   at the route level. He retains every other module.
--
-- HOW TO RUN
--   1. PART 0 - take the backup.
--   2. PART 1 - read-only. Confirm 1 user, 8 roles.
--   3. PART 2 - apply.
--   4. PART 3 - read-only. Confirm the end state.
-- ============================================================


-- ============================================================
-- PART 0 - BACKUP
-- ============================================================
-- NOTE: AspNetUserRoles_bak_20260823 already exists from the earlier
-- script, but it was taken BEFORE the nevine->Ayman role copy, so
-- restoring it would also undo that copy. Take a fresh point-in-time
-- backup for this change instead.
DROP TABLE IF EXISTS AspNetUserRoles_bak_20260823_ahmad;
CREATE TABLE AspNetUserRoles_bak_20260823_ahmad AS SELECT * FROM AspNetUserRoles;
-- Expected: 267 rows in dev.

-- Rollback, if ever needed:
--   DELETE FROM AspNetUserRoles;
--   INSERT INTO AspNetUserRoles SELECT * FROM AspNetUserRoles_bak_20260823_ahmad;


-- ============================================================
-- PART 1 - VERIFICATION (read-only, run first)
-- ============================================================

-- 1a. Resolve the account. Expected: exactly 1 row.
SELECT Id, UserName, Email FROM AspNetUsers
WHERE NormalizedEmail = 'AAGIBA@GMAIL.COM';
-- Expected in dev: 29a02dc0-70ea-46d0-a687-6a72b2f91d07  Ahmad  aagiba@gmail.com
-- If this returns 0 rows, STOP - the account does not exist in this
-- environment and PART 2 would silently delete nothing.

-- 1b. Exactly which roles will be removed. Expected: 8 rows.
SELECT r.Name AS role_to_be_removed
FROM AspNetUsers u
JOIN AspNetUserRoles ur ON ur.UserId = u.Id
JOIN AspNetRoles r      ON r.Id = ur.RoleId
WHERE u.NormalizedEmail = 'AAGIBA@GMAIL.COM' AND r.Name LIKE 'CRM%'
ORDER BY r.Name;

-- 1c. Role counts before the change. Expected: 41 total, 8 CRM.
SELECT COUNT(*) AS total_roles, SUM(r.Name LIKE 'CRM%') AS crm_roles
FROM AspNetUsers u
JOIN AspNetUserRoles ur ON ur.UserId = u.Id
JOIN AspNetRoles r      ON r.Id = ur.RoleId
WHERE u.NormalizedEmail = 'AAGIBA@GMAIL.COM';

-- 1d. Confirm no one else is touched - the other CRM holders and their
--     counts, which must be identical in PART 3. Expected: 12 rows.
SELECT u.UserName, u.Email, COUNT(*) AS crm_roles
FROM AspNetUsers u
JOIN AspNetUserRoles ur ON ur.UserId = u.Id
JOIN AspNetRoles r      ON r.Id = ur.RoleId
WHERE r.Name LIKE 'CRM%' AND u.NormalizedEmail <> 'AAGIBA@GMAIL.COM'
GROUP BY u.Id, u.UserName, u.Email
ORDER BY u.UserName;
-- Expected in dev: Ahlam 9, Alaa 9, Ayman 9, Emad 9, Eman 9, Gamal 9,
--                  Mark 9, Medhat 9, Nagy 9, Peter 9, Reham 9, Yasmin 9


-- ============================================================
-- PART 2 - APPLY
-- ============================================================
START TRANSACTION;

DELETE ur
FROM AspNetUserRoles ur
JOIN AspNetUsers u ON u.Id = ur.UserId
JOIN AspNetRoles r ON r.Id = ur.RoleId
WHERE r.Name LIKE 'CRM%'
  AND u.NormalizedEmail = 'AAGIBA@GMAIL.COM';
-- Expected: 8 rows affected. If it is not 8, run ROLLBACK; and stop.

COMMIT;


-- ============================================================
-- PART 3 - POST-RUN VERIFICATION (read-only)
-- ============================================================

-- 3a. Ahmad holds no CRM role. Expected: 0 rows.
SELECT r.Name
FROM AspNetUsers u
JOIN AspNetUserRoles ur ON ur.UserId = u.Id
JOIN AspNetRoles r      ON r.Id = ur.RoleId
WHERE u.NormalizedEmail = 'AAGIBA@GMAIL.COM' AND r.Name LIKE 'CRM%';

-- 3b. Ahmad's other roles survived. Expected: 33.
SELECT COUNT(*) AS remaining_roles
FROM AspNetUsers u
JOIN AspNetUserRoles ur ON ur.UserId = u.Id
WHERE u.NormalizedEmail = 'AAGIBA@GMAIL.COM';

-- 3c. CRM role holders after the change. Expected: 12 rows, 9 roles each,
--     identical to the result of query 1d.
SELECT u.UserName, u.Email, COUNT(*) AS crm_roles
FROM AspNetUsers u
JOIN AspNetUserRoles ur ON ur.UserId = u.Id
JOIN AspNetRoles r      ON r.Id = ur.RoleId
WHERE r.Name LIKE 'CRM%'
GROUP BY u.Id, u.UserName, u.Email
ORDER BY u.UserName;

-- 3d. Total row count. Expected: 259 in dev (267 - 8).
SELECT COUNT(*) AS live_rows FROM AspNetUserRoles;

-- 3e. No account was left with zero roles by this change. Expected: 0 rows.
SELECT u.UserName, u.Email
FROM AspNetUsers u
LEFT JOIN AspNetUserRoles ur ON ur.UserId = u.Id
GROUP BY u.Id, u.UserName, u.Email
HAVING COUNT(ur.RoleId) = 0;

-- ============================================================
-- After sign-off: DROP TABLE AspNetUserRoles_bak_20260823_ahmad;
--                 DROP TABLE AspNetUserRoles_bak_20260823;
-- ============================================================
