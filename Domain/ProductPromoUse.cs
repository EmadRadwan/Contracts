namespace Domain;

public class ProductPromoUse
{
    public string OrderId { get; set; } = null!;
    public string PromoSequenceId { get; set; } = null!;
    public string? ProductPromoId { get; set; }
    public string? ProductPromoCodeId { get; set; }
    public string? PartyId { get; set; }
    public decimal? TotalDiscountAmount { get; set; }
    public decimal? QuantityLeftInActions { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public OrderHeader Order { get; set; } = null!;
    public Party? Party { get; set; }
    public ProductPromo? ProductPromo { get; set; }
    public ProductPromoCode? ProductPromoCode { get; set; }
}