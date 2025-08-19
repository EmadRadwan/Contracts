using System.ComponentModel.DataAnnotations;

namespace Application.Catalog.ProductStores;

public class ProductStoreRecord
{
    [Key] public string ProductStoreId { get; set; } = null!;

    public string? PrimaryStoreGroupId { get; set; }
    public string? StoreName { get; set; }
    public string? CompanyName { get; set; }
    public string? Title { get; set; }
    public string? Subtitle { get; set; }
    public string? PayToPartyId { get; set; }
    public string? PayToPartyName { get; set; }
    public int? DaysToCancelNonPay { get; set; }
    public string? ManualAuthIsCapture { get; set; }
    public string? ProrateShipping { get; set; }
    public string? ProrateTaxes { get; set; }
    public string? ViewCartOnAdd { get; set; }
    public string? AutoSaveCart { get; set; }
    public string? AutoApproveReviews { get; set; }
    public string? IsDemoStore { get; set; }
    public string? IsImmediatelyFulfilled { get; set; }
    public string? InventoryFacilityId { get; set; }
    public string? InventoryFacilityName { get; set; }
    public string? OneInventoryFacility { get; set; }
    public string? CheckInventory { get; set; }
    public string? ReserveInventory { get; set; }
    public string? ReserveOrderEnumId { get; set; }
    public string? RequireInventory { get; set; }
    public string? BalanceResOnOrderCreation { get; set; }
    public string? RequirementMethodEnumId { get; set; }
    public string? SOrderNumberPrefix { get; set; }
    public string? POrderNumberPrefix { get; set; }
    public string? DefaultLocaleString { get; set; }
    public string? DefaultCurrencyUomId { get; set; }
    public string? DefaultTimeZoneString { get; set; }
    public string? DefaultSalesChannelEnumId { get; set; }
    public string? AllowPassword { get; set; }
    public string? DefaultPassword { get; set; }
    public string? ExplodeOrderItems { get; set; }
    public string? CheckGcBalance { get; set; }
    public string? RetryFailedAuths { get; set; }
    public string? HeaderApprovedStatus { get; set; }
    public string? ItemApprovedStatus { get; set; }
    public string? DigitalItemApprovedStatus { get; set; }
    public string? HeaderDeclinedStatus { get; set; }
    public string? ItemDeclinedStatus { get; set; }
    public string? HeaderCancelStatus { get; set; }
    public string? ItemCancelStatus { get; set; }
    public string? AuthDeclinedMessage { get; set; }
    public string? AuthFraudMessage { get; set; }
    public string? AuthErrorMessage { get; set; }
    public string? VisualThemeId { get; set; }
    public string? StoreCreditAccountEnumId { get; set; }
    public string? UsePrimaryEmailUsername { get; set; }
    public string? RequireCustomerRole { get; set; }
    public string? AutoInvoiceDigitalItems { get; set; }
    public string? ReqShipAddrForDigItems { get; set; }
    public string? ShowCheckoutGiftOptions { get; set; }
    public string? SelectPaymentTypePerItem { get; set; }
    public string? ShowPricesWithVatTax { get; set; }
    public string? ShowTaxIsExempt { get; set; }
    public string? VatTaxAuthGeoId { get; set; }
    public string? VatTaxAuthPartyId { get; set; }
    public string? EnableAutoSuggestionList { get; set; }
    public string? EnableDigProdUpload { get; set; }
    public string? ProdSearchExcludeVariants { get; set; }
    public string? DigProdUploadCategoryId { get; set; }
    public string? AutoOrderCcTryExp { get; set; }
    public string? AutoOrderCcTryOtherCards { get; set; }
    public string? AutoOrderCcTryLaterNsf { get; set; }
    public int? AutoOrderCcTryLaterMax { get; set; }
    public int? StoreCreditValidDays { get; set; }
    public string? AutoApproveInvoice { get; set; }
    public string? AutoApproveOrder { get; set; }
    public string? ShipIfCaptureFails { get; set; }
    public string? SetOwnerUponIssuance { get; set; }
    public string? ReqReturnInventoryReceive { get; set; }
    public string? AddToCartRemoveIncompat { get; set; }
    public string? AddToCartReplaceUpsell { get; set; }
    public string? SplitPayPrefPerShpGrp { get; set; }
    public string? ManagedByLot { get; set; }
    public string? ShowOutOfStockProducts { get; set; }
    public string? OrderDecimalQuantity { get; set; }
    public string? AllowComment { get; set; }
    public string? AllocateInventory { get; set; }
}