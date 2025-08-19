namespace Domain;

public class ReturnItemResponse
{
    public ReturnItemResponse()
    {
        ReturnItems = new HashSet<ReturnItem>();
    }

    public string ReturnItemResponseId { get; set; } = null!;
    public string? OrderPaymentPreferenceId { get; set; }
    public string? ReplacementOrderId { get; set; }
    public string? PaymentId { get; set; }
    public string? BillingAccountId { get; set; }
    public string? FinAccountTransId { get; set; }
    public decimal? ResponseAmount { get; set; }
    public DateTime? ResponseDate { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public BillingAccount? BillingAccount { get; set; }
    public FinAccountTran? FinAccountTrans { get; set; }
    public OrderPaymentPreference? OrderPaymentPreference { get; set; }
    public Payment? Payment { get; set; }
    public OrderHeader? ReplacementOrder { get; set; }
    public ICollection<ReturnItem> ReturnItems { get; set; }
}