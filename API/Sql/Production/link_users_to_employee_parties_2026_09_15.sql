-- "Created By / Approved By" shows blank for users created through the Users admin screen, 15 Sep 2026.
-- Those columns resolve PARTY.DESCRIPTION through AspNetUsers.PartyId (see ListPayments,
-- ListPaymentsToday, ListPaymentsByDateRange, GetCompanyReport, GetProjectReport). Users created
-- through the Users admin screen never had a PartyId set (the screen only captured the
-- organization), so PaymentHelperService stamped CREATED_BY_PARTY_ID = NULL for everything
-- this user created. The seeded users were linked to their PERSON party, hence the mismatch.
-- The Users admin screen now has an "Employee" picker; this script fixes the existing data.
--
-- 1. Link the user to the existing EMPLOYEE party 614 (عمرو غالى السباعي).
-- 2. Back-fill CREATED_BY_PARTY_ID on the 105 surviving payments this user created, identified
--    from ENTITY_AUDIT_LOG (Payment / *CREATE* rows carry the payment id as PaymentId=<id>).
--    No approvals were recorded for this user, so APPROVED_BY_PARTY_ID needs nothing.
-- Idempotent; safe to re-run. Applied on localhost 2026-09-15. Run on production BEFORE deploying the
-- build that requires a linked PartyId, and check the first SELECT returns 614 = عمرو غالى السباعي.
--
-- nevine@gmail.com and rawan@gmail.com are linked to their existing parties in the second block below.

SET @user_email = 'aghali@gmail.com';
SET @party_id   = '614';

-- Sanity: the party must exist and be an employee before we point anything at it.
SELECT PARTY_ID, DESCRIPTION, MAIN_ROLE, STATUS_ID
FROM PARTY
WHERE PARTY_ID = @party_id;

UPDATE AspNetUsers
SET PartyId = @party_id,
    LastUpdatedStamp = NOW()
WHERE Email = @user_email
  AND (PartyId IS NULL OR PartyId <> @party_id);

UPDATE PAYMENT p
JOIN ENTITY_AUDIT_LOG a
  ON a.CHANGED_ENTITY_NAME = 'Payment'
 AND a.CHANGED_FIELD_NAME = '*CREATE*'
 AND a.CHANGED_BY_INFO LIKE CONCAT(@user_email, ' (%')
 AND p.PAYMENT_ID = SUBSTRING_INDEX(a.PK_COMBINED_VALUE_TEXT, '=', -1)
SET p.CREATED_BY_PARTY_ID = @party_id
WHERE p.CREATED_BY_PARTY_ID IS NULL;

-- Verify: expect PartyId = 614 for the user, and 0 remaining null-creator payments of theirs.
SELECT Email, DisplayName, PartyId FROM AspNetUsers WHERE Email = @user_email;

SELECT COUNT(*) AS remaining_null_creator
FROM PAYMENT p
JOIN ENTITY_AUDIT_LOG a
  ON a.CHANGED_ENTITY_NAME = 'Payment'
 AND a.CHANGED_FIELD_NAME = '*CREATE*'
 AND a.CHANGED_BY_INFO LIKE CONCAT(@user_email, ' (%')
 AND p.PAYMENT_ID = SUBSTRING_INDEX(a.PK_COMBINED_VALUE_TEXT, '=', -1)
WHERE p.CREATED_BY_PARTY_ID IS NULL;

-- ---------------------------------------------------------------------------------------------
-- Added 15 Sep 2026 (same day): the two other unlinked users, linked to their existing parties
-- (confirmed by the developer). Neither user's activity stamps a creator party (nevine only
-- created SALES_REQUESTs, which carry no created-by column; rawan has no audit entries), so the
-- link is the whole fix. Party 342 is deliberately left as PREVIOUS_EMPLOYEE.
-- ---------------------------------------------------------------------------------------------

UPDATE AspNetUsers
SET PartyId = '584',                 -- روان وحيد عبد الواحد (EMPLOYEE)
    LastUpdatedStamp = NOW()
WHERE Email = 'rawan@gmail.com'
  AND (PartyId IS NULL OR PartyId <> '584');

UPDATE AspNetUsers
SET PartyId = '342',                 -- نفين سمير محمود (PREVIOUS_EMPLOYEE)
    LastUpdatedStamp = NOW()
WHERE Email = 'nevine@gmail.com'
  AND (PartyId IS NULL OR PartyId <> '342');

-- Verify: every user should now resolve to a party name.
SELECT u.Email, u.DisplayName, u.PartyId, p.DESCRIPTION, p.MAIN_ROLE
FROM AspNetUsers u
LEFT JOIN PARTY p ON p.PARTY_ID = u.PartyId
WHERE u.PartyId IS NULL OR u.Email IN ('aghali@gmail.com', 'rawan@gmail.com', 'nevine@gmail.com');
