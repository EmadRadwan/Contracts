namespace Application.Order.Orders.Returns;

public class ReturnDto
{
    public string? ReturnId { get; set; } = null!;
    public string? ReturnHeaderTypeId { get; set; }
    public string? ReturnHeaderTypeDescription { get; set; }
    public string? StatusId { get; set; }
    public string? StatusDescription { get; set; }
    public string? CreatedBy { get; set; }
    public string? FromPartyId { get; set; }
    public string? FromPartyName { get; set; }
    public string? ToPartyId { get; set; }
    public string? ToPartyName { get; set; }
    public string? PaymentMethodId { get; set; }
    public string? FinAccountId { get; set; }
    public string? BillingAccountId { get; set; }
    public DateTime? EntryDate { get; set; }
    public string? OriginContactMechId { get; set; }
    public string? DestinationFacilityId { get; set; }
    public string? NeedsInventoryReceive { get; set; }
    public string? CurrencyUomId { get; set; }
    public string? SupplierRmaId { get; set; }
    public List<ReturnItemDto>? OrderItems { get; set; }
}