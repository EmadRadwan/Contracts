-- Grant the Project Report feature to Abulla Adel and Amr Ghali.
--
-- Required roles for the Project Report (client-app/src/features/Projects/dashboard/ProjectsList.tsx):
--   Projects_View     - module wrapper: shows the Projects nav link + /projects route (RequireRole)
--   RunProjectReport  - feature role: renders the "Project Report" button (<Can perform="RunProjectReport">)
-- No endpoint-level [Authorize(Roles=...)] exists on ProjectController, so nothing else is needed server-side.
-- Both users already hold Projects_View (verified on localhost 2026-09-16); this adds RunProjectReport.
--
-- Idempotent: safe to re-run. Keyed by email so it works on prod where user Ids may differ from the seed.
-- Run:  MYSQL_PWD='<pwd>' mysql -h <host> -u root -D erp_contracts < scripts/grant_run_project_report.sql
-- Users must log out / back in afterwards - roles travel on the JWT (TokenService) and are not re-read mid-session.

START TRANSACTION;

-- Backup, following the existing AspNetUserRoles_bak_YYYYMMDD convention.
CREATE TABLE IF NOT EXISTS AspNetUserRoles_bak_20260916_runprojectreport AS
    SELECT * FROM AspNetUserRoles;

-- Role exists in every environment (created unconditionally on startup by SeedContracts.requiredRoles),
-- but create it if missing so the script is standalone.
INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
SELECT UUID(), 'RunProjectReport', 'RUNPROJECTREPORT', UUID()
WHERE NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE NormalizedName = 'RUNPROJECTREPORT');

-- Grant both roles (Projects_View is a no-op where already present).
INSERT INTO AspNetUserRoles (UserId, RoleId)
SELECT u.Id, r.Id
FROM AspNetUsers u
CROSS JOIN AspNetRoles r
WHERE u.NormalizedEmail IN ('AADEL@GMAIL.COM', 'AGHALI@GMAIL.COM')
  AND r.NormalizedName IN ('PROJECTS_VIEW', 'RUNPROJECTREPORT')
  AND NOT EXISTS (
      SELECT 1 FROM AspNetUserRoles ur WHERE ur.UserId = u.Id AND ur.RoleId = r.Id
  );

COMMIT;

-- Verification
SELECT u.DisplayName, u.Email, r.Name AS Role
FROM AspNetUserRoles ur
JOIN AspNetUsers u ON u.Id = ur.UserId
JOIN AspNetRoles r ON r.Id = ur.RoleId
WHERE u.NormalizedEmail IN ('AADEL@GMAIL.COM', 'AGHALI@GMAIL.COM')
  AND r.Name IN ('Projects_View', 'RunProjectReport')
ORDER BY u.DisplayName, r.Name;
