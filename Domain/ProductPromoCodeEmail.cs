namespace Domain;

public class ProductPromoCodeEmail
{
    public string ProductPromoCodeId { get; set; } = null!;
    public string EmailAddress { get; set; } = null!;
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ProductPromoCode ProductPromoCode { get; set; } = null!;
}