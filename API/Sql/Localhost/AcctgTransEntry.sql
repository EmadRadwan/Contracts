SELECT
    at.ACCTG_TRANS_ID,
    at.ACCTG_TRANS_TYPE_ID,
    at.TRANSACTION_DATE,
    at.IS_POSTED,
    at.POSTED_DATE,
    at.PARTY_ID,
    p.PARTY_TYPE_ID,
    p.STATUS_ID,

    -- Shipment
    at.SHIPMENT_ID,
    s.SHIPMENT_TYPE_ID,
    s.STATUS_ID AS SHIPMENT_STATUS,

    -- Invoice
    at.INVOICE_ID,
    i.INVOICE_TYPE_ID,
    i.INVOICE_DATE,
    i.STATUS_ID AS INVOICE_STATUS,

    -- Payment
    at.PAYMENT_ID,
    pay.PAYMENT_TYPE_ID,
    pay.EFFECTIVE_DATE,
    pay.STATUS_ID AS PAYMENT_STATUS,

    -- Accounting Entries
    ate.ACCTG_TRANS_ENTRY_SEQ_ID,
    ate.DEBIT_CREDIT_FLAG,
    ate.AMOUNT,
    ga.GL_ACCOUNT_ID,
    ga.ACCOUNT_NAME,
    ga.GL_ACCOUNT_TYPE_ID

FROM ACCTG_TRANS at

         LEFT JOIN PARTY p
                   ON at.PARTY_ID = p.PARTY_ID

         LEFT JOIN SHIPMENT s
                   ON at.SHIPMENT_ID = s.SHIPMENT_ID

         LEFT JOIN INVOICE i
                   ON at.INVOICE_ID = i.INVOICE_ID

         LEFT JOIN PAYMENT pay
                   ON at.PAYMENT_ID = pay.PAYMENT_ID

         LEFT JOIN ACCTG_TRANS_ENTRY ate
                   ON at.ACCTG_TRANS_ID = ate.ACCTG_TRANS_ID

         LEFT JOIN GL_ACCOUNT ga
                   ON ate.GL_ACCOUNT_ID = ga.GL_ACCOUNT_ID;
