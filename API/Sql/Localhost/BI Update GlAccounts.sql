UPDATE GL_ACCOUNT
SET
    GL_REPORT_ID = CASE
                       WHEN GL_ACCOUNT_CLASS_ID LIKE '%ASSET%' OR GL_ACCOUNT_CLASS_ID LIKE '%LIAB%' OR GL_ACCOUNT_CLASS_ID = 'EQUITY' THEN 'BS'
                       ELSE 'PL'
        END,
    GL_CLASS_COURSE_ID = CASE
                             WHEN GL_ACCOUNT_CLASS_ID LIKE '%ASSET%' THEN 'ASSET'
                             WHEN GL_ACCOUNT_CLASS_ID LIKE '%LIAB%' THEN 'LIABILITY'
                             WHEN GL_ACCOUNT_CLASS_ID = 'EQUITY' THEN 'EQUITY'
                             WHEN GL_ACCOUNT_CLASS_ID = 'REVENUE' THEN 'TRADING'
                             ELSE 'OPERATING'
        END;


UPDATE GL_ACCOUNT
SET
    GL_SUB_CLASS_2_ID = CASE
        -- Resource Type logic
                            WHEN GL_RESOURCE_TYPE_ID IN ('RAW_MATERIALS', 'FINISHED_GOODS') THEN 'DIRECT_COST'
                            WHEN GL_RESOURCE_TYPE_ID = 'SERVICES' AND GL_ACCOUNT_TYPE_ID LIKE '%MARKETING%' THEN 'MARKETING'
        -- Specific Type logic
                            WHEN GL_ACCOUNT_TYPE_ID IN ('COMMISSION_EXPENSE', 'PROMOTION_EXPENSE') THEN 'MARKETING'
                            WHEN GL_ACCOUNT_TYPE_ID IN ('UTILITIES_EXPENSE', 'RENT_EXPENSE') THEN 'ADMIN'
        -- Resource Type fallback
                            WHEN GL_RESOURCE_TYPE_ID = 'LABOR' THEN 'ADMIN'
                            ELSE 'NA'
        END;

UPDATE GL_ACCOUNT
SET
    GL_ACCOUNT_COURSE_LABEL_ID = CASE
                                     WHEN GL_RESOURCE_TYPE_ID = 'MONEY' THEN 'CASH_EQ'
                                     WHEN GL_ACCOUNT_TYPE_ID = 'ACCOUNTS_RECEIVABLE' THEN 'RECEIVABLES'
                                     WHEN GL_ACCOUNT_TYPE_ID = 'ACCOUNTS_PAYABLE' THEN 'PAYABLES'
                                     WHEN GL_ACCOUNT_CLASS_ID = 'REVENUE' THEN 'SALES_REV'
                                     ELSE 'RENT_UTIL'
        END;