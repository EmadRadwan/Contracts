public class ReturnItemOrAdjustmentContext
{
    // Shared properties
    public string ReturnId { get; set; } // The ID of the return
    public string ReturnTypeId { get; set; } // The return type (e.g., RTN_REFUND)
    public string? UserLoginPartyId { get; set; } // The ID of the user creating the return

    // Properties for items
    public string? ReturnItemMapKey { get; set; } // The type of the return item (e.g., RET_FPROD_ITEM)
    public string? ReturnItemTypeId { get; set; } // The type of the return item (e.g., RET_FPROD_ITEM)
    public string OrderId { get; set; } // The order ID related to the item
    public string OrderItemSeqId { get; set; } // The sequence ID of the item in the order
    public string? ProductId { get; set; } // The product ID
    public string? ItemDescription { get; set; } // The description of the item
    public decimal? ReturnQuantity { get; set; } // The quantity to return
    public decimal? ReturnPrice { get; set; } // The price of the returned item
    public string? ReturnReasonId { get; set; } // The reason for the return
    public string? ItemStatus { get; set; } // The expected status of the item after return
    public string IncludeAdjustments { get; set; } // Indicates whether adjustments should be included (Y/N)

    // Properties for adjustments
    public string? ReturnAdjustmentTypeId { get; set; } // The type of the adjustment (e.g., RET_PROMOTION_ADJ)
    public string? OrderAdjustmentId { get; set; } // The ID of the adjustment
    public string? Description { get; set; } // The description of the adjustment
    public decimal? Amount { get; set; } // The amount of the adjustment
}