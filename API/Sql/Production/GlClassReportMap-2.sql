DROP TABLE IF EXISTS GlClassReportMap;

CREATE TABLE GlClassReportMap (
                                  gl_account_class_id   VARCHAR(50)  NOT NULL PRIMARY KEY,

                                  statement_type        VARCHAR(30)  NOT NULL,   -- Balance Sheet / Profit and Loss
                                  statement_section     VARCHAR(50)  NOT NULL,   -- Assets, Liabilities, Equity, Revenue, Expense
                                  category              VARCHAR(100) NOT NULL,   -- Current Assets, Cash Expenses, etc.
                                  line_item             VARCHAR(100) NOT NULL,   -- Reporting line (lowest grouping)

                                  normal_balance        CHAR(1)      NOT NULL,   -- D / C

                                  section_order         INT          NOT NULL,
                                  category_order        INT          NOT NULL,
                                  line_item_order       INT          NOT NULL
);