-- ===========================================================================
--  DEV / LOCALHOST ONLY — realign schema with production and the EF model
--
--  SALES_OPPORTUNITY_PRODUCT was retired in April 2026:
--     - Domain/SalesOpportunityProduct.cs was deleted
--     - the entity was removed from Persistence/DataContext.cs
--     - it was dropped from DataContextModelSnapshot.cs
--     - BUT no `migrationBuilder.DropTable("SALES_OPPORTUNITY_PRODUCT")`
--       migration was ever generated. The drop was done by hand on production.
--
--  Result: production no longer has this table (correct — matches the model),
--  but local/dev databases that were seeded before April 2026, or restored
--  from a mixed dump, still carry it as an orphan. Nothing in the app reads
--  or writes it anymore.
--
--  Run this once against the local DB to drop the orphan so dev == prod.
--  Safe to run repeatedly (IF EXISTS).
-- ===========================================================================

-- Inspect first (optional):
-- SELECT COUNT(*) AS rows_in_orphan FROM SALES_OPPORTUNITY_PRODUCT;

SET FOREIGN_KEY_CHECKS = 0;
DROP TABLE IF EXISTS SALES_OPPORTUNITY_PRODUCT;
SET FOREIGN_KEY_CHECKS = 1;
