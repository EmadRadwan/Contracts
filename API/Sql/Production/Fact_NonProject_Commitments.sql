-- =============================================================================
-- FACT NON-PROJECT COMMITMENTS  (Power BI: Fact_NonProject_Commitments)
-- =============================================================================
-- Money the company has committed to pay, has NOT yet paid, and which no
-- project-scoped fact view accounts for.
--
-- WHY THIS VIEW EXISTS
-- --------------------
-- Fact_Project_DirectPayments_2 and Fact_Project_OperatingExpenses_2 both reach
-- a project through the GL account, not through PAYMENT.WORK_EFFORT_ID:
--
--     JOIN DimProject dp ON pyt.OVERRIDE_GL_ACCOUNT_ID = dp.GlAccountId
--     ... AND pyt.OVERRIDE_GL_ACCOUNT_ID IS NOT NULL
--       DirectPayments_2      -> dp.GlAccountType  = 'PROJECT_MAIN'
--       OperatingExpenses_2   -> dp.GlAccountType <> 'PROJECT_MAIN'
--
-- Between them they cover every disbursement whose override GL account resolves
-- to a project. A disbursement whose override account is NOT a project account
-- (or has none at all) is invisible to both — and therefore invisible to
-- [Total Future Expenses] on the المصروفات المستقبلية page. This view is the
-- exact complement of that pair.
--
-- MEASURED ON THE 17 AUGUST DATA COPY
-- -----------------------------------
-- Unpaid disbursements with a due date still ahead:
--     captured, PROJECT_MAIN      14 payments   82,472,646   (Direct Expenses)
--     captured, OPERATING_PARENT   9 payments    4,470,000   (Operating Expenses)
--     NOT captured -> this view   10 payments   55,896,380
--                                 --------------------------
--     company-wide total          33 payments  142,839,026
--
-- The 10 uncovered rows are three obligations against سيتى ووك and one land
-- purchase, maturing 2026-08-25 → 2027-01-15:
--     250530  محمد عاكف - شراء سيتى ووك          6 rows   48,000,000
--     124810  حق انتفاع الجهاز - شركاء سيتى ووك  2 rows    6,396,380
--     250444  ياسر سعودي - مالك ارض 500 متر      3 rows    1,500,000
--
-- "COMMITMENTS", NOT "EXPENSES" — READ THIS BEFORE WIRING IT TO A P&L MEASURE
-- ---------------------------------------------------------------------------
-- Every account above is a balance-sheet account: 2505xx / 250444 are payables
-- to land sellers, 124810 is a usufruct right. Settling them is a cash outflow
-- that discharges a liability or acquires an asset — it is NOT a P&L expense.
-- Do not add this view's total to [Total Spent] or to any profit measure; it
-- belongs in a cash-commitment / obligation view of the world. The column is
-- named AMOUNT rather than ExpenseAmount for exactly this reason.
--
-- DISJOINTNESS
-- ------------
-- Guaranteed three ways, following the idiom already used by
-- Fact_Project_OperatingExpenses_2: the DimProject anti-join, the PAYROL_PAYMENT
-- exclusion (project labour belongs to Fact_Project_Payroll), and explicit
-- NOT EXISTS against all three project fact views. Any one of them would
-- probably do; all three make double-counting impossible to reintroduce by
-- accident when the sibling views change.
--
-- Scope: unpaid disbursements only (STATUS_ID = 'PMNT_NOT_PAID'). Overdue rows
--        are kept, not filtered out — an obligation that has passed its due date
--        without being paid is still owed, and IsOverdue tells them apart.
-- =============================================================================

