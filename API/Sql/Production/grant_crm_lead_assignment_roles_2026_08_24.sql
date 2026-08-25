-- ============================================================
-- Grant the two new lead-assignment roles
-- Date: 2026-08-24
-- ============================================================
-- MUST BE RUN AS PART OF THE SAME DEPLOYMENT AS THE LEAD
-- ASSIGNMENT FEATURE. Read the warning below before deploying.
--
-- WHAT CHANGED IN THE CODE
--   ListLeads is now scoped: a user WITHOUT CRM_Leads_ViewAll sees only
--   the leads currently assigned to them. No lead is assigned yet, so on
--   the first deploy every CRM user without that role sees an EMPTY leads
--   list. This script prevents that.
--
-- WHAT THIS SCRIPT DOES
--   1. Creates the two roles if they do not already exist.
--      (The application seeder also creates them at startup; this makes
--       the script safe to run first, or on its own.)
--   2. Grants CRM_Leads_Assign to the two CRM Admins only:
--        eradwan1967@gmail.com (Emad)
--        RAYMAN@GMAIL.COM    (Ayman)
--   3. Grants CRM_Leads_ViewAll to EVERY current CRM_View holder, so
--      nobody's leads list changes on deploy day.
--
-- WHAT THIS SCRIPT DELIBERATELY DOES NOT DO
--   It does not take CRM_Leads_ViewAll away from the 10 regular CRM users.
--   That is the final step of the rollout and belongs in its own script,
--   run only once leads have actually been assigned to those reps -
--   otherwise they will log in to an empty list.
--
-- HOW TO RUN
--   1. PART 0 - backup.
--   2. PART 1 - read-only. Inspect.
--   3. PART 2 - apply.
--   4. PART 3 - read-only. Confirm.
-- ============================================================


-- ============================================================
-- PART 0 - BACKUP
-- ============================================================
DROP TABLE IF EXISTS AspNetUserRoles_bak_20260824;
CREATE TABLE AspNetUserRoles_bak_20260824 AS SELECT * FROM AspNetUserRoles;

-- Rollback, if ever needed:
--   DELETE FROM AspNetUserRoles;
--   INSERT INTO AspNetUserRoles SELECT * FROM AspNetUserRoles_bak_20260824;


-- ============================================================
-- PART 1 - VERIFICATION (read-only, run first)
-- ============================================================

-- 1a. Do the roles already exist? 0, 1 or 2 rows are all fine -
--     PART 2 creates whichever are missing.
SELECT Id, Name FROM AspNetRoles
WHERE Name IN ('CRM_Leads_Assign','CRM_Leads_ViewAll');

-- 1b. The two CRM Admins must resolve. Expected: 2 rows.
SELECT Id, UserName, Email FROM AspNetUsers
WHERE NormalizedEmail IN ('ERADWAN1967@GMAIL.COM','RAYMAN@GMAIL.COM');

-- 1c. Everyone who will receive CRM_Leads_ViewAll. Expected: 13 rows in dev
--     (all current CRM_View holders, Ahmad included).
SELECT u.UserName, u.Email
FROM AspNetUsers u
JOIN AspNetUserRoles ur ON ur.UserId = u.Id
JOIN AspNetRoles r      ON r.Id = ur.RoleId
WHERE r.Name = 'CRM_View'
ORDER BY u.UserName;

-- 1d. Sanity check on the assignment table - how many leads are already
--     assigned. If this is 0, do NOT skip granting CRM_Leads_ViewAll.
SELECT COUNT(*) AS active_lead_assignments
FROM PARTY_RELATIONSHIP
WHERE PARTY_RELATIONSHIP_TYPE_ID = 'LEAD_OWNER' AND THRU_DATE IS NULL;


-- ============================================================
-- PART 2 - APPLY
-- ============================================================
START TRANSACTION;

-- 2a. Create the roles if missing.
INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
SELECT UUID(), 'CRM_Leads_Assign', 'CRM_LEADS_ASSIGN', UUID()
WHERE NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'CRM_Leads_Assign');

INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
SELECT UUID(), 'CRM_Leads_ViewAll', 'CRM_LEADS_VIEWALL', UUID()
WHERE NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'CRM_Leads_ViewAll');

-- 2b. CRM_Leads_Assign -> the two CRM Admins only.
INSERT INTO AspNetUserRoles (UserId, RoleId)
SELECT u.Id, r.Id
FROM AspNetUsers u
CROSS JOIN AspNetRoles r
WHERE u.NormalizedEmail IN ('ERADWAN1967@GMAIL.COM','RAYMAN@GMAIL.COM')
  AND r.Name = 'CRM_Leads_Assign'
  AND NOT EXISTS (
      SELECT 1 FROM AspNetUserRoles ur
      WHERE ur.UserId = u.Id AND ur.RoleId = r.Id);
-- Expected: 2 rows affected.

-- 2c. CRM_Leads_ViewAll -> every current CRM_View holder, so no one's
--     leads list changes on deploy.
INSERT INTO AspNetUserRoles (UserId, RoleId)
SELECT u.Id, viewall.Id
FROM AspNetUsers u
JOIN AspNetUserRoles ur ON ur.UserId = u.Id
JOIN AspNetRoles crmview ON crmview.Id = ur.RoleId AND crmview.Name = 'CRM_View'
CROSS JOIN AspNetRoles viewall
WHERE viewall.Name = 'CRM_Leads_ViewAll'
  AND NOT EXISTS (
      SELECT 1 FROM AspNetUserRoles x
      WHERE x.UserId = u.Id AND x.RoleId = viewall.Id);
-- Expected: 13 rows affected in dev.

COMMIT;
-- If any count looks wrong, ROLLBACK; instead of COMMIT;


-- ============================================================
-- PART 3 - POST-RUN VERIFICATION (read-only)
-- ============================================================

-- 3a. Who can assign leads. Expected: exactly 2 - Emad and Ayman.
SELECT u.UserName, u.Email
FROM AspNetUsers u
JOIN AspNetUserRoles ur ON ur.UserId = u.Id
JOIN AspNetRoles r      ON r.Id = ur.RoleId
WHERE r.Name = 'CRM_Leads_Assign'
ORDER BY u.UserName;

-- 3b. Who can see all leads. Expected: 13 - every CRM_View holder.
SELECT u.UserName, u.Email
FROM AspNetUsers u
JOIN AspNetUserRoles ur ON ur.UserId = u.Id
JOIN AspNetRoles r      ON r.Id = ur.RoleId
WHERE r.Name = 'CRM_Leads_ViewAll'
ORDER BY u.UserName;

-- 3c. THE CRITICAL CHECK - anyone who can reach CRM but cannot see all
--     leads and owns none. These users will see an EMPTY leads list.
--     Expected: 0 rows.
SELECT u.UserName, u.Email
FROM AspNetUsers u
JOIN AspNetUserRoles ur ON ur.UserId = u.Id
JOIN AspNetRoles r      ON r.Id = ur.RoleId AND r.Name = 'CRM_View'
WHERE NOT EXISTS (
        SELECT 1 FROM AspNetUserRoles x
        JOIN AspNetRoles xr ON xr.Id = x.RoleId AND xr.Name = 'CRM_Leads_ViewAll'
        WHERE x.UserId = u.Id)
  AND NOT EXISTS (
        SELECT 1 FROM PARTY_RELATIONSHIP pr
        WHERE pr.PARTY_ID_FROM = u.PartyId
          AND pr.PARTY_RELATIONSHIP_TYPE_ID = 'LEAD_OWNER'
          AND pr.THRU_DATE IS NULL)
ORDER BY u.UserName;

-- ============================================================
-- NOTE: users must sign out and back in for the new roles to take
-- effect - roles are baked into the JWT at login and the token has a
-- 7-day lifetime.
--
-- After sign-off: DROP TABLE AspNetUserRoles_bak_20260824;
-- ============================================================
