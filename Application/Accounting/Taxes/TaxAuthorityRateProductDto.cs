namespace Application.Shipments.Taxes;

public class TaxAuthorityRateProductDto
{
    public string TaxAuthorityRateSeqId { get; set; }
    public string TaxAuthGeoId { get; set; }
    public string TaxAuthPartyId { get; set; }
    public string TaxAuthorityRateTypeId { get; set; }
    public string ProductStoreId { get; set; }
    public string ProductCategoryId { get; set; }
    public string TitleTransferEnumId { get; set; }
    public decimal MinItemPrice { get; set; }
    public decimal MinPurchase { get; set; }
    public string TaxShipping { get; set; }
    public decimal TaxPercentage { get; set; }
    public string TaxPromotions { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public string Description { get; set; }
    public string IsTaxInShippingPrice { get; set; }
    
    public string TaxAuthorityRateTypeDescription { get; set; } // New field
}

