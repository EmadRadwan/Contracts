using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedStampToInvoiceRecordsView : Migration
    {
        /// <inheritdoc />
       protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.Sql(@"
CREATE OR REPLACE VIEW InvoiceRecords AS
SELECT 
    inv.INVOICE_ID                          AS InvoiceId,
    inv.INVOICE_TYPE_ID                     AS InvoiceTypeId,
    COALESCE(invt.DESCRIPTION_ARABIC, invt.DESCRIPTION) AS InvoiceTypeDescription,
    inv.INVOICE_DATE                        AS InvoiceDate,
    inv.DUE_DATE                            AS DueDate,
    inv.PAID_DATE                           AS PaidDate,
    inv.STATUS_ID                           AS StatusId,
    COALESCE(sts.DESCRIPTION_ARABIC, sts.DESCRIPTION) AS StatusDescription,
    inv.DESCRIPTION                         AS Description,

    -- Party From
    inv.PARTY_ID_FROM                       AS PartyIdFrom_FromPartyId,
    from_party.DESCRIPTION                  AS PartyIdFrom_FromPartyName,

    -- Party To
    inv.PARTY_ID                            AS PartyId_FromPartyId,
    to_party.DESCRIPTION                    AS PartyId_FromPartyName,

    -- Backward compatibility
    to_party.DESCRIPTION                    AS ToPartyName,
    from_party.DESCRIPTION                  AS FromPartyName,

    inv.BILLING_ACCOUNT_ID                  AS BillingAccountId,
    bil.DESCRIPTION                         AS BillingAccountName,

    -- OutstandingAmount (placeholder)
    0.00                                    AS OutstandingAmount,

    -- Order & Certificate
    oib.ORDER_ID                            AS OrderId,
    we.CERTIFICATE_NUMBER                   AS CertificateNumber,

    -- ════════════════════════════════════════════════
    --   ★ NEW COLUMN ADDED HERE ★
    inv.CREATED_STAMP                       AS CreatedStamp,
    -- ════════════════════════════════════════════════

    -- === MAIN LOGIC: Total Calculation ===
    ROUND(
        CASE
            WHEN EXISTS (
                SELECT 1 
                FROM INVOICE_ITEM ii_cert 
                WHERE ii_cert.INVOICE_ID = inv.INVOICE_ID 
                AND ii_cert.INVOICE_ITEM_TYPE_ID = 'PINV_CERTIFICATE_ITEM'
            ) THEN
                COALESCE((
                    SELECT SUM(ii_cert.QUANTITY * ROUND(ii_cert.AMOUNT, 5))
                    FROM INVOICE_ITEM ii_cert 
                    WHERE ii_cert.INVOICE_ID = inv.INVOICE_ID 
                    AND ii_cert.INVOICE_ITEM_TYPE_ID = 'PINV_CERTIFICATE_ITEM'
                ), 0.00)
                -
                COALESCE((
                    SELECT SUM(ii_other.QUANTITY * ROUND(ii_other.AMOUNT, 5))
                    FROM INVOICE_ITEM ii_other 
                    WHERE ii_other.INVOICE_ID = inv.INVOICE_ID 
                    AND ii_other.INVOICE_ITEM_TYPE_ID != 'PINV_CERTIFICATE_ITEM'
                ), 0.00)
            ELSE
                COALESCE((
                    SELECT SUM(ii.QUANTITY * ROUND(ii.AMOUNT, 5))
                    FROM INVOICE_ITEM ii 
                    WHERE ii.INVOICE_ID = inv.INVOICE_ID
                ), 0.00)
        END, 2
    ) AS Total
FROM INVOICE inv
INNER JOIN INVOICE_TYPE invt            ON inv.INVOICE_TYPE_ID = invt.INVOICE_TYPE_ID
INNER JOIN PARTY from_party             ON inv.PARTY_ID_FROM   = from_party.PARTY_ID
INNER JOIN PARTY to_party               ON inv.PARTY_ID        = to_party.PARTY_ID
INNER JOIN STATUS_ITEM sts              ON inv.STATUS_ID       = sts.STATUS_ID
LEFT  JOIN BILLING_ACCOUNT bil          ON inv.BILLING_ACCOUNT_ID = bil.BILLING_ACCOUNT_ID
LEFT  JOIN ORDER_ITEM_BILLING oib       ON inv.INVOICE_ID      = oib.INVOICE_ID
LEFT  JOIN WORK_EFFORT we               ON oib.ORDER_ID        = we.RELATED_ORDER_ID 
                                        AND we.WORK_EFFORT_TYPE_ID = 'PROJECT_CERTIFICATE'
GROUP BY 
    inv.INVOICE_ID, inv.INVOICE_TYPE_ID, invt.DESCRIPTION_ARABIC, invt.DESCRIPTION,
    inv.INVOICE_DATE, inv.DUE_DATE, inv.PAID_DATE, inv.STATUS_ID, sts.DESCRIPTION_ARABIC, sts.DESCRIPTION,
    inv.DESCRIPTION,
    inv.PARTY_ID, to_party.DESCRIPTION,
    inv.PARTY_ID_FROM, from_party.DESCRIPTION,
    inv.BILLING_ACCOUNT_ID, bil.DESCRIPTION,
    oib.ORDER_ID, we.CERTIFICATE_NUMBER,
    inv.CREATED_STAMP;   -- ← important: add to GROUP BY
");
}

protected override void Down(MigrationBuilder migrationBuilder)
{
    // Re-create the previous version of the view (without CreatedStamp)
    // → copy the original CREATE VIEW statement from your first migration here
    migrationBuilder.Sql(@" 
-- paste your ORIGINAL view creation SQL here (the one without CreatedStamp)
-- including the original GROUP BY without inv.CREATED_STAMP
");
}
    }
}
