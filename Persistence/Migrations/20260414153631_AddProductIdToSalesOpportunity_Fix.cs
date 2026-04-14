using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProductIdToSalesOpportunity_Fix : Migration
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
              AND COLUMN_NAME = 'PRODUCT_ID'
        );

        SET @sql = IF(@col_exists = 0,
            'ALTER TABLE SALES_OPPORTUNITY ADD COLUMN PRODUCT_ID varchar(36) NULL',
            'SELECT 1');

        PREPARE stmt FROM @sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
    ");

            // Add index safely
            migrationBuilder.Sql(@"
        SET @idx_exists = (
            SELECT COUNT(*)
            FROM information_schema.STATISTICS
            WHERE TABLE_SCHEMA = DATABASE()
              AND TABLE_NAME = 'SALES_OPPORTUNITY'
              AND INDEX_NAME = 'IX_SalesOpportunity_PRODUCT_ID'
        );

        SET @sql = IF(@idx_exists = 0,
            'CREATE INDEX IX_SalesOpportunity_PRODUCT_ID ON SALES_OPPORTUNITY(PRODUCT_ID)',
            'SELECT 1');

        PREPARE stmt FROM @sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
    ");

            // Add FK safely
            migrationBuilder.Sql(@"
        SET @fk_exists = (
            SELECT COUNT(*)
            FROM information_schema.REFERENTIAL_CONSTRAINTS
            WHERE CONSTRAINT_SCHEMA = DATABASE()
              AND CONSTRAINT_NAME = 'FK_SalesOpportunity_Product_PRODUCT_ID'
        );

        SET @sql = IF(@fk_exists = 0,
            'ALTER TABLE SALES_OPPORTUNITY 
             ADD CONSTRAINT FK_SalesOpportunity_Product_PRODUCT_ID
             FOREIGN KEY (PRODUCT_ID) REFERENCES Product(PRODUCT_ID)',
            'SELECT 1');

        PREPARE stmt FROM @sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
    ");
        }
        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
