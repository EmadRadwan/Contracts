namespace Domain;

public class VendorProduct
{
    public string ProductId { get; set; } = null!;
    public string VendorPartyId { get; set; } = null!;
    public string ProductStoreGroupId { get; set; } = null!;
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Product Product { get; set; } = null!;
    public ProductStoreGroup ProductStoreGroup { get; set; } = null!;
    public Party VendorParty { get; set; } = null!;
}