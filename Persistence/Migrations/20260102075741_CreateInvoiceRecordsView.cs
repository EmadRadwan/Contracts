using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CreateInvoiceRecordsView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
  migrationBuilder.Sql(@"
    CREATE OR REPLACE VIEW InvoiceRecords AS
SELECT
    inv.INVOICE_ID AS InvoiceId,
    inv.INVOICE_TYPE_ID AS InvoiceTypeId,
    
    -- Arabic is default: use Arabic description, fallback to English if NULL
    COALESCE(invt.DESCRIPTION_ARABIC, invt.DESCRIPTION) AS InvoiceTypeDescription,

    inv.INVOICE_DATE AS InvoiceDate,
    inv.DUE_DATE AS DueDate,
    inv.PAID_DATE AS PaidDate,
    
    inv.STATUS_ID AS StatusId,
    COALESCE(sts.DESCRIPTION_ARABIC, sts.DESCRIPTION) AS StatusDescription,

    inv.DESCRIPTION AS Description,

    -- Party From (المورد / المورد من)
    inv.PARTY_ID_FROM AS PartyIdFrom_FromPartyId,
    from_party.DESCRIPTION AS PartyIdFrom_FromPartyName,

    -- Party To (الشركة / المستلم)
    inv.PARTY_ID AS PartyId_FromPartyId,
    to_party.DESCRIPTION AS PartyId_FromPartyName,

    -- Backward compatibility with current DTO structure
    to_party.DESCRIPTION AS ToPartyName,
    from_party.DESCRIPTION AS FromPartyName,

    inv.BILLING_ACCOUNT_ID AS BillingAccountId,
    bil.DESCRIPTION AS BillingAccountName,

    -- OutstandingAmount (placeholder - can be enhanced later)
    0.00 AS OutstandingAmount,

    -- OrderId and CertificateNumber via OrderItemBilling → WorkEffort
    oib.ORDER_ID AS OrderId,
    we.CERTIFICATE_NUMBER AS CertificateNumber,

    -- === MAIN LOGIC: Correct Total Calculation with per-line rounding ===
    ROUND(
        CASE
            -- Case 1: Invoice has at least one PINV_CERTIFICATE_ITEM
            WHEN EXISTS (
                SELECT 1
                FROM INVOICE_ITEM ii_cert
                WHERE ii_cert.INVOICE_ID = inv.INVOICE_ID
                  AND ii_cert.INVOICE_ITEM_TYPE_ID = 'PINV_CERTIFICATE_ITEM'
            )
            THEN
                -- Total of certificate items
                COALESCE((
                    SELECT SUM(ii_cert.QUANTITY * ROUND(ii_cert.AMOUNT, 5))
                    FROM INVOICE_ITEM ii_cert
                    WHERE ii_cert.INVOICE_ID = inv.INVOICE_ID
                      AND ii_cert.INVOICE_ITEM_TYPE_ID = 'PINV_CERTIFICATE_ITEM'
                ), 0.00)
                -
                -- Minus total of all non-certificate items
                COALESCE((
                    SELECT SUM(ii_other.QUANTITY * ROUND(ii_other.AMOUNT, 5))
                    FROM INVOICE_ITEM ii_other
                    WHERE ii_other.INVOICE_ID = inv.INVOICE_ID
                      AND ii_other.INVOICE_ITEM_TYPE_ID != 'PINV_CERTIFICATE_ITEM'
                ), 0.00)

            -- Case 2: No certificate item → normal sum of all items
            ELSE
                COALESCE((
                    SELECT SUM(ii.QUANTITY * ROUND(ii.AMOUNT, 5))
                    FROM INVOICE_ITEM ii
                    WHERE ii.INVOICE_ID = inv.INVOICE_ID
                ), 0.00)
        END,
        2
    ) AS Total

FROM INVOICE inv
INNER JOIN INVOICE_TYPE invt ON inv.INVOICE_TYPE_ID = invt.INVOICE_TYPE_ID
INNER JOIN PARTY from_party ON inv.PARTY_ID_FROM = from_party.PARTY_ID
INNER JOIN PARTY to_party ON inv.PARTY_ID = to_party.PARTY_ID
INNER JOIN STATUS_ITEM sts ON inv.STATUS_ID = sts.STATUS_ID

LEFT JOIN BILLING_ACCOUNT bil 
    ON inv.BILLING_ACCOUNT_ID = bil.BILLING_ACCOUNT_ID

-- Link to Order via OrderItemBilling
LEFT JOIN ORDER_ITEM_BILLING oib 
    ON inv.INVOICE_ID = oib.INVOICE_ID

-- Link to Project Certificate
LEFT JOIN WORK_EFFORT we 
    ON oib.ORDER_ID = we.RELATED_ORDER_ID
    AND we.WORK_EFFORT_TYPE_ID = 'PROJECT_CERTIFICATE'

-- Prevent row duplication if multiple order billings exist for same invoice
GROUP BY
    inv.INVOICE_ID,
    inv.INVOICE_TYPE_ID,
    invt.DESCRIPTION_ARABIC,
    invt.DESCRIPTION,
    inv.INVOICE_DATE,
    inv.DUE_DATE,
    inv.PAID_DATE,
    inv.STATUS_ID,
    sts.DESCRIPTION_ARABIC,
    sts.DESCRIPTION,
    inv.DESCRIPTION,
    inv.PARTY_ID,
    to_party.DESCRIPTION,
    inv.PARTY_ID_FROM,
    from_party.DESCRIPTION,
    inv.BILLING_ACCOUNT_ID,
    bil.DESCRIPTION,
    oib.ORDER_ID,
    we.CERTIFICATE_NUMBER;
");
        
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP VIEW IF EXISTS InvoiceRecords;
            ");
        }
    }
}
