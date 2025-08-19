namespace Application.Order.Orders.Returns;

public class CreateReturnAdjustmentContext
{
    public string ReturnId { get; set; }
    public string ReturnItemSeqId { get; set; } // "_NA_" for adjustments not tied to a return item
    public string ReturnAdjustmentTypeId { get; set; }
    public string OrderAdjustmentId { get; set; }
    public string Description { get; set; }
    public decimal? Amount { get; set; }

    // Additional fields for recalculations or mapping
    public string TaxAuthorityRateSeqId { get; set; } // Pulled from OrderAdjustment if applicable
}
