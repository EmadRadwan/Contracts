namespace Domain;

public class CustRequestCategory
{
    public CustRequestCategory()
    {
        CustRequests = new HashSet<CustRequest>();
    }

    public string CustRequestCategoryId { get; set; } = null!;
    public string? CustRequestTypeId { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public CustRequestType? CustRequestType { get; set; }
    public ICollection<CustRequest> CustRequests { get; set; }
}