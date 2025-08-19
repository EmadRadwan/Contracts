using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CreateGlAccountHierarchyView : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE VIEW GlAccountHierarchyView AS
                WITH RecursiveCTE AS
                (
                    SELECT
                        GA.GL_ACCOUNT_ID AS GlAccountId,
                        GA.GL_ACCOUNT_TYPE_ID AS GlAccountTypeId,
                        GA.GL_ACCOUNT_CLASS_ID AS GlAccountClassId,
                        GA.GL_RESOURCE_TYPE_ID AS GlResourceTypeId,
                        GA.PARENT_GL_ACCOUNT_ID AS ParentGlAccountId,
                        GA.ACCOUNT_CODE AS AccountCode,
                        GA.ACCOUNT_NAME AS AccountName,
                        0 AS Level -- Initial level for the root nodes
                    FROM
                        GL_ACCOUNT GA
                    WHERE
                        GA.PARENT_GL_ACCOUNT_ID IS NULL -- Root nodes

                    UNION ALL

                    SELECT
                        GA.GL_ACCOUNT_ID,
                        GA.GL_ACCOUNT_TYPE_ID,
                        GA.GL_ACCOUNT_CLASS_ID,
                        GA.GL_RESOURCE_TYPE_ID,
                        GA.PARENT_GL_ACCOUNT_ID,
                        GA.ACCOUNT_CODE,
                        GA.ACCOUNT_NAME,
                        Level + 1 -- Increment the level for each recursion
                    FROM
                        GL_ACCOUNT GA
                        INNER JOIN RecursiveCTE ON GA.PARENT_GL_ACCOUNT_ID = RecursiveCTE.GlAccountId
                )
                SELECT
                    GlAccountId,
                    GlAccountTypeId,
                    GlAccountClassId,
                    GlResourceTypeId,
                    ParentGlAccountId,
                    AccountCode,
                    AccountName,
                    Level
                FROM
                    RecursiveCTE;
            ");
        }


        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP VIEW GlAccountHierarchyView;");
        }
    }
}
