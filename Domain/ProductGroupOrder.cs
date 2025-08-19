namespace Domain;

public class ProductGroupOrder
{
    public ProductGroupOrder()
    {
        OrderItemGroupOrders = new HashSet<OrderItemGroupOrder>();
    }

    public string GroupOrderId { get; set; } = null!;
    public string? ProductId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public string? StatusId { get; set; }
    public decimal? ReqOrderQty { get; set; }
    public decimal? SoldOrderQty { get; set; }
    public string? JobId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public JobSandbox? Job { get; set; }
    public Product? Product { get; set; }
    public StatusItem? Status { get; set; }
    public ICollection<OrderItemGroupOrder> OrderItemGroupOrders { get; set; }
}