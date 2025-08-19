namespace Domain;

public class CustRequest
{
    public CustRequest()
    {
        CustRequestAttributes = new HashSet<CustRequestAttribute>();
        CustRequestCommEvents = new HashSet<CustRequestCommEvent>();
        CustRequestContents = new HashSet<CustRequestContent>();
        CustRequestItems = new HashSet<CustRequestItem>();
        CustRequestNotes = new HashSet<CustRequestNote>();
        CustRequestParties = new HashSet<CustRequestParty>();
        CustRequestStatuses = new HashSet<CustRequestStatus>();
        CustRequestWorkEfforts = new HashSet<CustRequestWorkEffort>();
        QuoteItems = new HashSet<QuoteItem>();
        RespondingParties = new HashSet<RespondingParty>();
    }

    public string CustRequestId { get; set; } = null!;
    public string? CustRequestTypeId { get; set; }
    public string? CustRequestCategoryId { get; set; }
    public string? StatusId { get; set; }
    public string? FromPartyId { get; set; }
    public int? Priority { get; set; }
    public DateTime CustRequestDate { get; set; }
    public DateTime? ResponseRequiredDate { get; set; }
    public string? CustRequestName { get; set; }
    public string? Description { get; set; }
    public string? MaximumAmountUomId { get; set; }
    public string? ProductStoreId { get; set; }
    public string? SalesChannelEnumId { get; set; }
    public string? FulfillContactMechId { get; set; }
    public string? CurrencyUomId { get; set; }
    public DateTime? OpenDateTime { get; set; }
    public DateTime? ClosedDateTime { get; set; }
    public string? InternalComment { get; set; }
    public string? Reason { get; set; }
    public DateTime? CreatedDate { get; set; }
    public string? CreatedByUserLogin { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public string? LastModifiedByUserLogin { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Uom? CurrencyUom { get; set; }
    public CustRequestCategory? CustRequestCategory { get; set; }
    public CustRequestType? CustRequestType { get; set; }
    public Party? FromParty { get; set; }
    public ContactMech? FulfillContactMech { get; set; }
    public Uom? MaximumAmountUom { get; set; }
    public ProductStore? ProductStore { get; set; }
    public Enumeration? SalesChannelEnum { get; set; }
    public StatusItem? Status { get; set; }
    public ICollection<CustRequestAttribute> CustRequestAttributes { get; set; }
    public ICollection<CustRequestCommEvent> CustRequestCommEvents { get; set; }
    public ICollection<CustRequestContent> CustRequestContents { get; set; }
    public ICollection<CustRequestItem> CustRequestItems { get; set; }
    public ICollection<CustRequestNote> CustRequestNotes { get; set; }
    public ICollection<CustRequestParty> CustRequestParties { get; set; }
    public ICollection<CustRequestStatus> CustRequestStatuses { get; set; }
    public ICollection<CustRequestWorkEffort> CustRequestWorkEfforts { get; set; }
    public ICollection<QuoteItem> QuoteItems { get; set; }
    public ICollection<RespondingParty> RespondingParties { get; set; }
}