-- Align the parent to the child's label
UPDATE gl_account
SET GL_ACCOUNT_COURSE_LABEL_ID = 'INVENTORY'
WHERE GL_ACCOUNT_ID = '124420';

-- Standardize all employee sub-ledgers just in case
UPDATE gl_account
SET GL_ACCOUNT_COURSE_LABEL_ID = 'INVENTORY'
WHERE PARENT_GL_ACCOUNT_ID = '124420';