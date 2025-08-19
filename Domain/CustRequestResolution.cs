namespace Domain;

public class CustRequestResolution
{
    public CustRequestResolution()
    {
        CustRequestItems = new HashSet<CustRequestItem>();
    }

    public string CustRequestResolutionId { get; set; } = null!;
    public string? CustRequestTypeId { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public CustRequestType? CustRequestType { get; set; }
    public ICollection<CustRequestItem> CustRequestItems { get; set; }
}