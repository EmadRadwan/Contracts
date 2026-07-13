DROP VIEW IF EXISTS Fact_GL_Transactions;

CREATE VIEW Fact_GL_Transactions AS
SELECT
    -- ── TRANSACTION HEADER ─────────────────────────────────────────
    t.ACCTG_TRANS_ID,
    t.ACCTG_TRANS_TYPE_ID,
    t.GL_FISCAL_TYPE_ID,
    t.IS_POSTED,

    DATE(t.TRANSACTION_DATE) AS transaction_date,
    DATE(t.POSTED_DATE)      AS posted_date,

    COALESCE(
            NULLIF(t.DESCRIPTION, ''),
            ed.entry_description
    ) AS trans_description,

    t.INVOICE_ID,
    t.PAYMENT_ID,
    t.PARTY_ID,
    t.VOUCHER_REF,

    -- ── ENTRY LINE (GRAIN = 1 ROW PER ENTRY) ──────────────────────
    e.ACCTG_TRANS_ENTRY_SEQ_ID,
    e.GL_ACCOUNT_ID,
    e.ORGANIZATION_PARTY_ID,

    e.DEBIT_CREDIT_FLAG,
    e.AMOUNT,

    e.CURRENCY_UOM_ID,
    e.RECONCILE_STATUS_ID

FROM ACCTG_TRANS t

         JOIN ACCTG_TRANS_ENTRY e
              ON t.ACCTG_TRANS_ID = e.ACCTG_TRANS_ID

         LEFT JOIN (
    SELECT
        ACCTG_TRANS_ID,
        MAX(DESCRIPTION) AS entry_description
    FROM ACCTG_TRANS_ENTRY
    WHERE DESCRIPTION IS NOT NULL
      AND DESCRIPTION <> ''
    GROUP BY ACCTG_TRANS_ID
) ed
                   ON t.ACCTG_TRANS_ID = ed.ACCTG_TRANS_ID

WHERE t.IS_POSTED = 'Y';