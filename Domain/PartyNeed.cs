namespace Domain;

public class PartyNeed
{
    public string PartyNeedId { get; set; } = null!;
    public string PartyId { get; set; } = null!;
    public string RoleTypeId { get; set; } = null!;
    public string? PartyTypeId { get; set; }
    public string? NeedTypeId { get; set; }
    public string? CommunicationEventId { get; set; }
    public string? ProductId { get; set; }
    public string? ProductCategoryId { get; set; }
    public string? VisitId { get; set; }
    public DateTime? DatetimeRecorded { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public CommunicationEvent? CommunicationEvent { get; set; }
    public NeedType? NeedType { get; set; }
    public Party Party { get; set; } = null!;
    public PartyType? PartyType { get; set; }
    public Product? Product { get; set; }
    public ProductCategory? ProductCategory { get; set; }
    public RoleType RoleType { get; set; } = null!;
}