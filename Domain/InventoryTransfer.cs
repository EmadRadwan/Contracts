namespace Domain;

public class InventoryTransfer
{
    public string InventoryTransferId { get; set; } = null!;
    public string? StatusId { get; set; }
    public string? InventoryItemId { get; set; }
    public string? FacilityId { get; set; }
    public string? LocationSeqId { get; set; }
    public string? ContainerId { get; set; }
    public string? FacilityIdTo { get; set; }
    public string? LocationSeqIdTo { get; set; }
    public string? ContainerIdTo { get; set; }
    public string? ItemIssuanceId { get; set; }
    public DateTime? SendDate { get; set; }
    public DateTime? ReceiveDate { get; set; }
    public string? Comments { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Container? Container { get; set; }
    public Container? ContainerIdToNavigation { get; set; }
    public Facility? Facility { get; set; }
    public Facility? FacilityIdToNavigation { get; set; }
    public InventoryItem? InventoryItem { get; set; }
    public ItemIssuance? ItemIssuance { get; set; }
    public StatusItem? Status { get; set; }
}