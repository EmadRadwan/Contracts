namespace Domain;

public class MarketingCampaignNote
{
    public string MarketingCampaignId { get; set; } = null!;
    public string NoteId { get; set; } = null!;
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public MarketingCampaign MarketingCampaign { get; set; } = null!;
    public NoteDatum Note { get; set; } = null!;
}