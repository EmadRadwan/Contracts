namespace Domain;

public class GlAccountGroupType
{
    public GlAccountGroupType()
    {
        GlAccountGroupMembers = new HashSet<GlAccountGroupMember>();
        GlAccountGroups = new HashSet<GlAccountGroup>();
    }

    public string GlAccountGroupTypeId { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<GlAccountGroupMember> GlAccountGroupMembers { get; set; }
    public ICollection<GlAccountGroup> GlAccountGroups { get; set; }
}