-- ============================================================
-- Drop the CRM roles that nothing enforces
-- Date: 2026-08-24
-- ============================================================
-- WHY
--   Nine CRM roles have been assigned to users since the module was seeded.
--   Only five are read by the application. The other six gate nothing at all -
--   some describe features that do not exist (there is no Contacts screen, and
--   no delete function anywhere in CRM). Carrying them makes the permissions
--   table look like a policy that is not in force.
--
--   Verified by searching the codebase for each role name.
--
--   KEPT - enforced:
--     CRM_View            module routes (Routes.tsx, Header.tsx)
--     CRM_Leads_Create    New Lead + Excel upload, UI and API
--     CRM_Leads_Edit      lead edit form, UI and API
--     CRM_Leads_Assign    assign / bulk-assign / unassign / history, UI and API
--     CRM_Leads_ViewAll   lead visibility scoping in ListLeads
--
--   DROPPED - read by nothing:
--     CRM_Leads_View        (no reference; "CRM_Leads_ViewAll" is a different role)
--     CRM_Leads_Delete      (no delete feature exists in CRM)
--     CRM_Contacts_View     (no Contacts screen, route, controller or handler)
--     CRM_Contacts_Create
--     CRM_Contacts_Edit
--     CRM_Contacts_Delete
--
-- EFFECT ON USERS
--   None. No screen, button or endpoint checks any of these six, so removing
--   them changes nothing a user can see or do. This is bookkeeping: the
--   permissions table starts telling the truth.
--
-- DOES THIS NEED THE NEW BUILD? - depends which parts you run
--   The seeder behaves differently for creating roles and for granting them.
--
--   ROLE GRANTS ARE GUARDED. AssignRoles(...) is only called from inside
--   `if (!await userManager.Users.AnyAsync())` (SeedContracts.cs:3108-3127).
--   Users exist in dev and prod, so that block never runs and nothing is ever
--   re-granted.
--   => PART 2a IS SAFE ON ANY BUILD, including the one running today.
--
--   ROLE CREATION IS NOT GUARDED. The `requiredRoles` loop runs on every
--   startup and recreates any role missing from AspNetRoles
--   (SeedContracts.cs:3072-3085). This is how CRM_Leads_Assign and
--   CRM_Leads_ViewAll came into existence - the seeder made them, unprompted.
--   => PART 2c (deleting the role definitions) REQUIRES the updated
--      SeedContracts.cs, which no longer lists these six. Run 2c on an older
--      build and all six rows quietly reappear in AspNetRoles at next startup.
--
--   PART 2c is commented out by default, so unless you enable it this script
--   can be run as-is against the current production build.
--
-- ADDING THEM BACK
--   When a Contacts screen or a delete function is actually built, add the role
--   to `requiredRoles` in SeedContracts.cs. The seeder creates it at the next
--   startup; grant it with a small INSERT modelled on PART 2b below. Do not
--   expect the per-user block in AssignRoles to grant it - that path is dead on
--   any environment that already has users.
--
-- HOW TO RUN
--   1. PART 0 backup.  2. PART 1 read-only, inspect.  3. PART 2 apply.
--   4. PART 3 verify.  No re-login required - nothing these roles gate exists.
-- ============================================================


-- ============================================================
-- PART 0 - BACKUP
-- ============================================================
DROP TABLE IF EXISTS AspNetUserRoles_bak_20260824_deadroles;
CREATE TABLE AspNetUserRoles_bak_20260824_deadroles AS SELECT * FROM AspNetUserRoles;

DROP TABLE IF EXISTS AspNetRoles_bak_20260824_deadroles;
CREATE TABLE AspNetRoles_bak_20260824_deadroles AS SELECT * FROM AspNetRoles;

-- Rollback:
--   DELETE FROM AspNetUserRoles;
--   INSERT INTO AspNetUserRoles SELECT * FROM AspNetUserRoles_bak_20260824_deadroles;
--   (and restore AspNetRoles the same way if PART 2c was run)


-- ============================================================
-- PART 1 - VERIFICATION (read-only)
-- ============================================================

-- 1a. Current holders of the six dead roles.
SELECT r.Name AS role, COUNT(*) AS holders
FROM AspNetRoles r JOIN AspNetUserRoles ur ON ur.RoleId = r.Id
WHERE r.Name IN ('CRM_Leads_View','CRM_Leads_Delete',
                 'CRM_Contacts_View','CRM_Contacts_Create',
                 'CRM_Contacts_Edit','CRM_Contacts_Delete')
GROUP BY r.Name ORDER BY r.Name;

