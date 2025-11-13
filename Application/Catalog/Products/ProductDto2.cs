namespace Application.Catalog.Products;

public class ProductDto2
{
    // Core identifiers
    public string? ProductId { get; set; }
    public string? ModelProductId { get; set; }                     // REFACTOR: Added for variant support
    public string? GoodIdentificationId { get; set; }
    public string? ProductIdentificationDescription { get; set; }

    // Product metadata
    public string? ProductTypeId { get; set; }
    public string? ProductName { get; set; }
    public string? Comments { get; set; }
    public string? IsVirtual { get; set; }                         // "Y" / "N"
    public string? IsVariant { get; set; }                         // "Y" / "N"

    // Category & UOM
    public string? PrimaryProductCategoryId { get; set; }
    public string? QuantityUomId { get; set; }
    public string? QuantityUomDescription { get; set; }

    // Feature IDs (color, size, trademark)
    public string? ProductColorId { get; set; }
    public string? ProductSizeId { get; set; }
    public string? ProductTrademarkId { get; set; }

    // Feature descriptions (joined from lookup tables)
    public string? ProductColorDescription { get; set; }
    public string? ProductTrademarkDescription { get; set; }

    // Descriptions from joins
    public string? Description { get; set; }
    public string? ProductTypeDescription { get; set; }
    public string? PrimaryProductCategoryDescription { get; set; }

    // Dates
    public DateTime? IntroductionDate { get; set; }

    // Media
    public string? OriginalImageUrl { get; set; }

    // Quantities
    public decimal? QuantityIncluded { get; set; }
    public decimal? PiecesIncluded { get; set; }

    // Existence flags (for UI indicators)
    public bool? GoodIdentificationsExist { get; set; }
    public bool? ProductPricesExist { get; set; }
    public bool? QuoteItemsExist { get; set; }
    public bool? OrderItemsExist { get; set; }
    public bool? ProductAssocProductsExist { get; set; }
    public bool? ProductFacilitiesExist { get; set; }

    // REFACTOR: Added Apartment-specific fields to support APARTMENT product type
    // Purpose: Enable full creation/editing of apartment products in the form
    // Why: These fields are now submitted by ProductForm and persisted via CreateProduct handler
    public string? ProjectId { get; set; }
    public string? FloorNumber { get; set; }
    public decimal? ApartmentSpaceM2 { get; set; }
    public decimal? GardenSpaceM2 { get; set; }
    public decimal? ApartmentPricePerM2 { get; set; }
    public decimal? GardenPricePerM2 { get; set; }
    public string? ApartmentStatusId { get; set; }
    public string? LandNumber { get; set; }
}