DROP VIEW IF EXISTS Fact_NonProject_Commitments;
CREATE OR REPLACE VIEW Fact_NonProject_Commitments AS
SELECT
    pyt.PAYMENT_ID                                      AS CommitmentId,           -- KEY

    pyt.PAYMENT_TYPE_ID,
    ptt.DESCRIPTION_ARABIC                              AS CommitmentTypeArabic,
    ptt.DESCRIPTION                                     AS CommitmentTypeEnglish,

    -- Who it is owed to
    pyt.PARTY_ID_FROM                                   AS PartyIdFrom,
    pty_from.DESCRIPTION                                AS PartyIdFromName,
    pyt.PARTY_ID_TO                                     AS PartyIdTo,
    COALESCE(pty_to.DESCRIPTION,
             CASE WHEN pyt.PARTY_ID_TO = 'Company' THEN 'Golden Land'
                  ELSE pyt.PARTY_ID_TO END)             AS PartyIdToName,

    -- Status (always PMNT_NOT_PAID here; carried so the model can display it)
    pyt.STATUS_ID,
    sts.DESCRIPTION_ARABIC                              AS StatusDescription,
    sts.DESCRIPTION                                     AS StatusDescriptionEnglish,

    -- Maturity. EFFECTIVE_DATE is the due date for an unpaid payment; this is
    -- the column to relate to DateTbl.Date, exactly as the sibling views do.
    pyt.EFFECTIVE_DATE,
    pyt.CREATED_STAMP                                   AS CreatedStamp,

    pyt.AMOUNT,
    COALESCE(pyt.ACTUAL_CURRENCY_AMOUNT, pyt.AMOUNT)    AS ActualCurrencyAmount,
    COALESCE(pyt.CURRENCY_UOM_ID, 'EGP')                AS CurrencyUomId,

    -- The account that made this non-project: it exists, but DimProject does
    -- not recognise it as belonging to any project.
    pyt.OVERRIDE_GL_ACCOUNT_ID                          AS GlAccountId,
    COALESCE(ga.ACCOUNT_NAME_ARABIC, ga.ACCOUNT_NAME)   AS GlAccountName,
    ga.ACCOUNT_NAME                                     AS GlAccountNameEnglish,

    -- Exposed for transparency: currently NULL on every row, which is why
    -- keying "is this a project payment?" off WORK_EFFORT_ID rather than off
    -- the GL account gives a different — and wrong — answer.
    pyt.WORK_EFFORT_ID                                  AS WorkEffortId,

    pyt.CHEQUENUMBER                                    AS ChequeNumber,
    pyt.CHEQUEDATE                                      AS ChequeDate,
    pyt.PAYMENT_REF_NUM                                 AS PaymentRefNum,
    pyt.COMMENTS,

    -- ── Timing flags ────────────────────────────────────────────────────────
    -- Mirrors the revenue side's FutureAmount / LateAmount split so the expense
    -- and revenue halves of the report can be read the same way.
    CASE WHEN pyt.EFFECTIVE_DATE > CURDATE() THEN 1 ELSE 0 END  AS IsFuture,
    CASE WHEN pyt.EFFECTIVE_DATE < CURDATE() THEN 1 ELSE 0 END  AS IsOverdue,
    CASE WHEN pyt.EFFECTIVE_DATE > CURDATE() THEN pyt.AMOUNT ELSE 0 END AS FutureAmount,
    CASE WHEN pyt.EFFECTIVE_DATE < CURDATE() THEN pyt.AMOUNT ELSE 0 END AS OverdueAmount,

    DATEDIFF(pyt.EFFECTIVE_DATE, CURDATE())             AS DaysUntilDue,

    -- ── Maturity bucket (Arabic, for slicers and matrix rows) ───────────────
    -- Deliberately coarser than Fact_Project_DirectPayments_2.DueStatusArabic:
    -- these are long-dated obligations (2026 → 2031), so day-level buckets like
    -- "متأخرة بيومين" produce a hundred one-row categories. Sort with
    -- DueBucketSort, not alphabetically.
    CASE
        WHEN pyt.EFFECTIVE_DATE IS NULL                      THEN 'غير محدد'
        WHEN pyt.EFFECTIVE_DATE <  CURDATE()                 THEN 'متأخرة'
        WHEN pyt.EFFECTIVE_DATE =  CURDATE()                 THEN 'مستحقة اليوم'
        WHEN DATEDIFF(pyt.EFFECTIVE_DATE, CURDATE()) <=  30  THEN 'خلال شهر'
        WHEN DATEDIFF(pyt.EFFECTIVE_DATE, CURDATE()) <=  90  THEN 'خلال 3 أشهر'
        WHEN DATEDIFF(pyt.EFFECTIVE_DATE, CURDATE()) <= 365  THEN 'خلال سنة'
        ELSE 'أكثر من سنة'
    END                                                 AS DueBucketArabic,

    CASE
        WHEN pyt.EFFECTIVE_DATE IS NULL                      THEN 99
        WHEN pyt.EFFECTIVE_DATE <  CURDATE()                 THEN 1
        WHEN pyt.EFFECTIVE_DATE =  CURDATE()                 THEN 2
        WHEN DATEDIFF(pyt.EFFECTIVE_DATE, CURDATE()) <=  30  THEN 3
        WHEN DATEDIFF(pyt.EFFECTIVE_DATE, CURDATE()) <=  90  THEN 4
        WHEN DATEDIFF(pyt.EFFECTIVE_DATE, CURDATE()) <= 365  THEN 5
        ELSE 6
    END                                                 AS DueBucketSort

