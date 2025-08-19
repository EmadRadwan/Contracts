namespace Domain;

public class ProductPaymentMethodType
{
    public string ProductId { get; set; } = null!;
    public string PaymentMethodTypeId { get; set; } = null!;
    public string ProductPricePurposeId { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public int? SequenceNum { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public PaymentMethodType PaymentMethodType { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public ProductPricePurpose ProductPricePurpose { get; set; } = null!;
}