namespace Domain;

public class ProductPriceRule
{
    public ProductPriceRule()
    {
        MarketingCampaignPrices = new HashSet<MarketingCampaignPrice>();
        ProductPriceActions = new HashSet<ProductPriceAction>();
        ProductPriceConds = new HashSet<ProductPriceCond>();
    }

    public string ProductPriceRuleId { get; set; } = null!;
    public string? RuleName { get; set; }
    public string? Description { get; set; }
    public string? IsSale { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<MarketingCampaignPrice> MarketingCampaignPrices { get; set; }
    public ICollection<ProductPriceAction> ProductPriceActions { get; set; }
    public ICollection<ProductPriceCond> ProductPriceConds { get; set; }
}