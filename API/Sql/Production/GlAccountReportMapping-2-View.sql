DROP VIEW IF EXISTS dim_GlAccountReportMapping;

CREATE VIEW dim_GlAccountReportMapping AS
SELECT
    -- KEYS
    id,
    organization_party_id,
    gl_account_id,
    gl_account_class_id,

    -- STATEMENT
    statement_type,

    -- ENGLISH HIERARCHY
    statement_section,
    category,
    line_item,

    -- ARABIC HIERARCHY
    statement_section_ar,
    category_ar,
    line_item_ar,

    -- ACCOUNT DETAILS (EN)
    account_type,

    -- ACCOUNT DETAILS (AR)
    account_type_ar,
    account_name_ar,

    account_code,

    -- ACCOUNTING
    normal_balance,
    sign_multiplier,

    -- SORTING
    section_order,
    category_order,
    line_item_order

FROM GlAccountReportMapping;