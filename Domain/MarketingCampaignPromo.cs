namespace Domain;

public class MarketingCampaignPromo
{
    public string MarketingCampaignId { get; set; } = null!;
    public string ProductPromoId { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public MarketingCampaign MarketingCampaign { get; set; } = null!;
    public ProductPromo ProductPromo { get; set; } = null!;
}