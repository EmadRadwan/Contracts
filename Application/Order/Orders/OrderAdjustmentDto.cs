using Application.Catalog.Products;

namespace Application.Order.Orders;

public class OrderAdjustmentDto
{
    public string OrderAdjustmentId { get; set; }
    public string OrderAdjustmentTypeId { get; set; }
    public string OrderAdjustmentTypeDescription { get; set; }
    public string OrderId { get; set; }
    public string? OrderItemSeqId { get; set; }
    public string? ProductPromoId { get; set; }

    public string? Comments { get; set; }
    public string? Description { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public string? IsManual { get; set; }
    public DateTime? CreatedDate { get; set; }
    public string? TaxAuthorityRateSeqId { get; set; }
    public string? TaxAuthGeoId { get; set; }
    public string? TaxAuthPartyId { get; set; }


    public decimal? Amount { get; set; }
    public ProductLovDto? CorrespondingProductId { get; set; }
    public string? CorrespondingProductName { get; set; }
    public decimal? SourcePercentage { get; set; }
    public bool IsAdjustmentDeleted { get; set; }
}