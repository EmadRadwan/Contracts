namespace Domain;

public class ReturnItemBilling
{
    public string ReturnId { get; set; } = null!;
    public string ReturnItemSeqId { get; set; } = null!;
    public string InvoiceId { get; set; } = null!;
    public string InvoiceItemSeqId { get; set; } = null!;
    public string? ShipmentReceiptId { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? Amount { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public InvoiceItem InvoiceI { get; set; } = null!;
    public ReturnHeader Return { get; set; } = null!;
    public ReturnItem ReturnI { get; set; } = null!;
    public ShipmentReceipt? ShipmentReceipt { get; set; }
}