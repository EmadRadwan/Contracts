INSERT INTO GlAccountReportMapping (
    organization_party_id,
    gl_account_id,
    gl_account_class_id,

    statement_type,
    statement_section,
    category,
    line_item,

    statement_section_ar,
    category_ar,
    line_item_ar,

    account_type,
    account_type_ar,
    account_name_ar,
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

    m.statement_section_ar,
    m.category_ar,
    m.line_item_ar,

    t.DESCRIPTION AS account_type,

    -- ⚠️ fallback (until Arabic exists in OFBiz types)
    t.DESCRIPTION AS account_type_ar,

    -- EN / AR account names
    g.ACCOUNT_NAME_ARABIC   AS account_name_ar,

    g.ACCOUNT_CODE,

    m.normal_balance,

    -- ✅ central sign logic
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

WHERE m.statement_type <> 'Excluded';