-- =============================================================================
-- FACT NON-PROJECT PAYROLL  (Power BI: Fact_NonProject_Payroll)
-- =============================================================================
-- Posted labour cost that Fact_Project_Payroll cannot see, because the GL
-- account it was charged to does not resolve to any project.
--
-- WHY THIS VIEW EXISTS
-- --------------------
-- Fact_Project_Payroll reaches a project through the GL account on the entry:
--
--     JOIN DimProject dp ON dp.GlAccountId = ate.GL_ACCOUNT_ID
--
-- Payroll charged to a company-level account (601000 الرواتب والأجور,
-- 600000 المصروفات العمومية والإدارية) satisfies no such join and vanishes.
-- On the 2026-09-04 data copy that is 1,396,856 against 1,098,954 visible —
-- roughly half of all posted labour cost was invisible to the report.
--
-- This is the exact complement, built the same way Fact_NonProject_Commitments
-- was built for the disbursement side: same source, same posting rules, the
-- DimProject join inverted to an anti-join.
--
--     Fact_Project_Payroll      dp.GlAccountId IS NOT NULL   1,098,954
--     Fact_NonProject_Payroll   dp.GlAccountId IS     NULL   1,396,856
--                                                            ---------
--     all posted labour cost                                 2,495,810
--
-- THE '276' RULE IS INHERITED, AND IT MATTERS MORE HERE
-- ----------------------------------------------------
-- Fact_Project_Payroll excludes the batch payroll run's aggregate payment
-- (PARTY_ID_TO = '276') because three historical batches debited project
-- EXPENSE accounts instead of clearing the accrued payable — a double-book of
-- 388,967. The same fault exists on non-project accounts and is EIGHT TIMES
-- LARGER:
--
--     11544   2026-03-01  600000  607,782      15317   2026-04-30  601000  403,360
--     13431   2026-03-31  601000  458,196      14007   2026-04-06  601000  289,749
--     14228   2026-02-01  601000  449,624      O17409  2026-07-30  600021    8,500
--     16021   2026-05-25  600000  448,396      16849   2026-06-29  303000/302000 11,867
--     14240   2025-12-31  601000  419,550
--                                              ------------------------------------
--                                              10 entries          3,097,024
--
-- Without the exclusion this view would report 4,493,880 instead of 1,396,856
-- — overstating company labour cost by 222%. The accrual already charged those
-- accounts; the settlement should have debited 220xxx and cleared the payable,
-- which is what the other 55 batch entries (666,143) correctly do.
--
-- *** DO NOT REMOVE THE '276' CONDITION. *** The 3,097,024 still needs
-- reversing in the ledger. That is an accounting task, not a reporting one,
-- and this view neither fixes nor hides it — IsExcludedDoubleBook below counts
-- it so the size of the problem stays visible.
--
-- NOT AN EXPENSE-ACCOUNT FILTER
-- -----------------------------
-- Every row here is a debit that the ledger treats as cost. 303000/302000
-- (جارى الشريك) appear only inside the excluded batch rows, so no owner-equity
-- movement reaches the view.
--
-- Scope: posted entries only (IS_POSTED = 'Y'), matching both sibling views.
-- =============================================================================

