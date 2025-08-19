namespace Domain;

public class FacilityGroupMember
{
    public string FacilityId { get; set; } = null!;
    public string FacilityGroupId { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public int? SequenceNum { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Facility Facility { get; set; } = null!;
    public FacilityGroup FacilityGroup { get; set; } = null!;
}