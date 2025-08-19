namespace Application.Order.Orders;

public class CommonItemDto
{
    public string OrderId { get; set; }
    public string OrderItemSeqId { get; set; }
    public string InventoryItemId { get; set; }
    public string ProductId { get; set; }
    public string? ProductCategoryId { get; set; }
    public string? ProductPromoId { get; set; }
    public string? ProductPromoRuleId { get; set; }
    public string? ProductPromoActionSeqId { get; set; }

    public string ResultMessage { get; set; }
    public string ProductName { get; set; }
    public bool IsProductDeleted { get; set; }
    public string? ItemDescription { get; set; }

    public decimal? Quantity { get; set; }
    public decimal? PromoAmount { get; set; }
    public decimal? PromoActionAmount { get; set; }
    public string PromoText { get; set; }

    public decimal? UnitPrice { get; set; }
    public decimal? LastPrice { get; set; }
    public decimal? SubTotal { get; set; }
    public decimal? ListPrice { get; set; }
    public decimal? DefaultPrice { get; set; }
    public decimal? QuantityAccepted { get; set; }
    public decimal? QuantityRejected { get; set; }
    public string? RejectionReasonId { get; set; }
    public string? IsPromo { get; set; }
    public string? OrderItemTypeId { get; set; }
    public string? StatusId { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public bool? IncludeThisItem { get; set; }
}