using System.ComponentModel.DataAnnotations;

namespace Application.Shipments.FixedAssets;

public class FixedAssetRecord
{
    [Key] public string FixedAssetId { get; set; } = null!;

    public string? FixedAssetTypeId { get; set; }
    public string? FixedAssetTypeDescription { get; set; }
    public string? ParentFixedAssetId { get; set; }
    public string? InstanceOfProductId { get; set; }
    public string? ClassEnumId { get; set; }
    public string? PartyId { get; set; }
    public string? RoleTypeId { get; set; }
    public string? FixedAssetName { get; set; }
    public string? AcquireOrderId { get; set; }
    public string? AcquireOrderItemSeqId { get; set; }
    public DateTime? DateAcquired { get; set; }
    public DateTime? DateLastServiced { get; set; }
    public DateTime? DateNextService { get; set; }
    public DateTime? ExpectedEndOfLife { get; set; }
    public DateTime? ActualEndOfLife { get; set; }
    public decimal? ProductionCapacity { get; set; }
    public string? UomId { get; set; }
    public string? CalendarId { get; set; }
    public string? SerialNumber { get; set; }
    public string? LocatedAtFacilityId { get; set; }
    public string? LocatedAtLocationSeqId { get; set; }
    public decimal? SalvageValue { get; set; }
    public decimal? Depreciation { get; set; }
    public decimal? PurchaseCost { get; set; }
    public string? PurchaseCostUomId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }
}