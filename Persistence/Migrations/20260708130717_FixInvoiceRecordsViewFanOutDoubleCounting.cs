using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixInvoiceRecordsViewFanOutDoubleCounting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Previous view joined ORDER_ITEM_BILLING/WORK_EFFORT and INVOICE_ITEM directly in the
            // same FROM clause. Both are child tables with multiple rows per invoice, so the join
            // produced a cross product (rows(oib) x rows(ii)) before the outer GROUP BY, multiplying
            // the summed Total by the number of ORDER_ITEM_BILLING rows for the invoice (e.g. INV1082
            // had 2 billing rows and 2 invoice items -> Total was double the real 11,050 total).
            // Fix: pre-aggregate each child table to one row per invoice in its own subquery, then
            // join those single-row-per-invoice results onto INVOICE so there is no fan-out.
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Restore the previous (buggy) fan-out view for rollback purposes.
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
    oib.ORDER_ID                                          AS OrderId,
    we.CERTIFICATE_NUMBER                                 AS CertificateNumber,
    inv.CREATED_STAMP                                     AS CreatedStamp,
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
FROM INVOICE inv
INNER JOIN INVOICE_TYPE       invt       ON inv.INVOICE_TYPE_ID     = invt.INVOICE_TYPE_ID
INNER JOIN PARTY              from_party ON inv.PARTY_ID_FROM       = from_party.PARTY_ID
INNER JOIN PARTY              to_party   ON inv.PARTY_ID            = to_party.PARTY_ID
INNER JOIN STATUS_ITEM        sts        ON inv.STATUS_ID           = sts.STATUS_ID
LEFT  JOIN BILLING_ACCOUNT    bil        ON inv.BILLING_ACCOUNT_ID  = bil.BILLING_ACCOUNT_ID
LEFT  JOIN ORDER_ITEM_BILLING oib        ON inv.INVOICE_ID          = oib.INVOICE_ID
LEFT  JOIN WORK_EFFORT        we         ON oib.ORDER_ID            = we.RELATED_ORDER_ID
                                        AND we.WORK_EFFORT_TYPE_ID  = 'PROJECT_CERTIFICATE'
LEFT  JOIN INVOICE_ITEM       ii         ON inv.INVOICE_ID          = ii.INVOICE_ID
LEFT  JOIN INVOICE_ITEM_TYPE  iit        ON ii.INVOICE_ITEM_TYPE_ID = iit.INVOICE_ITEM_TYPE_ID
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
    we.CERTIFICATE_NUMBER,
    inv.CREATED_STAMP;
");
        }
    }
}
