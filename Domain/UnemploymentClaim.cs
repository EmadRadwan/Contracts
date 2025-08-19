namespace Domain;

public class UnemploymentClaim
{
    public string UnemploymentClaimId { get; set; } = null!;
    public DateTime? UnemploymentClaimDate { get; set; }
    public string? Description { get; set; }
    public string? StatusId { get; set; }
    public string? PartyIdFrom { get; set; }
    public string? PartyIdTo { get; set; }
    public string? RoleTypeIdFrom { get; set; }
    public string? RoleTypeIdTo { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }
}