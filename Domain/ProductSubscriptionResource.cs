namespace Domain;

public class ProductSubscriptionResource
{
    public string ProductId { get; set; } = null!;
    public string SubscriptionResourceId { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public DateTime? PurchaseFromDate { get; set; }
    public DateTime? PurchaseThruDate { get; set; }
    public int? MaxLifeTime { get; set; }
    public string? MaxLifeTimeUomId { get; set; }
    public int? AvailableTime { get; set; }
    public string? AvailableTimeUomId { get; set; }
    public int? UseCountLimit { get; set; }
    public int? UseTime { get; set; }
    public string? UseTimeUomId { get; set; }
    public string? UseRoleTypeId { get; set; }
    public string? AutomaticExtend { get; set; }
    public int? CanclAutmExtTime { get; set; }
    public string? CanclAutmExtTimeUomId { get; set; }
    public int? GracePeriodOnExpiry { get; set; }
    public string? GracePeriodOnExpiryUomId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Uom? AvailableTimeUom { get; set; }
    public Uom? CanclAutmExtTimeUom { get; set; }
    public Uom? GracePeriodOnExpiryUom { get; set; }
    public Uom? MaxLifeTimeUom { get; set; }
    public Product Product { get; set; } = null!;
    public SubscriptionResource SubscriptionResource { get; set; } = null!;
    public RoleType? UseRoleType { get; set; }
    public Uom? UseTimeUom { get; set; }
}