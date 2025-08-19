namespace Application.Catalog.Products.Services.Inventory;

public class CreateInventoryItemParam
{
    public string? InventoryItemId { get; set; }
    public string? InventoryItemTypeId { get; set; }
    public string? FacilityId { get; set; }
    public string? ProductId { get; set; }
    public string? ContainerId { get; set; }
    public string? SupplierId { get; set; }
    public string? WorkEffortId { get; set; }
    public string? FacilityOwnerPartyId { get; set; }
    public string? CurrencyUomId { get; set; }
    public string? OrderId { get; set; }
    public string? OrderItemSeqId { get; set; }
    public decimal? QuantityOnHand { get; set; }
    public decimal? QuantityOnHandTotal { get; set; }
    public decimal? AvailableToPromise { get; set; }
    public DateTime? DateTimeReceived { get; set; } // Added property
    public DateTime? DateTimeManufactured { get; set; } // Added property
    public string? Comments { get; set; } // Added property
    public decimal? UnitCost { get; set; } // Added property
    public string? LotId { get; set; } // Added property
    public string? LocationSeqId { get; set; } // Added property
    public string? UomId { get; set; } // Added property
    public string? IsReturned { get; set; } // Added property
    public string? StatusId { get; set; } // Added property
}
