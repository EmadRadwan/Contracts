#nullable enable
namespace Application.Facilities.FacilityInventories;

public class FacilityInventoryItemDto
{
    public string? ProductId { get; set; }
    public string? ProductName { get; set; }
    public string InventoryItemId { get; set; } = null!;
    public string? StatusId { get; set; }
    public string? PartyId { get; set; }
    public string? PartyName { get; set; }

    public DateTime? DatetimeReceived { get; set; }
    public DateTime? DatetimeManufactured { get; set; }
    public decimal? UnitCost { get; set; }


    public DateTime? ExpireDate { get; set; }
    public string? FacilityId { get; set; }
    public string? FacilityName { get; set; }
    public decimal? QuantityOnHandTotal { get; set; }
    public decimal? AvailableToPromiseTotal { get; set; }
}