INSERT INTO GlAccountReportMapping (
    organization_party_id,
    gl_account_id,
    gl_account_class_id,

    statement_type,
    statement_section,
    category,
    line_item,

    account_type,
    account_name,
    account_code,

    normal_balance,
    sign_multiplier,

    section_order,
    category_order,
    line_item_order
)
SELECT
    o.ORGANIZATION_PARTY_ID,
    g.GL_ACCOUNT_ID,
    g.GL_ACCOUNT_CLASS_ID,

    m.statement_type,
    m.statement_section,
    m.category,
    m.line_item,

    t.DESCRIPTION AS account_type,
    g.ACCOUNT_NAME_ARABIC AS account_name,
    g.ACCOUNT_CODE,

    m.normal_balance,

    -- ✅ core simplification (used in Power BI)
    CASE
        WHEN m.normal_balance = 'D' THEN 1
        ELSE -1
        END AS sign_multiplier,

    m.section_order,
    m.category_order,
    m.line_item_order

FROM GL_ACCOUNT g

         JOIN GlClassReportMap m
              ON g.GL_ACCOUNT_CLASS_ID = m.gl_account_class_id

         LEFT JOIN GL_ACCOUNT_TYPE t
                   ON g.GL_ACCOUNT_TYPE_ID = t.GL_ACCOUNT_TYPE_ID

         JOIN GL_ACCOUNT_ORGANIZATION o
              ON g.GL_ACCOUNT_ID = o.GL_ACCOUNT_ID

-- keep excluded out of the model
WHERE m.statement_type <> 'Excluded';