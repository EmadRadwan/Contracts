using System.ComponentModel.DataAnnotations;

namespace Application.Catalog.Products;

public class ProductRecord
{
    [Key] public string? ProductId { get; set; }

    public string? GoodIdentificationId { get; set; }
    public string? ProductIdentificationDescription { get; set; }
    public string? ProductTypeId { get; set; }
    public string? Comments { get; set; }
    public string? ProductName { get; set; }
    public string? PrimaryProductCategoryId { get; set; }
    public string? Description { get; set; }
    public string? IsVirtual { get; set; }
    public string? IsVariant { get; set; }
    public DateTime? IntroductionDate { get; set; }
    public string? OriginalImageUrl { get; set; }
    public string? QuantityUomId { get; set; }
    public string? QuantityUomDescription { get; set; }
    public string? ProductTypeDescription { get; set; }
    public string ProductColorId { get; set; }
    public string ProductColorDescription { get; set; }
    public string ProductSizeId { get; set; }
    public string ProductSizeDescription { get; set; }
    public string ProductTrademarkId { get; set; }
    public string ProductTrademarkDescription { get; set; }
    public decimal? QuantityIncluded { get; set; }
    public decimal? PiecesIncluded { get; set; }
    public string? PrimaryProductCategoryDescription { get; set; }

    public bool? GoodIdentificationsExist { get; set; }

    public bool? ProductPricesExist { get; set; }

    public bool? QuoteItemsExist { get; set; }

    public bool? OrderItemsExist { get; set; }

    public bool? ProductAssocProductsExist { get; set; }

    public bool? ProductFacilitiesExist { get; set; }
}