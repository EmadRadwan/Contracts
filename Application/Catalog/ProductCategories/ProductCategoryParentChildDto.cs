namespace Application.Catalog.ProductCategories;

public class ProductCategoryParentChildDto
{
    public string? ParentProductCategoryId { get; set; }
    public string? ProductCategoryId { get; set; }

    public string? Description { get; set; }
    public string? Text { get; set; }
    public List<ProductCategoryParentChildDto>? Items { get; set; }
}

//SPARE_PARTS
//VEHICLE_SERVICES