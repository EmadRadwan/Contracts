-- ============================================================
-- Restrict lead creation and editing to the CRM Admins
-- Date: 2026-08-24
-- ============================================================
-- WHY THIS IS NEEDED
--   The application now enforces CRM_Leads_Create and CRM_Leads_Edit on the
--   New Lead button, the Excel upload, and the lead edit form - both in the UI
--   and on the API endpoints.
--
--   But EVERY CRM user currently holds both roles, so that enforcement changes
--   nothing on its own. This script is what actually restricts them.
--
-- WHAT IT DOES
--   Revokes CRM_Leads_Create and CRM_Leads_Edit from every CRM user EXCEPT the
--   two CRM Admins. Leaves CRM_Leads_View, CRM_Leads_Delete and the
--   CRM_Contacts_* roles alone - those are still unread by the code.
--
--   NOTE ON EMAILS: production uses rayman@gmail.com for the second admin;
--   development uses rmostafa@gmail.com. Both resolve to the user shown as
--   "Ayman", party 346. Adjust the two lists below to match the environment.
--
-- EFFECT ON REGULAR CRM USERS
--   * "New Lead" and "Upload Excel" buttons disappear
--   * Lead names are no longer clickable - the edit form cannot be opened
--   * POST /parties/createLead, POST /parties/createLeadsBatch and
--     PUT /leads/{id} return 403
--   They keep full read access to their leads, and all Sales Opportunity
--   functionality is untouched.
--
-- HOW TO RUN
--   1. PART 0 backup.  2. PART 1 read-only, inspect.  3. PART 2 apply.
--   4. PART 3 verify.  5. Affected users must sign out and back in.
-- ============================================================


-- ============================================================
-- PART 0 - BACKUP
-- ============================================================
DROP TABLE IF EXISTS AspNetUserRoles_bak_20260824_leadcreate;
CREATE TABLE AspNetUserRoles_bak_20260824_leadcreate AS SELECT * FROM AspNetUserRoles;

-- Rollback:
--   DELETE FROM AspNetUserRoles;
--   INSERT INTO AspNetUserRoles SELECT * FROM AspNetUserRoles_bak_20260824_leadcreate;


-- ============================================================
-- PART 1 - VERIFICATION (read-only)
-- ============================================================

-- 1a. Confirm the two CRM Admins resolve. Expected: 2 rows.
--     PRODUCTION list:
SELECT Id, UserName, Email FROM AspNetUsers
WHERE NormalizedEmail IN ('ERADWAN1967@GMAIL.COM','RAYMAN@GMAIL.COM');
--     DEVELOPMENT list (use instead if running against dev):
-- SELECT Id, UserName, Email FROM AspNetUsers
-- WHERE NormalizedEmail IN ('ERADWAN1967@GMAIL.COM','RMOSTAFA@GMAIL.COM');
--
-- If this returns fewer than 2 rows, STOP - the revoke would leave the system
-- with fewer lead creators than intended.

-- 1b. Exactly who loses lead create/edit.
SELECT DISTINCT u.UserName, u.Email
FROM AspNetUsers u
JOIN AspNetUserRoles ur ON ur.UserId = u.Id
JOIN AspNetRoles r      ON r.Id = ur.RoleId
WHERE r.Name IN ('CRM_Leads_Create','CRM_Leads_Edit')
  AND u.NormalizedEmail NOT IN ('ERADWAN1967@GMAIL.COM','RAYMAN@GMAIL.COM')
ORDER BY u.UserName;

-- 1c. How many role rows will be removed.
SELECT COUNT(*) AS rows_to_delete
FROM AspNetUserRoles ur
JOIN AspNetUsers u ON u.Id = ur.UserId
JOIN AspNetRoles r ON r.Id = ur.RoleId
WHERE r.Name IN ('CRM_Leads_Create','CRM_Leads_Edit')
  AND u.NormalizedEmail NOT IN ('ERADWAN1967@GMAIL.COM','RAYMAN@GMAIL.COM');


-- ============================================================
-- PART 2 - APPLY
-- ============================================================
START TRANSACTION;

-- Make sure both admins actually hold the two roles first (idempotent).
INSERT INTO AspNetUserRoles (UserId, RoleId)
SELECT u.Id, r.Id
FROM AspNetUsers u
CROSS JOIN AspNetRoles r
WHERE u.NormalizedEmail IN ('ERADWAN1967@GMAIL.COM','RAYMAN@GMAIL.COM')
  AND r.Name IN ('CRM_Leads_Create','CRM_Leads_Edit')
  AND NOT EXISTS (
      SELECT 1 FROM AspNetUserRoles ur
      WHERE ur.UserId = u.Id AND ur.RoleId = r.Id);

-- Revoke from everyone else.
DELETE ur
FROM AspNetUserRoles ur
JOIN AspNetUsers u ON u.Id = ur.UserId
JOIN AspNetRoles r ON r.Id = ur.RoleId
WHERE r.Name IN ('CRM_Leads_Create','CRM_Leads_Edit')
  AND u.NormalizedEmail NOT IN ('ERADWAN1967@GMAIL.COM','RAYMAN@GMAIL.COM');

COMMIT;
-- If the delete count does not match 1c, ROLLBACK; instead of COMMIT;


-- ============================================================
-- PART 3 - POST-RUN VERIFICATION (read-only)
-- ============================================================

-- 3a. Who can create or edit leads. Expected: the 2 admins only.
SELECT u.UserName, u.Email, GROUP_CONCAT(r.Name ORDER BY r.Name) AS roles
FROM AspNetUsers u
JOIN AspNetUserRoles ur ON ur.UserId = u.Id
JOIN AspNetRoles r      ON r.Id = ur.RoleId
WHERE r.Name IN ('CRM_Leads_Create','CRM_Leads_Edit')
GROUP BY u.Id, u.UserName, u.Email
ORDER BY u.UserName;

-- 3b. Regular CRM users keep read access. Expected: every non-admin CRM user.
SELECT u.UserName
FROM AspNetUsers u
JOIN AspNetUserRoles ur ON ur.UserId = u.Id
JOIN AspNetRoles r      ON r.Id = ur.RoleId AND r.Name = 'CRM_Leads_ViewAll'
ORDER BY u.UserName;

-- 3c. Nobody lost CRM access entirely. Expected: 0 rows.
SELECT u.UserName, u.Email
FROM AspNetUsers u
JOIN AspNetUserRoles ur ON ur.UserId = u.Id
JOIN AspNetRoles r      ON r.Id = ur.RoleId AND r.Name = 'CRM_View'
WHERE NOT EXISTS (
    SELECT 1 FROM AspNetUserRoles x JOIN AspNetRoles xr ON xr.Id = x.RoleId
    WHERE x.UserId = u.Id AND xr.Name IN ('CRM_Leads_ViewAll','CRM_Leads_View'));

-- ============================================================
-- Affected users must sign out and back in - roles live in the JWT.
-- After sign-off: DROP TABLE AspNetUserRoles_bak_20260824_leadcreate;
-- ============================================================
