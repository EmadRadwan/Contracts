-- ============================================================================
-- Resolves duplicate PROJECT_CERTIFICATE numbers.
--
-- Bug: CreateProjectCertificate numbered each certificate from a COUNT of the
-- party's work efforts (+1), so any time that set shrank or drifted from the
-- set actually carrying numbers, the next certificate reused a number already
-- issued. The signature is visible in the data: the SITE series runs 1..90 over
-- 90 certificates but holds only 86 distinct numbers -- 4 duplicates
-- (SITE-0031/0041/0063/0070) against exactly 4 gaps (SITE-0028/0039/0058/0068).
-- Party 205 shows the same shape: 205-0003 twice, 205-0001 never issued.
--
-- Code fix that stops this recurring:
--   CreateProjectCertificate.cs -- numbers from MAX(existing serial for the
--   party) + 1 instead of COUNT(work efforts) + 1.
--
-- Renumbering rule: the LATER certificate of each pair takes the next FREE
-- number in its series (SITE-0091..0094, 205-0009). The gaps are deliberately
-- NOT backfilled -- a gap may belong to a certificate that was deleted after
-- its number appeared on a printed document, and reusing it would resurrect
-- that collision. Gaps in a document series are normal and auditable.
--
-- STATUS
--   localhost   APPLIED 2026-07-28. Verified after commit: 0 duplicate
--               certificate numbers DB-wide; SITE series 90 certificates / 90
--               distinct numbers; 205 series 8 / 8.
--   production  NOT APPLIED. Re-run the BEFORE query there first -- the
--               duplicate set may differ.
--
-- Verified: ACCTG_TRANS.DESCRIPTION, WORK_EFFORT.NOTES/DESCRIPTION,
-- NOTE_DATA.NOTE_INFO, INVOICE.DESCRIPTION/REFERENCE_NUMBER and
-- PAYMENT.COMMENTS hold none of these numbers. ORDER_HEADER.INTERNAL_REMARKS
-- holds two ("Auto-generated from Certificate 205-0003"), handled in STEP 2.
--
-- REVIEW BEFORE RUNNING. Take a backup first:
--   mysqldump erp_contracts WORK_EFFORT ORDER_HEADER \
--     > backup_before_certnumber_fix.sql
-- ============================================================================

START TRANSACTION;

-- ---------------------------------------------------------------------------
-- BEFORE: expect 5 rows, each with N = 2
-- ---------------------------------------------------------------------------


-- ---------------------------------------------------------------------------
-- STEP 1 -- renumber the later certificate of each pair.
--
--   WE       project  was         becomes     last updated
--   10462    113      SITE-0031   SITE-0091   2026-04-09 14:39
--   11247    10339    SITE-0041   SITE-0092   2026-05-06 12:30
--   16365    105      SITE-0063   SITE-0093   2026-06-14 13:43
--   17139    105      SITE-0070   SITE-0094   2026-06-30 09:38
--   10151    111      205-0003    205-0009    2026-01-24 16:23
--
-- Each WHERE pins BOTH the work effort id and the number it is expected to
-- hold, so the statement is idempotent and cannot touch the wrong row if the
-- script is run twice or against an environment already partly corrected.
-- ---------------------------------------------------------------------------

UPDATE WORK_EFFORT SET CERTIFICATE_NUMBER = 'SITE-0091', LAST_UPDATED_STAMP = NOW()
 WHERE WORK_EFFORT_ID = '10462' AND CERTIFICATE_NUMBER = 'SITE-0031';

UPDATE WORK_EFFORT SET CERTIFICATE_NUMBER = 'SITE-0092', LAST_UPDATED_STAMP = NOW()
 WHERE WORK_EFFORT_ID = '11247' AND CERTIFICATE_NUMBER = 'SITE-0041';

UPDATE WORK_EFFORT SET CERTIFICATE_NUMBER = 'SITE-0093', LAST_UPDATED_STAMP = NOW()
 WHERE WORK_EFFORT_ID = '16365' AND CERTIFICATE_NUMBER = 'SITE-0063';

