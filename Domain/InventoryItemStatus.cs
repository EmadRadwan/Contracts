namespace Domain;

public class InventoryItemStatus
{
    public string InventoryItemId { get; set; } = null!;
    public string StatusId { get; set; } = null!;
    public DateTime StatusDatetime { get; set; }
    public DateTime? StatusEndDatetime { get; set; }
    public string? ChangeByUserLoginId { get; set; }
    public string? OwnerPartyId { get; set; }
    public string? ProductId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public UserLogin? ChangeByUserLogin { get; set; }
    public InventoryItem InventoryItem { get; set; } = null!;
    public StatusItem Status { get; set; } = null!;
}