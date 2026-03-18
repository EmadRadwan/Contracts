SELECT 
'Before 31-Dec-2025'                          AS period,
    SUM(CASE WHEN e.DEBIT_CREDIT_FLAG = 'D' THEN e.AMOUNT ELSE 0 END)   AS total_debit,
    SUM(CASE WHEN e.DEBIT_CREDIT_FLAG = 'C' THEN e.AMOUNT ELSE 0 END)   AS total_credit,
    SUM(CASE 
WHEN e.DEBIT_CREDIT_FLAG = 'D' THEN e.AMOUNT 
    WHEN e.DEBIT_CREDIT_FLAG = 'C' THEN -e.AMOUNT 
ELSE 0 
END)                                           AS net_movement,
    
-- Opening balance from the very first entry (usually the only one with description like 'الرصيد الافتتاحي')
MAX(CASE WHEN e.DESCRIPTION = 'الرصيد الافتتاحي' 
THEN e.AMOUNT 
    ELSE 0 END)                               AS opening_balance
    
FROM acctg_trans h
    INNER JOIN acctg_trans_entry e 
    ON h.ACCTG_TRANS_ID = e.ACCTG_TRANS_ID
    
WHERE e.GL_ACCOUNT_ID = '111010'
AND h.TRANSACTION_DATE < '2025-12-31'     -- strict before 31-Dec-2025
    -- Optional: AND h.IS_POSTED = 'Y'        -- uncomment if you only want posted transactions
    ;

-- Alternative version: separate rows for clarity (easier to read in some tools)
SELECT 'Opening balance' AS item, 
    SUM(e.AMOUNT)     AS amount
    FROM acctg_trans h
INNER JOIN acctg_trans_entry e ON h.ACCTG_TRANS_ID = e.ACCTG_TRANS_ID
WHERE e.GL_ACCOUNT_ID = '111010'
AND e.DESCRIPTION = 'الرصيد الافتتاحي'

UNION ALL

SELECT 'Total Debit before 31-Dec-2025', 
SUM(e.AMOUNT)
FROM acctg_trans h
    INNER JOIN acctg_trans_entry e ON h.ACCTG_TRANS_ID = e.ACCTG_TRANS_ID
WHERE e.GL_ACCOUNT_ID = '111010'
AND e.DEBIT_CREDIT_FLAG = 'D'
AND h.TRANSACTION_DATE < '2025-12-31'

UNION ALL

SELECT 'Total Credit before 31-Dec-2025', 
SUM(e.AMOUNT)
FROM acctg_trans h
    INNER JOIN acctg_trans_entry e ON h.ACCTG_TRANS_ID = e.ACCTG_TRANS_ID
WHERE e.GL_ACCOUNT_ID = '111010'
AND e.DEBIT_CREDIT_FLAG = 'C'
AND h.TRANSACTION_DATE < '2025-12-31';