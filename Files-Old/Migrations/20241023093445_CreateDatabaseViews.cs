using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
   public partial class CreateDatabaseViews : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Create PartyView
        migrationBuilder.Sql(@"
            CREATE VIEW PartyView AS
            SELECT prty.Party_Id AS FromPartyId,
                   prty.Description AS FromPartyName,
                   tn.Contact_Number AS FromPartyPhone
            FROM Party prty
            JOIN Party_Contact_Mech pcm ON prty.Party_Id = pcm.Party_Id
            JOIN Contact_Mech cm ON pcm.Contact_Mech_Id = cm.Contact_Mech_Id
            JOIN Telecom_Number tn ON cm.Contact_Mech_Id = tn.Contact_Mech_Id
            JOIN Party_Contact_Mech_Purpose pcmp ON pcm.Party_Id = pcmp.Party_Id AND pcm.Contact_Mech_Id = pcmp.Contact_Mech_Id
            JOIN Contact_Mech_Purpose_Type cmpt ON pcmp.Contact_Mech_Purpose_Type_Id = cmpt.Contact_Mech_Purpose_Type_Id
            WHERE pcmp.Contact_Mech_Purpose_Type_Id = 'PRIMARY_PHONE';
        ");

        // Create OrderView
        migrationBuilder.Sql(@"
CREATE VIEW OrderView AS
SELECT
    ord.ORDER_ID AS orderId,
    ord.ORDER_TYPE_ID AS orderTypeId,
    ordt.DESCRIPTION AS orderTypeDescription,
    opp.PAYMENT_METHOD_ID AS paymentMethodId,
    opp.PAYMENT_METHOD_TYPE_ID AS paymentMethodTypeId,
    ord.AGREEMENT_ID AS agreementId,
    pty.PARTY_ID AS fromPartyId,
    pty.DESCRIPTION AS fromPartyNameDescription,
    
    -- Keep the original name column
    pty.DESCRIPTION AS fromPartyName,
    
    -- Add contact number as a separate column
    tn.CONTACT_NUMBER AS fromPartyContactNumber,
    
    ord.ORDER_DATE AS orderDate,
    sts.DESCRIPTION AS statusDescription,
    ord.CURRENCY_UOM AS currencyUomId,
    uoms.DESCRIPTION AS currencyUomDescription,
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
JOIN PARTY_CONTACT_MECH_PURPOSE pcmp ON pcm.PARTY_ID = pcmp.PARTY_ID AND pcm.CONTACT_MECH_ID = pcmp.CONTACT_MECH_ID
JOIN CONTACT_MECH_PURPOSE_TYPE cmpt ON pcmp.CONTACT_MECH_PURPOSE_TYPE_ID = cmpt.CONTACT_MECH_PURPOSE_TYPE_ID
LEFT JOIN (
    SELECT opp.ORDER_ID, opp.PAYMENT_METHOD_ID, opp.PAYMENT_METHOD_TYPE_ID, opp.CREATED_STAMP
    FROM ORDER_PAYMENT_PREFERENCE opp
    WHERE opp.CREATED_STAMP = (
        SELECT MAX(opp2.CREATED_STAMP)
        FROM ORDER_PAYMENT_PREFERENCE opp2
        WHERE opp2.ORDER_ID = opp.ORDER_ID
    )
) opp ON ord.ORDER_ID = opp.ORDER_ID
WHERE pcmp.CONTACT_MECH_PURPOSE_TYPE_ID = 'PRIMARY_PHONE' 
  AND ((ordt.ORDER_TYPE_ID = 'PURCHASE_ORDER' AND orole.ROLE_TYPE_ID = 'BILL_FROM_VENDOR') 
       OR (ordt.ORDER_TYPE_ID = 'SALES_ORDER' AND orole.ROLE_TYPE_ID = 'PLACING_CUSTOMER'));

        
");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Drop PartyView
        migrationBuilder.Sql("DROP VIEW IF EXISTS PartyView;");

        // Drop OrderView
        migrationBuilder.Sql("DROP VIEW IF EXISTS OrderView;");
    }
}

}
