namespace Domain;

public class Container
{
    public Container()
    {
        ContainerGeoPoints = new HashSet<ContainerGeoPoint>();
        InventoryItems = new HashSet<InventoryItem>();
        InventoryTransferContainerIdToNavigations = new HashSet<InventoryTransfer>();
        InventoryTransferContainers = new HashSet<InventoryTransfer>();
    }

    public string ContainerId { get; set; } = null!;
    public string? ContainerTypeId { get; set; }
    public string? FacilityId { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ContainerType? ContainerType { get; set; }
    public Facility? Facility { get; set; }
    public ICollection<ContainerGeoPoint> ContainerGeoPoints { get; set; }
    public ICollection<InventoryItem> InventoryItems { get; set; }
    public ICollection<InventoryTransfer> InventoryTransferContainerIdToNavigations { get; set; }
    public ICollection<InventoryTransfer> InventoryTransferContainers { get; set; }
}