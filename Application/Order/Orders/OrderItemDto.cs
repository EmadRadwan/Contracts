using Application.Catalog.Products;

namespace Application.Order.Orders;

public class OrderItemDto
{
    public string OrderId { get; set; }
    public string OrderItemSeqId { get; set; }
    public ProductLovDto ProductId { get; set; }
    public string? ProductTypeId { get; set; }
    public string? IsPromo { get; set; }

    public string ProductName { get; set; }
    public bool IsProductDeleted { get; set; }

    public decimal? TotalAdjustments { get; set; }
    public string? QuoteId { get; set; }
    public string? QuoteItemSeqId { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
    public decimal? UnitListPrice { get; set; }
}