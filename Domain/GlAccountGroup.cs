namespace Domain;

public class GlAccountGroup
{
    public GlAccountGroup()
    {
        GlAccountGroupMembers = new HashSet<GlAccountGroupMember>();
    }

    public string GlAccountGroupId { get; set; } = null!;
    public string? GlAccountGroupTypeId { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public GlAccountGroupType? GlAccountGroupType { get; set; }
    public ICollection<GlAccountGroupMember> GlAccountGroupMembers { get; set; }
}