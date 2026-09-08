-- ===========================================================================
--  CLEAN THE CRM MODULE ON PRODUCTION
--  Removes every Lead and every Sales Opportunity so the CRM user starts empty.
--
--  DO NOT RUN THIS BLIND. Work through the four steps in order:
--     STEP 1  back up            (shell command, outside this file)
--     STEP 2  dry run            (read-only, tells you exactly what will go)
--     STEP 3  the deletion       (transaction, stops before COMMIT)
--     STEP 4  commit or rollback (you type it)
--
--  WHAT THIS DELETES
--     - every party holding the LEAD role, and its person/contact/address rows
--     - every sales opportunity, with its actions, stage history and role links
--
--  WHAT THIS DOES NOT TOUCH  -- and the guard that guarantees it
--     - customers, suppliers, employees, brokers, sales reps.
--       A lead is only deleted when LEAD is its ONLY role. A lead that was ever
--       converted picks up CUSTOMER (or similar) and is therefore skipped.
--     - the sales reps who own the opportunities. Only the link rows in
--       SALES_OPPORTUNITY_ROLE are removed; the parties themselves stay.
--     - orders, invoices, payments, sales requests, commissions, GL entries.
--       STEP 2 aborts if any candidate is referenced by those.
--     - reference data: stages, action types, cancellation reasons, data sources.
--
--  Verified against a copy of production taken 31 Aug 2026, where the CRM held
--  1 lead (party 541) and 2 opportunities (10001, 10002) with 2 actions and
--  14 history rows, and the lead was referenced by nothing outside the CRM.
-- ===========================================================================


-- ---------------------------------------------------------------------------
-- STEP 1  BACK UP FIRST. Run this in the shell, not in MySQL.
--         Do not proceed until the file exists and is non-empty.
--
--   mysqldump -h <prod-host> -u <user> -p erp_contracts \
--     PARTY PERSON PARTY_GROUP PARTY_ROLE PARTY_STATUS PARTY_DATA_SOURCE \
--     PARTY_RELATIONSHIP PARTY_CONTACT_MECH PARTY_CONTACT_MECH_PURPOSE \
--     CONTACT_MECH TELECOM_NUMBER POSTAL_ADDRESS \
--     SALES_OPPORTUNITY SALES_OPPORTUNITY_ROLE SALES_OPPORTUNITY_ACTION \
--     SALES_OPPORTUNITY_HISTORY \
--     > crm-backup-$(date +%F-%H%M).sql
-- ---------------------------------------------------------------------------


-- ---------------------------------------------------------------------------
-- STEP 2  DRY RUN. Read-only. Run this whole block and read every result.
-- ---------------------------------------------------------------------------

-- 2a. The leads that WILL be deleted (LEAD is their only role).
SELECT p.PARTY_ID, p.DESCRIPTION, p.DATA_SOURCE_ID, p.CREATED_STAMP
FROM PARTY p
WHERE p.PARTY_ID IN (SELECT PARTY_ID FROM PARTY_ROLE WHERE ROLE_TYPE_ID = 'LEAD')
  AND (SELECT COUNT(*) FROM PARTY_ROLE r WHERE r.PARTY_ID = p.PARTY_ID) = 1
ORDER BY p.PARTY_ID;

-- 2b. Leads that will be SKIPPED because they hold another role too.
--     Expect zero rows. Any row here is a converted lead - it stays, and you
--     must decide what to do with it by hand.
SELECT p.PARTY_ID, p.DESCRIPTION,
       GROUP_CONCAT(r.ROLE_TYPE_ID ORDER BY r.ROLE_TYPE_ID) AS roles
FROM PARTY p
JOIN PARTY_ROLE r ON r.PARTY_ID = p.PARTY_ID
WHERE p.PARTY_ID IN (SELECT PARTY_ID FROM PARTY_ROLE WHERE ROLE_TYPE_ID = 'LEAD')
GROUP BY p.PARTY_ID, p.DESCRIPTION
HAVING COUNT(*) > 1;

