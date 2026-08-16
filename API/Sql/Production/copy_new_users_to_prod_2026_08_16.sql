-- ============================================================
-- Copy dev-created user accounts to production
-- Accounts: rawan (fictitious HR/Payroll test account) +
--           11 CRM accounts linked to real employee PartyIds
-- ============================================================
-- HOW TO RUN:
--   1. Run PART 1 (verification, read-only) first.
--   2. Inspect every result set. Do NOT run PART 2/3 if:
--        - any PartyId is missing or the Description doesn't match
--        - any UserName/Email/Id already exists
--        - any required role name is missing from AspNetRoles
--   3. If a role is missing, create it first (via the app's
--      "createRole" endpoint or admin UI) before running PART 3,
--      otherwise that role simply won't be assigned (no error,
--      the INSERT...SELECT for that row just yields 0 rows).
--   4. Only after PART 1 looks clean, run PART 2 then PART 3.
-- ============================================================


-- ============================================================
-- PART 1 — VERIFICATION (read-only, run first)
-- ============================================================

-- 1a. Confirm these PartyIds exist in production with matching names
SELECT PARTY_ID, DESCRIPTION
FROM PARTY
WHERE PARTY_ID IN ('300','317','318','338','344','345','346','347','349','509','510')
ORDER BY PARTY_ID;
-- Expected (11 rows):
--   300  احمد ناجي مبروك
--   317  ايمان صلاح عاشور عبدالوهاب
--   318  ريهام وسيم صدقى بطرس
--   338  ياسمين ياسر امين طوطح
--   344  محمد جمال محمد حسين
--   345  محمد مدحت محمد  الغندور
--   346  ريهام ايمن اسماعيل مصطفى
--   347  الاء سمير زكى على محمد
--   349  مارك سعيد تادرس
--   509  احلام ناجى
--   510  بيتر هانى

-- 1b. Confirm none of these Ids / usernames / emails already exist in prod (should return 0 rows)
SELECT Id, UserName, Email FROM AspNetUsers
WHERE Id IN (
    'c99d1bc5-0b70-4630-a7a0-fe6470600813', -- rawan
    '328d4d00-fc0d-45c3-b12a-09164bf62fb8', -- Nagy
    '59c58656-c8d7-4236-be11-6b1f859ca3f1', -- Eman
    '318e0db6-df8a-4ccd-abdf-061d1e072c4b', -- Reham
    '5a08ffb2-db92-401e-8078-68b7150051a4', -- Yasmin
    '2e70cd41-517b-43b3-9755-a604ca83f0e5', -- Gamal
    '4b5b8b23-7750-46f9-a7f9-09e621077642', -- Medhat
    '63df1d61-1cc5-4b9b-96f1-164f37a612b6', -- Ayman
    'a5bcc7ed-28f3-4a82-b74f-d24fa5f99695', -- Alaa
    'd479dc43-f34a-44d2-b478-003585e4df54', -- Mark
    '6e3e81be-f19e-43d0-a7d0-2156187f8dfe', -- Ahlam
    '971000f3-7eca-4b99-936e-4bb524fc0f3d'  -- Peter
)
OR NormalizedUserName IN ('RAWAN','NAGY','EMAN','REHAM','YASMIN','GAMAL','MEDHAT','AYMAN','ALAA','MARK','AHLAM','PETER')
OR NormalizedEmail IN (
    'RAWAN@GMAIL.COM','AMABROUK@GMAIL.COM','EABDELWAHAB@GMAIL.COM','RBOUTROS@GMAIL.COM',
    'YTAWTAH@GMAIL.COM','MHUSSEIN@GMAIL.COM','MELGHANDOUR@GMAIL.COM','RMOSTAFA@GMAIL.COM',
    'AMOHAMED@GMAIL.COM','MTADROS@GMAIL.COM','ANAGY@GMAIL.COM','PHANY@GMAIL.COM'
);

