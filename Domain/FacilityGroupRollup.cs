namespace Domain;

public class FacilityGroupRollup
{
    public string FacilityGroupId { get; set; } = null!;
    public string ParentFacilityGroupId { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public int? SequenceNum { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public FacilityGroup FacilityGroup { get; set; } = null!;
    public FacilityGroup ParentFacilityGroup { get; set; } = null!;
}