namespace Application.Order.Quotes;

public class QuoteDto
{
    public string? VehicleId { get; set; }
    public string? ChassisNumber { get; set; }

    public string? CustomerRemarks { get; set; }
    public string? InternalRemarks { get; set; }
    public string? QuoteId { get; set; }
    public string? QuoteTypeId { get; set; }
    public string? FromPartyId { get; set; }
    public string? FromPartyName { get; set; }
    public DateTime? IssueDate { get; set; }
    public string? StatusId { get; set; }
    public string? CurrencyUomId { get; set; }
    public string? AgreementId { get; set; }
    public string? ProductStoreId { get; set; }
    public string? SalesChannelEnumId { get; set; }
    public DateTime? ValidFromDate { get; set; }
    public DateTime? ValidThruDate { get; set; }
    public string? QuoteName { get; set; }
    public string? Description { get; set; }
    public string? StatusDescription { get; set; }
    public decimal? GrandTotal { get; set; }
    public string? ModificationType { get; set; }

    public int? CurrentMileage { get; set; }

    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public List<QuoteItemDto2>? QuoteItems { get; set; }
    public List<QuoteAdjustmentDto2>? QuoteAdjustments { get; set; }
}