-- ============================================================
-- Sales Commission security roles
-- Date: 2026-09-15
-- ============================================================
-- WHAT THIS DOES
--   1. Creates the 6 Sales Commission roles (no-op if they already
--      exist - the app seed also creates them on startup).
--   2. Grants view / create / update to 4 users:
--        Amr Ghali      aghali@gmail.com
--        Ashraf Agiba   ashrafa@gmail.com
--        Ahmad Agiba    aagiba@gmail.com
--        Abulla Adel    aadel@gmail.com
--   3. Grants approve / reset / delete to Ahmad Agiba ONLY.
--   4. Grants all 6 to eradwan1967@gmail.com (admin).
--
-- THE ROLE SET (6 roles):
--   ViewSalesCommission, CreateSalesCommission, UpdateSalesCommission,
--   ApproveSalesCommission, ResetSalesCommission, DeleteSalesCommission
--
-- ============================================================
-- !! READ BEFORE RUNNING !!
-- ============================================================
-- The frontend build that introduces these roles narrows access to
-- /sales-commissions: today anyone with Sales_View reaches it; after
-- the deploy ONLY holders of View/Create/UpdateSalesCommission do,
-- and the Approve / Reset / Delete actions are hidden from everyone
-- except holders of the matching role. Run this script BEFORE (or
-- together with) that deploy, otherwise the 4 users above lose the
-- page until it is run.
--
-- Enforcement is frontend-only at this stage (the API still relies on
-- the global authenticated-user filter) - backend [Authorize] is part
-- of the version-2 work.
--
-- All 4 users already hold Sales_View (the module wrapper the route
-- lives under) in production - PART 1 verifies this.
--
-- HOW TO RUN
--   1. Run PART 1 (read-only). Inspect every result set.
--   2. Only if PART 1 matches expectations, run PART 2.
--   3. Run PART 3 (read-only) to confirm the end state.
--   4. Take a backup of AspNetUserRoles first - see PART 0.
--
-- Users are resolved by NormalizedEmail and roles by NormalizedName,
-- not by hard-coded Id, so the script is safe in dev and production.
-- Every insert is INSERT ... SELECT ... WHERE NOT EXISTS, so it is
-- idempotent - re-running it is harmless.
-- ============================================================


-- ============================================================
-- PART 0 - BACKUP (run first, keep until the change is signed off)
-- ============================================================
DROP TABLE IF EXISTS AspNetUserRoles_bak_20260915;
CREATE TABLE AspNetUserRoles_bak_20260915 AS SELECT * FROM AspNetUserRoles;

-- Rollback, if ever needed:
--   DELETE FROM AspNetUserRoles;
--   INSERT INTO AspNetUserRoles SELECT * FROM AspNetUserRoles_bak_20260915;
--   DELETE FROM AspNetRoles WHERE Name LIKE '%SalesCommission';


-- ============================================================
-- PART 1 - VERIFICATION (read-only, run first)
-- ============================================================

-- 1a. The 5 target users exist and hold Sales_View.
--     Expected: 5 rows, has_sales_view = 1 on every row.
SELECT u.Email, u.DisplayName,
       EXISTS (SELECT 1 FROM AspNetUserRoles ur
               JOIN AspNetRoles r ON r.Id = ur.RoleId
               WHERE ur.UserId = u.Id AND r.NormalizedName = 'SALES_VIEW') AS has_sales_view
FROM AspNetUsers u
WHERE u.NormalizedEmail IN ('AGHALI@GMAIL.COM', 'ASHRAFA@GMAIL.COM', 'AAGIBA@GMAIL.COM',
                            'AADEL@GMAIL.COM', 'ERADWAN1967@GMAIL.COM')
ORDER BY u.Email;

-- 1b. Which of the 6 roles already exist.
--     Expected: 0 rows on a box that has not yet started the new build,
--     6 rows if the app seed already ran.
SELECT Id, Name FROM AspNetRoles WHERE Name LIKE '%SalesCommission' ORDER BY Name;

