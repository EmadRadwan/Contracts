namespace Application.Order.Orders.Returns
{
    public class ReturnableItemInfo
    {
        // Shared Properties
        public string OrderId { get; set; } // The ID of the order
        public string ItemTypeKey { get; set; } 
        public string ItemType { get; set; } // Either "OrderItem" or "OrderAdjustment"
        public decimal ReturnableQuantity { get; set; } // Quantity that can be returned
        public decimal? OrderedQuantity { get; set; } // Quantity that can be returned
        public decimal ReturnablePrice { get; set; } // Price that can be returned

        // OrderItem-Specific Properties
        public string? OrderItemSeqId { get; set; } // Sequence ID of the order item
        public string? ReturnItemSeqId { get; set; } // Sequence ID of the order item
        public string? ProductId { get; set; } // ID of the product
        public string? ItemDescription { get; set; } // Description of the item
        public decimal? UnitPrice { get; set; } // Unit price of the item
        public string? StatusId { get; set; } // Status of the item

        // OrderAdjustment-Specific Properties
        public string? OrderAdjustmentId { get; set; } // ID of the order adjustment
        public string? ReturnAdjustmentTypeId { get; set; } // Type of the return adjustment
        public string? Description { get; set; } // Description of the adjustment
        public decimal? Amount { get; set; } // Amount of the adjustment
    }
}