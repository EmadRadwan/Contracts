namespace Domain;

public class FacilityGroupType
{
    public FacilityGroupType()
    {
        FacilityGroups = new HashSet<FacilityGroup>();
    }

    public string FacilityGroupTypeId { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<FacilityGroup> FacilityGroups { get; set; }
}