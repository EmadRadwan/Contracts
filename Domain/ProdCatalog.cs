namespace Domain;

public class ProdCatalog
{
    public ProdCatalog()
    {
        CartAbandonedLines = new HashSet<CartAbandonedLine>();
        ProdCatalogCategories = new HashSet<ProdCatalogCategory>();
        ProdCatalogInvFacilities = new HashSet<ProdCatalogInvFacility>();
        ProdCatalogRoles = new HashSet<ProdCatalogRole>();
        ProductStoreCatalogs = new HashSet<ProductStoreCatalog>();
    }

    public string ProdCatalogId { get; set; } = null!;
    public string? CatalogName { get; set; }
    public string? UseQuickAdd { get; set; }
    public string? StyleSheet { get; set; }
    public string? HeaderLogo { get; set; }
    public string? ContentPathPrefix { get; set; }
    public string? TemplatePathPrefix { get; set; }
    public string? ViewAllowPermReqd { get; set; }
    public string? PurchaseAllowPermReqd { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<CartAbandonedLine> CartAbandonedLines { get; set; }
    public ICollection<ProdCatalogCategory> ProdCatalogCategories { get; set; }
    public ICollection<ProdCatalogInvFacility> ProdCatalogInvFacilities { get; set; }
    public ICollection<ProdCatalogRole> ProdCatalogRoles { get; set; }
    public ICollection<ProductStoreCatalog> ProductStoreCatalogs { get; set; }
}