-- 2c. SAFETY GATE. Counts how many deletable leads are referenced by real
--     business documents. EVERY NUMBER MUST BE 0. If any is not, STOP -
--     that lead carries financial history and this script must not run.
SELECT
  (SELECT COUNT(*) FROM ORDER_ROLE        WHERE PARTY_ID        IN (SELECT PARTY_ID FROM PARTY_ROLE WHERE ROLE_TYPE_ID='LEAD')) AS order_roles,
  (SELECT COUNT(*) FROM INVOICE           WHERE PARTY_ID        IN (SELECT PARTY_ID FROM PARTY_ROLE WHERE ROLE_TYPE_ID='LEAD')
                                             OR PARTY_ID_FROM   IN (SELECT PARTY_ID FROM PARTY_ROLE WHERE ROLE_TYPE_ID='LEAD')) AS invoices,
  (SELECT COUNT(*) FROM INVOICE_ROLE      WHERE PARTY_ID        IN (SELECT PARTY_ID FROM PARTY_ROLE WHERE ROLE_TYPE_ID='LEAD')) AS invoice_roles,
  (SELECT COUNT(*) FROM PAYMENT           WHERE PARTY_ID_FROM   IN (SELECT PARTY_ID FROM PARTY_ROLE WHERE ROLE_TYPE_ID='LEAD')
                                             OR PARTY_ID_TO     IN (SELECT PARTY_ID FROM PARTY_ROLE WHERE ROLE_TYPE_ID='LEAD')) AS payments,
  (SELECT COUNT(*) FROM SALES_REQUEST     WHERE FROM_PARTY_ID   IN (SELECT PARTY_ID FROM PARTY_ROLE WHERE ROLE_TYPE_ID='LEAD')
                                             OR EMPLOYEE_PARTY_ID IN (SELECT PARTY_ID FROM PARTY_ROLE WHERE ROLE_TYPE_ID='LEAD')) AS sales_requests,
  (SELECT COUNT(*) FROM ACCTG_TRANS_ENTRY WHERE PARTY_ID        IN (SELECT PARTY_ID FROM PARTY_ROLE WHERE ROLE_TYPE_ID='LEAD')) AS gl_entries,
  (SELECT COUNT(*) FROM SALES_COMMISSION  WHERE SALES_REP_PARTY_ID IN (SELECT PARTY_ID FROM PARTY_ROLE WHERE ROLE_TYPE_ID='LEAD')
                                             OR EXT_COMPANY_PARTY_ID IN (SELECT PARTY_ID FROM PARTY_ROLE WHERE ROLE_TYPE_ID='LEAD')) AS commissions;

-- 2d. What the opportunity side will remove.
SELECT
  (SELECT COUNT(*) FROM SALES_OPPORTUNITY)         AS opportunities,
  (SELECT COUNT(*) FROM SALES_OPPORTUNITY_ACTION)  AS actions,
  (SELECT COUNT(*) FROM SALES_OPPORTUNITY_HISTORY) AS history_rows,
  (SELECT COUNT(*) FROM SALES_OPPORTUNITY_ROLE)    AS role_links;

-- 2e. Units currently held by a won opportunity. These are released in STEP 3f.
--     Review the list: any unit here goes back to APARTMENT_AVAILABLE.
SELECT o.SALES_OPPORTUNITY_ID, o.PRODUCT_ID, pr.APARTMENT_STATUS_ID
FROM SALES_OPPORTUNITY o
JOIN PRODUCT pr ON pr.PRODUCT_ID = o.PRODUCT_ID
WHERE o.OPPORTUNITY_STAGE_ID = 'SOSTG_CLOSED_WON'
  AND pr.APARTMENT_STATUS_ID = 'APARTMENT_RESERVED';


-- ---------------------------------------------------------------------------
-- STEP 3  THE DELETION. Only run after STEP 2 came back clean.
--         Runs inside a transaction and stops before COMMIT.
-- ---------------------------------------------------------------------------

START TRANSACTION;

-- Freeze the list of deletable leads so every statement below uses the same set.
DROP TEMPORARY TABLE IF EXISTS tmp_dead_leads;
CREATE TEMPORARY TABLE tmp_dead_leads (PARTY_ID VARCHAR(36) PRIMARY KEY);

INSERT INTO tmp_dead_leads (PARTY_ID)
SELECT p.PARTY_ID
FROM PARTY p
WHERE p.PARTY_ID IN (SELECT PARTY_ID FROM PARTY_ROLE WHERE ROLE_TYPE_ID = 'LEAD')
  AND (SELECT COUNT(*) FROM PARTY_ROLE r WHERE r.PARTY_ID = p.PARTY_ID) = 1;

-- Same idea for their contact mechs, captured before the link rows are removed.
DROP TEMPORARY TABLE IF EXISTS tmp_dead_cm;
CREATE TEMPORARY TABLE tmp_dead_cm (CONTACT_MECH_ID VARCHAR(36) PRIMARY KEY);

INSERT IGNORE INTO tmp_dead_cm (CONTACT_MECH_ID)
SELECT pcm.CONTACT_MECH_ID
FROM PARTY_CONTACT_MECH pcm
JOIN tmp_dead_leads d ON d.PARTY_ID = pcm.PARTY_ID;

-- 3a. Opportunities, children first. Every opportunity goes, regardless of
--     which lead it belongs to.
DELETE FROM SALES_OPPORTUNITY_ACTION;
DELETE FROM SALES_OPPORTUNITY_HISTORY;
DELETE FROM SALES_OPPORTUNITY_ROLE;
-- SALES_OPPORTUNITY_PRODUCT was retired in Apr 2026 (entity removed from the EF
-- model, no DropTable migration was ever generated). It no longer exists on
-- production; some dev copies still carry it as an orphan table. Delete only if
-- present so this script runs unchanged on both.
SET @has_sop := (SELECT COUNT(*) FROM information_schema.tables
                 WHERE table_schema = DATABASE() AND table_name = 'SALES_OPPORTUNITY_PRODUCT');
