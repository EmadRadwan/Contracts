namespace Domain;

public class PortletAttribute
{
    public string PortalPageId { get; set; } = null!;
    public string PortalPortletId { get; set; } = null!;
    public string PortletSeqId { get; set; } = null!;
    public string AttrName { get; set; } = null!;
    public string? AttrValue { get; set; }
    public string? AttrDescription { get; set; }
    public string? AttrType { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public PortalPortlet PortalPortlet { get; set; } = null!;
}