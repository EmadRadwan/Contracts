namespace Domain;

public class PartyInvitationRoleAssoc
{
    public string PartyInvitationId { get; set; } = null!;
    public string RoleTypeId { get; set; } = null!;
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public PartyInvitation PartyInvitation { get; set; } = null!;
    public RoleType RoleType { get; set; } = null!;
}