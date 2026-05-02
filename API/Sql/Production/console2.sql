INSERT INTO INVOICE_ITEM_TYPE (
    INVOICE_ITEM_TYPE_ID,
    PARENT_TYPE_ID,
    HAS_TABLE,
    DESCRIPTION,
    DESCRIPTION_ARABIC,
    DEFAULT_GL_ACCOUNT_ID,
    LAST_UPDATED_STAMP,
    LAST_UPDATED_TX_STAMP,
    CREATED_STAMP,
    CREATED_TX_STAMP,
    IS_POSITIVE_AMOUNT
) VALUES (
             'INV_LAND_PARTNERSHIP_ITEM',
             NULL,
             'N',
             'Invoice Land Partnership Item (Sales)',
             'بند أراضي مشاركات (مبيعات)',
             '250448',
             NULL,
             NULL,
             NOW(),
             NOW(),
             NULL
         );