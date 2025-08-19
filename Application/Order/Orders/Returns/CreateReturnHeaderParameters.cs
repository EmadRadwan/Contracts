namespace Application.Order.Orders.Returns;

public class CreateReturnHeaderParameters
{
    public string ReturnHeaderTypeId { get; set; }
    public string? ToPartyId { get; set; }
    public string FromPartyId { get; set; }
    public string DestinationFacilityId { get; set; }
    public string CurrencyUomId { get; set; }
    public string? PaymentMethodId { get; set; }
    public string? NeedsInventoryReceive { get; set; }
}
