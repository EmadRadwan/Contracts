namespace Domain;

public class PortalPageColumn
{
    public string PortalPageId { get; set; } = null!;
    public string ColumnSeqId { get; set; } = null!;
    public int? ColumnWidthPixels { get; set; }
    public int? ColumnWidthPercentage { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public PortalPage PortalPage { get; set; } = null!;
}