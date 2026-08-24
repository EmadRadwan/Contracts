-- ============================================================
-- CRM role realignment + role copy
-- Date: 2026-08-23
-- ============================================================
-- WHAT THIS DOES
--   1. Makes eradwan1967@gmail.com (Emad) and rmostafa@gmail.com (Ayman)
--      the ONLY holders of the 9 CRM roles ("CRM admin" set).
--   2. Revokes all 9 CRM roles from every other user.
--   3. Copies every role held by nevine@gmail.com to rmostafa@gmail.com
--      (additive - Ayman keeps what he already has).
--
-- THE "CRM ADMIN" ROLE SET (9 roles, all currently exist in AspNetRoles):
--   CRM_View
--   CRM_Leads_View,    CRM_Leads_Create,    CRM_Leads_Edit,    CRM_Leads_Delete
--   CRM_Contacts_View, CRM_Contacts_Create, CRM_Contacts_Edit, CRM_Contacts_Delete
--
-- ============================================================
-- !! READ BEFORE RUNNING !!
-- ============================================================
-- This revokes CRM access from 11 accounts. Because the application
-- gates the entire CRM module on CRM_View at the route level, those
-- 11 users LOSE ALL ACCESS to Leads and Sales Opportunities:
--
--   Ahlam, Ahmad, Alaa, Eman, Gamal, Mark,
--   Medhat, Nagy, Peter, Reham, Yasmin
--
-- Note in particular:
--   * Ahmad (aagiba@gmail.com) is the second-most-privileged account
--     in the system (41 roles). He keeps his other 33 roles but loses
--     CRM entirely. Confirm this is intended.
--   * The other 10 are the CRM-only sales accounts - CRM is the only
--     thing they can reach, so after this script they can log in and
--     do nothing at all. Consider disabling those logins separately
--     if that is the actual intent.
--
-- HOW TO RUN
--   1. Run PART 1 (read-only). Inspect every result set.
--   2. Only if PART 1 matches expectations, run PART 2.
--   3. Run PART 3 (read-only) to confirm the end state.
--   4. Take a backup of AspNetUserRoles first - see PART 0.
--
-- Users are resolved by NormalizedEmail, not by hard-coded Id, so the
-- script is safe to run in dev and production without editing.
-- ============================================================


-- ============================================================
-- PART 0 - BACKUP (run first, keep until the change is signed off)
-- ============================================================
DROP TABLE IF EXISTS AspNetUserRoles_bak_20260823;
CREATE TABLE AspNetUserRoles_bak_20260823 AS SELECT * FROM AspNetUserRoles;

-- Rollback, if ever needed:
--   DELETE FROM AspNetUserRoles;
--   INSERT INTO AspNetUserRoles SELECT * FROM AspNetUserRoles_bak_20260823;


-- ============================================================
-- PART 1 - VERIFICATION (read-only, run first)
-- ============================================================

-- 1a. All three users must exist. Expected: exactly 3 rows.
SELECT Id, UserName, Email
FROM AspNetUsers
WHERE NormalizedEmail IN ('ERADWAN1967@GMAIL.COM','RMOSTAFA@GMAIL.COM','NEVINE@GMAIL.COM');
-- Expected in dev:
--   3bb4e859-1157-4cc7-81b5-10f419359a41  Emad    eradwan1967@gmail.com
--   63df1d61-1cc5-4b9b-96f1-164f37a612b6  Ayman   rmostafa@gmail.com
--   88a66f72-ee3c-4410-a647-a52caf42a5e2  nevine  nevine@gmail.com

-- 1b. All 9 CRM roles must exist. Expected: 9 rows.
--     A missing role here is NOT an error at run time - the
--     INSERT..SELECT simply yields 0 rows and that role is silently
--     never granted. So check the count.
SELECT Name FROM AspNetRoles
WHERE Name IN ('CRM_View',
               'CRM_Leads_View','CRM_Leads_Create','CRM_Leads_Edit','CRM_Leads_Delete',
               'CRM_Contacts_View','CRM_Contacts_Create','CRM_Contacts_Edit','CRM_Contacts_Delete')
