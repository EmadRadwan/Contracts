namespace Domain;

public class OrderBlacklist
{
    public string BlacklistString { get; set; } = null!;
    public string OrderBlacklistTypeId { get; set; } = null!;
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public OrderBlacklistType OrderBlacklistType { get; set; } = null!;
}