-- 1c. Confirm all required roles exist in prod's AspNetRoles (should return all 14 names below)
SELECT Name FROM AspNetRoles
WHERE Name IN (
    'HR_View','ViewEmployeeAdvances','Accounting_Payroll_Run_View','Accounting_View','Party_View',
    'CRM_View','CRM_Leads_View','CRM_Leads_Create','CRM_Leads_Edit','CRM_Leads_Delete',
    'CRM_Contacts_View','CRM_Contacts_Create','CRM_Contacts_Edit','CRM_Contacts_Delete'
)
ORDER BY Name;
-- If fewer than 14 rows come back, note which names are missing before proceeding to PART 3.
-- (HR_View / ViewEmployeeAdvances / Accounting_Payroll_Run_View were NOT in the auto-seed list
--  as of this writing, so they may not exist in prod unless created manually before.)


-- ============================================================
-- PART 2 — INSERT USERS
-- All 12 accounts use the same default password as dev: Pa$$w0rd
-- (MustChangePassword = 1, so each will be forced to change it on first login)
-- ============================================================

INSERT INTO AspNetUsers
(Id, DisplayName, MustChangePassword, PartyId, OrganizationPartyId, DualLanguage, ProductStoreId, UserLoginId,
 UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp,
 PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnd, LockoutEnabled, AccessFailedCount,
 CreatedStamp, LastUpdatedStamp)
VALUES
('c99d1bc5-0b70-4630-a7a0-fe6470600813', 'Rawan', 1, NULL, 'Company', 'N', NULL, NULL,
 'rawan', 'RAWAN', 'rawan@gmail.com', 'RAWAN@GMAIL.COM', 1,
 'AQAAAAIAAYagAAAAEMpb8cCZp9QnDZpL8dpwhv9LKCoT9H9cm6rLap2xSFEaaEMXf1FxRBvAhQ9PAJH/ow==',
 'BIR4HPM7EGFABV7VNVGSYYQTVHVVCUBX', '3fc6b8b3-f875-477c-8634-f092a3795a87',
 NULL, 0, 0, NULL, 1, 0, '2026-08-16 14:48:03', '2026-08-16 14:48:03'),

('328d4d00-fc0d-45c3-b12a-09164bf62fb8', 'Ahmed Nagy', 1, '300', 'Company', NULL, NULL, NULL,
 'Nagy', 'NAGY', 'amabrouk@gmail.com', 'AMABROUK@GMAIL.COM', 1,
 'AQAAAAIAAYagAAAAEPq1a/A6QuFHLrBfjXcLaPXK7hQ6UvFeY018eoWMBQYfjzG6s2x7OZ4eYzfy5g+3Jg==',
 '4GQSTEL45BF7OTGOP2RWJAUT73IIIPJM', '78eee277-f09d-45c6-915d-940c8b33ff4b',
 NULL, 0, 0, NULL, 1, 0, '2026-08-16 15:55:48', '2026-08-16 15:55:48'),

('59c58656-c8d7-4236-be11-6b1f859ca3f1', 'Eman Abdelwahab', 1, '317', 'Company', NULL, NULL, NULL,
 'Eman', 'EMAN', 'eabdelwahab@gmail.com', 'EABDELWAHAB@GMAIL.COM', 1,
 'AQAAAAIAAYagAAAAEEawKlxDHXZc1xCa0UJ0aLbulPLKHOFTeq6bVnWOt0QlhjO4Nph8lpmelqQCkPi9Ww==',
 'IDYRRJ7LDCY5YEFMVZCRBLZW6W27UW2S', 'e636d943-634f-48cc-973d-bc7b0bb44605',
 NULL, 0, 0, NULL, 1, 0, '2026-08-16 15:55:49', '2026-08-16 15:55:49'),

('318e0db6-df8a-4ccd-abdf-061d1e072c4b', 'Reham Boutros', 1, '318', 'Company', NULL, NULL, NULL,
 'Reham', 'REHAM', 'rboutros@gmail.com', 'RBOUTROS@GMAIL.COM', 1,
 'AQAAAAIAAYagAAAAEG34HROlcoirBPEnlUh8UzTKCWzCgEe5eOBEu0bKuxNY9XLgcL9E9v0mOlAd+hIFRw==',
 'FMAMKW46NB7TXB3V722M2JWNIIA5MLGC', '8f10c5b7-3c35-452c-bc2e-05515f03e563',
 NULL, 0, 0, NULL, 1, 0, '2026-08-16 15:55:49', '2026-08-16 15:55:49'),

