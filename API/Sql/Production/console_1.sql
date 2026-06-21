INSERT INTO `SEQUENCE_VALUE_ITEM` (
    `SEQ_NAME`,
    `SEQ_ID`,
    `LAST_UPDATED_STAMP`,
    `LAST_UPDATED_TX_STAMP`,
    `CREATED_STAMP`,
    `CREATED_TX_STAMP`
) VALUES (
             'SalesCommission',
             11111,                    -- ← Change this to the actual sequence ID
             '2026-06-20 15:43:00',    -- ← Update with current timestamp if needed
             NULL,
             '2026-06-20 15:43:00',    -- ← Usually same as LAST_UPDATED_STAMP for new entries
             NULL
         );