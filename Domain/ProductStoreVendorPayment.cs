namespace Domain;

public class ProductStoreVendorPayment
{
    public string ProductStoreId { get; set; } = null!;
    public string VendorPartyId { get; set; } = null!;
    public string PaymentMethodTypeId { get; set; } = null!;
    public string CreditCardEnumId { get; set; } = null!;
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Enumeration CreditCardEnum { get; set; } = null!;
    public PaymentMethodType PaymentMethodType { get; set; } = null!;
    public ProductStore ProductStore { get; set; } = null!;
    public Party VendorParty { get; set; } = null!;
}