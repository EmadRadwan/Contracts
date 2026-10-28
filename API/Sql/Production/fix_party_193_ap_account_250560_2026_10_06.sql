-- Party 193 (عيسى النميسى) vendor GL mapping: 125100 → 250560.
-- 2026-10-05 the client gave 125100 (current asset under 124600) by mistake; the correct account is
-- 250560 (current liability under 250400 ذمم دائنة أخرى). On prod they already unassigned 125100 from
-- Company and re-posted cheques O18281/O18282 to debit 250560; only this mapping still points at 125100,
-- so approving INV1775 would fail the GL_ACCOUNT_ORGANIZATION check.
-- After this, INV1775 approval posts D 140704 / C 250560 12,204,000 and 250560 nets to zero.
-- UPPERCASE names: prod Linux MySQL is case-sensitive.

START TRANSACTION;

UPDATE PARTY_GL_ACCOUNT
SET GL_ACCOUNT_ID = '250560', LAST_UPDATED_STAMP = NOW(), LAST_UPDATED_TX_STAMP = NOW()
WHERE ORGANIZATION_PARTY_ID = 'Company' AND PARTY_ID = '193'
  AND ROLE_TYPE_ID = 'BILL_FROM_VENDOR' AND GL_ACCOUNT_TYPE_ID = 'ACCOUNTS_PAYABLE'
  AND GL_ACCOUNT_ID = '125100';

-- Verify before committing: mapping → 250560, and 250560 assigned to Company (ASSIGNED = 1)
SELECT p.*, (SELECT COUNT(*) FROM GL_ACCOUNT_ORGANIZATION o
             WHERE o.GL_ACCOUNT_ID = p.GL_ACCOUNT_ID AND o.ORGANIZATION_PARTY_ID = 'Company') AS ASSIGNED
FROM PARTY_GL_ACCOUNT p WHERE p.PARTY_ID = '193';

COMMIT;
