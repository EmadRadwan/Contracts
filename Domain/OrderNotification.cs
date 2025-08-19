namespace Domain;

public class OrderNotification
{
    public string OrderNotificationId { get; set; } = null!;
    public string? OrderId { get; set; }
    public string? EmailType { get; set; }
    public string? Comments { get; set; }
    public DateTime? NotificationDate { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Enumeration? EmailTypeNavigation { get; set; }
    public OrderHeader? Order { get; set; }
}