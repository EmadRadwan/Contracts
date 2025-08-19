namespace Domain;

public class FacilityGroupRole
{
    public string FacilityGroupId { get; set; } = null!;
    public string PartyId { get; set; } = null!;
    public string RoleTypeId { get; set; } = null!;
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public FacilityGroup FacilityGroup { get; set; } = null!;
    public PartyRole PartyRole { get; set; } = null!;
}