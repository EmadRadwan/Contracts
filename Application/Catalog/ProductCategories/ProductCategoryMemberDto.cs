namespace Application.ProductCategories;

public class ProductCategoryMemberDto
{
    public string ProductCategoryId { get; set; }
    public string Description { get; set; }
    public string ProductId { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public string? Comments { get; set; }
}