('5a08ffb2-db92-401e-8078-68b7150051a4', 'Yasmin Tawtah', 1, '338', 'Company', NULL, NULL, NULL,
 'Yasmin', 'YASMIN', 'ytawtah@gmail.com', 'YTAWTAH@GMAIL.COM', 1,
 'AQAAAAIAAYagAAAAECwxPfkSJ7QORaJNe5JtmTwsFEDsgNwWMa0+gHrysBkWaDzbMCboruybfrWGPjyXoA==',
 'DSTDDUEGUTLCDHKXYEXH5MFJNPMZM6ES', 'efcac274-ceda-4e7e-925c-d9bfd9286a18',
 NULL, 0, 0, NULL, 1, 0, '2026-08-16 15:55:49', '2026-08-16 15:55:49'),

('2e70cd41-517b-43b3-9755-a604ca83f0e5', 'Mohamed Gamal', 1, '344', 'Company', NULL, NULL, NULL,
 'Gamal', 'GAMAL', 'mhussein@gmail.com', 'MHUSSEIN@GMAIL.COM', 1,
 'AQAAAAIAAYagAAAAEPZbG2QLxFsfGzccZ2lmktSNLw1xoUAhcyxvIscjAiyN0X/taZVVKA7x4pOtC+o1Og==',
 'G3D4PIJUD7A6PIW3RJIEE2H5UUTSA427', 'c77cb379-33f1-49bf-8228-0319c358e349',
 NULL, 0, 0, NULL, 1, 0, '2026-08-16 15:55:50', '2026-08-16 15:55:50'),

('4b5b8b23-7750-46f9-a7f9-09e621077642', 'Mohamed Medhat', 1, '345', 'Company', NULL, NULL, NULL,
 'Medhat', 'MEDHAT', 'melghandour@gmail.com', 'MELGHANDOUR@GMAIL.COM', 1,
 'AQAAAAIAAYagAAAAEM6hOBFYlrDazvjaqW1SHbyda67uklH0GAHe1Sr8ditPSqCfcvvo4QNeo3e+7fgoZQ==',
 'S63VPANUK6FGXJHHDFVOZWE3WFWVSCFM', 'd8efd758-43ba-42ac-9415-7193ce773aae',
 NULL, 0, 0, NULL, 1, 0, '2026-08-16 15:55:50', '2026-08-16 15:55:50'),

('63df1d61-1cc5-4b9b-96f1-164f37a612b6', 'Reham Mostafa', 1, '346', 'Company', NULL, NULL, NULL,
 'Ayman', 'AYMAN', 'rmostafa@gmail.com', 'RMOSTAFA@GMAIL.COM', 1,
 'AQAAAAIAAYagAAAAEPl9CRi0KgauQ2BIasDIXGBc7T+j9fGS0fBQtxBXA5hGL442+uGir8d3F15lpJscvQ==',
 'SCKYINN3FBEGZBDUBISOS26D4M6JBJHB', '20d8b4a9-bea8-4cce-b2a9-f4721e065247',
 NULL, 0, 0, NULL, 1, 0, '2026-08-16 15:55:51', '2026-08-16 15:55:51'),

('a5bcc7ed-28f3-4a82-b74f-d24fa5f99695', 'Alaa Mohamed', 1, '347', 'Company', NULL, NULL, NULL,
 'Alaa', 'ALAA', 'amohamed@gmail.com', 'AMOHAMED@GMAIL.COM', 1,
 'AQAAAAIAAYagAAAAEOUlkI+k9YH+n9ZJZTzz9UnwU64zJOctFWNTzEds+HD+ORApL8rTC0uePbDZI8Cz9Q==',
 'NLXA76EQUARTXXNSFEL44V6EJFJALZDM', '3e36da63-2df2-412d-975b-38e6ce385ae9',
 NULL, 0, 0, NULL, 1, 0, '2026-08-16 15:55:51', '2026-08-16 15:55:51'),

('d479dc43-f34a-44d2-b478-003585e4df54', 'Mark Tadros', 1, '349', 'Company', NULL, NULL, NULL,
 'Mark', 'MARK', 'mtadros@gmail.com', 'MTADROS@GMAIL.COM', 1,
 'AQAAAAIAAYagAAAAEOjl4+GlF1JWn++lnskb8Rf5hh6OhcIuO4EyXM4ApgnYbZry/z9O47ELbboVGMi/gA==',
 'SGX3W5KSNCKWYHMOOCEHFKZCERXPFCNP', '4925c879-1f91-4e2f-bc73-56eab0c85e3f',
 NULL, 0, 0, NULL, 1, 0, '2026-08-16 15:55:51', '2026-08-16 15:55:51'),

