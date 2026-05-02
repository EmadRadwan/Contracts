DROP TABLE IF EXISTS GlAccountReportMapping;

CREATE TABLE GlAccountReportMapping (
                                        id                      INT             NOT NULL AUTO_INCREMENT,
                                        organization_party_id   VARCHAR(20)     NOT NULL,
                                        gl_account_id           VARCHAR(20)     NOT NULL,
                                        gl_account_class_id     VARCHAR(50)     NOT NULL,

    -- FINANCIAL STRUCTURE
                                        statement_type          VARCHAR(30)     NOT NULL,   -- Balance Sheet / Profit and Loss
                                        statement_section       VARCHAR(50)     NOT NULL,   -- Assets, Liabilities, Equity, Revenue, Expenses
                                        category                VARCHAR(100)    NOT NULL,   -- Current Assets, Cash Expenses, etc.
                                        line_item               VARCHAR(100)    NOT NULL,   -- reporting line

    -- ACCOUNT INFO
                                        account_type            VARCHAR(200)    NULL,
                                        account_name            VARCHAR(200)    NOT NULL,
                                        account_code            VARCHAR(20)     NOT NULL,

    -- ACCOUNTING BEHAVIOR
                                        normal_balance          CHAR(1)         NOT NULL,   -- D / C

    -- OPTIONAL (recommended → removes need for logic in fact)
                                        sign_multiplier         TINYINT         NOT NULL,   -- +1 / -1

    -- SORTING
                                        section_order           INT             NOT NULL,
                                        category_order          INT             NOT NULL,
                                        line_item_order         INT             NOT NULL,

                                        created_stamp           DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
                                        last_updated_stamp      DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP
                                            ON UPDATE CURRENT_TIMESTAMP,

                                        CONSTRAINT PK_GlAccountReportMapping
                                            PRIMARY KEY (id),

                                        CONSTRAINT UQ_GlAccountReportMapping_OrgAccount
                                            UNIQUE (organization_party_id, gl_account_id),

                                        CONSTRAINT CHK_normal_balance
                                            CHECK (normal_balance IN ('D','C')),

                                        CONSTRAINT FK_GlAccountReportMapping_GlAccount
                                            FOREIGN KEY (gl_account_id)
                                                REFERENCES GL_ACCOUNT (GL_ACCOUNT_ID)
);

CREATE INDEX IX_GlAccountReportMapping_Org
    ON GlAccountReportMapping (organization_party_id);

CREATE INDEX IX_GlAccountReportMapping_Report
    ON GlAccountReportMapping (organization_party_id, statement_type, section_order);

CREATE INDEX IX_GlAccountReportMapping_GlAccount
    ON GlAccountReportMapping (gl_account_id);