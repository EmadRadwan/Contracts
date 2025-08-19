using Application.Catalog.Products;

namespace Application.Order.Orders;

public class OrderItemDto2
{
    public string? OrderId { get; set; }
    public ProductLovDto? OrderItemProduct { get; set; }
    public string? OrderItemSeqId { get; set; }
    public string? ParentOrderItemSeqId { get; set; }
    public string? ShipGroupSeqId { get; set; }
    public string? ReserveOrderEnumId { get; set; }
    public string? ProductId { get; set; }
    public string? FacilityId { get; set; }
    public string? SupplierId { get; set; }
    public string? ProductTypeId { get; set; }
    public string? ProductCategoryId { get; set; }
    public string? ProductPromoId { get; set; }
    public string? PromoActionEnumId { get; set; }

    public string? ProductName { get; set; }
    public bool IsProductDeleted { get; set; }
    public string? ItemDescription { get; set; }

    public decimal? Quantity { get; set; }
    public decimal? DefaultQuantityToReceive { get; set; }
    public decimal? UnitPrice { get; set; }
    public decimal? LastPrice { get; set; }
    public decimal? SubTotal { get; set; }
    public decimal? UnitListPrice { get; set; }
    public decimal? TotalItemTaxAdjustments { get; set; }
    public decimal? DiscountAndPromotionAdjustments { get; set; }
    public decimal? QuantityAccepted { get; set; }
    public decimal? QuantityRejected { get; set; }
    public string? RejectionReasonId { get; set; }
    public string? IsPromo { get; set; }
    public string? ProductFeatureId { get; set; }
    public string? OrderItemTypeId { get; set; }
    public string? StatusId { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public bool? IncludeThisItem { get; set; }
    public bool? CollectTax { get; set; }
    public bool? ValidItem { get; set; }
    public bool IsBackOrdered { get; set; }
}