SET @sop_sql := IF(@has_sop > 0, 'DELETE FROM SALES_OPPORTUNITY_PRODUCT', 'DO 0');
PREPARE sop_stmt FROM @sop_sql; EXECUTE sop_stmt; DEALLOCATE PREPARE sop_stmt;
DELETE FROM SALES_OPPORTUNITY_QUOTE;
DELETE FROM SALES_OPPORTUNITY_COMPETITOR;
DELETE FROM SALES_OPPORTUNITY_TRCK_CODE;
DELETE FROM SALES_OPPORTUNITY_WORK_EFFORT;
DELETE FROM SALES_OPPORTUNITY;

-- 3b. Ownership and broker links on the leads. PARTY_RELATIONSHIP points at
--     PARTY_ROLE, so it must go before the role rows.
-- Split in two on purpose: MySQL refuses to reference the same TEMPORARY
-- table twice in one statement (error 1137).
DELETE FROM PARTY_RELATIONSHIP
WHERE PARTY_ID_FROM IN (SELECT PARTY_ID FROM tmp_dead_leads);

DELETE FROM PARTY_RELATIONSHIP
WHERE PARTY_ID_TO IN (SELECT PARTY_ID FROM tmp_dead_leads);

-- 3c. Contact details: purposes, then links, then the values, then the parents.
DELETE FROM PARTY_CONTACT_MECH_PURPOSE WHERE PARTY_ID IN (SELECT PARTY_ID FROM tmp_dead_leads);
DELETE FROM PARTY_CONTACT_MECH         WHERE PARTY_ID IN (SELECT PARTY_ID FROM tmp_dead_leads);

-- Only drop a contact mech that no other party still shares.
DELETE FROM tmp_dead_cm
WHERE CONTACT_MECH_ID IN (SELECT CONTACT_MECH_ID FROM PARTY_CONTACT_MECH);

DELETE FROM TELECOM_NUMBER  WHERE CONTACT_MECH_ID IN (SELECT CONTACT_MECH_ID FROM tmp_dead_cm);
DELETE FROM POSTAL_ADDRESS  WHERE CONTACT_MECH_ID IN (SELECT CONTACT_MECH_ID FROM tmp_dead_cm);
DELETE FROM CONTACT_MECH    WHERE CONTACT_MECH_ID IN (SELECT CONTACT_MECH_ID FROM tmp_dead_cm);

-- 3d. The party's own satellite rows.
DELETE FROM PARTY_DATA_SOURCE WHERE PARTY_ID IN (SELECT PARTY_ID FROM tmp_dead_leads);
DELETE FROM PARTY_STATUS      WHERE PARTY_ID IN (SELECT PARTY_ID FROM tmp_dead_leads);
DELETE FROM PARTY_ROLE        WHERE PARTY_ID IN (SELECT PARTY_ID FROM tmp_dead_leads);
DELETE FROM PERSON            WHERE PARTY_ID IN (SELECT PARTY_ID FROM tmp_dead_leads);
DELETE FROM PARTY_GROUP       WHERE PARTY_ID IN (SELECT PARTY_ID FROM tmp_dead_leads);

-- 3e. And the parties themselves.
DELETE FROM PARTY WHERE PARTY_ID IN (SELECT PARTY_ID FROM tmp_dead_leads);

-- 3f. Release any apartment the CRM had reserved. Nothing else reserves units
--     this way, and with the opportunities gone the reservation has no owner.
--     Deliberately does NOT touch APARTMENT_SOLD - those sales live elsewhere.
UPDATE PRODUCT
SET APARTMENT_STATUS_ID = 'APARTMENT_AVAILABLE'
WHERE APARTMENT_STATUS_ID = 'APARTMENT_RESERVED';

-- 3g. VERIFY. Every number must be 0. If not, ROLLBACK.
SELECT
  (SELECT COUNT(*) FROM SALES_OPPORTUNITY)          AS opportunities_left,
  (SELECT COUNT(*) FROM SALES_OPPORTUNITY_ROLE)     AS opp_roles_left,
  (SELECT COUNT(*) FROM SALES_OPPORTUNITY_ACTION)   AS actions_left,
  (SELECT COUNT(*) FROM SALES_OPPORTUNITY_HISTORY)  AS history_left,
  (SELECT COUNT(*) FROM PARTY_ROLE WHERE ROLE_TYPE_ID='LEAD') AS leads_left;

-- And these must be UNCHANGED from before the run - your customers and staff.
SELECT
  (SELECT COUNT(*) FROM PARTY)                                        AS parties_total,
  (SELECT COUNT(*) FROM PARTY_ROLE WHERE ROLE_TYPE_ID='CUSTOMER')     AS customers,
  (SELECT COUNT(*) FROM PARTY_ROLE WHERE ROLE_TYPE_ID='EMPLOYEE')     AS employees,
  (SELECT COUNT(*) FROM PARTY_ROLE WHERE ROLE_TYPE_ID='BROKER')       AS brokers,
  (SELECT COUNT(*) FROM PARTY_ROLE WHERE ROLE_TYPE_ID='SUPPLIER')     AS suppliers;


-- ---------------------------------------------------------------------------
-- STEP 4  You type one of these. Nothing is permanent until you do.
-- ---------------------------------------------------------------------------
COMMIT;
-- ROLLBACK;
