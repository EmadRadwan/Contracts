namespace Domain;

public class SecurityGroup
{
    public SecurityGroup()
    {
        PartyRelationships = new HashSet<PartyRelationship>();
        PortalPages = new HashSet<PortalPage>();
        ProtectedViews = new HashSet<ProtectedView>();
        SecurityGroupPermissions = new HashSet<SecurityGroupPermission>();
        UserLoginSecurityGroups = new HashSet<UserLoginSecurityGroup>();
    }

    public string GroupId { get; set; } = null!;
    public string? GroupName { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<PartyRelationship> PartyRelationships { get; set; }
    public ICollection<PortalPage> PortalPages { get; set; }
    public ICollection<ProtectedView> ProtectedViews { get; set; }
    public ICollection<SecurityGroupPermission> SecurityGroupPermissions { get; set; }
    public ICollection<UserLoginSecurityGroup> UserLoginSecurityGroups { get; set; }
}