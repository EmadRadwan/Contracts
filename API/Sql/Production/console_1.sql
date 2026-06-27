INSERT INTO PAYMENT_METHOD (
    PAYMENT_METHOD_ID,
    PAYMENT_METHOD_TYPE_ID,
    PARTY_ID,
    GL_ACCOUNT_ID,
    FIN_ACCOUNT_ID,
    DESCRIPTION,
    FROM_DATE,
    THRU_DATE,
    LAST_UPDATED_STAMP,
    LAST_UPDATED_TX_STAMP,
    CREATED_STAMP,
    CREATED_TX_STAMP
) VALUES (
             'QNB_CHECKING_USD',
             'COMPANY_CHECK',
             'Company',
             '114300',
             NULL,
             'QNB - USD',
             '2026-06-22 19:06:32',
             NULL,
             '2026-06-22 19:06:32',
             '2026-06-22 19:06:32',
             '2026-06-22 19:06:32',
             '2026-06-22 19:06:32'
         );