namespace Domain;

public class ProdPromoCodeContactMech
{
    public string ProductPromoCodeId { get; set; } = null!;
    public string ContactMechId { get; set; } = null!;
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ContactMech ContactMech { get; set; } = null!;
    public ProductPromoCode ProductPromoCode { get; set; } = null!;
}