-- 1b. Total assignment rows this will remove.
SELECT COUNT(*) AS rows_to_delete
FROM AspNetUserRoles ur JOIN AspNetRoles r ON r.Id = ur.RoleId
WHERE r.Name IN ('CRM_Leads_View','CRM_Leads_Delete',
                 'CRM_Contacts_View','CRM_Contacts_Create',
                 'CRM_Contacts_Edit','CRM_Contacts_Delete');

-- 1c. The five roles that must SURVIVE, and who holds them.
--     Compare this against PART 3a - it must be identical.
SELECT r.Name AS role, COUNT(*) AS holders
FROM AspNetRoles r JOIN AspNetUserRoles ur ON ur.RoleId = r.Id
WHERE r.Name IN ('CRM_View','CRM_Leads_Create','CRM_Leads_Edit',
                 'CRM_Leads_Assign','CRM_Leads_ViewAll')
GROUP BY r.Name ORDER BY r.Name;


-- ============================================================
-- PART 2 - APPLY
-- ============================================================
START TRANSACTION;

-- 2a. Remove the assignments.
DELETE ur
FROM AspNetUserRoles ur
JOIN AspNetRoles r ON r.Id = ur.RoleId
WHERE r.Name IN ('CRM_Leads_View','CRM_Leads_Delete',
                 'CRM_Contacts_View','CRM_Contacts_Create',
                 'CRM_Contacts_Edit','CRM_Contacts_Delete');

-- 2b. Reference only - how to grant a role back later.
--     INSERT INTO AspNetUserRoles (UserId, RoleId)
--     SELECT u.Id, r.Id FROM AspNetUsers u CROSS JOIN AspNetRoles r
--     WHERE u.NormalizedEmail IN ('ERADWAN1967@GMAIL.COM')
--       AND r.Name = 'CRM_Contacts_View'
--       AND NOT EXISTS (SELECT 1 FROM AspNetUserRoles x
--                       WHERE x.UserId=u.Id AND x.RoleId=r.Id);

-- 2c. OPTIONAL - also delete the role definitions themselves.
--     Safe only once the updated SeedContracts.cs is deployed, otherwise the
--     seeder recreates them at the next startup. Leaving them in place is
--     harmless: an unassigned role gates nothing. Uncomment to remove.
--
-- DELETE FROM AspNetRoles
-- WHERE Name IN ('CRM_Leads_View','CRM_Leads_Delete',
--                'CRM_Contacts_View','CRM_Contacts_Create',
--                'CRM_Contacts_Edit','CRM_Contacts_Delete');

COMMIT;
-- If the 2a count does not match 1b, ROLLBACK; instead of COMMIT;


-- ============================================================
-- PART 3 - POST-RUN VERIFICATION (read-only)
-- ============================================================

-- 3a. The five enforced roles are untouched. Must match query 1c exactly.
SELECT r.Name AS role, COUNT(*) AS holders
FROM AspNetRoles r JOIN AspNetUserRoles ur ON ur.RoleId = r.Id
WHERE r.Name IN ('CRM_View','CRM_Leads_Create','CRM_Leads_Edit',
                 'CRM_Leads_Assign','CRM_Leads_ViewAll')
GROUP BY r.Name ORDER BY r.Name;

-- 3b. No user holds any of the six any more. Expected: 0 rows.
SELECT u.UserName, r.Name
FROM AspNetUsers u
JOIN AspNetUserRoles ur ON ur.UserId = u.Id
JOIN AspNetRoles r      ON r.Id = ur.RoleId
WHERE r.Name IN ('CRM_Leads_View','CRM_Leads_Delete',
                 'CRM_Contacts_View','CRM_Contacts_Create',
                 'CRM_Contacts_Edit','CRM_Contacts_Delete');

-- 3c. Every CRM user still reaches the module and still sees leads.
--     Expected: 0 rows.
SELECT u.UserName, u.Email
FROM AspNetUsers u
JOIN AspNetUserRoles ur ON ur.UserId = u.Id
JOIN AspNetRoles r      ON r.Id = ur.RoleId AND r.Name = 'CRM_View'
WHERE NOT EXISTS (
    SELECT 1 FROM AspNetUserRoles x JOIN AspNetRoles xr ON xr.Id = x.RoleId
    WHERE x.UserId = u.Id AND xr.Name = 'CRM_Leads_ViewAll');

-- ============================================================
-- After sign-off:
--   DROP TABLE AspNetUserRoles_bak_20260824_deadroles;
--   DROP TABLE AspNetRoles_bak_20260824_deadroles;
-- ============================================================
