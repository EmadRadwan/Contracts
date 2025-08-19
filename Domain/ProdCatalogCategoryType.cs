namespace Domain;

public class ProdCatalogCategoryType
{
    public ProdCatalogCategoryType()
    {
        InverseParentType = new HashSet<ProdCatalogCategoryType>();
        ProdCatalogCategories = new HashSet<ProdCatalogCategory>();
    }

    public string ProdCatalogCategoryTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ProdCatalogCategoryType? ParentType { get; set; }
    public ICollection<ProdCatalogCategoryType> InverseParentType { get; set; }
    public ICollection<ProdCatalogCategory> ProdCatalogCategories { get; set; }
}