namespace Domain;

public class ProdCatalogCategory
{
    public string ProdCatalogId { get; set; } = null!;
    public string ProductCategoryId { get; set; } = null!;
    public string ProdCatalogCategoryTypeId { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public int? SequenceNum { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ProdCatalog ProdCatalog { get; set; } = null!;
    public ProdCatalogCategoryType ProdCatalogCategoryType { get; set; } = null!;
    public ProductCategory ProductCategory { get; set; } = null!;
}