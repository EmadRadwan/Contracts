CREATE OR REPLACE VIEW dim_gl_class_report_map AS
SELECT
    gl_account_class_id,
    report,
    class,
    sub_class,
    sub_class2,
    normal_balance,
    sort_order
FROM GlClassReportMap;