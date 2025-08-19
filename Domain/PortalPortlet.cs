namespace Domain;

public class PortalPortlet
{
    public PortalPortlet()
    {
        PortalPagePortlets = new HashSet<PortalPagePortlet>();
        PortletAttributes = new HashSet<PortletAttribute>();
        PortletPortletCategories = new HashSet<PortletPortletCategory>();
    }

    public string PortalPortletId { get; set; } = null!;
    public string? PortletName { get; set; }
    public string? ScreenName { get; set; }
    public string? ScreenLocation { get; set; }
    public string? EditFormName { get; set; }
    public string? EditFormLocation { get; set; }
    public string? Description { get; set; }
    public string? Screenshot { get; set; }
    public string? SecurityServiceName { get; set; }
    public string? SecurityMainAction { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<PortalPagePortlet> PortalPagePortlets { get; set; }
    public ICollection<PortletAttribute> PortletAttributes { get; set; }
    public ICollection<PortletPortletCategory> PortletPortletCategories { get; set; }
}