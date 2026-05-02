CREATE TABLE GlAccountReportMapping (
                                        id                    INT             NOT NULL AUTO_INCREMENT,
                                        organization_party_id VARCHAR(20)     NOT NULL,
                                        gl_account_id         VARCHAR(20)     NOT NULL,
                                        gl_account_class_id   VARCHAR(50)     NOT NULL,

                                        report                VARCHAR(50)     NOT NULL,
                                        class                 VARCHAR(100)    NOT NULL,
                                        sub_class             VARCHAR(100)    NOT NULL,
                                        sub_class2            VARCHAR(100)    NOT NULL,

                                        account               VARCHAR(200)    NULL,
                                        sub_account           VARCHAR(200)    NOT NULL,
                                        account_code          VARCHAR(20)     NOT NULL,

                                        normal_balance        CHAR(1)         NOT NULL,
                                        sort_order            INT             NOT NULL,

                                        created_stamp         DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
                                        last_updated_stamp    DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP
                                            ON UPDATE CURRENT_TIMESTAMP,

                                        CONSTRAINT PK_GlAccountReportMapping
                                            PRIMARY KEY (id),

                                        CONSTRAINT UQ_GlAccountReportMapping_OrgAccount
                                            UNIQUE (organization_party_id, gl_account_id),

                                        CONSTRAINT CHK_normal_balance
                                            CHECK (normal_balance IN ('D','C')),

                                        CONSTRAINT FK_GlAccountReportMapping_GlAccount
                                            FOREIGN KEY (gl_account_id)
                                                REFERENCES GL_ACCOUNT (GL_ACCOUNT_ID),

                                        CONSTRAINT FK_GlAccountReportMapping_GlClassReportMap
                                            FOREIGN KEY (gl_account_class_id)
                                                REFERENCES GlClassReportMap (gl_account_class_id)
);

CREATE INDEX IX_GlAccountReportMapping_Org
    ON GlAccountReportMapping (organization_party_id);

CREATE INDEX IX_GlAccountReportMapping_Report
    ON GlAccountReportMapping (organization_party_id, report, sort_order);

CREATE INDEX IX_GlAccountReportMapping_GlAccount
    ON GlAccountReportMapping (gl_account_id);

-- =============================================================================


INSERT INTO GlAccountReportMapping (
    organization_party_id,
    gl_account_id,
    gl_account_class_id,
    report,
    class,
    sub_class,
    sub_class2,
    account,
    sub_account,
    account_code,
    normal_balance,
    sort_order
)
SELECT
    o.ORGANIZATION_PARTY_ID                                        AS organization_party_id,
    g.GL_ACCOUNT_ID                                                AS gl_account_id,
    g.GL_ACCOUNT_CLASS_ID                                           AS gl_account_class_id,
    m.report,
    m.class,
    m.sub_class,
    m.sub_class2,
    t.DESCRIPTION                                                AS account,
    g.ACCOUNT_NAME_ARABIC                                                AS sub_account,
    g.ACCOUNT_CODE                                                AS account_code,
    m.normal_balance,
    m.sort_order * 1000 + COALESCE(CAST(g.ACCOUNT_CODE AS UNSIGNED), 0)
                                                                 AS computed_sort_order

FROM       GL_ACCOUNT             g
               JOIN       GlClassReportMap      m  ON  g.GL_ACCOUNT_CLASS_ID    = m.gl_account_class_id
               LEFT JOIN  GL_ACCOUNT_TYPE         t  ON  g.GL_ACCOUNT_TYPE_ID     = t.GL_ACCOUNT_TYPE_ID
               JOIN       GL_ACCOUNT_ORGANIZATION o  ON  g.GL_ACCOUNT_ID         = o.GL_ACCOUNT_ID
    AND  o.ORGANIZATION_PARTY_ID = 'Company'

WHERE  m.report <> 'Excluded'

ORDER  BY computed_sort_order;