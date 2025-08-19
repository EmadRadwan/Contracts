using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    public partial class CreatePartyView : Migration
    {
            protected override void Up(MigrationBuilder migrationBuilder)
            {
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
        
            }

            protected override void Down(MigrationBuilder migrationBuilder)
            {
                migrationBuilder.Sql(@"DROP VIEW PartyView;");
            }
    }
}