FROM PAYMENT pyt

JOIN PAYMENT_TYPE ptt   ON pyt.PAYMENT_TYPE_ID = ptt.PAYMENT_TYPE_ID
JOIN STATUS_ITEM sts    ON pyt.STATUS_ID       = sts.STATUS_ID
JOIN PARTY pty_from     ON pyt.PARTY_ID_FROM   = pty_from.PARTY_ID

LEFT JOIN PARTY pty_to  ON pyt.PARTY_ID_TO     = pty_to.PARTY_ID
LEFT JOIN GL_ACCOUNT ga ON ga.GL_ACCOUNT_ID    = pyt.OVERRIDE_GL_ACCOUNT_ID

-- ── The anti-join that defines "non-project" ────────────────────────────────
-- LEFT JOIN + IS NULL, matching the exact condition the two project views use
-- to claim a row. Change one, change the other, or money goes missing.
LEFT JOIN DimProject dp ON pyt.OVERRIDE_GL_ACCOUNT_ID = dp.GlAccountId

WHERE (ptt.PARENT_TYPE_ID = 'DISBURSEMENT'
    OR ptt.PAYMENT_TYPE_ID = 'DISBURSEMENT')

  -- not claimed by either project view
  AND dp.GlAccountId IS NULL

  -- committed but not yet paid — the whole point of the view
  AND pyt.STATUS_ID = 'PMNT_NOT_PAID'

  -- project labour belongs to Fact_Project_Payroll; same exclusion as
  -- Fact_Project_OperatingExpenses_2, for the same reason
  AND pyt.PAYMENT_TYPE_ID <> 'PAYROL_PAYMENT'

  -- belt and braces against double-counting if the sibling views ever widen
  AND NOT EXISTS (SELECT 1 FROM Fact_Project_DirectPayments_2 f    WHERE f.PaymentId = pyt.PAYMENT_ID)
  AND NOT EXISTS (SELECT 1 FROM Fact_Project_OperatingExpenses_2 f WHERE f.PaymentId = pyt.PAYMENT_ID)
  AND NOT EXISTS (SELECT 1 FROM Fact_Project_Expenses f            WHERE f.PaymentId = pyt.PAYMENT_ID);

-- =============================================================================
-- Verification lives in Fact_NonProject_Commitments.verify.sql — run that file
-- next. It is kept separate on purpose: a trailing block of commented-out
-- queries at the end of this file gets sent to the server as a statement
-- containing nothing but comments, which MySQL rejects with
--   [42000][1064] ... check the manual ... near '' at line 1
-- The view is created correctly when that happens; only the trailing empty
-- statement fails. This file therefore ends on a real statement.
-- =============================================================================

SELECT 'Fact_NonProject_Commitments created' AS status,
       COUNT(*)                              AS total_rows,
       SUM(IsFuture)                         AS future_rows,
       SUM(IsOverdue)                        AS overdue_rows
FROM Fact_NonProject_Commitments;
