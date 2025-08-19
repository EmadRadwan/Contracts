namespace Domain;

public class PartyInvitation
{
    public PartyInvitation()
    {
        PartyInvitationGroupAssocs = new HashSet<PartyInvitationGroupAssoc>();
        PartyInvitationRoleAssocs = new HashSet<PartyInvitationRoleAssoc>();
    }

    public string PartyInvitationId { get; set; } = null!;
    public string? PartyIdFrom { get; set; }
    public string? PartyId { get; set; }
    public string? ToName { get; set; }
    public string? EmailAddress { get; set; }
    public string? StatusId { get; set; }
    public DateTime? LastInviteDate { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Party? PartyIdFromNavigation { get; set; }
    public StatusItem? Status { get; set; }
    public ICollection<PartyInvitationGroupAssoc> PartyInvitationGroupAssocs { get; set; }
    public ICollection<PartyInvitationRoleAssoc> PartyInvitationRoleAssocs { get; set; }
}