namespace Domain;

public class ProductContent
{
    public string ProductId { get; set; } = null!;
    public string ContentId { get; set; } = null!;
    public string ProductContentTypeId { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public DateTime? PurchaseFromDate { get; set; }
    public DateTime? PurchaseThruDate { get; set; }
    public int? UseCountLimit { get; set; }
    public int? UseTime { get; set; }
    public string? UseTimeUomId { get; set; }
    public string? UseRoleTypeId { get; set; }
    public int? SequenceNum { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Content Content { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public ProductContentType ProductContentType { get; set; } = null!;
    public RoleType? UseRoleType { get; set; }
    public Uom? UseTimeUom { get; set; }
}