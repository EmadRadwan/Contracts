using Application.Order.Orders;
using Application.Order.Quotes;

namespace Application.Services;

public class JobQuoteDto2
{
    public string QuoteId { get; set; }
    public string QuoteTypeId { get; set; }
    public string VehicleJobQuoteId { get; set; }
    public VehicleLovDto VehicleId { get; set; }
    public int? CurrentMileage { get; set; }

    public decimal? GrandTotal { get; set; }

    public string ChassisNumber { get; set; }
    public string CustomerRemarks { get; set; }
    public string InternalRemarks { get; set; }
    public OrderPartyDto FromPartyId { get; set; }
    public string FromPartyName { get; set; }
    public string StatusDescription { get; set; }

    public DateTime IssueDate { get; set; }
    public string StatusId { get; set; }
    public string CurrencyUomId { get; set; }
    public string ProductStoreId { get; set; }
    public string SalesChannelEnumId { get; set; }
    public DateTime ValidFromDate { get; set; }
    public decimal? TotalAdjustments { get; set; }

    public DateTime ValidThruDate { get; set; }
    public string QuoteName { get; set; }
    public string Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public string CustRequestId { get; set; }
    public bool AllowSubmit { get; set; }
    public ICollection<QuoteItemDto> QuoteItems { get; set; }
    public ICollection<QuoteAdjustmentDto> QuoteAdjustments { get; set; }
}