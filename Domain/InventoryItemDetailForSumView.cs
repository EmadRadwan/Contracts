namespace Domain
{
    public class InventoryItemDetailForSumView
    {
        public string? InventoryItemTypeId { get; set; }
        public string? FacilityId { get; set; }
        public string? ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? QuantityUomDescription { get; set; }
        public decimal? UnitCost { get; set; }
        public string? CurrencyUomId { get; set; }
        public decimal? QuantityOnHandSum { get; set; }
        public decimal? AccountingQuantitySum { get; set; }

        // Newly added fields to support filtering as in Ofbiz
        public DateTime? EffectiveDate { get; set; }
        public string? OwnerPartyId { get; set; }
        public string? OrderId { get; set; }
    }
}
