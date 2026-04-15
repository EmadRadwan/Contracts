using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkEffortIdToSalesOpportunity_Fix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add column only if it doesn't exist
            migrationBuilder.Sql(@"
                SET @col_exists = (
                    SELECT COUNT(*)
                    FROM information_schema.COLUMNS
                    WHERE TABLE_SCHEMA = DATABASE()
                      AND TABLE_NAME = 'SALES_OPPORTUNITY'
                      AND COLUMN_NAME = 'WORK_EFFORT_ID'
                );

                SET @sql = IF(@col_exists = 0,
                    'ALTER TABLE SALES_OPPORTUNITY ADD COLUMN WORK_EFFORT_ID varchar(36) NULL',
                    'SELECT 1');

                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            // Add index safely (using the name from your modelBuilder)
            migrationBuilder.Sql(@"
                SET @idx_exists = (
                    SELECT COUNT(*)
                    FROM information_schema.STATISTICS
                    WHERE TABLE_SCHEMA = DATABASE()
                      AND TABLE_NAME = 'SALES_OPPORTUNITY'
                      AND INDEX_NAME = 'SLSOPP_WORKEFFORT'
                );

                SET @sql = IF(@idx_exists = 0,
                    'CREATE INDEX SLSOPP_WORKEFFORT ON SALES_OPPORTUNITY(WORK_EFFORT_ID)',
                    'SELECT 1');

                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            // Add FK safely (using the constraint name from your modelBuilder)
            migrationBuilder.Sql(@"
                SET @fk_exists = (
                    SELECT COUNT(*)
                    FROM information_schema.REFERENTIAL_CONSTRAINTS
                    WHERE CONSTRAINT_SCHEMA = DATABASE()
                      AND CONSTRAINT_NAME = 'SLSOPP_TO_WORKEFFORT'
                );

                SET @sql = IF(@fk_exists = 0,
                    'ALTER TABLE SALES_OPPORTUNITY 
                     ADD CONSTRAINT SLSOPP_TO_WORKEFFORT
                     FOREIGN KEY (WORK_EFFORT_ID) REFERENCES WORK_EFFORT(WORK_EFFORT_ID)
                     ON DELETE SET NULL',
                    'SELECT 1');

                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Down is intentionally empty (safe fix migration)
            // If you ever need to revert, you can drop the FK / index / column manually
        }
    }
}