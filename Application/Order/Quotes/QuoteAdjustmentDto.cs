using Application.Catalog.Products;

namespace Application.Order.Quotes;

public class QuoteAdjustmentDto
{
    public string QuoteAdjustmentId { get; set; }
    public string QuoteAdjustmentTypeId { get; set; }
    public string QuoteAdjustmentTypeDescription { get; set; }
    public string QuoteId { get; set; }
    public string? QuoteItemSeqId { get; set; }
    public string? Comments { get; set; }
    public string? Description { get; set; }
    public decimal? Amount { get; set; }
    public ProductLovDto? CorrespondingProductId { get; set; }
    public string? CorrespondingProductName { get; set; }
    public decimal? SourcePercentage { get; set; }
    public bool IsAdjustmentDeleted { get; set; }
}