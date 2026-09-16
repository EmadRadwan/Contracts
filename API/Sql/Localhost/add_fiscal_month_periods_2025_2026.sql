-- Ledger-integrity step 3 (auditor soft-delete requirement), 14 Sep 2026.
-- Creates one FISCAL_MONTH period per month for 2025 and 2026 as children of the existing
-- quarters (2026) or the fiscal year (2025, which has no quarters), so months can be closed on
-- a cadence. Closing a quarter is refused by CloseFinancialTimePeriod while any child month is
-- open, which is the intended discipline. Idempotent; safe to re-run.
-- Ids follow the existing scheme (5010 = FY2025, 6010 = FY2026, 6011..6014 = 2026 quarters):
-- 2025 months = 5101..5112, 2026 months = 6101..6112.
-- Apply manually to localhost first, then production.

INSERT INTO CUSTOM_TIME_PERIOD
    (CUSTOM_TIME_PERIOD_ID, PARENT_PERIOD_ID, PERIOD_TYPE_ID, PERIOD_NUM, PERIOD_NAME, FROM_DATE, THRU_DATE,
     IS_CLOSED, ORGANIZATION_PARTY_ID, LAST_UPDATED_STAMP, LAST_UPDATED_TX_STAMP, CREATED_STAMP, CREATED_TX_STAMP)
SELECT m.id, m.parent, 'FISCAL_MONTH', m.num, m.name, m.from_date, m.thru_date,
       'N', 'Company', NOW(), NOW(), NOW(), NOW()
FROM (
    SELECT '5101' id, '5010' parent, 1  num, '2025-01' name, '2025-01-01 00:00:00' from_date, '2025-01-31 23:59:59' thru_date UNION ALL
    SELECT '5102', '5010', 2,  '2025-02', '2025-02-01 00:00:00', '2025-02-28 23:59:59' UNION ALL
    SELECT '5103', '5010', 3,  '2025-03', '2025-03-01 00:00:00', '2025-03-31 23:59:59' UNION ALL
    SELECT '5104', '5010', 4,  '2025-04', '2025-04-01 00:00:00', '2025-04-30 23:59:59' UNION ALL
    SELECT '5105', '5010', 5,  '2025-05', '2025-05-01 00:00:00', '2025-05-31 23:59:59' UNION ALL
    SELECT '5106', '5010', 6,  '2025-06', '2025-06-01 00:00:00', '2025-06-30 23:59:59' UNION ALL
    SELECT '5107', '5010', 7,  '2025-07', '2025-07-01 00:00:00', '2025-07-31 23:59:59' UNION ALL
    SELECT '5108', '5010', 8,  '2025-08', '2025-08-01 00:00:00', '2025-08-31 23:59:59' UNION ALL
    SELECT '5109', '5010', 9,  '2025-09', '2025-09-01 00:00:00', '2025-09-30 23:59:59' UNION ALL
    SELECT '5110', '5010', 10, '2025-10', '2025-10-01 00:00:00', '2025-10-31 23:59:59' UNION ALL
    SELECT '5111', '5010', 11, '2025-11', '2025-11-01 00:00:00', '2025-11-30 23:59:59' UNION ALL
    SELECT '5112', '5010', 12, '2025-12', '2025-12-01 00:00:00', '2025-12-31 23:59:59' UNION ALL
    SELECT '6101', '6011', 1,  '2026-01', '2026-01-01 00:00:00', '2026-01-31 23:59:59' UNION ALL
    SELECT '6102', '6011', 2,  '2026-02', '2026-02-01 00:00:00', '2026-02-28 23:59:59' UNION ALL
    SELECT '6103', '6011', 3,  '2026-03', '2026-03-01 00:00:00', '2026-03-31 23:59:59' UNION ALL
    SELECT '6104', '6012', 4,  '2026-04', '2026-04-01 00:00:00', '2026-04-30 23:59:59' UNION ALL
    SELECT '6105', '6012', 5,  '2026-05', '2026-05-01 00:00:00', '2026-05-31 23:59:59' UNION ALL
    SELECT '6106', '6012', 6,  '2026-06', '2026-06-01 00:00:00', '2026-06-30 23:59:59' UNION ALL
    SELECT '6107', '6013', 7,  '2026-07', '2026-07-01 00:00:00', '2026-07-31 23:59:59' UNION ALL
    SELECT '6108', '6013', 8,  '2026-08', '2026-08-01 00:00:00', '2026-08-31 23:59:59' UNION ALL
    SELECT '6109', '6013', 9,  '2026-09', '2026-09-01 00:00:00', '2026-09-30 23:59:59' UNION ALL
    SELECT '6110', '6014', 10, '2026-10', '2026-10-01 00:00:00', '2026-10-31 23:59:59' UNION ALL
    SELECT '6111', '6014', 11, '2026-11', '2026-11-01 00:00:00', '2026-11-30 23:59:59' UNION ALL
    SELECT '6112', '6014', 12, '2026-12', '2026-12-01 00:00:00', '2026-12-31 23:59:59'
) m
WHERE NOT EXISTS (SELECT 1 FROM CUSTOM_TIME_PERIOD c WHERE c.CUSTOM_TIME_PERIOD_ID = m.id);

-- Verification: 24 month rows, all open, each under its parent.
SELECT c.CUSTOM_TIME_PERIOD_ID, c.PERIOD_NAME, c.PARENT_PERIOD_ID, p.PERIOD_TYPE_ID AS PARENT_TYPE, c.IS_CLOSED
FROM CUSTOM_TIME_PERIOD c LEFT JOIN CUSTOM_TIME_PERIOD p ON p.CUSTOM_TIME_PERIOD_ID = c.PARENT_PERIOD_ID
WHERE c.PERIOD_TYPE_ID = 'FISCAL_MONTH' ORDER BY c.FROM_DATE;

-- Suggested cadence: close month N around the 15th of month N+1 from the Time Periods screen
-- (Organization GL Settings > Time Periods > Close). Start with 2025 and the months of 2026 whose
-- figures the auditor has signed off; do not close June 2026 until the payroll correction in
-- API/Sql/Production/fix_june_payroll_inv1038_2026_09_06.sql has been applied and both June
-- payments sent.
