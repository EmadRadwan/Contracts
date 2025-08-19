namespace Domain;

public class ProductPromoCodeParty
{
    public string ProductPromoCodeId { get; set; } = null!;
    public string PartyId { get; set; } = null!;
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Party Party { get; set; } = null!;
    public ProductPromoCode ProductPromoCode { get; set; } = null!;
}