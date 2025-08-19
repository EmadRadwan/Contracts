namespace Domain;

public class ItemIssuanceRole
{
    public string ItemIssuanceId { get; set; } = null!;
    public string PartyId { get; set; } = null!;
    public string RoleTypeId { get; set; } = null!;
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ItemIssuance ItemIssuance { get; set; } = null!;
    public Party Party { get; set; } = null!;
    public PartyRole PartyRole { get; set; } = null!;
}