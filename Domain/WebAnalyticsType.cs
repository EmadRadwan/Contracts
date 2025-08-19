namespace Domain;

public class WebAnalyticsType
{
    public WebAnalyticsType()
    {
        InverseParentType = new HashSet<WebAnalyticsType>();
    }

    public string WebAnalyticsTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public WebAnalyticsType? ParentType { get; set; }
    public ICollection<WebAnalyticsType> InverseParentType { get; set; }
}