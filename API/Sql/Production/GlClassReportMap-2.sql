DROP TABLE IF EXISTS GlClassReportMap;

CREATE TABLE GlClassReportMap (
                                  gl_account_class_id   VARCHAR(50)  NOT NULL PRIMARY KEY,

    -- STATEMENT
                                  statement_type        VARCHAR(30)  NOT NULL,   -- Balance Sheet / Profit and Loss

    -- ENGLISH LABELS
                                  statement_section     VARCHAR(50)  NOT NULL,
                                  category              VARCHAR(100) NOT NULL,
                                  line_item             VARCHAR(100) NOT NULL,

    -- ARABIC LABELS
                                  statement_section_ar  VARCHAR(100) NOT NULL,
                                  category_ar           VARCHAR(100) NOT NULL,
                                  line_item_ar          VARCHAR(100) NOT NULL,

    -- ACCOUNTING BEHAVIOR
                                  normal_balance        CHAR(1)      NOT NULL,   -- D / C

    -- SORTING
                                  section_order         INT          NOT NULL,
                                  category_order        INT          NOT NULL,
                                  line_item_order       INT          NOT NULL
);