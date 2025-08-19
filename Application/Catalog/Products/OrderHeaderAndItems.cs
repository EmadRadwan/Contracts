namespace Application.Catalog.Products;

public class OrderHeaderAndItems
{
    public string OrderId { get; set; }
    public string OrderItemSeqId { get; set; }
    public string OrderStatusId { get; set; }
    public string ItemStatusId { get; set; }
    public string OrderTypeId { get; set; }
    public string ProductId { get; set; }
    public DateTime EstimatedDeliveryDate { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal Quantity { get; set; }
    public decimal? CancelQuantity { get; set; }
}