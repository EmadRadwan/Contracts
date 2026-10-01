-- Link the two Jan-2026 multi-payment certificate postings that were written without WORK_EFFORT_ID.
--
-- Why: Reset finds a certificate's postings by WORK_EFFORT_ID. These two were invisible to it, so
-- certificate 10124 was posted a second time on re-approval (13342, 2026-04-14) on top of 10970
-- (2026-01-17) — 2,220 twice on 124422 and on 140702, 4,440 twice out of cash 111010.
-- 10862 (certificate 10078) has the same gap but was never reset, so it is not duplicated.
--
-- Run this BEFORE reversing 10970 from the app, so the reversal inherits the link.
-- Does not change any amount or balance.

START TRANSACTION;

-- Preview: expect exactly 2 rows, both with WORK_EFFORT_ID NULL
SELECT ACCTG_TRANS_ID, ACCTG_TRANS_TYPE_ID, TRANSACTION_DATE, WORK_EFFORT_ID, DESCRIPTION
FROM ACCTG_TRANS
WHERE ACCTG_TRANS_ID IN ('10862', '10970');

UPDATE ACCTG_TRANS SET WORK_EFFORT_ID = '10078'
WHERE ACCTG_TRANS_ID = '10862' AND WORK_EFFORT_ID IS NULL
  AND DESCRIPTION = 'مستند دفع متعدد 10078';

UPDATE ACCTG_TRANS SET WORK_EFFORT_ID = '10124'
WHERE ACCTG_TRANS_ID = '10970' AND WORK_EFFORT_ID IS NULL
  AND DESCRIPTION = 'مستند دفع متعدد 10124';

-- Check: no multi-payment certificate posting left without a link (expect 0)
SELECT COUNT(*) AS UNLINKED_LEFT
FROM ACCTG_TRANS
WHERE DESCRIPTION LIKE 'مستند دفع متعدد%' AND WORK_EFFORT_ID IS NULL;

COMMIT;

-- Next step (in the app, not SQL): القيود المحاسبية → transaction 10970 → عكس القيد
-- Reason: "ترحيل مكرر للمستخلص 10124 – القيد 13342 هو القيد المعتمد"
-- January 2026 is open, so the reversal is dated 2026-01-17 (the original date).
