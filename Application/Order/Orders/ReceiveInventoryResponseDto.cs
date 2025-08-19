namespace Application.Order.Orders;

public class ReceiveInventoryResponseDto
    {
        /// <summary>
        /// Indicates whether the receive operation is partial.
        /// </summary>
        public bool PartialReceive { get; set; }

        /// <summary>
        /// List of Supplier Party IDs associated with the products in the order.
        /// </summary>
        public List<string> SupplierPartyIds { get; set; }

        /// <summary>
        /// List of Shipments related to the purchase order and facility.
        /// </summary>
        public List<ShipmentDto> Shipments { get; set; }

        /// <summary>
        /// Specific Shipment details if a shipmentId is provided.
        /// </summary>
        public ShipmentDto Shipment { get; set; }

        /// <summary>
        /// Dictionary mapping OrderItemSeqId to shipped quantities.
        /// </summary>
        public Dictionary<string, double> ShippedQuantities { get; set; }

        /// <summary>
        /// List of Purchase Order Items with detailed information.
        /// </summary>
        public List<OrderItemDto2> PurchaseOrderItems { get; set; }

        /// <summary>
        /// Dictionary mapping OrderItemSeqId to original and converted unit prices.
        /// </summary>
        public Dictionary<string, decimal> OrderCurrencyUnitPriceMap { get; set; }

        /// <summary>
        /// Dictionary mapping OrderItemSeqId to received quantities.
        /// </summary>
        public Dictionary<string, double> ReceivedQuantities { get; set; }

        /// <summary>
        /// Dictionary mapping OrderItemSeqId to associated Sales Order Items.
        /// </summary>
        public Dictionary<string, SalesOrderItemAssocDto> SalesOrderItems { get; set; }

        /// <summary>
        /// List of Received Items combining ShipmentReceipts and InventoryItems.
        /// </summary>
        public List<ShipmentReceiptAndItemDto> ReceivedItems { get; set; }

        /// <summary>
        /// Indicates if a provided ProductId is invalid.
        /// </summary>
        public string InvalidProductId { get; set; }

        /*/// <summary>
        /// List of available Rejection Reasons.
        /// </summary>
        public List<RejectionReason> RejectReasons { get; set; }

        /// <summary>
        /// List of Inventory Item Types.
        /// </summary>
        public List<InventoryItemType> InventoryItemTypes { get; set; }

        /// <summary>
        /// List of all Facilities.
        /// </summary>
        public List<Facility> Facilities { get; set; }
        */

        /// <summary>
        /// Dictionary mapping ProductId to their standard costs.
        /// </summary>
        public Dictionary<string, decimal> StandardCosts { get; set; }
    }