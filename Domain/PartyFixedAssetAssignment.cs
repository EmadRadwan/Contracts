namespace Domain;

public class PartyFixedAssetAssignment
{
    public string PartyId { get; set; } = null!;
    public string RoleTypeId { get; set; } = null!;
    public string FixedAssetId { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public DateTime? AllocatedDate { get; set; }
    public string? StatusId { get; set; }
    public string? Comments { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public FixedAsset FixedAsset { get; set; } = null!;
    public PartyRole PartyRole { get; set; } = null!;
    public StatusItem? Status { get; set; }
}