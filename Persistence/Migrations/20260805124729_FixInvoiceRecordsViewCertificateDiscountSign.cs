using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixInvoiceRecordsViewCertificateDiscountSign : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // PINV_CERTIFICATE_DISCOUNT is flagged IS_POSITIVE_AMOUNT=0 like every other
            // "subtractive" invoice item type (PINV_DEDUCTION_ITEM, PAYROL_DD_*), so the view
            // applied the same "* -1 when IS_POSITIVE_AMOUNT=0" sign flip to it. But unlike
            // those other types -- which store a positive magnitude and rely entirely on this
            // flag for sign -- CreateProjectCertificate.cs/UpdateProjectCertificate.cs create the
            // discount's ORDER_ADJUSTMENT (and hence its billed INVOICE_ITEM) already negative
            // (-item.Discount.Value). Every other consumer of that value (the certificate's own
            // grandTotal sum, GL posting) already treats it as pre-signed and sums it directly,
            // so it's correct everywhere except here, where the extra flip double-negates it:
            // -0.73 * -1 = +0.73, turning a subtraction into an addition (e.g. INV1380 showed
            // 48,698.46 instead of the correct 48,697.00 -- exactly 2x the discount off).
            // Fix: skip the sign flip specifically for PINV_CERTIFICATE_DISCOUNT, since its
            // amount is already correctly signed and needs no further sign manipulation.
            migrationBuilder.Sql(@"
CREATE OR REPLACE VIEW InvoiceRecords AS
SELECT
    inv.INVOICE_ID                                        AS InvoiceId,
    inv.INVOICE_TYPE_ID                                   AS InvoiceTypeId,
    COALESCE(invt.DESCRIPTION_ARABIC, invt.DESCRIPTION)  AS InvoiceTypeDescription,
    inv.INVOICE_DATE                                      AS InvoiceDate,
    inv.DUE_DATE                                          AS DueDate,
    inv.PAID_DATE                                         AS PaidDate,
    inv.STATUS_ID                                         AS StatusId,
    COALESCE(sts.DESCRIPTION_ARABIC, sts.DESCRIPTION)    AS StatusDescription,
    inv.DESCRIPTION                                       AS Description,
    inv.PARTY_ID_FROM                                     AS PartyIdFrom_FromPartyId,
    from_party.DESCRIPTION                                AS PartyIdFrom_FromPartyName,
    inv.PARTY_ID                                          AS PartyId_FromPartyId,
    to_party.DESCRIPTION                                  AS PartyId_FromPartyName,
    to_party.DESCRIPTION                                  AS ToPartyName,
    from_party.DESCRIPTION                                AS FromPartyName,
    inv.BILLING_ACCOUNT_ID                                AS BillingAccountId,
    bil.DESCRIPTION                                       AS BillingAccountName,
    0.00                                                  AS OutstandingAmount,
    ob.ORDER_ID                                           AS OrderId,
    ob.CERTIFICATE_NUMBER                                 AS CertificateNumber,
    inv.CREATED_STAMP                                     AS CreatedStamp,
    COALESCE(it.Total, 0.00)                              AS Total
FROM INVOICE inv
INNER JOIN INVOICE_TYPE       invt       ON inv.INVOICE_TYPE_ID     = invt.INVOICE_TYPE_ID
INNER JOIN PARTY              from_party ON inv.PARTY_ID_FROM       = from_party.PARTY_ID
INNER JOIN PARTY              to_party   ON inv.PARTY_ID            = to_party.PARTY_ID
INNER JOIN STATUS_ITEM        sts        ON inv.STATUS_ID           = sts.STATUS_ID
LEFT  JOIN BILLING_ACCOUNT    bil        ON inv.BILLING_ACCOUNT_ID  = bil.BILLING_ACCOUNT_ID
LEFT JOIN (
    SELECT
        oib.INVOICE_ID       AS INVOICE_ID,
        MIN(oib.ORDER_ID)    AS ORDER_ID,
        MIN(we.CERTIFICATE_NUMBER) AS CERTIFICATE_NUMBER
    FROM ORDER_ITEM_BILLING oib
    LEFT JOIN WORK_EFFORT we
           ON oib.ORDER_ID           = we.RELATED_ORDER_ID
          AND we.WORK_EFFORT_TYPE_ID = 'PROJECT_CERTIFICATE'
    GROUP BY oib.INVOICE_ID
) ob ON inv.INVOICE_ID = ob.INVOICE_ID
LEFT JOIN (
    SELECT
        ii.INVOICE_ID AS INVOICE_ID,
        ROUND(
            CASE
                WHEN MAX(CASE WHEN ii.INVOICE_ITEM_TYPE_ID = 'PINV_CERTIFICATE_ITEM' THEN 1 ELSE 0 END) = 1
                THEN
                    COALESCE(SUM(
                        CASE WHEN ii.INVOICE_ITEM_TYPE_ID = 'PINV_CERTIFICATE_ITEM'
                        THEN COALESCE(ii.QUANTITY, 1) * ROUND(COALESCE(ii.AMOUNT, 0), 5)
                             * IF(COALESCE(iit.IS_POSITIVE_AMOUNT, 1) = 0
                                  AND ii.INVOICE_ITEM_TYPE_ID != 'PINV_CERTIFICATE_DISCOUNT', -1, 1)
                        ELSE 0 END
                    ), 0.00)
                    -
                    COALESCE(SUM(
                        CASE WHEN ii.INVOICE_ITEM_TYPE_ID != 'PINV_CERTIFICATE_ITEM'
                        THEN COALESCE(ii.QUANTITY, 1) * ROUND(COALESCE(ii.AMOUNT, 0), 5)
                             * IF(COALESCE(iit.IS_POSITIVE_AMOUNT, 1) = 0
                                  AND ii.INVOICE_ITEM_TYPE_ID != 'PINV_CERTIFICATE_DISCOUNT', -1, 1)
                        ELSE 0 END
                    ), 0.00)
                ELSE
                    COALESCE(SUM(
                        COALESCE(ii.QUANTITY, 1) * ROUND(COALESCE(ii.AMOUNT, 0), 5)
                        * IF(COALESCE(iit.IS_POSITIVE_AMOUNT, 1) = 0
                             AND ii.INVOICE_ITEM_TYPE_ID != 'PINV_CERTIFICATE_DISCOUNT', -1, 1)
                    ), 0.00)
            END,
            2
        ) AS Total
    FROM INVOICE_ITEM ii
    LEFT JOIN INVOICE_ITEM_TYPE iit ON ii.INVOICE_ITEM_TYPE_ID = iit.INVOICE_ITEM_TYPE_ID
    GROUP BY ii.INVOICE_ID
) it ON inv.INVOICE_ID = it.INVOICE_ID;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Restore the previous (double-negating) view for rollback purposes.
            migrationBuilder.Sql(@"
CREATE OR REPLACE VIEW InvoiceRecords AS
SELECT
    inv.INVOICE_ID                                        AS InvoiceId,
    inv.INVOICE_TYPE_ID                                   AS InvoiceTypeId,
    COALESCE(invt.DESCRIPTION_ARABIC, invt.DESCRIPTION)  AS InvoiceTypeDescription,
    inv.INVOICE_DATE                                      AS InvoiceDate,
    inv.DUE_DATE                                          AS DueDate,
    inv.PAID_DATE                                         AS PaidDate,
    inv.STATUS_ID                                         AS StatusId,
    COALESCE(sts.DESCRIPTION_ARABIC, sts.DESCRIPTION)    AS StatusDescription,
    inv.DESCRIPTION                                       AS Description,
    inv.PARTY_ID_FROM                                     AS PartyIdFrom_FromPartyId,
    from_party.DESCRIPTION                                AS PartyIdFrom_FromPartyName,
    inv.PARTY_ID                                          AS PartyId_FromPartyId,
    to_party.DESCRIPTION                                  AS PartyId_FromPartyName,
    to_party.DESCRIPTION                                  AS ToPartyName,
    from_party.DESCRIPTION                                AS FromPartyName,
    inv.BILLING_ACCOUNT_ID                                AS BillingAccountId,
    bil.DESCRIPTION                                       AS BillingAccountName,
    0.00                                                  AS OutstandingAmount,
    ob.ORDER_ID                                           AS OrderId,
    ob.CERTIFICATE_NUMBER                                 AS CertificateNumber,
    inv.CREATED_STAMP                                     AS CreatedStamp,
    COALESCE(it.Total, 0.00)                              AS Total
FROM INVOICE inv
INNER JOIN INVOICE_TYPE       invt       ON inv.INVOICE_TYPE_ID     = invt.INVOICE_TYPE_ID
INNER JOIN PARTY              from_party ON inv.PARTY_ID_FROM       = from_party.PARTY_ID
INNER JOIN PARTY              to_party   ON inv.PARTY_ID            = to_party.PARTY_ID
INNER JOIN STATUS_ITEM        sts        ON inv.STATUS_ID           = sts.STATUS_ID
LEFT  JOIN BILLING_ACCOUNT    bil        ON inv.BILLING_ACCOUNT_ID  = bil.BILLING_ACCOUNT_ID
LEFT JOIN (
    SELECT
        oib.INVOICE_ID       AS INVOICE_ID,
        MIN(oib.ORDER_ID)    AS ORDER_ID,
        MIN(we.CERTIFICATE_NUMBER) AS CERTIFICATE_NUMBER
    FROM ORDER_ITEM_BILLING oib
    LEFT JOIN WORK_EFFORT we
           ON oib.ORDER_ID           = we.RELATED_ORDER_ID
          AND we.WORK_EFFORT_TYPE_ID = 'PROJECT_CERTIFICATE'
    GROUP BY oib.INVOICE_ID
) ob ON inv.INVOICE_ID = ob.INVOICE_ID
LEFT JOIN (
    SELECT
        ii.INVOICE_ID AS INVOICE_ID,
        ROUND(
            CASE
                WHEN MAX(CASE WHEN ii.INVOICE_ITEM_TYPE_ID = 'PINV_CERTIFICATE_ITEM' THEN 1 ELSE 0 END) = 1
                THEN
                    COALESCE(SUM(
                        CASE WHEN ii.INVOICE_ITEM_TYPE_ID = 'PINV_CERTIFICATE_ITEM'
                        THEN COALESCE(ii.QUANTITY, 1) * ROUND(COALESCE(ii.AMOUNT, 0), 5)
                             * IF(COALESCE(iit.IS_POSITIVE_AMOUNT, 1) = 0, -1, 1)
                        ELSE 0 END
                    ), 0.00)
                    -
                    COALESCE(SUM(
                        CASE WHEN ii.INVOICE_ITEM_TYPE_ID != 'PINV_CERTIFICATE_ITEM'
                        THEN COALESCE(ii.QUANTITY, 1) * ROUND(COALESCE(ii.AMOUNT, 0), 5)
                             * IF(COALESCE(iit.IS_POSITIVE_AMOUNT, 1) = 0, -1, 1)
                        ELSE 0 END
                    ), 0.00)
                ELSE
                    COALESCE(SUM(
                        COALESCE(ii.QUANTITY, 1) * ROUND(COALESCE(ii.AMOUNT, 0), 5)
                        * IF(COALESCE(iit.IS_POSITIVE_AMOUNT, 1) = 0, -1, 1)
                    ), 0.00)
            END,
            2
        ) AS Total
    FROM INVOICE_ITEM ii
    LEFT JOIN INVOICE_ITEM_TYPE iit ON ii.INVOICE_ITEM_TYPE_ID = iit.INVOICE_ITEM_TYPE_ID
    GROUP BY ii.INVOICE_ID
) it ON inv.INVOICE_ID = it.INVOICE_ID;
");
        }
    }
}
