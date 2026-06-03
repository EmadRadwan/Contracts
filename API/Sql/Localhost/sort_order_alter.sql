-- =============================================================================
-- ADD SORT_ORDER COLUMN — MySQL compatible
-- Uses IGNORE on duplicate column error (MySQL 8.0+)
-- OR: just run each ALTER once; if column exists it will error — safe to ignore
-- =============================================================================

ALTER TABLE GL_REPORT                   ADD COLUMN SORT_ORDER INT NULL;
ALTER TABLE GL_CLASS_COURSE             ADD COLUMN SORT_ORDER INT NULL;
ALTER TABLE GL_SUB_CLASS                ADD COLUMN SORT_ORDER INT NULL;
ALTER TABLE GL_SUB_CLASS_2              ADD COLUMN SORT_ORDER INT NULL;
ALTER TABLE GL_ACCOUNT_COURSE_LABEL     ADD COLUMN SORT_ORDER INT NULL;
ALTER TABLE GL_SUB_ACCOUNT_COURSE_LABEL ADD COLUMN SORT_ORDER INT NULL;

