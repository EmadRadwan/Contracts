-- Copy all app roles of aagiba@gmail.com to aghali@gmail.com and mhamdy@gmail.com (2026-10-31)
-- Additive only: INSERT IGNORE skips roles a target already has; safe to re-run.
-- Identity tables are mixed-case (AspNetUsers, AspNetUserRoles, AspNetRoles) - keep exact case for prod Linux MySQL.
-- Users may need to log out/in for new roles to take effect.

-- 1) Preview: source user's roles, and that all 3 users exist (expect 3 rows in the second query)
SELECT U.Email, R.Name AS RoleName
FROM AspNetUsers U
JOIN AspNetUserRoles UR ON UR.UserId = U.Id
JOIN AspNetRoles R      ON R.Id = UR.RoleId
WHERE U.NormalizedEmail = 'AAGIBA@GMAIL.COM';

SELECT Id, Email FROM AspNetUsers
WHERE NormalizedEmail IN ('AAGIBA@GMAIL.COM','AGHALI@GMAIL.COM','MHAMDY@GMAIL.COM');

-- 2) Copy the roles
START TRANSACTION;

INSERT IGNORE INTO AspNetUserRoles (UserId, RoleId)
SELECT T.Id, UR.RoleId
FROM AspNetUsers S
JOIN AspNetUserRoles UR ON UR.UserId = S.Id
JOIN AspNetUsers T      ON T.NormalizedEmail IN ('AGHALI@GMAIL.COM','MHAMDY@GMAIL.COM')
WHERE S.NormalizedEmail = 'AAGIBA@GMAIL.COM';

-- 3) Verify
SELECT U.Email, R.Name AS RoleName
FROM AspNetUsers U
JOIN AspNetUserRoles UR ON UR.UserId = U.Id
JOIN AspNetRoles R      ON R.Id = UR.RoleId
WHERE U.NormalizedEmail IN ('AAGIBA@GMAIL.COM','AGHALI@GMAIL.COM','MHAMDY@GMAIL.COM')
ORDER BY U.Email, R.Name;

COMMIT;  -- or ROLLBACK if step 3 looks wrong