('6e3e81be-f19e-43d0-a7d0-2156187f8dfe', 'Ahlam Nagy', 1, '509', 'Company', NULL, NULL, NULL,
 'Ahlam', 'AHLAM', 'anagy@gmail.com', 'ANAGY@GMAIL.COM', 1,
 'AQAAAAIAAYagAAAAELi9tlYm2hLVmpJuNvbjtPAqUcr/fOuSi3DY4hMTz7uQbjgJNYN8Aes8SLMWK6lslQ==',
 'QTQSXWLMIYW6VLZICTG4KGILV2GFF6SU', 'afb90715-c3ff-440e-8e36-ddc618df2e62',
 NULL, 0, 0, NULL, 1, 0, '2026-08-16 15:55:52', '2026-08-16 15:55:52'),

('971000f3-7eca-4b99-936e-4bb524fc0f3d', 'Peter Hany', 1, '510', 'Company', NULL, NULL, NULL,
 'Peter', 'PETER', 'phany@gmail.com', 'PHANY@GMAIL.COM', 1,
 'AQAAAAIAAYagAAAAEJXGWb28IKe7Ul/YP+hGiy50fzmdQSlXbrs07ENPSHbhskq5kB5ic+MBvyoFW7ziMQ==',
 'JSEPESHL7R2TSDXNX72IE7AA5IZKN55Z', '90476513-6667-434a-b73d-de924e9e416d',
 NULL, 0, 0, NULL, 1, 0, '2026-08-16 15:55:52', '2026-08-16 15:55:52');


-- ============================================================
-- PART 3 — INSERT ROLE ASSIGNMENTS
-- Looks up each role's Id by NAME in prod's own AspNetRoles table
-- (prod's role GUIDs differ from dev's, so we never hardcode RoleId).
-- If a role name doesn't exist yet in prod, that (UserId, RoleId)
-- pair is silently skipped — check PART 1c output first.
-- ============================================================

