using Application.Order.Orders;

namespace Application.Catalog.ProductSuppliers;

public class SupplierProductDto
{
    public string ProductId { get; set; }
    public OrderPartyDto FromPartyId { get; set; }
    public string PartyName { get; set; }
    public DateTime AvailableFromDate { get; set; }
    public DateTime? AvailableThruDate { get; set; }
    public string? SupplierPrefOrderId { get; set; }
    public string? SupplierRatingTypeId { get; set; }
    public decimal? StandardLeadTimeDays { get; set; }
    public decimal? MinimumOrderQuantity { get; set; }
    public decimal? OrderQtyIncrements { get; set; }
    public decimal? UnitsIncluded { get; set; }
    public string? QuantityUomId { get; set; }
    public string? QuantityUomDescription { get; set; }
    public string? AgreementId { get; set; }
    public string? AgreementItemSeqId { get; set; }
    public decimal? LastPrice { get; set; }
    public decimal? ShippingPrice { get; set; }
    public string? CurrencyUomId { get; set; }
    public string CurrencyUomDescription { get; set; }
    public string? SupplierProductName { get; set; }
    public string? SupplierProductId { get; set; }
    public string? CanDropShip { get; set; }
    public string? Comments { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
}