using System;

namespace Domain
{
    public class WorkEffortInventoryRes
    {
        public string WorkEffortInvResId { get; set; } = null!;      // FK to WorkEffort
        public string WorkEffortId { get; set; } = null!;      // FK to WorkEffort
        public string? InventoryItemId { get; set; }           // FK to InventoryItem (optional if you reserve by item)
        public string? ProductId { get; set; }                 // If you also reserve by product (optional)

        public string? ReserveOrderEnumId { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? QuantityNotAvailable { get; set; }

        public DateTime? ReservedDatetime { get; set; }
        public DateTime? CreatedDatetime { get; set; }
        public DateTime? PromisedDatetime { get; set; }
        public DateTime? CurrentPromisedDate { get; set; }

        public int? Priority { get; set; }
        public int? SequenceId { get; set; }

        public DateTime? PickStartDate { get; set; }
        public DateTime? LastUpdatedStamp { get; set; }
        public DateTime? LastUpdatedTxStamp { get; set; }
        public DateTime? CreatedStamp { get; set; }
        public DateTime? CreatedTxStamp { get; set; }

        // -------------------------
        // NAVIGATION PROPERTIES
        // -------------------------

        // Navigation to the related WorkEffort
        public WorkEffort? WorkEffort { get; set; }

        // Navigation to the related InventoryItem
        public InventoryItem? InventoryItem { get; set; }

        // (Optional) If you maintain a separate Product entity and want direct reference:
        // public Product? Product { get; set; }
    }
}