DROP VIEW IF EXISTS Fact_NonProject_Payroll;
CREATE OR REPLACE VIEW Fact_NonProject_Payroll AS
SELECT
    CONCAT(at.ACCTG_TRANS_ID, ':', ate.ACCTG_TRANS_ENTRY_SEQ_ID)         AS PayrollId,   -- KEY

    at.ACCTG_TRANS_TYPE_ID                                               AS PAYMENT_TYPE_ID,
    CASE WHEN at.ACCTG_TRANS_TYPE_ID = 'PAYROL_INVOICE'
         THEN 'رواتب عامة (استحقاق)'
         ELSE 'رواتب عامة (دفع)' END                                     AS PaymentTypeDescription,

    -- Who the labour cost belongs to
    ate.PARTY_ID                                                         AS PartyIdFrom,
    COALESCE(emp.DESCRIPTION, ate.PARTY_ID, '')                          AS PartyIdFromName,
    'Company'                                                            AS PartyIdTo,
    'Golden Land'                                                        AS PartyIdToName,

    'POSTED'                                                             AS STATUS_ID,
    'تم الترحيل'                                                          AS StatusDescription,
    'Posted'                                                             AS StatusDescriptionEnglish,

    CAST(at.TRANSACTION_DATE AS DATE)                                    AS EFFECTIVE_DATE,
    at.CREATED_STAMP                                                     AS CreatedStamp,
    COALESCE(ate.DESCRIPTION, at.DESCRIPTION)                            AS COMMENTS,
    COALESCE(ate.VOUCHER_REF, at.VOUCHER_REF)                            AS PaymentRefNum,

    COALESCE(ate.AMOUNT, 0)                                              AS AMOUNT,
    COALESCE(ate.AMOUNT, 0)                                              AS ActualCurrencyAmount,
    COALESCE(ate.CURRENCY_UOM_ID, 'EGP')                                 AS CurrencyUomId,

    1                                                                    AS IsDisbursement,
    -- 1 = the monthly accrual (real labour cost incurred)
    -- 0 = an ad-hoc salary paid outside the accrual
    CASE WHEN at.ACCTG_TRANS_TYPE_ID = 'PAYROL_INVOICE' THEN 1 ELSE 0 END AS IsAccrual,

    ate.ORGANIZATION_PARTY_ID                                            AS OrganizationPartyId,

    -- The account that made this non-project: it exists, but DimProject does
    -- not recognise it as belonging to any project.
    ate.GL_ACCOUNT_ID                                                    AS GlAccountId,
    COALESCE(ga.ACCOUNT_NAME_ARABIC, ga.ACCOUNT_NAME)                    AS GlAccountName,
    ga.ACCOUNT_NAME                                                      AS GlAccountNameEnglish,

    'خارج نطاق المشاريع'                                                  AS ProjectName,
    'تم الترحيل'                                                          AS DueStatusArabic

FROM ACCTG_TRANS_ENTRY ate

JOIN ACCTG_TRANS at     ON at.ACCTG_TRANS_ID  = ate.ACCTG_TRANS_ID
LEFT JOIN PAYMENT pmt   ON pmt.PAYMENT_ID     = at.PAYMENT_ID
LEFT JOIN PARTY emp     ON emp.PARTY_ID       = ate.PARTY_ID
LEFT JOIN GL_ACCOUNT ga ON ga.GL_ACCOUNT_ID   = ate.GL_ACCOUNT_ID

-- ── The anti-join that defines "non-project" ────────────────────────────────
-- Mirror image of Fact_Project_Payroll's inner join. Change one, change the
-- other, or labour cost goes missing from both.
LEFT JOIN DimProject dp ON dp.GlAccountId     = ate.GL_ACCOUNT_ID

WHERE ate.DEBIT_CREDIT_FLAG = 'D'
  AND ate.GL_ACCOUNT_ID IS NOT NULL
  AND at.IS_POSTED = 'Y'

  -- not claimed by Fact_Project_Payroll
  AND dp.GlAccountId IS NULL

  AND (
        at.ACCTG_TRANS_TYPE_ID = 'PAYROL_INVOICE'
     OR (    at.ACCTG_TRANS_TYPE_ID = 'OUTGOING_PAYMENT'
         AND pmt.PAYMENT_TYPE_ID    = 'PAYROL_PAYMENT'
         AND pmt.PARTY_ID_TO       <> '276')   -- see the block comment above
      );

-- =============================================================================
-- Verification lives in Fact_NonProject_Payroll.verify.sql — run that next.
-- This file ends on a real statement so it can be executed whole: a trailing
-- comment-only statement is rejected by MySQL with [42000][1064] near ''.
-- =============================================================================

SELECT 'Fact_NonProject_Payroll created' AS status,
       COUNT(*)                          AS total_rows,
       ROUND(SUM(AMOUNT))                AS total_amount,
       SUM(IsAccrual)                    AS accrual_rows
FROM Fact_NonProject_Payroll;
