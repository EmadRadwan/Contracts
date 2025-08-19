namespace Domain;

public class CommunicationEventRole
{
    public string CommunicationEventId { get; set; } = null!;
    public string PartyId { get; set; } = null!;
    public string RoleTypeId { get; set; } = null!;
    public string? ContactMechId { get; set; }
    public string? StatusId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public CommunicationEvent CommunicationEvent { get; set; } = null!;
    public ContactMech? ContactMech { get; set; }
    public Party Party { get; set; } = null!;
    public PartyRole PartyRole { get; set; } = null!;
    public StatusItem? Status { get; set; }
}