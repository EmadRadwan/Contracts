namespace Application.Order.CustomerRequests;

public class CustRequestDto2
{
    public string CustRequestId { get; set; }
    public string CustRequestTypeId { get; set; }
    public string CustRequestTypeDescription { get; set; }
    public string StatusId { get; set; }
    public string StatusDescription { get; set; }
    public CustRequestPartyDto FromPartyId { get; set; }
    public bool AllowSubmit { get; set; }

    public DateTime? CustRequestDate { get; set; }
    public string? ProductStoreId { get; set; }
    public string? ProductStoreName { get; set; }
    public string? SalesChannelEnumId { get; set; }
    public string? SalesChannelEnumDescription { get; set; }
    public string? CurrencyUomId { get; set; }
    public string? CurrencyUomDescription { get; set; }
    public DateTime? OpenDateTime { get; set; }
    public DateTime? ClosedDateTime { get; set; }
    public string? InternalComment { get; set; }
    public DateTime? CreatedDate { get; set; }
    public string? CreatedByUserLogin { get; set; }
    public string? CreatedByUserLoginName { get; set; }
    public string? LastModifiedByUserLogin { get; set; }
    public string? LastModifiedByUserLoginName { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public string? Billed { get; set; }
    public ICollection<CustRequestItemDto> CustRequestItems { get; set; }
}