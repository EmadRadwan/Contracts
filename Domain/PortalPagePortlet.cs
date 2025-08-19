namespace Domain;

public class PortalPagePortlet
{
    public string PortalPageId { get; set; } = null!;
    public string PortalPortletId { get; set; } = null!;
    public string PortletSeqId { get; set; } = null!;
    public string? ColumnSeqId { get; set; }
    public int? SequenceNum { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public PortalPage PortalPage { get; set; } = null!;
    public PortalPortlet PortalPortlet { get; set; } = null!;
}