ORDER BY Name;

-- 1c. Who holds CRM roles right now, and how many.
SELECT u.UserName, u.Email, COUNT(*) AS crm_role_count
FROM AspNetUsers u
JOIN AspNetUserRoles ur ON ur.UserId = u.Id
JOIN AspNetRoles r      ON r.Id = ur.RoleId
WHERE r.Name LIKE 'CRM%'
GROUP BY u.Id, u.UserName, u.Email
ORDER BY u.UserName;
-- Expected in dev: 13 rows. Emad 9, Ayman 9, Ahmad 8, the other 10 at 9 each.

-- 1d. EXACTLY who will lose CRM access. Review this list carefully.
SELECT DISTINCT u.UserName, u.Email
FROM AspNetUsers u
JOIN AspNetUserRoles ur ON ur.UserId = u.Id
JOIN AspNetRoles r      ON r.Id = ur.RoleId
WHERE r.Name LIKE 'CRM%'
  AND u.NormalizedEmail NOT IN ('ERADWAN1967@GMAIL.COM','RMOSTAFA@GMAIL.COM')
ORDER BY u.UserName;
-- Expected in dev: 11 rows (Ahlam, Ahmad, Alaa, Eman, Gamal, Mark,
--                           Medhat, Nagy, Peter, Reham, Yasmin)

-- 1e. Which of nevine's roles Ayman does NOT already have (these get added).
SELECT r.Name AS role_to_be_added
FROM AspNetUserRoles nur
JOIN AspNetUsers  n ON n.Id = nur.UserId AND n.NormalizedEmail = 'NEVINE@GMAIL.COM'
JOIN AspNetRoles  r ON r.Id = nur.RoleId
WHERE NOT EXISTS (
    SELECT 1 FROM AspNetUserRoles aur
    JOIN AspNetUsers a ON a.Id = aur.UserId AND a.NormalizedEmail = 'RMOSTAFA@GMAIL.COM'
    WHERE aur.RoleId = nur.RoleId)
ORDER BY r.Name;
-- Expected in dev: 4 rows - Catalog_View, CreateReserveRequest,
--                           CreateSalesRequest, Sales_View


-- ============================================================
-- PART 2 - APPLY (run only after PART 1 is clean)
-- ============================================================
START TRANSACTION;

-- 2a. Grant the full 9-role CRM admin set to the two designated users.
--     Idempotent: the NOT EXISTS guard skips roles already held.
INSERT INTO AspNetUserRoles (UserId, RoleId)
SELECT u.Id, r.Id
FROM AspNetUsers u
CROSS JOIN AspNetRoles r
WHERE u.NormalizedEmail IN ('ERADWAN1967@GMAIL.COM','RMOSTAFA@GMAIL.COM')
  AND r.Name IN ('CRM_View',
                 'CRM_Leads_View','CRM_Leads_Create','CRM_Leads_Edit','CRM_Leads_Delete',
                 'CRM_Contacts_View','CRM_Contacts_Create','CRM_Contacts_Edit','CRM_Contacts_Delete')
  AND NOT EXISTS (
      SELECT 1 FROM AspNetUserRoles ur
      WHERE ur.UserId = u.Id AND ur.RoleId = r.Id);
-- Expected in dev: 0 rows affected (both users already hold all 9).

-- 2b. Revoke every CRM role from everyone else.
DELETE ur
FROM AspNetUserRoles ur
JOIN AspNetUsers u ON u.Id = ur.UserId
JOIN AspNetRoles r ON r.Id = ur.RoleId
WHERE r.Name LIKE 'CRM%'
  AND u.NormalizedEmail NOT IN ('ERADWAN1967@GMAIL.COM','RMOSTAFA@GMAIL.COM');
