namespace Domain;

public class PortalPage
{
    public PortalPage()
    {
        InverseParentPortalPage = new HashSet<PortalPage>();
        PortalPageColumns = new HashSet<PortalPageColumn>();
        PortalPagePortlets = new HashSet<PortalPagePortlet>();
    }

    public string PortalPageId { get; set; } = null!;
    public string? PortalPageName { get; set; }
    public string? Description { get; set; }
    public string? OwnerUserLoginId { get; set; }
    public string? OriginalPortalPageId { get; set; }
    public string? ParentPortalPageId { get; set; }
    public int? SequenceNum { get; set; }
    public string? SecurityGroupId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }
    public string? HelpContentId { get; set; }

    public Content? HelpContent { get; set; }
    public PortalPage? ParentPortalPage { get; set; }
    public SecurityGroup? SecurityGroup { get; set; }
    public ICollection<PortalPage> InverseParentPortalPage { get; set; }
    public ICollection<PortalPageColumn> PortalPageColumns { get; set; }
    public ICollection<PortalPagePortlet> PortalPagePortlets { get; set; }
}