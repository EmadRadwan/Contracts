-- =============================================================
-- BILLING_ACCOUNTS – Enriched active billing accounts view
-- Joins resolve foreign keys for project, party (role), and currency names
-- Computed used/remaining balance from related payments (active accounts only)
-- =============================================================
DROP VIEW IF EXISTS BillingAccounts;
CREATE OR REPLACE VIEW BillingAccounts AS

SELECT
    -- =================================================================
    -- Core Keys (keep for relationships)
    -- =================================================================
    ba.BILLING_ACCOUNT_ID                                   AS BillingAccountId,

    -- =================================================================
    -- Account Limits & Balances
    -- =================================================================
    ba.ACCOUNT_LIMIT                                        AS AccountLimit,
    COALESCE(
            (
                SELECT SUM(p.AMOUNT)
                FROM PAYMENT p
                WHERE p.WORK_EFFORT_ID = ba.WORK_EFFORT_ID
                  AND p.PARTY_ID_TO = bar.PARTY_ID
                  AND p.STATUS_ID = 'PMNT_SENT'
                  AND p.PAYMENT_TYPE_ID = 'ADVANCE_TO_VENDOR_CONTRACTOR'
            ), 0
        )                                                       AS UsedBalance,             -- Sum of qualifying advance payments

    -- REFACTOR: Computed as AccountLimit minus UsedBalance (handles NULL AccountLimit as 0)
    (COALESCE(ba.ACCOUNT_LIMIT, 0) - COALESCE(
            (
                SELECT SUM(p.AMOUNT)
                FROM PAYMENT p
                WHERE p.WORK_EFFORT_ID = ba.WORK_EFFORT_ID
                  AND p.PARTY_ID_TO = bar.PARTY_ID
                  AND p.STATUS_ID = 'PMNT_SENT'
                  AND p.PAYMENT_TYPE_ID = 'ADVANCE_TO_VENDOR_CONTRACTOR'
            ), 0
        ))                                                      AS RemainingBalance,

    -- =================================================================
    -- Project (Work Effort)
    -- =================================================================
    ba.WORK_EFFORT_ID                                       AS ProjectId,
    we.PROJECT_NAME                                         AS ProjectName,             -- e.g. "مشروع 1234"

    -- =================================================================
    -- Currency
    -- =================================================================
    ba.ACCOUNT_CURRENCY_UOM_ID                              AS AccountCurrencyUomId,
    uom.DESCRIPTION                                         AS AccountCurrencyUomDescription,

    -- =================================================================
    -- Party (via BillingAccountRole)
    -- =================================================================
    bar.PARTY_ID                                            AS PartyId,
    pty.DESCRIPTION                                         AS PartyName,               -- e.g. "المورد أحمد", "عميل 123"

    -- =================================================================
    -- Dates
    -- =================================================================
    ba.FROM_DATE                                            AS FromDate,
    ba.THRU_DATE                                            AS ThruDate               -- NULL for active accounts (filtered below)

    -- =================================================================
    -- Additional raw columns (for completeness, add more if needed)
    -- =================================================================
    -- ba.OTHER_COLUMN                                       AS OtherColumnExample

FROM BILLING_ACCOUNT ba

-- Join to roles (assuming one active role per account; adjust if multiple possible)
         INNER JOIN BILLING_ACCOUNT_ROLE bar
                    ON ba.BILLING_ACCOUNT_ID = bar.BILLING_ACCOUNT_ID

-- Party from role
         INNER JOIN PARTY pty
                    ON bar.PARTY_ID = pty.PARTY_ID

-- Currency UoM
         INNER JOIN UOM uom
                    ON ba.ACCOUNT_CURRENCY_UOM_ID = uom.UOM_ID

-- Project (Work Effort)
         INNER JOIN WORK_EFFORT we
                    ON ba.WORK_EFFORT_ID = we.WORK_EFFORT_ID

-- Filter for active billing accounts only (ThruDate IS NULL)
WHERE ba.THRU_DATE IS NULL

ORDER BY ba.BILLING_ACCOUNT_ID;