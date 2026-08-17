-- =============================================================================
-- set_is_company_project_flags.sql
-- Purpose: Populate WORK_EFFORT.IS_COMPANY_PROJECT (company's own project vs
--          "work done for others") from the accountant's curated workbook
--          'Golden-projects.xlsx' (sheet: "المصروف حسب المشروع (نقدي)").
--
-- Rule (per instruction, tuned): only rows NOT highlighted yellow in the
--          workbook are the company's own project -> IS_COMPANY_PROJECT = 1.
--          Everything else defaults to IS_COMPANY_PROJECT = 0, including:
--            - rows highlighted YELLOW (cell fill FFFFFF00) in the workbook, and
--            - projects that exist in WORK_EFFORT but don't appear in the
--              workbook at all (mostly rental-income "ايراد ايجار عقارات"
--              projects with no expense line in this report).
--
-- Matching: workbook project names matched to WORK_EFFORT.PROJECT_NAME by eye
--          (one has a trailing-space variant in the DB: 'فيلا سعودى ' / id 108)
--          and confirmed 1:1 against WORK_EFFORT_ID below.
--
-- Scope: all WORK_EFFORT rows with WORK_EFFORT_TYPE_ID = 'PROJECT'.
-- Idempotent: deterministic UPDATEs — safe to re-run.
-- =============================================================================

START TRANSACTION;

-- ── Step 1: default every project to NOT the company's own project ─────────────
UPDATE WORK_EFFORT SET IS_COMPANY_PROJECT = 0
  WHERE WORK_EFFORT_TYPE_ID = 'PROJECT';

-- ── Step 2: flip the company's own projects to true ─────────────────────────────
-- (the 10 rows NOT highlighted yellow in the workbook)

-- الاسطبل الجديد - الصحراوى 3 فدان
UPDATE WORK_EFFORT SET IS_COMPANY_PROJECT = 1 WHERE WORK_EFFORT_ID = '105';

-- الثروة الخضراء - علاء العمدة
UPDATE WORK_EFFORT SET IS_COMPANY_PROJECT = 1 WHERE WORK_EFFORT_ID = '107';

-- الصحراوى 10.5 فدان
UPDATE WORK_EFFORT SET IS_COMPANY_PROJECT = 1 WHERE WORK_EFFORT_ID = '101';

-- بيت الوطن لادريس أكتوبر
UPDATE WORK_EFFORT SET IS_COMPANY_PROJECT = 1 WHERE WORK_EFFORT_ID = '113';

-- بيت الوطن لادريس التجمع
UPDATE WORK_EFFORT SET IS_COMPANY_PROJECT = 1 WHERE WORK_EFFORT_ID = '114';

-- جولدن لاند الادارة
UPDATE WORK_EFFORT SET IS_COMPANY_PROJECT = 1 WHERE WORK_EFFORT_ID = '10231';

-- سوا
UPDATE WORK_EFFORT SET IS_COMPANY_PROJECT = 1 WHERE WORK_EFFORT_ID = '10339';

-- قرية السدة - الصحراوى 2 فدان
UPDATE WORK_EFFORT SET IS_COMPANY_PROJECT = 1 WHERE WORK_EFFORT_ID = '100';

-- مكتب الثالث - تجديد و صيانة
UPDATE WORK_EFFORT SET IS_COMPANY_PROJECT = 1 WHERE WORK_EFFORT_ID = '10113';

-- نسيم - الثروة الخضراء
UPDATE WORK_EFFORT SET IS_COMPANY_PROJECT = 1 WHERE WORK_EFFORT_ID = '109';

-- ── For reference: rows that remain 0 ────────────────────────────────────────────
-- Yellow-highlighted in the workbook (not the company's project):
--   17399 B168-205 عائشة مبارك - تشطيب
--   110   الثالث زايد
--   111   السابع زايد
--   11330 شقة الدقي - الشيخ عبد العزيز تشطيبات و فرش
--   108   فيلا سعودى
--   10068 مزرعة السنية
--   10067 مشروع الواحات
-- Not present in the workbook at all:
--   112   التاسع زايد
--   106   الصحراوى 4 فدان
--   10072 ايراد ايجار عقارات - الصحراوى 2 فدان
--   10070 جولدن ووك - ايراد ايجار عقارات
--   10069 سيتى ووك - ايراد ايجار عقارات
--   10071 مطعم السدة - ايراد ايجار عقارات

COMMIT;

-- ── Verification (run after commit) ─────────────────────────────────────────────
SELECT WORK_EFFORT_ID, PROJECT_NAME, IS_COMPANY_PROJECT
FROM WORK_EFFORT
WHERE WORK_EFFORT_TYPE_ID = 'PROJECT'
ORDER BY IS_COMPANY_PROJECT DESC, PROJECT_NAME;
