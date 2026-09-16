-- Ledger-integrity step 2 (auditor soft-delete requirement), 13 Sep 2026.
-- Adds the one cancelled status the classification found missing, so a sales request can be
-- voided (kept, marked cancelled) instead of physically deleted. Idempotent; safe to re-run.
-- Apply manually to localhost first, then production (copy to API/Sql/Production when approved).

INSERT INTO STATUS_ITEM (STATUS_ID, STATUS_TYPE_ID, STATUS_CODE, SEQUENCE_ID, DESCRIPTION,
                         LAST_UPDATED_STAMP, LAST_UPDATED_TX_STAMP, CREATED_STAMP, CREATED_TX_STAMP)
SELECT 'SALES_REQUEST_CANCELLED', 'SALES_REQUEST_STATUS', 'CANCELLED', '99', 'Cancelled',
       NOW(), NOW(), NOW(), NOW()
WHERE NOT EXISTS (SELECT 1 FROM STATUS_ITEM WHERE STATUS_ID = 'SALES_REQUEST_CANCELLED');

-- Valid transitions: a created or approved request may be cancelled; nothing leaves cancelled.
INSERT INTO STATUS_VALID_CHANGE (STATUS_ID, STATUS_ID_TO, CONDITION_EXPRESSION, TRANSITION_NAME,
                                 LAST_UPDATED_STAMP, LAST_UPDATED_TX_STAMP, CREATED_STAMP, CREATED_TX_STAMP)
SELECT 'SALES_REQUEST_CREATED', 'SALES_REQUEST_CANCELLED', NULL, 'Cancel', NOW(), NOW(), NOW(), NOW()
WHERE NOT EXISTS (SELECT 1 FROM STATUS_VALID_CHANGE
                  WHERE STATUS_ID = 'SALES_REQUEST_CREATED' AND STATUS_ID_TO = 'SALES_REQUEST_CANCELLED');

INSERT INTO STATUS_VALID_CHANGE (STATUS_ID, STATUS_ID_TO, CONDITION_EXPRESSION, TRANSITION_NAME,
                                 LAST_UPDATED_STAMP, LAST_UPDATED_TX_STAMP, CREATED_STAMP, CREATED_TX_STAMP)
SELECT 'SALES_REQUEST_APPROVED', 'SALES_REQUEST_CANCELLED', NULL, 'Cancel', NOW(), NOW(), NOW(), NOW()
WHERE NOT EXISTS (SELECT 1 FROM STATUS_VALID_CHANGE
                  WHERE STATUS_ID = 'SALES_REQUEST_APPROVED' AND STATUS_ID_TO = 'SALES_REQUEST_CANCELLED');

-- Verification
SELECT STATUS_ID, STATUS_TYPE_ID, DESCRIPTION FROM STATUS_ITEM WHERE STATUS_TYPE_ID = 'SALES_REQUEST_STATUS';
SELECT STATUS_ID, STATUS_ID_TO FROM STATUS_VALID_CHANGE WHERE STATUS_ID_TO = 'SALES_REQUEST_CANCELLED';

-- Diagnostic for section 5.2 of the classification: bank rows sitting on draft payments.
-- Post-dated cheques recorded before receipt are legitimate; anything else is the ResetPayment leak.
SELECT p.PAYMENT_ID, p.PAYMENT_TYPE_ID, p.STATUS_ID, p.`ChequeNumber`, p.`ChequeDate`,
       f.FIN_ACCOUNT_TRANS_ID, f.FIN_ACCOUNT_TRANS_TYPE_ID, f.STATUS_ID AS FAT_STATUS, f.AMOUNT, f.TRANSACTION_DATE,
       CASE WHEN p.`ChequeNumber` IS NOT NULL AND p.`ChequeNumber` <> '' THEN 'cheque (probably legitimate)'
            ELSE 'leak candidate' END AS ASSESSMENT
FROM FIN_ACCOUNT_TRANS f
JOIN PAYMENT p ON p.PAYMENT_ID = f.PAYMENT_ID
WHERE p.STATUS_ID = 'PMNT_NOT_PAID'
ORDER BY ASSESSMENT, f.TRANSACTION_DATE;
