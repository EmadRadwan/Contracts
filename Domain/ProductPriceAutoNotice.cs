namespace Domain;

public class ProductPriceAutoNotice
{
    public string ProductPriceNoticeId { get; set; } = null!;
    public string? FacilityId { get; set; }
    public DateTime? RunDate { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }
}