using Application.Catalog.Products;

namespace Application.Order.Orders;

public class ReceiveInventoryItemDto
{
    public string? OrderId { get; set; }
    public string? OrderItemSeqId { get; set; }
    public string? ProductId { get; set; }
    public string? FacilityId { get; set; }
    public string? Color { get; set; }

    public decimal? Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
    public decimal? QuantityAccepted { get; set; }
    public decimal? QuantityRejected { get; set; }
    public string? RejectionReasonId { get; set; }
}