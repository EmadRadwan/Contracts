namespace Domain;

public class ProductStoreKeywordOvrd
{
    public string ProductStoreId { get; set; } = null!;
    public string Keyword { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public string? Target { get; set; }
    public string? TargetTypeEnumId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ProductStore ProductStore { get; set; } = null!;
    public Enumeration? TargetTypeEnum { get; set; }
}