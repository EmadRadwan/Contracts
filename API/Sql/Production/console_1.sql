INSERT INTO INVOICE_ITEM_TYPE (
    INVOICE_ITEM_TYPE_ID,
    PARENT_TYPE_ID,
    HAS_TABLE,
    DESCRIPTION,
    DESCRIPTION_ARABIC,
    DEFAULT_GL_ACCOUNT_ID,
    IS_POSITIVE_AMOUNT,
    CREATED_STAMP,
    CREATED_TX_STAMP,
    LAST_UPDATED_STAMP,
    LAST_UPDATED_TX_STAMP
) VALUES (
             'INV_STOCK_ITEM',
             NULL,
             'N',
             'Invoice Stock Item(Sales)',
             'بند بيع الأسهم (مبيعات)',
             '303000',
             1,
             NOW(),
             NOW(),
             NOW(),
             NOW()
         );  