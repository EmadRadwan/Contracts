using System;
using Application.Facilities;
using Domain;

namespace Application.Shipments
{
    public class ItemDisplay
    {
        // Represents the OrderItem (if applicable). It can be null for aggregated results.
        public OrderItem OrderItem { get; set; }
        public string OrderId { get; set; }
        public string OrderItemSeqId { get; set; }
        
        // The quantity value to display.
        public decimal Quantity { get; set; }
        public decimal QuantityOrdered { get; set; }
        public decimal TotQuantityReserved { get; set; }
        
        // The product id to display (usually from InventoryItem).
        public string ProductId { get; set; }
        public string ProductName { get; set; }


        /// <summary>
        /// Constructor for PicklistItem input.
        /// When the incoming entity is a PicklistItem, we use its quantity and related order/inventory data.
        /// </summary>
        /// <param name="picklistItem">A domain object representing a picklist item.</param>
        public ItemDisplay(PicklistItem picklistItem)
        {
            try
            {
                // Round the quantity to 2 decimals using AwayFromZero rounding.
                Quantity = Math.Round((decimal)picklistItem.Quantity, 2, MidpointRounding.AwayFromZero);
                OrderItem = picklistItem.OrderI; // Assume PicklistItem has a property OrderI.
                ProductId = picklistItem.InventoryItem.ProductId; // And a related InventoryItem with ProductId.
                DebugLogInfo($"Created ItemDisplay (PicklistItem): Quantity={Quantity} ({ProductId})");
            }
            catch (Exception ex)
            {
                DebugLogError(ex, "Error creating ItemDisplay from PicklistItem");
            }
        }

        /// <summary>
        /// Constructor for aggregated join result input.
        /// This represents the result of joining OrderItem, OrderItemShipGrpInvRes, and InventoryItem.
        /// </summary>
        /// <param name="dto">An OrderItemReservationDto instance containing the joined data.</param>
        public ItemDisplay(ShippableItemDto dto)
        {
            try
            {
                // Use the InventoryProductId as the product id.
                ProductId = dto.ProductId;
                ProductName = dto.ProductName;
                OrderId = dto.OrderId;
                OrderItemSeqId = dto.OrderItemSeqId;
                QuantityOrdered = Math.Round(dto.QuantityOrdered, 2, MidpointRounding.AwayFromZero);
                // Use totQuantityReserved from the DTO for display, rounded to 2 decimals.
                Quantity = Math.Round(dto.TotQuantityReserved, 2, MidpointRounding.AwayFromZero);
                
                OrderItem = null; // For now, leave it null or create an instance if needed.
                DebugLogInfo($"Created ItemDisplay (Aggregated): Quantity={Quantity} ({ProductId})");
            }
            catch (Exception ex)
            {
                DebugLogError(ex, "Error creating ItemDisplay from aggregated data");
            }
        }

        /// <summary>
        /// New constructor for PicklistItemDto input.
        /// </summary>
        /// <param name="dto">A PicklistItemDto instance.</param>
        public ItemDisplay(PicklistItemDto dto)
        {
            try
            {
                Quantity = Math.Round(dto.Quantity, 2, MidpointRounding.AwayFromZero);
                OrderItem = null; // No full order item details provided in the DTO.
                // For ProductId, you might either assign InventoryItemId or perform a lookup.
                ProductId = dto.InventoryItemId;
                DebugLogInfo($"Created ItemDisplay (PicklistItemDto): Quantity={Quantity} ({ProductId})");
            }
            catch (Exception ex)
            {
                DebugLogError(ex, "Error creating ItemDisplay from PicklistItemDto");
            }
        }

        // --- Logging Helpers ---
        private void DebugLogInfo(string message)
        {
            Console.WriteLine("[INFO] " + message);
        }

        private void DebugLogError(Exception ex, string message)
        {
            Console.WriteLine("[ERROR] " + message + " Exception: " + ex.Message);
        }

        // Optionally, override Equals and GetHashCode so that two ItemDisplay instances
        // representing the same logical item are considered equal.
        public override bool Equals(object obj)
        {
            if (obj is ItemDisplay other)
            {
                // For example, compare ProductId and (if available) order item identifiers.
                return this.ProductId == other.ProductId &&
                       (this.OrderItem?.OrderItemSeqId ?? "") == (other.OrderItem?.OrderItemSeqId ?? "");
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ProductId, OrderItem?.OrderItemSeqId);
        }
    }
}
