CREATE VIEW vw_GL_Transactions AS
SELECT
    -- ── TRANSACTION HEADER ─────────────────────────────────────────
t.ACCTG_TRANS_ID,
t.ACCTG_TRANS_TYPE_ID,
t.GL_FISCAL_TYPE_ID,
DATE(t.TRANSACTION_DATE)            AS transaction_date,  -- join key to Calendar
t.POSTED_DATE,
t.DESCRIPTION                       AS trans_description,
    t.INVOICE_ID,
t.PAYMENT_ID,
t.PARTY_ID,
t.VOUCHER_REF,

-- ── ENTRY LINE ─────────────────────────────────────────────────
e.ACCTG_TRANS_ENTRY_SEQ_ID,
e.GL_ACCOUNT_ID,
e.ORGANIZATION_PARTY_ID,
e.DEBIT_CREDIT_FLAG,
e.AMOUNT,
e.CURRENCY_UOM_ID,
e.RECONCILE_STATUS_ID,

-- ── CALCULATED AMOUNTS ─────────────────────────────────────────
CASE
    WHEN m.normal_balance = e.DEBIT_CREDIT_FLAG THEN  e.AMOUNT
ELSE                                              -e.AMOUNT
END                                 AS signed_amount,
    CASE WHEN e.DEBIT_CREDIT_FLAG = 'D' THEN e.AMOUNT ELSE 0 END AS debit_amount,
    CASE WHEN e.DEBIT_CREDIT_FLAG = 'C' THEN e.AMOUNT ELSE 0 END AS credit_amount,

-- ── ACCOUNT HIERARCHY ──────────────────────────────────────────
m.report,
m.class,
m.sub_class,
m.sub_class2,
m.account,
m.sub_account,
m.account_code,
m.normal_balance,
m.sort_order

    FROM       ACCTG_TRANS              t
JOIN       ACCTG_TRANS_ENTRY        e  ON  t.ACCTG_TRANS_ID       = e.ACCTG_TRANS_ID
JOIN       GlAccountReportMapping   m  ON  e.GL_ACCOUNT_ID         = m.gl_account_id
AND  e.ORGANIZATION_PARTY_ID = m.organization_party_id
WHERE  t.IS_POSTED         = 'Y'
AND  t.GL_FISCAL_TYPE_ID = 'ACTUAL'
AND  m.report            <> 'Excluded';