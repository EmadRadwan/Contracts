namespace Domain;

public class PortletPortletCategory
{
    public string PortalPortletId { get; set; } = null!;
    public string PortletCategoryId { get; set; } = null!;
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public PortalPortlet PortalPortlet { get; set; } = null!;
    public PortletCategory PortletCategory { get; set; } = null!;
}