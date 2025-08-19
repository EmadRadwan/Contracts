namespace Domain;

public class ProductStoreCatalog
{
    public string ProductStoreId { get; set; } = null!;
    public string ProdCatalogId { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public int? SequenceNum { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ProdCatalog ProdCatalog { get; set; } = null!;
    public ProductStore ProductStore { get; set; } = null!;
}