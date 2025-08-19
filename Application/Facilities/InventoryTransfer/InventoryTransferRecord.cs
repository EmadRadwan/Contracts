using System.ComponentModel.DataAnnotations;

namespace Application.Facilities.InventoryTransfer;

public class InventoryTransferRecord
{
    [Key] public string InventoryTransferId { get; set; } = null!;

    public string? StatusId { get; set; }
    public string? StatusDescription { get; set; }
    public string? InventoryItemId { get; set; }
    public string? ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? FacilityId { get; set; }
    public string? FacilityName { get; set; }
    public string? LocationSeqId { get; set; }
    public string? ContainerId { get; set; }
    public string? FacilityIdTo { get; set; }
    public string? FacilityToName { get; set; }
    public string? LocationSeqIdTo { get; set; }
    public string? ContainerIdTo { get; set; }
    public string? ItemIssuanceId { get; set; }
    public DateTime? SendDate { get; set; }
    public DateTime? ReceiveDate { get; set; }
    public string? Comments { get; set; }
}