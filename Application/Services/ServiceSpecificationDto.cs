using Application.Catalog.Products;

namespace Application.Services;

public class ServiceSpecificationDto
{
    public string? ServiceSpecificationId { get; set; }

    public ProductServiceLovDto? ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? MakeId { get; set; }
    public string? MakeDescription { get; set; }
    public string? ModelId { get; set; }
    public string? ModelDescription { get; set; }
    public int StandardTimeInMinutes { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
}