namespace Domain;

public class FacilityGroup
{
    public FacilityGroup()
    {
        Facilities = new HashSet<Facility>();
        FacilityGroupMembers = new HashSet<FacilityGroupMember>();
        FacilityGroupRoles = new HashSet<FacilityGroupRole>();
        FacilityGroupRollupFacilityGroups = new HashSet<FacilityGroupRollup>();
        FacilityGroupRollupParentFacilityGroups = new HashSet<FacilityGroupRollup>();
        InversePrimaryParentGroup = new HashSet<FacilityGroup>();
    }

    public string FacilityGroupId { get; set; } = null!;
    public string? FacilityGroupTypeId { get; set; }
    public string? PrimaryParentGroupId { get; set; }
    public string? FacilityGroupName { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public FacilityGroupType? FacilityGroupType { get; set; }
    public FacilityGroup? PrimaryParentGroup { get; set; }
    public ICollection<Facility> Facilities { get; set; }
    public ICollection<FacilityGroupMember> FacilityGroupMembers { get; set; }
    public ICollection<FacilityGroupRole> FacilityGroupRoles { get; set; }
    public ICollection<FacilityGroupRollup> FacilityGroupRollupFacilityGroups { get; set; }
    public ICollection<FacilityGroupRollup> FacilityGroupRollupParentFacilityGroups { get; set; }
    public ICollection<FacilityGroup> InversePrimaryParentGroup { get; set; }
}