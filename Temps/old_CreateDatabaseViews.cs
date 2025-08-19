using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CreateDatabaseViews : Migration
    {
        /// <inheritdoc />
       protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Create PartyView
        migrationBuilder.Sql(@"
    CREATE VIEW `PARTY_VIEW` AS
    SELECT PRTY.PARTY_ID AS FromPartyId,
           PRTY.DESCRIPTION AS FromPartyName,
           TN.CONTACT_NUMBER AS FromPartyPhone
    FROM `PARTY` PRTY  
    JOIN `PARTY_CONTACT_MECH` PCM ON PRTY.PARTY_ID = PCM.PARTY_ID
    JOIN `CONTACT_MECH` CM ON PCM.CONTACT_MECH_ID = CM.CONTACT_MECH_ID
    JOIN `TELECOM_NUMBER` TN ON CM.CONTACT_MECH_ID = TN.CONTACT_MECH_ID
    JOIN `PARTY_CONTACT_MECH_PURPOSE` PCMP ON PCM.PARTY_ID = PCMP.PARTY_ID 
                                            AND PCM.CONTACT_MECH_ID = PCMP.CONTACT_MECH_ID
    JOIN `CONTACT_MECH_PURPOSE_TYPE` CMPT ON PCMP.CONTACT_MECH_PURPOSE_TYPE_ID = CMPT.CONTACT_MECH_PURPOSE_TYPE_ID
    WHERE PCMP.CONTACT_MECH_PURPOSE_TYPE_ID = 'PRIMARY_PHONE';
");
        
        migrationBuilder.Sql(@"
    CREATE VIEW `InventoryItemDetailForSumView` AS
    SELECT 
        II.INVENTORY_ITEM_TYPE_ID AS InventoryItemTypeId,
        II.FACILITY_ID AS FacilityId,
        II.PRODUCT_ID AS ProductId,
        II.UNIT_COST AS UnitCost,
        II.CURRENCY_UOM_ID AS CurrencyUomId,
        P.PRODUCT_NAME AS ProductName,
        U.DESCRIPTION AS QuantityUomDescription,
        SUM(IID.QUANTITY_ON_HAND_DIFF) AS QuantityOnHandSum,
        SUM(IID.ACCOUNTING_QUANTITY_DIFF) AS AccountingQuantitySum,
        MAX(IID.EFFECTIVE_DATE) AS EffectiveDate,
        II.OWNER_PARTY_ID AS OwnerPartyId,
        MIN(IID.ORDER_ID) AS OrderId
    FROM `INVENTORY_ITEM` II
    INNER JOIN `INVENTORY_ITEM_DETAIL` IID ON II.INVENTORY_ITEM_ID = IID.INVENTORY_ITEM_ID
    INNER JOIN `PRODUCT` P ON II.PRODUCT_ID = P.PRODUCT_ID
    INNER JOIN `UOM` U ON P.QUANTITY_UOM_ID = U.UOM_ID
    GROUP BY 
        II.INVENTORY_ITEM_TYPE_ID,
        II.FACILITY_ID,
        II.PRODUCT_ID,
        II.UNIT_COST,
        II.CURRENCY_UOM_ID,
        II.OWNER_PARTY_ID,
        P.PRODUCT_NAME,
        U.DESCRIPTION;
");
        
        
        // Create OrderView
        migrationBuilder.Sql(@"
CREATE VIEW OrderView AS
SELECT
    ord.ORDER_ID AS orderId,
    ord.ORDER_TYPE_ID AS orderTypeId,
    ordt.DESCRIPTION AS orderTypeDescription,
    ordt.DESCRIPTION_ARABIC AS orderTypeDescriptionArabic,
    ordt.DESCRIPTION_TURKISH AS orderTypeDescriptionTurkish,

    -- Earliest OPP (the user's actual original PaymentMethodTypeId)
    oppEarliest.earliestPaymentMethodId AS paymentMethodId,
    oppEarliest.earliestPaymentMethodTypeId AS paymentMethodTypeId,
    oppEarliest.earliestOppId AS orderPaymentPreferenceId,      -- The first (min) preference

    -- Latest OPP (used for linking with Payment)
    oppLatest.latestOppId AS orderPaymentPreferenceIdLatest,
    oppLatest.latestPaymentMethodId AS paymentMethodIdLatest,
    oppLatest.latestPaymentMethodTypeId AS paymentMethodTypeIdLatest,

    pay.PAYMENT_ID AS paymentId,       -- Payment ID from the latest preference
    pa.INVOICE_ID AS invoiceId,        -- Invoice ID from Payment Application
    
    ord.AGREEMENT_ID AS agreementId,
    pty.PARTY_ID AS fromPartyId,
    pty.DESCRIPTION AS fromPartyNameDescription,
    pty.DESCRIPTION AS fromPartyName,
    tn.CONTACT_NUMBER AS fromPartyContactNumber,
    ord.ORDER_DATE AS orderDate,
    sts.STATUS_ID AS orderStatus,
    sts.DESCRIPTION AS statusDescription,
    sts.DESCRIPTION_ARABIC AS statusDescriptionArabic,
    sts.DESCRIPTION_TURKISH AS statusDescriptionTurkish,
    ord.CURRENCY_UOM AS currencyUomId,
    uoms.DESCRIPTION AS currencyUomDescription,
    uoms.DESCRIPTION_ARABIC AS currencyUomDescriptionArabic,
    uoms.DESCRIPTION_TURKISH AS currencyUomDescriptionTurkish,
    ord.GRAND_TOTAL AS grandTotal,
    ord.BILLING_ACCOUNT_ID AS billingAccountId

FROM ORDER_HEADER ord
JOIN ORDER_ROLE orole ON ord.ORDER_ID = orole.ORDER_ID
JOIN PARTY pty ON orole.PARTY_ID = pty.PARTY_ID
JOIN ORDER_TYPE ordt ON ord.ORDER_TYPE_ID = ordt.ORDER_TYPE_ID
JOIN STATUS_ITEM sts ON ord.STATUS_ID = sts.STATUS_ID
JOIN PARTY_CONTACT_MECH pcm ON pty.PARTY_ID = pcm.PARTY_ID
JOIN CONTACT_MECH cm ON pcm.CONTACT_MECH_ID = cm.CONTACT_MECH_ID
JOIN TELECOM_NUMBER tn ON cm.CONTACT_MECH_ID = tn.CONTACT_MECH_ID
JOIN UOM uoms ON ord.CURRENCY_UOM = uoms.UOM_ID
JOIN PARTY_CONTACT_MECH_PURPOSE pcmp
  ON pcm.PARTY_ID = pcmp.PARTY_ID
  AND pcm.CONTACT_MECH_ID = pcmp.CONTACT_MECH_ID
JOIN CONTACT_MECH_PURPOSE_TYPE cmpt ON pcmp.CONTACT_MECH_PURPOSE_TYPE_ID = cmpt.CONTACT_MECH_PURPOSE_TYPE_ID

-- Subselect #1: EARLIEST (MIN) ORDER_PAYMENT_PREFERENCE
LEFT JOIN
(
    SELECT
      opp1.ORDER_ID,
      opp1.ORDER_PAYMENT_PREFERENCE_ID AS earliestOppId,
      opp1.PAYMENT_METHOD_ID AS earliestPaymentMethodId,
      opp1.PAYMENT_METHOD_TYPE_ID AS earliestPaymentMethodTypeId
    FROM ORDER_PAYMENT_PREFERENCE opp1
    WHERE opp1.CREATED_STAMP =
      (
        SELECT MIN(opp2.CREATED_STAMP)
        FROM ORDER_PAYMENT_PREFERENCE opp2
        WHERE opp2.ORDER_ID = opp1.ORDER_ID
      )
) oppEarliest ON ord.ORDER_ID = oppEarliest.ORDER_ID

-- Subselect #2: LATEST (MAX) ORDER_PAYMENT_PREFERENCE
LEFT JOIN
(
    SELECT
      opp1.ORDER_ID,
      opp1.ORDER_PAYMENT_PREFERENCE_ID AS latestOppId,
      opp1.PAYMENT_METHOD_ID AS latestPaymentMethodId,
      opp1.PAYMENT_METHOD_TYPE_ID AS latestPaymentMethodTypeId
    FROM ORDER_PAYMENT_PREFERENCE opp1
    WHERE opp1.CREATED_STAMP =
      (
        SELECT MAX(opp2.CREATED_STAMP)
        FROM ORDER_PAYMENT_PREFERENCE opp2
        WHERE opp2.ORDER_ID = opp1.ORDER_ID
      )
) oppLatest ON ord.ORDER_ID = oppLatest.ORDER_ID

-- Link Payment & Payment Application via the LATEST Opp ID
LEFT JOIN PAYMENT pay ON oppLatest.latestOppId = pay.PAYMENT_PREFERENCE_ID
LEFT JOIN PAYMENT_APPLICATION pa ON pay.PAYMENT_ID = pa.PAYMENT_ID

WHERE pcmp.CONTACT_MECH_PURPOSE_TYPE_ID = 'PRIMARY_PHONE'
  AND (
       (ordt.ORDER_TYPE_ID = 'PURCHASE_ORDER' AND orole.ROLE_TYPE_ID = 'BILL_FROM_VENDOR')
       OR (ordt.ORDER_TYPE_ID = 'SALES_ORDER' AND orole.ROLE_TYPE_ID = 'PLACING_CUSTOMER')
      );
     
");
       
        // Create OUTSTANDING_PURCHASED_QUANTITY_VIEW
        migrationBuilder.Sql(@"
    CREATE VIEW `OUTSTANDING_PURCHASED_QUANTITY_VIEW` AS
    SELECT
        P.PRODUCT_ID,
        IFNULL(SUM(OI.QUANTITY - IFNULL(OI.CANCEL_QUANTITY, 0)), 0) AS QUANTITY_ON_ORDER
    FROM `PRODUCT` P
    LEFT JOIN `ORDER_ITEM` OI ON P.PRODUCT_ID = OI.PRODUCT_ID
    LEFT JOIN `ORDER_HEADER` OH ON OI.ORDER_ID = OH.ORDER_ID
    WHERE OH.ORDER_TYPE_ID = 'PURCHASE_ORDER'
      AND OH.STATUS_ID NOT IN ('ORDER_COMPLETED', 'ORDER_CANCELLED', 'ORDER_REJECTED')
      AND OI.STATUS_ID NOT IN ('ITEM_COMPLETED', 'ITEM_CANCELLED', 'ITEM_REJECTED')
      AND OI.QUANTITY > 0
    GROUP BY P.PRODUCT_ID;
");
        
         // Create the FACILITY_INVENTORY_RECORD_VIEW - This is a complex view that aggregates inventory, price, usage, and product facility data
           migrationBuilder.Sql(@"
    CREATE VIEW `FACILITY_INVENTORY_RECORD_VIEW` AS
    WITH
        -- CTE 1: INV_AGG (Inventory Aggregation)
        INV_AGG AS (
            SELECT
                INV.PRODUCT_ID,
                INV.FACILITY_ID,
                SUM(INV.QUANTITY_ON_HAND_TOTAL) AS QUANTITY_ON_HAND_TOTAL,
                SUM(INV.AVAILABLE_TO_PROMISE_TOTAL) AS AVAILABLE_TO_PROMISE_TOTAL
            FROM `INVENTORY_ITEM` INV
            GROUP BY
                INV.PRODUCT_ID,
                INV.FACILITY_ID
        ),

        -- CTE 2: PRICE_AGG (Price Aggregation)
       PRICE_AGG AS (
        SELECT
            PP.PRODUCT_ID,
            PP.PRICE AS DEFAULT_PRICE
        FROM `PRODUCT_PRICE` PP
        INNER JOIN (
            SELECT
                PRODUCT_ID,
                MAX(FROM_DATE) AS LATEST_FROM_DATE
            FROM `PRODUCT_PRICE`
            WHERE PRODUCT_PRICE_TYPE_ID = 'DEFAULT_PRICE'
            AND CURRENT_DATE >= FROM_DATE
            AND (THRU_DATE IS NULL OR CURRENT_DATE <= THRU_DATE)
            GROUP BY PRODUCT_ID
        ) LATEST_PRICE ON PP.PRODUCT_ID = LATEST_PRICE.PRODUCT_ID
            AND PP.FROM_DATE = LATEST_PRICE.LATEST_FROM_DATE
            AND PP.PRODUCT_PRICE_TYPE_ID = 'DEFAULT_PRICE'
    ),


        -- CTE 3: OPQ_AGG (Outstanding Purchase Quantity Aggregation)
        OPQ_AGG AS (
            SELECT
                OPQ.PRODUCT_ID,
                IFNULL(SUM(OPQ.QUANTITY_ON_ORDER), 0) AS QUANTITY_ON_ORDER
            FROM `OUTSTANDING_PURCHASED_QUANTITY_VIEW` OPQ
            GROUP BY OPQ.PRODUCT_ID
        ),

        -- CTE 4: USAGE_AGG (Usage Aggregation)
        USAGE_AGG AS (
            SELECT
                INV_AG.PRODUCT_ID,
                INV_AG.FACILITY_ID,
                IFNULL(SALES_USAGE.SALES_USAGE, 0) + IFNULL(PRODUCTION_USAGE.PRODUCTION_USAGE, 0) AS USAGE_QUANTITY
            FROM INV_AGG INV_AG
            LEFT JOIN (
                -- Sub-CTE: Sales Usage Aggregation
                SELECT
                    INVIT_INNER.PRODUCT_ID,
                    INVIT_INNER.FACILITY_ID,
                    SUM(OI_INNER.QUANTITY) AS SALES_USAGE
                FROM `ORDER_ITEM` OI_INNER
                JOIN `ORDER_HEADER` OH_INNER ON OI_INNER.ORDER_ID = OH_INNER.ORDER_ID
                JOIN `ITEM_ISSUANCE` ITISS_INNER ON OI_INNER.ORDER_ID = ITISS_INNER.ORDER_ID
                                                 AND OI_INNER.ORDER_ITEM_SEQ_ID = ITISS_INNER.ORDER_ITEM_SEQ_ID
                JOIN `INVENTORY_ITEM` INVIT_INNER ON ITISS_INNER.INVENTORY_ITEM_ID = INVIT_INNER.INVENTORY_ITEM_ID
                WHERE OH_INNER.STATUS_ID IN ('ORDER_COMPLETED', 'ORDER_APPROVED', 'ORDER_HELD')
                AND OH_INNER.ORDER_TYPE_ID = 'SALES_ORDER'
                GROUP BY INVIT_INNER.PRODUCT_ID, INVIT_INNER.FACILITY_ID
            ) SALES_USAGE ON INV_AG.PRODUCT_ID = SALES_USAGE.PRODUCT_ID
                           AND INV_AG.FACILITY_ID = SALES_USAGE.FACILITY_ID
            LEFT JOIN (
                -- Sub-CTE: Production Usage Aggregation
                SELECT
                    II_INNER.PRODUCT_ID,
                    II_INNER.FACILITY_ID,
                    SUM(WEA_INNER.QUANTITY) AS PRODUCTION_USAGE
                FROM `WORK_EFFORT_INVENTORY_ASSIGN` WEA_INNER
                JOIN `WORK_EFFORT` WE_INNER ON WEA_INNER.WORK_EFFORT_ID = WE_INNER.WORK_EFFORT_ID
                JOIN `INVENTORY_ITEM` II_INNER ON WEA_INNER.INVENTORY_ITEM_ID = II_INNER.INVENTORY_ITEM_ID
                WHERE WE_INNER.WORK_EFFORT_TYPE_ID = 'PROD_ORDER_TASK'
                GROUP BY II_INNER.PRODUCT_ID, II_INNER.FACILITY_ID
            ) PRODUCTION_USAGE ON INV_AG.PRODUCT_ID = PRODUCTION_USAGE.PRODUCT_ID
                                AND INV_AG.FACILITY_ID = PRODUCTION_USAGE.FACILITY_ID
        ),

        -- CTE 5: PRODUCT_FACILITY_AGG (Product Facility Aggregation)
        PRODUCT_FACILITY_AGG AS (
            SELECT
                PF.PRODUCT_ID,
                PF.FACILITY_ID,
                PF.MINIMUM_STOCK,
                PF.REORDER_QUANTITY,
                PF.DAYS_TO_SHIP
            FROM `PRODUCT_FACILITY` PF
        )

SELECT
    IFNULL(F.FACILITY_ID, 'N/A') AS FACILITY_ID,
    IFNULL(F.FACILITY_NAME, 'No Facility') AS FACILITY_NAME,
    IFNULL(F.FACILITY_NAME_ARABIC, 'No Facility') AS FACILITY_NAME_ARABIC,
    IFNULL(F.FACILITY_NAME_TURKISH, 'No Facility') AS FACILITY_NAME_TURKISH,

    P.PRODUCT_ID AS PRODUCT_ID,
    P.PRODUCT_NAME AS PRODUCT_NAME,
    IFNULL(P.PRODUCT_NAME_ARABIC, 'No Name') AS PRODUCT_NAME_ARABIC,
    IFNULL(P.PRODUCT_NAME_TURKISH, 'No Name') AS PRODUCT_NAME_TURKISH,
    P.DESCRIPTION AS DESCRIPTION,
    P.QUANTITY_UOM_ID AS QUANTITY_UOM_ID,

    IFNULL(INV_AG.QUANTITY_ON_HAND_TOTAL, 0) AS QUANTITY_ON_HAND_TOTAL,
    IFNULL(INV_AG.AVAILABLE_TO_PROMISE_TOTAL, 0) AS AVAILABLE_TO_PROMISE_TOTAL,

    IFNULL(PRICE_AGG.DEFAULT_PRICE, 0) AS DEFAULT_PRICE,
    IFNULL(PRICE_AGG.WHOLESALE_PRICE, 0) AS WHOLESALE_PRICE,
    IFNULL(PRICE_AGG.LIST_PRICE, 0) AS LIST_PRICE,

    IFNULL(OPQ_AGG.QUANTITY_ON_ORDER, 0) AS QUANTITY_ON_ORDER,
    IFNULL(USAGE_AGG.USAGE_QUANTITY, 0) AS USAGE_QUANTITY,

    IFNULL(PF_AGG.MINIMUM_STOCK, 0) AS MINIMUM_STOCK,
    IFNULL(PF_AGG.REORDER_QUANTITY, 0) AS REORDER_QUANTITY,
    IFNULL(PF_AGG.DAYS_TO_SHIP, 0) AS DAYS_TO_SHIP,

    IFNULL(INV_AG.AVAILABLE_TO_PROMISE_TOTAL, 0) - IFNULL(PF_AGG.MINIMUM_STOCK, 0) AS AVAILABLE_TO_PROMISE_MINUS_MINIMUM_STOCK,
    IFNULL(INV_AG.QUANTITY_ON_HAND_TOTAL, 0) - IFNULL(PF_AGG.MINIMUM_STOCK, 0) AS QUANTITY_ON_HAND_MINUS_MINIMUM_STOCK

FROM INV_AGG INV_AG
JOIN `PRODUCT` P ON INV_AG.PRODUCT_ID = P.PRODUCT_ID
LEFT JOIN PRICE_AGG ON P.PRODUCT_ID = PRICE_AGG.PRODUCT_ID
LEFT JOIN `FACILITY` F ON INV_AG.FACILITY_ID = F.FACILITY_ID
LEFT JOIN OPQ_AGG ON P.PRODUCT_ID = OPQ_AGG.PRODUCT_ID
LEFT JOIN USAGE_AGG ON INV_AG.PRODUCT_ID = USAGE_AGG.PRODUCT_ID
                     AND INV_AG.FACILITY_ID = USAGE_AGG.FACILITY_ID
LEFT JOIN PRODUCT_FACILITY_AGG PF_AGG ON P.PRODUCT_ID = PF_AGG.PRODUCT_ID
                                        AND INV_AG.FACILITY_ID = PF_AGG.FACILITY_ID;
");

        
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Drop PartyView
        migrationBuilder.Sql("DROP VIEW IF EXISTS PartyView;");

        // Drop OrderView
        migrationBuilder.Sql("DROP VIEW IF EXISTS OrderView;");
        
        // Drop OUTSTANDING_PURCHASED_QUANTITY_VIEW
        migrationBuilder.Sql("DROP VIEW IF EXISTS OUTSTANDING_PURCHASED_QUANTITY_VIEW;");
        
        // Drop the FACILITY_INVENTORY_RECORD_VIEW if it exists
        migrationBuilder.Sql(@"
                DROP VIEW IF EXISTS FACILITY_INVENTORY_RECORD_VIEW;
            ");

    }
    }
}
