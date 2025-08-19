namespace Domain;

public class ProdConfItemContent
{
    public string ConfigItemId { get; set; } = null!;
    public string ContentId { get; set; } = null!;
    public string ConfItemContentTypeId { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ProdConfItemContentType ConfItemContentType { get; set; } = null!;
    public ProductConfigItem ConfigItem { get; set; } = null!;
    public Content Content { get; set; } = null!;
}