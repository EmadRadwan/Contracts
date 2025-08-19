namespace Domain;

public class FixedAssetProduct
{
    public string FixedAssetId { get; set; } = null!;
    public string ProductId { get; set; } = null!;
    public string FixedAssetProductTypeId { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public string? Comments { get; set; }
    public int? SequenceNum { get; set; }
    public decimal? Quantity { get; set; }
    public string? QuantityUomId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public FixedAsset FixedAsset { get; set; } = null!;
    public FixedAssetProductType FixedAssetProductType { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public Uom? QuantityUom { get; set; }
}