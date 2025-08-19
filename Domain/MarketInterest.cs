namespace Domain;

public class MarketInterest
{
    public string ProductCategoryId { get; set; } = null!;
    public string PartyClassificationGroupId { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public PartyClassificationGroup PartyClassificationGroup { get; set; } = null!;
    public ProductCategory ProductCategory { get; set; } = null!;
}