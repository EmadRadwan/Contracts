namespace Application.Order.Orders.Returns;

public class ReturnItemDto
{
    public string ReturnId { get; set; } = null!;
    public string ReturnItemSeqId { get; set; } = null!;
    public string? ReturnReasonId { get; set; }
    public string? ReturnTypeId { get; set; }
    public string? ReturnItemTypeId { get; set; }
    public string? ProductId { get; set; }
    public string? Description { get; set; }
    public string? OrderId { get; set; }
    public string? OrderItemSeqId { get; set; }
    public string? StatusId { get; set; }
    public string? ExpectedItemStatus { get; set; }
    public decimal? ReturnQuantity { get; set; }
    public decimal? ReceivedQuantity { get; set; }
    public decimal? ReturnPrice { get; set; }
    public string? ReturnItemResponseId { get; set; }
}