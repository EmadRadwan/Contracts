-- Link 51 hand-keyed نسيم commission payments to project 109 (نسيم).
-- Found in the 2026-09-19 project-report review: these COMMISSION_PAYMENT rows were entered before
-- the commissions feature existed. They are booked on نسيم's commission GL accounts (600025/26/27,
-- children of 600024) but carry no WORK_EFFORT_ID, so the project report could not attribute them
-- (they now appear neither in العمولات المدفوعة nor, since commissions were excluded from operating
-- expenses, anywhere else). The accountant confirmed on 2026-09-20 that all 51 — including the six
-- lump-sum / "تحت حساب العمولات" transfers (13439, 14054, 15174, O17490, O17951, O18061) — belong
-- to نسيم. No ledger entries change: the GL account was already correct; only the project link.
-- Idempotent: the WHERE guards on type + NULL project, so a re-run updates 0 rows.
--
-- Verification BEFORE (expect 51 rows / 5,937,356.00):
--   SELECT COUNT(*), SUM(AMOUNT) FROM PAYMENT
--   WHERE PAYMENT_TYPE_ID='COMMISSION_PAYMENT' AND WORK_EFFORT_ID IS NULL
--     AND OVERRIDE_GL_ACCOUNT_ID IN ('600024','600025','600026','600027','600029')
--     AND STATUS_ID NOT IN ('PMNT_VOID','PMNT_CANCELLED');

UPDATE PAYMENT
SET WORK_EFFORT_ID = '109',
    LAST_UPDATED_STAMP = NOW()
WHERE PAYMENT_TYPE_ID = 'COMMISSION_PAYMENT'
  AND WORK_EFFORT_ID IS NULL
  AND OVERRIDE_GL_ACCOUNT_ID IN ('600024','600025','600026','600027','600029')
  AND PAYMENT_ID IN (
    '13439','14054','14179','14180','14233','14267','14670','14698','14699','14700','14701','14750',
    '14764','14932','14933','15042','15045','15047','15048','15050','15051','15052','15057','15058',
    '15151','15159','15160','15174','15249','15639','15640','16404','16415','16427','16453','16454',
    '16456','17042','O17235','O17242','O17246','O17252','O17262','O17357','O17413','O17490','O17675',
    'O17848','O17944','O17951','O18061'
  );

-- Verification AFTER (expect 0 rows left unlinked, and the 51 now on project 109):
--   SELECT COUNT(*) FROM PAYMENT
--   WHERE PAYMENT_TYPE_ID='COMMISSION_PAYMENT' AND WORK_EFFORT_ID IS NULL
--     AND OVERRIDE_GL_ACCOUNT_ID IN ('600024','600025','600026','600027','600029');
--   SELECT COUNT(*), SUM(AMOUNT) FROM PAYMENT WHERE WORK_EFFORT_ID='109' AND PAYMENT_ID IN (... the 51 ids ...);
--
-- Rollback (only if needed, same id list):
--   UPDATE PAYMENT SET WORK_EFFORT_ID = NULL WHERE PAYMENT_ID IN (... the 51 ids ...) AND WORK_EFFORT_ID='109';
