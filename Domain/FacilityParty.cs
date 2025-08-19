namespace Domain;

public class FacilityParty
{
    public string FacilityId { get; set; } = null!;
    public string PartyId { get; set; } = null!;
    public string RoleTypeId { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Facility Facility { get; set; } = null!;
    public Party Party { get; set; } = null!;
    public PartyRole PartyRole { get; set; } = null!;
    public RoleType RoleType { get; set; } = null!;
}