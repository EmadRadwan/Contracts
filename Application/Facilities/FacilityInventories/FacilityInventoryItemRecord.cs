#nullable enable
using System.ComponentModel.DataAnnotations;
using Application.Catalog.Products;

namespace Application.Facilities.FacilityInventories;

public class FacilityInventoryItemRecord
{
    [Key] public string InventoryItemId { get; set; } = null!;

    public string? ProductId { get; set; }
    public ProductLovDto? ProductIdObject { get; set; }
    public string? ProductName { get; set; }
    public string? StatusId { get; set; }
    public string? PartyId { get; set; }
    public string? PartyName { get; set; }
    public DateTime? DatetimeReceived { get; set; }
    public DateTime? ExpireDate { get; set; }
    public string? FacilityId { get; set; }
    public string? FacilityName { get; set; }
    public decimal? QuantityOnHandTotal { get; set; }
    public decimal? AvailableToPromiseTotal { get; set; }
    public decimal? UnitCost { get; set; }
    public string? CurrencyUomId { get; set; }
    public string? LocationSeqId { get; set; }
    public string ColorFeatureId { get; set; }
    public string ColorFeatureDescription { get; set; }
    public string SizeFeatureId { get; set; }
    public string SizeFeatureDescription { get; set; }
}