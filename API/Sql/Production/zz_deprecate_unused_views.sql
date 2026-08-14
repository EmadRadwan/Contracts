-- ============================================================================
-- zz_deprecate_unused_views — retire the views nothing reads any more
-- ============================================================================
-- WHY THIS EXISTS
--   erp_contracts holds 54 views. The Power BI semantic model reads 18 of them.
--   The other 36 are earlier generations that were never removed, and three of
--   them are dangerous name-alikes of views that ARE in use:
--
--       fact_project_directpayments      34,501 rows   210x the correct view
--       fact_project_operatingexpenses      120 rows   understates by 72 rows
--       dim_gl_account2 / vw_dim_gl_account          different account populations
--
--   Picking one of those from a dropdown by mistake silently changes the answer.
--   Renaming them with a zz_deprecated_ prefix sorts them to the bottom of every
--   tool's object list and makes the mistake hard to make, WITHOUT destroying
--   anything: a rename is instantly reversible (section 6), a DROP is not.
--   Drop them for real once a full refresh cycle has passed with no breakage.
--
-- WHAT IS RENAMED: 31 of the 36 — NOT all 36.
--   Five of the "unused" views are unused BY POWER BI but read by the ERP itself,
--   through EF Core entity mappings in Persistence/DataContext.cs:
--
--       PARTY_VIEW                      .ToView("PARTY_VIEW")
--       OrderView                       .ToView("OrderView")
--       InventoryItemDetailForSumView   .ToView("InventoryItemDetailForSumView")
--       FACILITY_INVENTORY_RECORD_VIEW  .ToView("FACILITY_INVENTORY_RECORD_VIEW")
--       InvoiceRecords                  .ToView("InvoiceRecords")
--
--   Renaming any of those breaks the running application — the party list, the
--   order list, the invoice list and an inventory report would all fail. They are
--   excluded here and must stay excluded.
--
-- HOW THE SAFETY WAS ESTABLISHED (all read-only, before this script was written)
--   1. The 18 in use were taken from the Item="..." navigation steps in
--      Projects-12-Aug.SemanticModel/definition/tables/*.tmdl. Fact_TrialBalance
--      is not a view — it comes from the API — and the measure tables are not
--      views either.
--   2. Every view definition was parsed and a dependency graph built. The
--      transitive closure of the 23 keepers (18 Power BI + 5 EF) pulls in NONE of
--      the 31 below: no kept view reads a renamed one.
--   3. The C# was searched for raw SQL — FromSqlRaw, FromSqlInterpolated,
--      ExecuteSqlRaw, SqlQuery. There is none, anywhere, so the EF mapping list is
--      the complete set of application access paths.
--
-- CASE SENSITIVITY — why this is a procedure and not 31 RENAME lines
--   On Linux (the server Power BI reads) view names keep the case they were
--   created with: DimApartments, PaymentCertificates, InventoryItemsDetails. On a
--   developer Mac, MySQL usually folds them to lower case. A hardcoded rename list
--   would therefore work on one machine and fail on the other. The procedure below
--   matches case-insensitively and renames whatever name is actually stored, so it
--   is correct on both.
--
-- HOW TO RUN
--   Against erp_contracts on 129.146.22.240:3308.
--       CALL zz_deprecate_views(1);   -- dry run: prints what it WOULD do
--       CALL zz_deprecate_views(0);   -- performs the renames
--   Re-running is safe: anything already prefixed is skipped.
--   This changes no data and reads no rows. It is independent of the
--   Fact_Project_Payroll release and can ship on its own.
-- ============================================================================


-- ────────────────────────────────────────────────────────────────────────────
-- 1. Preflight — run these first and read the output
-- ────────────────────────────────────────────────────────────────────────────
-- Expect 54 total. If this server has drifted from the one that was audited,
-- stop and re-check before renaming anything.
SELECT COUNT(*) AS total_views
FROM information_schema.VIEWS
WHERE TABLE_SCHEMA = DATABASE();

-- The 23 that must survive. Expect 23 rows; anything missing means this server
-- differs from the audited one.
SELECT TABLE_NAME AS keeper
FROM information_schema.VIEWS
WHERE TABLE_SCHEMA = DATABASE()
  AND LOWER(TABLE_NAME) IN (
    -- 18 read by the Power BI model
    'billingaccounts','dimparties','dimproductcategories','dimproductrawmaterials',
    'dimproductservices','dimproducts','dimproject','dimprojects','dimsuppliers',
    'dim_customtimeperiod','dim_gl_account','fact_gl_transactions',
    'fact_project_directpayments_2','fact_project_expenses',
    'fact_project_operatingexpenses_2','fact_project_revenues','payments',
    'salesrequests',
    -- 5 read by the ERP through EF Core
    'party_view','orderview','inventoryitemdetailforsumview',
    'facility_inventory_record_view','invoicerecords'
  )
ORDER BY 1;


-- ────────────────────────────────────────────────────────────────────────────
-- 2. The procedure
-- ────────────────────────────────────────────────────────────────────────────
DROP PROCEDURE IF EXISTS zz_deprecate_views;

DELIMITER $$

CREATE PROCEDURE zz_deprecate_views(IN dry_run TINYINT)
BEGIN
    DECLARE done      INT DEFAULT 0;
    DECLARE v_name    VARCHAR(64);
    DECLARE v_sql     TEXT;
    DECLARE n_done    INT DEFAULT 0;

    -- The 31. Listed lower case and matched with LOWER() so the stored casing on
    -- the server does not matter. Every name here was verified unreferenced by any
    -- kept view and unmapped in DataContext.cs.
    DECLARE cur CURSOR FOR
        SELECT TABLE_NAME
        FROM information_schema.VIEWS
        WHERE TABLE_SCHEMA = DATABASE()
          AND LOWER(TABLE_NAME) NOT LIKE 'zz\_deprecated\_%'
          AND LOWER(TABLE_NAME) IN (
            -- superseded generations of views still in use — the dangerous ones
            'fact_project_directpayments',      -- 210x the correct view
            'fact_project_operatingexpenses',
            'fact_projects_expenses',
            'dim_gl_account2',
            'vw_dim_gl_account',
            'payments_2',                       -- exact twin of payments
            -- abandoned GL classification mapping attempts
            'dim_gl_account_report_mapping',
            'dim_gl_class_report_map',
            'dim_glaccountreportmapping',
            'vw_gl_account_report_mapping',
            'dim_project_operatingexpense_gl_hierarchy',
            -- dimension views the model never took up
            'dimapartments','dimcontractors','dimcostcenters','dimemployees',
            'dimpaymentmethods','dimpaymentmethodtypes','dimpaymenttypes',
            'dimstatusitems',
            -- fact/reporting views the model never took up
            'acctgtransactions','acctgtransentries',
            'fact_apartment_installment_payment_link','fact_apartment_payments',
            'paymentcertificateitems','paymentcertificates','reserverequests',
            'inventoryitemsdetails','outstanding_purchased_quantity_view',
            -- one-off analysis views
            'view_gl_account_audit','view_gl_account_audit_org',
            'view_ofbiz_coa_active_analysis'
          )
        ORDER BY TABLE_NAME;

    DECLARE CONTINUE HANDLER FOR NOT FOUND SET done = 1;

    DROP TEMPORARY TABLE IF EXISTS zz_deprecate_log;
    CREATE TEMPORARY TABLE zz_deprecate_log (
        old_name VARCHAR(64),
        new_name VARCHAR(64),
        action   VARCHAR(16)
    );

    OPEN cur;
    read_loop: LOOP
        FETCH cur INTO v_name;
        IF done = 1 THEN
            LEAVE read_loop;
        END IF;

        SET v_sql = CONCAT('RENAME TABLE `', v_name,
                           '` TO `zz_deprecated_', v_name, '`');

        INSERT INTO zz_deprecate_log VALUES (
            v_name,
            CONCAT('zz_deprecated_', v_name),
            IF(dry_run = 1, 'would rename', 'renamed')
        );

        IF dry_run = 0 THEN
            SET @s = v_sql;
            PREPARE stmt FROM @s;
            EXECUTE stmt;
            DEALLOCATE PREPARE stmt;
        END IF;

        SET n_done = n_done + 1;
    END LOOP;
    CLOSE cur;

    SELECT * FROM zz_deprecate_log ORDER BY old_name;
    SELECT n_done                              AS views_affected,
           IF(dry_run = 1, 'DRY RUN — nothing changed',
                           'RENAMED — see section 4 to verify') AS outcome;
END$$

DELIMITER ;


-- ────────────────────────────────────────────────────────────────────────────
-- 3. Run it
-- ────────────────────────────────────────────────────────────────────────────
-- Expect 31 rows on a server that has not been through this before.
CALL zz_deprecate_views(1);   -- dry run first, read the list

-- CALL zz_deprecate_views(0); -- uncomment to perform the renames


-- ────────────────────────────────────────────────────────────────────────────
-- 4. Verification — run after CALL zz_deprecate_views(0)
-- ────────────────────────────────────────────────────────────────────────────
-- 4a. The 23 keepers must all still be queryable. CHECK TABLE reports
--     "View '...' references invalid table(s)" for anything broken.
--     Every row must say status = OK.
CHECK TABLE
    BillingAccounts, DimParties, DimProductCategories, DimProductRawMaterials,
    DimProductServices, DimProducts, DimProject, DimProjects, DimSuppliers,
    Dim_CustomTimePeriod, Dim_gl_account, Fact_GL_Transactions,
    Fact_Project_DirectPayments_2, Fact_Project_Expenses,
    Fact_Project_OperatingExpenses_2, Fact_Project_Revenues, Payments,
    SalesRequests,
    PARTY_VIEW, OrderView, InventoryItemDetailForSumView,
    FACILITY_INVENTORY_RECORD_VIEW, InvoiceRecords;

-- 4b. Counts: 31 prefixed, 23 not.
SELECT SUM(LOWER(TABLE_NAME) LIKE 'zz\_deprecated\_%') AS deprecated_31,
       SUM(LOWER(TABLE_NAME) NOT LIKE 'zz\_deprecated\_%') AS live_23,
       COUNT(*)                                          AS total_54
FROM information_schema.VIEWS
WHERE TABLE_SCHEMA = DATABASE();

-- 4c. Then refresh the Power BI model once and confirm it still loads. It reads
--     only the 18, so it should be completely unaffected — that is the point of
--     the exercise. Open the ERP and load the party list, order list and invoice
--     list to confirm the five EF views were left alone.


-- ────────────────────────────────────────────────────────────────────────────
-- 5. Two known, accepted consequences
-- ────────────────────────────────────────────────────────────────────────────
-- 5a. zz_deprecated_paymentcertificateitems will be BROKEN after the rename.
--     It reads paymentcertificates, and MySQL stores that reference by name, so
--     renaming the parent leaves the child pointing at a name that no longer
--     exists. This is the only such pair among the 31, both halves are deprecated,
--     and nothing reads either one — so it is left broken deliberately. If you
--     ever need that pair working again, roll both back with section 6.
--
-- 5b. The GRANTs in _CreatePowerBiUser.sql are per view name, so the grants for
--     the 31 now point at names that no longer exist. Harmless — MySQL keeps the
--     stale grant and it matches nothing. It also means powerbi_user can no longer
--     select the renamed views even by typing the new name, which is a second lock
--     on the same door. Optional tidy-up, only if you want the grant table clean:
--
--         REVOKE SELECT ON DimApartments FROM 'powerbi_user'@'%';
--         ...  -- one line per deprecated view
--
--     Do NOT re-run _CreatePowerBiUser.sql as it stands: it would fail on the
--     renamed views. Trim it to the 18 first.


-- ────────────────────────────────────────────────────────────────────────────
-- 6. Rollback — undo everything
-- ────────────────────────────────────────────────────────────────────────────
-- Renames are fully reversible. This puts every prefixed view back.
DROP PROCEDURE IF EXISTS zz_undeprecate_views;

DELIMITER $$

CREATE PROCEDURE zz_undeprecate_views()
BEGIN
    DECLARE done   INT DEFAULT 0;
    DECLARE v_name VARCHAR(64);
    DECLARE cur CURSOR FOR
        SELECT TABLE_NAME FROM information_schema.VIEWS
        WHERE TABLE_SCHEMA = DATABASE()
          AND LOWER(TABLE_NAME) LIKE 'zz\_deprecated\_%';
    DECLARE CONTINUE HANDLER FOR NOT FOUND SET done = 1;

    OPEN cur;
    undo_loop: LOOP
        FETCH cur INTO v_name;
        IF done = 1 THEN
            LEAVE undo_loop;
        END IF;
        SET @s = CONCAT('RENAME TABLE `', v_name, '` TO `',
                        SUBSTRING(v_name, 15), '`');
        PREPARE stmt FROM @s;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
    END LOOP;
    CLOSE cur;
END$$

DELIMITER ;

-- CALL zz_undeprecate_views();

-- ────────────────────────────────────────────────────────────────────────────
-- 7. When these can actually be dropped
-- ────────────────────────────────────────────────────────────────────────────
-- After one full refresh cycle with no breakage — say a month of month-end
-- reporting — generate the DROPs from whatever is still prefixed:
--
--     SELECT CONCAT('DROP VIEW `', TABLE_NAME, '`;') AS stmt
--     FROM information_schema.VIEWS
--     WHERE TABLE_SCHEMA = DATABASE()
--       AND LOWER(TABLE_NAME) LIKE 'zz\_deprecated\_%';
--
-- Review that output, then run it. Take a mysqldump of the view definitions
-- first — DROP is the one step in this file that cannot be undone.
-- ============================================================================