-- Expected in dev: 98 rows affected (10 users x 9 roles + Ahmad's 8).

-- 2c. Copy every role held by nevine to Ayman (additive, idempotent).
--     Deliberately NOT restricted to CRM roles - this copies the whole
--     role set as requested.
INSERT INTO AspNetUserRoles (UserId, RoleId)
SELECT a.Id, nur.RoleId
FROM AspNetUserRoles nur
JOIN AspNetUsers n ON n.Id = nur.UserId AND n.NormalizedEmail = 'NEVINE@GMAIL.COM'
CROSS JOIN AspNetUsers a
WHERE a.NormalizedEmail = 'RMOSTAFA@GMAIL.COM'
  AND NOT EXISTS (
      SELECT 1 FROM AspNetUserRoles ur
      WHERE ur.UserId = a.Id AND ur.RoleId = nur.RoleId);
-- Expected in dev: 4 rows affected.

COMMIT;
-- If any count above looks wrong, run ROLLBACK; instead of COMMIT;


-- ============================================================
-- PART 3 - POST-RUN VERIFICATION (read-only)
-- ============================================================

-- 3a. CRM roles must now be held by exactly 2 users, 9 roles each.
SELECT u.UserName, u.Email, COUNT(*) AS crm_role_count
FROM AspNetUsers u
JOIN AspNetUserRoles ur ON ur.UserId = u.Id
JOIN AspNetRoles r      ON r.Id = ur.RoleId
WHERE r.Name LIKE 'CRM%'
GROUP BY u.Id, u.UserName, u.Email
ORDER BY u.UserName;
-- Expected: exactly 2 rows - Ayman 9, Emad 9.

-- 3b. Nobody else retains any CRM role. Expected: 0 rows.
SELECT u.UserName, u.Email, r.Name
FROM AspNetUsers u
JOIN AspNetUserRoles ur ON ur.UserId = u.Id
JOIN AspNetRoles r      ON r.Id = ur.RoleId
WHERE r.Name LIKE 'CRM%'
  AND u.NormalizedEmail NOT IN ('ERADWAN1967@GMAIL.COM','RMOSTAFA@GMAIL.COM');

-- 3c. Ayman must now hold every role nevine holds. Expected: 0 rows.
SELECT r.Name AS missing_from_ayman
FROM AspNetUserRoles nur
JOIN AspNetUsers n ON n.Id = nur.UserId AND n.NormalizedEmail = 'NEVINE@GMAIL.COM'
JOIN AspNetRoles r ON r.Id = nur.RoleId
WHERE NOT EXISTS (
    SELECT 1 FROM AspNetUserRoles aur
    JOIN AspNetUsers a ON a.Id = aur.UserId AND a.NormalizedEmail = 'RMOSTAFA@GMAIL.COM'
    WHERE aur.RoleId = nur.RoleId);

-- 3d. Ayman's full role list after the change.
SELECT r.Name
FROM AspNetUsers u
JOIN AspNetUserRoles ur ON ur.UserId = u.Id
JOIN AspNetRoles r      ON r.Id = ur.RoleId
WHERE u.NormalizedEmail = 'RMOSTAFA@GMAIL.COM'
ORDER BY r.Name;
-- Expected in dev: 13 roles - the 9 CRM roles plus Catalog_View,
--                  CreateReserveRequest, CreateSalesRequest, Sales_View.

-- 3e. Users left with a login but no roles at all - review whether
--     these accounts should be disabled.
SELECT u.UserName, u.Email
FROM AspNetUsers u
LEFT JOIN AspNetUserRoles ur ON ur.UserId = u.Id
GROUP BY u.Id, u.UserName, u.Email
HAVING COUNT(ur.RoleId) = 0
ORDER BY u.UserName;
-- Expected in dev: the 10 CRM-only sales accounts (Ahlam, Alaa, Eman,
--   Gamal, Mark, Medhat, Nagy, Peter, Reham, Yasmin) - CRM was all
--   they had. Ahmad is NOT expected here; he keeps 33 other roles.

-- ============================================================
-- After sign-off:  DROP TABLE AspNetUserRoles_bak_20260823;
-- ============================================================
