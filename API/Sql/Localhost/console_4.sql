SELECT
    pga.PARTY_ID,
    COUNT(*) AS transaction_count
FROM PARTY_GL_ACCOUNT pga
         INNER JOIN ACCTG_TRANS_ENTRY ate
                    ON ate.GL_ACCOUNT_ID = pga.GL_ACCOUNT_ID
         INNER JOIN ACCTG_TRANS at
                    ON at.ACCTG_TRANS_ID = ate.ACCTG_TRANS_ID
WHERE at.IS_POSTED = 'Y'
GROUP BY pga.PARTY_ID
ORDER BY transaction_count DESC
LIMIT 1;