-- 1c. Nobody holds any of the 6 roles yet (unless the seed ran for admin).
SELECT u.Email, r.Name
FROM AspNetUserRoles ur
JOIN AspNetUsers u ON u.Id = ur.UserId
JOIN AspNetRoles r ON r.Id = ur.RoleId
WHERE r.Name LIKE '%SalesCommission'
ORDER BY u.Email, r.Name;


-- ============================================================
-- PART 2 - CHANGES
-- ============================================================

-- 2a. Create the roles that are missing. Id is a GUID string like the
--     rows Identity creates; ConcurrencyStamp is left NULL as the
--     existing seeded roles have it.
INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
SELECT UUID(), n.Name, UPPER(n.Name), NULL
FROM (SELECT 'ViewSalesCommission'    AS Name UNION ALL
      SELECT 'CreateSalesCommission'          UNION ALL
      SELECT 'UpdateSalesCommission'          UNION ALL
      SELECT 'ApproveSalesCommission'         UNION ALL
      SELECT 'ResetSalesCommission'           UNION ALL
      SELECT 'DeleteSalesCommission') n
WHERE NOT EXISTS (SELECT 1 FROM AspNetRoles r WHERE r.NormalizedName = UPPER(n.Name));

-- 2b. View / Create / Update for the 4 users + admin.
INSERT INTO AspNetUserRoles (UserId, RoleId)
SELECT u.Id, r.Id
FROM AspNetUsers u
JOIN AspNetRoles r
  ON r.NormalizedName IN ('VIEWSALESCOMMISSION', 'CREATESALESCOMMISSION', 'UPDATESALESCOMMISSION')
WHERE u.NormalizedEmail IN ('AGHALI@GMAIL.COM', 'ASHRAFA@GMAIL.COM', 'AAGIBA@GMAIL.COM',
                            'AADEL@GMAIL.COM', 'ERADWAN1967@GMAIL.COM')
  AND NOT EXISTS (SELECT 1 FROM AspNetUserRoles x WHERE x.UserId = u.Id AND x.RoleId = r.Id);

-- 2c. Approve / Reset / Delete for Ahmad Agiba and admin only.
INSERT INTO AspNetUserRoles (UserId, RoleId)
SELECT u.Id, r.Id
FROM AspNetUsers u
JOIN AspNetRoles r
  ON r.NormalizedName IN ('APPROVESALESCOMMISSION', 'RESETSALESCOMMISSION', 'DELETESALESCOMMISSION')
WHERE u.NormalizedEmail IN ('AAGIBA@GMAIL.COM', 'ERADWAN1967@GMAIL.COM')
  AND NOT EXISTS (SELECT 1 FROM AspNetUserRoles x WHERE x.UserId = u.Id AND x.RoleId = r.Id);


-- ============================================================
-- PART 3 - CONFIRM END STATE (read-only)
-- ============================================================

-- 3a. Expected: 6 rows.
SELECT Id, Name, NormalizedName FROM AspNetRoles WHERE Name LIKE '%SalesCommission' ORDER BY Name;

-- 3b. Expected:
--   aadel@gmail.com        Create, Update, View                       (3)
--   aagiba@gmail.com       Approve, Create, Delete, Reset, Update, View (6)
--   aghali@gmail.com       Create, Update, View                       (3)
--   ashrafa@gmail.com      Create, Update, View                       (3)
--   eradwan1967@gmail.com  all 6                                      (6)
SELECT u.Email, COUNT(*) AS role_count,
       GROUP_CONCAT(REPLACE(r.Name, 'SalesCommission', '') ORDER BY r.Name SEPARATOR ', ') AS commission_roles
FROM AspNetUserRoles ur
JOIN AspNetUsers u ON u.Id = ur.UserId
JOIN AspNetRoles r ON r.Id = ur.RoleId
WHERE r.Name LIKE '%SalesCommission'
GROUP BY u.Email
ORDER BY u.Email;
