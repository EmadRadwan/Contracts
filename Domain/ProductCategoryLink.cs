namespace Domain;

public class ProductCategoryLink
{
    public string ProductCategoryId { get; set; } = null!;
    public string LinkSeqId { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public string? Comments { get; set; }
    public int? SequenceNum { get; set; }
    public string? TitleText { get; set; }
    public string? DetailText { get; set; }
    public string? ImageUrl { get; set; }
    public string? ImageTwoUrl { get; set; }
    public string? LinkTypeEnumId { get; set; }
    public string? LinkInfo { get; set; }
    public string? DetailSubScreen { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Enumeration? LinkTypeEnum { get; set; }
    public ProductCategory ProductCategory { get; set; } = null!;
}