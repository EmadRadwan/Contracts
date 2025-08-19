namespace Domain;

public class CustRequestItem
{
    public CustRequestItem()
    {
        CustRequestItemNotes = new HashSet<CustRequestItemNote>();
        CustRequestItemWorkEfforts = new HashSet<CustRequestItemWorkEffort>();
        QuoteItems = new HashSet<QuoteItem>();
        RequirementCustRequests = new HashSet<RequirementCustRequest>();
    }

    public string CustRequestId { get; set; } = null!;
    public string CustRequestItemSeqId { get; set; } = null!;
    public string? CustRequestResolutionId { get; set; }
    public string? StatusId { get; set; }
    public int? Priority { get; set; }
    public int? SequenceNum { get; set; }
    public DateTime? RequiredByDate { get; set; }
    public string? ProductId { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? SelectedAmount { get; set; }
    public decimal? MaximumAmount { get; set; }
    public DateTime? ReservStart { get; set; }
    public decimal? ReservLength { get; set; }
    public decimal? ReservPersons { get; set; }
    public string? ConfigId { get; set; }
    public string? Description { get; set; }
    public string? Story { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public CustRequest CustRequest { get; set; } = null!;
    public CustRequestResolution? CustRequestResolution { get; set; }
    public Product? Product { get; set; }
    public StatusItem? Status { get; set; }
    public ICollection<CustRequestItemNote> CustRequestItemNotes { get; set; }
    public ICollection<CustRequestItemWorkEffort> CustRequestItemWorkEfforts { get; set; }
    public ICollection<QuoteItem> QuoteItems { get; set; }
    public ICollection<RequirementCustRequest> RequirementCustRequests { get; set; }
}