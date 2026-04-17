-- =============================================================
-- ACCOUNTING_TRANSACTION_ENTRIES – Enriched view with names and multilingual support
-- Resolves foreign keys for transaction, GL account (with Arabic/English names), party, project
-- Optional company filter handled in application layer (via OData), but view includes organization link
-- =============================================================
DROP VIEW IF EXISTS AccountingTransactionEntries;
CREATE OR REPLACE VIEW AccountingTransactionEntries AS

SELECT
    -- =================================================================
    -- Core Keys
    -- =================================================================
    te.ACCTG_TRANS_ENTRY_SEQ_ID                            AS AcctgTransEntrySeqId,
    te.ACCTG_TRANS_ID                                      AS AcctgTransId,

    gla.ACCOUNT_NAME                                        AS GlAccountNameEnglish,
    gla.ACCOUNT_NAME_ARABIC                                 AS GlAccountNameArabic,

    -- =================================================================
    -- Description & Amounts
    -- =================================================================
    te.DESCRIPTION                                          AS Description,
    te.AMOUNT                                               AS Amount,
    te.DEBIT_CREDIT_FLAG                                    AS DebitCreditFlag,
    te.GL_ACCOUNT_TYPE_ID                                   AS GlAccountTypeId,
    te.GL_ACCOUNT_ID                                        AS GlAccountId,

    -- =================================================================
    -- Party
    -- =================================================================
    te.PARTY_ID                                             AS PartyId,
    p.DESCRIPTION                                           AS PartyName,

    -- =================================================================
    -- Product (raw ID, name not joined – add if PRODUCT table has description)
    -- =================================================================
    te.PRODUCT_ID                                           AS ProductId,

    -- =================================================================
    -- Related Documents
    -- =================================================================
    t.INVOICE_ID                                            AS InvoiceId,
    t.PAYMENT_ID                                            AS PaymentId,

    -- =================================================================
    -- Project (Work Effort)
    -- =================================================================
    t.WORK_EFFORT_ID                                        AS WorkEffortId,
    we.CERTIFICATE_NUMBER                                   AS CertificateNumber,       -- As used in query

    -- =================================================================
    -- Transaction Level Fields
    -- =================================================================
    t.IS_POSTED                                             AS IsPosted,
    t.POSTED_DATE                                           AS PostedDate,
    t.TRANSACTION_DATE                                      AS TransactionDate,
    t.GL_FISCAL_TYPE_ID                                     AS GlFiscalTypeId,

    -- =================================================================
    -- Organization (Company) Link – for filtering by CompanyId
    -- =================================================================
    go.ORGANIZATION_PARTY_ID                                AS OrganizationPartyId      -- Use in app: WHERE OrganizationPartyId = @CompanyId OR @CompanyId IS NULL

    -- Additional raw fields can be added here if needed

FROM ACCTG_TRANS_ENTRY te

-- Transaction (left join – entry can exist without trans in some cases)
         LEFT JOIN ACCTG_TRANS t
                   ON te.ACCTG_TRANS_ID = t.ACCTG_TRANS_ID

-- GL Account
         LEFT JOIN GL_ACCOUNT gla
                   ON te.GL_ACCOUNT_ID = gla.GL_ACCOUNT_ID

-- GL Account Organization (for company scoping)
         LEFT JOIN GL_ACCOUNT_ORGANIZATION go
                   ON gla.GL_ACCOUNT_ID = go.GL_ACCOUNT_ID

-- Party
         LEFT JOIN PARTY p
                   ON te.PARTY_ID = p.PARTY_ID

-- Work Effort (Project)
         LEFT JOIN WORK_EFFORT we
                   ON t.WORK_EFFORT_ID = we.WORK_EFFORT_ID;