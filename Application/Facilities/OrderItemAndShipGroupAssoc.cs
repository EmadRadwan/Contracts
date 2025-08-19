namespace Application.Facilities
{
    public class OrderItemAndShipGroupAssoc
    {
        // Fields from OrderItem (alias OI)
        public string OrderId { get; set; }
        public string OrderItemSeqId { get; set; }
        public string ProductId { get; set; }
        public string SupplierProductId { get; set; }
        public string ProductFeatureId { get; set; }
        public string ItemDescription { get; set; }
        public string StatusId { get; set; }
        public string ItemTypeEnumId { get; set; }
        public string OverrideGlAccountId { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal UnitRecurringPrice { get; set; }
        public DateTime? EstimatedShipDate { get; set; }
        public DateTime? EstimatedDeliveryDate { get; set; }
        public decimal ReservLength { get; set; }
        public decimal ReservPersons { get; set; }
        public decimal Quantity { get; set; }
        public decimal CancelQuantity { get; set; }

        // Additional fields from OrderItem (OI)
        public decimal OrderItemQuantity { get; set; }
        public decimal OrderItemCancelQuantity { get; set; }

        // Fields from OrderItemShipGroupAssoc (alias OISGA)
        public string ShipGroupSeqId { get; set; }
        public DateTime? ShipAfterDate { get; set; }
        public DateTime? ShipByDate { get; set; }
        public DateTime? EstimatedShipDateFromAssoc { get; set; }
        public decimal QuantityFromAssoc { get; set; }
        public string CarrierPartyId { get; set; }
        public string ShipmentMethodTypeId { get; set; }
        public string SupplierPartyId { get; set; }
        public string TrackingNumber { get; set; }
        public string ShippingInstructions { get; set; }
        public string GiftMessage { get; set; }
        public string IsGift { get; set; }
        public string MaySplit { get; set; }
        public string ShipGroupStatusId { get; set; }

        // Additional fields from OrderItemShipGroupAssoc (OISGA)
        public string OrderItemAssocTypeId { get; set; }
    }
}

