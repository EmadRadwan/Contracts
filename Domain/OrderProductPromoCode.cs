namespace Domain;

public class OrderProductPromoCode
{
    public string OrderId { get; set; } = null!;
    public string ProductPromoCodeId { get; set; } = null!;
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public OrderHeader Order { get; set; } = null!;
    public ProductPromoCode ProductPromoCode { get; set; } = null!;
}