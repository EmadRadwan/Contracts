namespace Domain;

public class OrderItemChange
{
    public string OrderItemChangeId { get; set; } = null!;
    public string? OrderId { get; set; }
    public string? OrderItemSeqId { get; set; }
    public string? ChangeTypeEnumId { get; set; }
    public DateTime? ChangeDatetime { get; set; }
    public string? ChangeUserLogin { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? CancelQuantity { get; set; }
    public decimal? UnitPrice { get; set; }
    public string? ItemDescription { get; set; }
    public string? ReasonEnumId { get; set; }
    public string? ChangeComments { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Enumeration? ChangeTypeEnum { get; set; }
    public UserLogin? ChangeUserLoginNavigation { get; set; }
    public OrderItem? OrderI { get; set; }
    public Enumeration? ReasonEnum { get; set; }
}