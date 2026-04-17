UPDATE PAYMENT
SET
    OVERRIDE_GL_ACCOUNT_ID = NULL,
    STATUS_ID = 'PMNT_NOT_PAID',
    LAST_UPDATED_STAMP = NOW()
WHERE PAYMENT_ID = '10848';

-- Delete entries belonging to any transaction created from this payment
DELETE ate
FROM ACCTG_TRANS_ENTRY ate
         INNER JOIN ACCTG_TRANS at
                    ON ate.ACCTG_TRANS_ID = at.ACCTG_TRANS_ID
WHERE at.PAYMENT_ID = '10848';

-- Then delete the transaction header(s)
DELETE FROM ACCTG_TRANS
WHERE PAYMENT_ID = '10848';