UPDATE WORK_EFFORT SET CERTIFICATE_NUMBER = 'SITE-0094', LAST_UPDATED_STAMP = NOW()
 WHERE WORK_EFFORT_ID = '17139' AND CERTIFICATE_NUMBER = 'SITE-0070';

UPDATE WORK_EFFORT SET CERTIFICATE_NUMBER = '205-0009', LAST_UPDATED_STAMP = NOW()
 WHERE WORK_EFFORT_ID = '10151' AND CERTIFICATE_NUMBER = '205-0003';

-- ---------------------------------------------------------------------------
-- STEP 2 -- follow the renumber into the one place the string is embedded.
-- PO10712 is WORK_EFFORT 10151's RELATED_ORDER_ID; PO10694 belongs to 10107,
-- which keeps 205-0003 and is left alone.
-- ---------------------------------------------------------------------------

UPDATE ORDER_HEADER
   SET INTERNAL_REMARKS = 'Auto-generated from Certificate 205-0009'
 WHERE ORDER_ID = 'PO10712'
   AND INTERNAL_REMARKS = 'Auto-generated from Certificate 205-0003';

-- ---------------------------------------------------------------------------
-- AFTER: all three checks must return ZERO rows.
-- ---------------------------------------------------------------------------

-- 1. No certificate number is used twice.
SELECT 'STILL DUPLICATED' flag, CERTIFICATE_NUMBER, COUNT(*) n,
       GROUP_CONCAT(WORK_EFFORT_ID) we_ids
FROM WORK_EFFORT
WHERE CERTIFICATE_NUMBER IS NOT NULL
  AND WORK_EFFORT_TYPE_ID = 'PROJECT_CERTIFICATE'
GROUP BY CERTIFICATE_NUMBER
HAVING n > 1;

-- 2. No renumbered certificate lost its number or landed on an occupied one.
SELECT 'BAD RENUMBER' flag, WORK_EFFORT_ID, CERTIFICATE_NUMBER
FROM WORK_EFFORT
WHERE WORK_EFFORT_ID IN ('10462', '11247', '16365', '17139', '10151')
  AND CERTIFICATE_NUMBER NOT IN ('SITE-0091', 'SITE-0092', 'SITE-0093', 'SITE-0094', '205-0009');

-- 3. No stale reference to a reassigned number remains.
SELECT 'STALE REFERENCE' flag, ORDER_ID, INTERNAL_REMARKS
FROM ORDER_HEADER
WHERE INTERNAL_REMARKS LIKE '%205-0003%'
  AND ORDER_ID <> 'PO10694';

-- Series shape after the fix: SITE -> 90 certificates, 90 distinct numbers,
-- max serial 94 (gaps at 28/39/58/68 left intentionally).
SELECT 'SITE series' label, COUNT(*) certificates,
       COUNT(DISTINCT CERTIFICATE_NUMBER) distinct_numbers,
       MAX(CAST(SUBSTRING_INDEX(CERTIFICATE_NUMBER, '-', -1) AS UNSIGNED)) max_serial
FROM WORK_EFFORT
WHERE CERTIFICATE_NUMBER LIKE 'SITE-%' AND WORK_EFFORT_TYPE_ID = 'PROJECT_CERTIFICATE';

SELECT '205 series' label, COUNT(*) certificates,
       COUNT(DISTINCT CERTIFICATE_NUMBER) distinct_numbers,
       MAX(CAST(SUBSTRING_INDEX(CERTIFICATE_NUMBER, '-', -1) AS UNSIGNED)) max_serial
FROM WORK_EFFORT
WHERE CERTIFICATE_NUMBER LIKE '205-%' AND WORK_EFFORT_TYPE_ID = 'PROJECT_CERTIFICATE';

-- ============================================================================
-- Review the output above, then run ONE of:
--   COMMIT;
--   ROLLBACK;
-- ============================================================================
