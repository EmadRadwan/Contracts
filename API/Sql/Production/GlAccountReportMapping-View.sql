CREATE OR REPLACE VIEW dim_gl_account_report_mapping AS
SELECT
    organization_party_id,
    report,
    class,
    sub_class,
    sub_class2,
    account,
    sub_account,
    account_code,
    gl_account_id,
    gl_account_class_id,
    normal_balance,
    sort_order
FROM GlAccountReportMapping;