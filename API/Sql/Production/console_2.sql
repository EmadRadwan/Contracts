INSERT INTO GL_ACCOUNT (
    GL_ACCOUNT_ID,
    GL_ACCOUNT_TYPE_ID,
    GL_ACCOUNT_CLASS_ID,
    GL_RESOURCE_TYPE_ID,
    GL_XBRL_CLASS_ID,
    PARENT_GL_ACCOUNT_ID,
    ACCOUNT_CODE,
    ACCOUNT_NAME,
    DESCRIPTION,
    ACCOUNT_NAME_ARABIC,
    PRODUCT_ID,
    EXTERNAL_ID,
    LAST_UPDATED_STAMP,
    LAST_UPDATED_TX_STAMP,
    CREATED_STAMP,
    CREATED_TX_STAMP
) VALUES (
             '401010',
             NULL,
             'REVENUE',
             'MONEY',
             NULL,
             '400000',
             '401010',
             'Land SALES',
             NULL,
             'إيرادات  بيع الاراضى',
             NULL,
             NULL,
             '2022-05-27 12:24:27',
             '2022-05-27 12:24:25',
             '2022-05-27 12:24:27',
             '2022-05-27 12:24:25'
         );

INSERT INTO INVOICE_ITEM_TYPE (
    INVOICE_ITEM_TYPE_ID,
    PARENT_TYPE_ID,
    HAS_TABLE,
    DESCRIPTION,
    DESCRIPTION_ARABIC,
    DEFAULT_GL_ACCOUNT_ID
) VALUES (
             'INV_LAND_ITEM',
             NULL,
             'N',
             'Invoice Land Item (Sales)',
             'بند أراضى (مبيعات)',
             '401010'
         );