INSERT INTO AspNetUserRoles (UserId, RoleId)
SELECT u.UserId, r.Id
FROM (
    SELECT 'c99d1bc5-0b70-4630-a7a0-fe6470600813' AS UserId, 'Party_View' AS RoleName UNION ALL
    SELECT 'c99d1bc5-0b70-4630-a7a0-fe6470600813', 'HR_View' UNION ALL
    SELECT 'c99d1bc5-0b70-4630-a7a0-fe6470600813', 'ViewEmployeeAdvances' UNION ALL
    SELECT 'c99d1bc5-0b70-4630-a7a0-fe6470600813', 'Accounting_View' UNION ALL
    SELECT 'c99d1bc5-0b70-4630-a7a0-fe6470600813', 'Accounting_Payroll_Run_View' UNION ALL

    SELECT '328d4d00-fc0d-45c3-b12a-09164bf62fb8', 'CRM_View' UNION ALL
    SELECT '328d4d00-fc0d-45c3-b12a-09164bf62fb8', 'CRM_Leads_View' UNION ALL
    SELECT '328d4d00-fc0d-45c3-b12a-09164bf62fb8', 'CRM_Leads_Create' UNION ALL
    SELECT '328d4d00-fc0d-45c3-b12a-09164bf62fb8', 'CRM_Leads_Edit' UNION ALL
    SELECT '328d4d00-fc0d-45c3-b12a-09164bf62fb8', 'CRM_Leads_Delete' UNION ALL
    SELECT '328d4d00-fc0d-45c3-b12a-09164bf62fb8', 'CRM_Contacts_View' UNION ALL
    SELECT '328d4d00-fc0d-45c3-b12a-09164bf62fb8', 'CRM_Contacts_Create' UNION ALL
    SELECT '328d4d00-fc0d-45c3-b12a-09164bf62fb8', 'CRM_Contacts_Edit' UNION ALL
    SELECT '328d4d00-fc0d-45c3-b12a-09164bf62fb8', 'CRM_Contacts_Delete' UNION ALL

    SELECT '59c58656-c8d7-4236-be11-6b1f859ca3f1', 'CRM_View' UNION ALL
    SELECT '59c58656-c8d7-4236-be11-6b1f859ca3f1', 'CRM_Leads_View' UNION ALL
    SELECT '59c58656-c8d7-4236-be11-6b1f859ca3f1', 'CRM_Leads_Create' UNION ALL
    SELECT '59c58656-c8d7-4236-be11-6b1f859ca3f1', 'CRM_Leads_Edit' UNION ALL
    SELECT '59c58656-c8d7-4236-be11-6b1f859ca3f1', 'CRM_Leads_Delete' UNION ALL
    SELECT '59c58656-c8d7-4236-be11-6b1f859ca3f1', 'CRM_Contacts_View' UNION ALL
    SELECT '59c58656-c8d7-4236-be11-6b1f859ca3f1', 'CRM_Contacts_Create' UNION ALL
    SELECT '59c58656-c8d7-4236-be11-6b1f859ca3f1', 'CRM_Contacts_Edit' UNION ALL
    SELECT '59c58656-c8d7-4236-be11-6b1f859ca3f1', 'CRM_Contacts_Delete' UNION ALL

    SELECT '318e0db6-df8a-4ccd-abdf-061d1e072c4b', 'CRM_View' UNION ALL
    SELECT '318e0db6-df8a-4ccd-abdf-061d1e072c4b', 'CRM_Leads_View' UNION ALL
    SELECT '318e0db6-df8a-4ccd-abdf-061d1e072c4b', 'CRM_Leads_Create' UNION ALL
    SELECT '318e0db6-df8a-4ccd-abdf-061d1e072c4b', 'CRM_Leads_Edit' UNION ALL
    SELECT '318e0db6-df8a-4ccd-abdf-061d1e072c4b', 'CRM_Leads_Delete' UNION ALL
    SELECT '318e0db6-df8a-4ccd-abdf-061d1e072c4b', 'CRM_Contacts_View' UNION ALL
    SELECT '318e0db6-df8a-4ccd-abdf-061d1e072c4b', 'CRM_Contacts_Create' UNION ALL
    SELECT '318e0db6-df8a-4ccd-abdf-061d1e072c4b', 'CRM_Contacts_Edit' UNION ALL
    SELECT '318e0db6-df8a-4ccd-abdf-061d1e072c4b', 'CRM_Contacts_Delete' UNION ALL

    SELECT '5a08ffb2-db92-401e-8078-68b7150051a4', 'CRM_View' UNION ALL
    SELECT '5a08ffb2-db92-401e-8078-68b7150051a4', 'CRM_Leads_View' UNION ALL
    SELECT '5a08ffb2-db92-401e-8078-68b7150051a4', 'CRM_Leads_Create' UNION ALL
    SELECT '5a08ffb2-db92-401e-8078-68b7150051a4', 'CRM_Leads_Edit' UNION ALL
    SELECT '5a08ffb2-db92-401e-8078-68b7150051a4', 'CRM_Leads_Delete' UNION ALL
    SELECT '5a08ffb2-db92-401e-8078-68b7150051a4', 'CRM_Contacts_View' UNION ALL
    SELECT '5a08ffb2-db92-401e-8078-68b7150051a4', 'CRM_Contacts_Create' UNION ALL
    SELECT '5a08ffb2-db92-401e-8078-68b7150051a4', 'CRM_Contacts_Edit' UNION ALL
    SELECT '5a08ffb2-db92-401e-8078-68b7150051a4', 'CRM_Contacts_Delete' UNION ALL

    SELECT '2e70cd41-517b-43b3-9755-a604ca83f0e5', 'CRM_View' UNION ALL
    SELECT '2e70cd41-517b-43b3-9755-a604ca83f0e5', 'CRM_Leads_View' UNION ALL
    SELECT '2e70cd41-517b-43b3-9755-a604ca83f0e5', 'CRM_Leads_Create' UNION ALL
    SELECT '2e70cd41-517b-43b3-9755-a604ca83f0e5', 'CRM_Leads_Edit' UNION ALL
    SELECT '2e70cd41-517b-43b3-9755-a604ca83f0e5', 'CRM_Leads_Delete' UNION ALL
    SELECT '2e70cd41-517b-43b3-9755-a604ca83f0e5', 'CRM_Contacts_View' UNION ALL
    SELECT '2e70cd41-517b-43b3-9755-a604ca83f0e5', 'CRM_Contacts_Create' UNION ALL
    SELECT '2e70cd41-517b-43b3-9755-a604ca83f0e5', 'CRM_Contacts_Edit' UNION ALL
    SELECT '2e70cd41-517b-43b3-9755-a604ca83f0e5', 'CRM_Contacts_Delete' UNION ALL

    SELECT '4b5b8b23-7750-46f9-a7f9-09e621077642', 'CRM_View' UNION ALL
    SELECT '4b5b8b23-7750-46f9-a7f9-09e621077642', 'CRM_Leads_View' UNION ALL
    SELECT '4b5b8b23-7750-46f9-a7f9-09e621077642', 'CRM_Leads_Create' UNION ALL
    SELECT '4b5b8b23-7750-46f9-a7f9-09e621077642', 'CRM_Leads_Edit' UNION ALL
    SELECT '4b5b8b23-7750-46f9-a7f9-09e621077642', 'CRM_Leads_Delete' UNION ALL
    SELECT '4b5b8b23-7750-46f9-a7f9-09e621077642', 'CRM_Contacts_View' UNION ALL
    SELECT '4b5b8b23-7750-46f9-a7f9-09e621077642', 'CRM_Contacts_Create' UNION ALL
    SELECT '4b5b8b23-7750-46f9-a7f9-09e621077642', 'CRM_Contacts_Edit' UNION ALL
    SELECT '4b5b8b23-7750-46f9-a7f9-09e621077642', 'CRM_Contacts_Delete' UNION ALL

    SELECT '63df1d61-1cc5-4b9b-96f1-164f37a612b6', 'CRM_View' UNION ALL
    SELECT '63df1d61-1cc5-4b9b-96f1-164f37a612b6', 'CRM_Leads_View' UNION ALL
    SELECT '63df1d61-1cc5-4b9b-96f1-164f37a612b6', 'CRM_Leads_Create' UNION ALL
    SELECT '63df1d61-1cc5-4b9b-96f1-164f37a612b6', 'CRM_Leads_Edit' UNION ALL
    SELECT '63df1d61-1cc5-4b9b-96f1-164f37a612b6', 'CRM_Leads_Delete' UNION ALL
    SELECT '63df1d61-1cc5-4b9b-96f1-164f37a612b6', 'CRM_Contacts_View' UNION ALL
    SELECT '63df1d61-1cc5-4b9b-96f1-164f37a612b6', 'CRM_Contacts_Create' UNION ALL
    SELECT '63df1d61-1cc5-4b9b-96f1-164f37a612b6', 'CRM_Contacts_Edit' UNION ALL
    SELECT '63df1d61-1cc5-4b9b-96f1-164f37a612b6', 'CRM_Contacts_Delete' UNION ALL

    SELECT 'a5bcc7ed-28f3-4a82-b74f-d24fa5f99695', 'CRM_View' UNION ALL
    SELECT 'a5bcc7ed-28f3-4a82-b74f-d24fa5f99695', 'CRM_Leads_View' UNION ALL
    SELECT 'a5bcc7ed-28f3-4a82-b74f-d24fa5f99695', 'CRM_Leads_Create' UNION ALL
    SELECT 'a5bcc7ed-28f3-4a82-b74f-d24fa5f99695', 'CRM_Leads_Edit' UNION ALL
    SELECT 'a5bcc7ed-28f3-4a82-b74f-d24fa5f99695', 'CRM_Leads_Delete' UNION ALL
    SELECT 'a5bcc7ed-28f3-4a82-b74f-d24fa5f99695', 'CRM_Contacts_View' UNION ALL
    SELECT 'a5bcc7ed-28f3-4a82-b74f-d24fa5f99695', 'CRM_Contacts_Create' UNION ALL
    SELECT 'a5bcc7ed-28f3-4a82-b74f-d24fa5f99695', 'CRM_Contacts_Edit' UNION ALL
    SELECT 'a5bcc7ed-28f3-4a82-b74f-d24fa5f99695', 'CRM_Contacts_Delete' UNION ALL

    SELECT 'd479dc43-f34a-44d2-b478-003585e4df54', 'CRM_View' UNION ALL
    SELECT 'd479dc43-f34a-44d2-b478-003585e4df54', 'CRM_Leads_View' UNION ALL
    SELECT 'd479dc43-f34a-44d2-b478-003585e4df54', 'CRM_Leads_Create' UNION ALL
    SELECT 'd479dc43-f34a-44d2-b478-003585e4df54', 'CRM_Leads_Edit' UNION ALL
    SELECT 'd479dc43-f34a-44d2-b478-003585e4df54', 'CRM_Leads_Delete' UNION ALL
    SELECT 'd479dc43-f34a-44d2-b478-003585e4df54', 'CRM_Contacts_View' UNION ALL
    SELECT 'd479dc43-f34a-44d2-b478-003585e4df54', 'CRM_Contacts_Create' UNION ALL
    SELECT 'd479dc43-f34a-44d2-b478-003585e4df54', 'CRM_Contacts_Edit' UNION ALL
    SELECT 'd479dc43-f34a-44d2-b478-003585e4df54', 'CRM_Contacts_Delete' UNION ALL

    SELECT '6e3e81be-f19e-43d0-a7d0-2156187f8dfe', 'CRM_View' UNION ALL
    SELECT '6e3e81be-f19e-43d0-a7d0-2156187f8dfe', 'CRM_Leads_View' UNION ALL
    SELECT '6e3e81be-f19e-43d0-a7d0-2156187f8dfe', 'CRM_Leads_Create' UNION ALL
    SELECT '6e3e81be-f19e-43d0-a7d0-2156187f8dfe', 'CRM_Leads_Edit' UNION ALL
    SELECT '6e3e81be-f19e-43d0-a7d0-2156187f8dfe', 'CRM_Leads_Delete' UNION ALL
    SELECT '6e3e81be-f19e-43d0-a7d0-2156187f8dfe', 'CRM_Contacts_View' UNION ALL
    SELECT '6e3e81be-f19e-43d0-a7d0-2156187f8dfe', 'CRM_Contacts_Create' UNION ALL
    SELECT '6e3e81be-f19e-43d0-a7d0-2156187f8dfe', 'CRM_Contacts_Edit' UNION ALL
    SELECT '6e3e81be-f19e-43d0-a7d0-2156187f8dfe', 'CRM_Contacts_Delete' UNION ALL

    SELECT '971000f3-7eca-4b99-936e-4bb524fc0f3d', 'CRM_View' UNION ALL
    SELECT '971000f3-7eca-4b99-936e-4bb524fc0f3d', 'CRM_Leads_View' UNION ALL
    SELECT '971000f3-7eca-4b99-936e-4bb524fc0f3d', 'CRM_Leads_Create' UNION ALL
    SELECT '971000f3-7eca-4b99-936e-4bb524fc0f3d', 'CRM_Leads_Edit' UNION ALL
    SELECT '971000f3-7eca-4b99-936e-4bb524fc0f3d', 'CRM_Leads_Delete' UNION ALL
    SELECT '971000f3-7eca-4b99-936e-4bb524fc0f3d', 'CRM_Contacts_View' UNION ALL
    SELECT '971000f3-7eca-4b99-936e-4bb524fc0f3d', 'CRM_Contacts_Create' UNION ALL
    SELECT '971000f3-7eca-4b99-936e-4bb524fc0f3d', 'CRM_Contacts_Edit' UNION ALL
    SELECT '971000f3-7eca-4b99-936e-4bb524fc0f3d', 'CRM_Contacts_Delete'
) u
JOIN AspNetRoles r ON r.Name = u.RoleName;


-- ============================================================
-- PART 4 — POST-CHECK (optional, run after PART 2/3)
-- ============================================================
SELECT u.UserName, u.DisplayName, u.Email, u.PartyId, COUNT(ur.RoleId) AS RoleCount
FROM AspNetUsers u
LEFT JOIN AspNetUserRoles ur ON ur.UserId = u.Id
WHERE u.UserName IN ('rawan','Nagy','Eman','Reham','Yasmin','Gamal','Medhat','Ayman','Alaa','Mark','Ahlam','Peter')
GROUP BY u.Id, u.UserName, u.DisplayName, u.Email, u.PartyId
ORDER BY u.UserName;
-- Expected RoleCount: rawan=5, all 11 CRM accounts=9
