using System.ComponentModel.DataAnnotations;
using Application.Order.Orders;
using Application.Services;

namespace Application.Order.Quotes;

public class QuoteRecord
{
    [Key] public string QuoteId { get; set; }

    public string QuoteTypeId { get; set; }

    public decimal? GrandTotal { get; set; }

    public string CustomerRemarks { get; set; }
    public string InternalRemarks { get; set; }
    public OrderPartyDto FromPartyId { get; set; }
    public string FromPartyName { get; set; }
    public string StatusDescription { get; set; }
    public string AgreementId { get; set; }
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
}