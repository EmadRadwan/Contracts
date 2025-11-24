using System.ComponentModel.DataAnnotations;

namespace Application.Catalog.Products;

public class ProductRecord
{
    [Key] public string? ProductId { get; set; }

    public string? ProductTypeId { get; set; }
    public string? Comments { get; set; }
    public string? ProductName { get; set; }
    public string? PrimaryProductCategoryId { get; set; }
    public string? Description { get; set; }
    public string? OriginalImageUrl { get; set; }
    public string? ProductTypeDescription { get; set; }
    public string? PrimaryProductCategoryDescription { get; set; }
    public string? ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public string? FloorNumber { get; set; }
    public decimal? ApartmentSpaceM2 { get; set; }
    public decimal? GardenSpaceM2 { get; set; }
    public decimal? ApartmentPricePerM2 { get; set; }
    public decimal? GardenPricePerM2 { get; set; }
    public string? ApartmentStatusId { get; set; }
    public string? BuildingNumber { get; set; }
}