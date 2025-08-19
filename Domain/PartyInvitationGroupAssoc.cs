namespace Domain;

public class PartyInvitationGroupAssoc
{
    public string PartyInvitationId { get; set; } = null!;
    public string PartyIdTo { get; set; } = null!;
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public PartyGroup PartyIdTo1 { get; set; } = null!;
    public Party PartyIdToNavigation { get; set; } = null!;
    public PartyInvitation PartyInvitation { get; set; } = null!;
}